using Godot;
using System;
using System.Xml.Schema;
using System.Linq;
using System.ComponentModel;


public partial class MultiplayerController : Node
{
	[Export]
	private int port = 8910;

	[Export]
	private string address = "127.0.0.1";

	private UIController ui;
	private ENetMultiplayerPeer peer;
	private string _name = "";
	
	public override void _Ready()
	{
		ui = GetNode<UIController>("/root/Main/UI");
		ui.OnHostClicked += HostGame;
		ui.OnJoinClicked += JoinGame;
		ui.OnStartClicked += StartGame;

		Multiplayer.ConnectedToServer += Puppet_ConnectedToServer;
		Multiplayer.PeerConnected += Server_PeerConnected;
		Multiplayer.PeerDisconnected += Server_PeerDisconnected;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		if(OS.GetCmdlineArgs().Contains("--server"))
		{
			HostGame("", false);
		}
	}

    public override void _ExitTree()
    {
        ui.OnHostClicked -= HostGame;
		ui.OnJoinClicked -= JoinGame;
		ui.OnStartClicked -= StartGame;

		Multiplayer.ConnectedToServer -= Puppet_ConnectedToServer;
		Multiplayer.PeerConnected -= Server_PeerConnected;
		Multiplayer.PeerDisconnected -= Server_PeerDisconnected;
		Multiplayer.ConnectionFailed -= ConnectionFailed;
    }

    #region multiplayer callbacks


    private void Puppet_ConnectedToServer()
    {
		//Called in peers when they connect to server. 
		// Send your name and ID to server.
        GD.Print("(Client) Connected To Server.");
		RpcId(1, "RPC_SendPlayerInformation", _name, Multiplayer.GetUniqueId());
    }

    private void Server_PeerConnected(long id)
    {
		//Dont do anything here, the peer will send its info to server,
		// including its name.
        GD.Print("(Server/Client) Peer Connected to server: " + id.ToString());
    }
	
    private void Server_PeerDisconnected(long id)
    {
        GD.Print("Player Disconnected: " + id.ToString());
		GameManager.Players.Remove(GameManager.Players.Where(i => i.PeerID == id).First<PlayerInfo>());
		var players = GetTree().GetNodesInGroup("Player");
		
		foreach (var item in players)
		{
			if(item.Name == id.ToString()){
				item.QueueFree();
			}
		}
    }

    private void ConnectionFailed()
    {
		GD.Print("CONNECTION FAILED");
    }


	#endregion
	
	public void HostGame(string name, bool spawnServerPlayer = false){
		_name = name;
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 2);
		if(error != Error.Ok)
		{
			GD.Print("error cannot host! :" + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting For Players!");
		
		if(spawnServerPlayer)
			RPC_SendPlayerInformation(_name, 1);
	}

	public void JoinGame(string name)
	{
		_name = name;
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address, port);

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game!");
	}

	public void StartGame()
	{
		LoadMap();
		Rpc("RPC_StartGame");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_StartGame()
	{
		ui.OnGameStarted?.Invoke();
	}

	[Rpc]
	private void RPC_SendPlayerInformation(string name, int id)
	{
		PlayerInfo playerInfo = new PlayerInfo(){
			Name = name,
			PeerID = id
		};
		
		if(!GameManager.Players.Contains(playerInfo)){
			GameManager.Players.Add(playerInfo);
		}

		if(Multiplayer.IsServer()){
			foreach (var item in GameManager.Players)
			{
				Rpc("RPC_SendPlayerInformation", item.Name, item.PeerID);
			}
		}
	}

	//Map sync is handled by the MapSpawner object
	private void LoadMap()
	{
		if(!Multiplayer.IsServer()) return;

		var map = GetNode("../Map");
		foreach(var c in map.GetChildren())
		{
			map.RemoveChild(c);
			c.QueueFree();
		}

		Node3D newmap = ResourceLoader.Load<PackedScene>("res://Scenes/Maps/MainMap.tscn").Instantiate<Node3D>();
		map.AddChild(newmap);
	}
}
