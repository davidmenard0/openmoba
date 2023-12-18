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

	private UIController UI;
	private ENetMultiplayerPeer peer;
	private string _name = "";
	
	public override void _Ready()
	{
		UI = GetNode<UIController>("/root/Main/UI");
		UI.OnHostClicked += HostGame;
		UI.OnJoinClicked += JoinGame;
		UI.OnStartClicked += StartGame;

		Multiplayer.ConnectedToServer += Client_ConnectedToServer;
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		if(OS.GetCmdlineArgs().Contains("--server"))
		{
			HostGame("", false);
		}
	}

    public override void _ExitTree()
    {
        UI.OnHostClicked -= HostGame;
		UI.OnJoinClicked -= JoinGame;
		UI.OnStartClicked -= StartGame;

		Multiplayer.ConnectedToServer -= Client_ConnectedToServer;
		Multiplayer.PeerConnected -= PeerConnected;
		Multiplayer.PeerDisconnected -= PeerDisconnected;
		Multiplayer.ConnectionFailed -= ConnectionFailed;
    }

    #region multiplayer callbacks


    private void Client_ConnectedToServer()
    {
		//Called in clients when they connect to server. 
		// Send your name and ID to server.
        GD.Print("(Client) Connected To Server. Sending player info.");
		RpcId(1, "RPC_SendInfoToServer", Multiplayer.GetUniqueId(), _name);
    }

    private void PeerConnected(long id)
    {
		//Dont do anything here, the client will send its info to server,
		// including its name.
        GD.Print("(Server/Client) Peer Connected to server: " + id.ToString());
    }
	
    private void PeerDisconnected(long id)
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
	

	#region RPCs

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_StartGame()
	{
		UI.OnGameStarted?.Invoke();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_SendInfoToServer(int id, string name)
	{
		UpdatePlayerInfo(id, name);
		
		//Server sends back player info to everyone
		if(Multiplayer.IsServer()){
			foreach (var item in GameManager.Players)
			{
				Rpc("RPC_ReceiveInfoOnClients", item.PeerID, item.Name);
			}
		}
	}

	//Dont call local, server has the info
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_ReceiveInfoOnClients(int id, string name)
	{
		UpdatePlayerInfo(id, name);
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
			RPC_SendInfoToServer(1, _name);
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

	private void UpdatePlayerInfo(int id, string name)
	{
		PlayerInfo playerInfo = new PlayerInfo(){
			Name = name,
			PeerID = id
		};
		
		if(!GameManager.Players.Contains(playerInfo)){
			GameManager.Players.Add(playerInfo);
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

		Node3D newmap = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/Maps/MainMap.tscn").Instantiate<Node3D>();
		map.AddChild(newmap);
	}
}
