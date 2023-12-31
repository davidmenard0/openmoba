using Godot;
using System;
using System.Xml.Schema;
using System.Linq;
using System.ComponentModel;


public partial class MultiplayerController : Node
{
	private int port = 8910;
	private string address = "127.0.0.1";

	private ENetMultiplayerPeer peer;
	private string _name = "";
	
	public override void _Ready()
	{
		UIController.Instance.OnHostClicked += HostGame;
		UIController.Instance.OnJoinClicked += JoinGame;
		UIController.Instance.OnStartClicked += StartGame;

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
        UIController.Instance.OnHostClicked -= HostGame;
		UIController.Instance.OnJoinClicked -= JoinGame;
		UIController.Instance.OnStartClicked -= StartGame;

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
        Logger.Log("(Client) Connected To Server. Sending player info.");
		RpcId(1, "RPC_SendInfoToServer", Multiplayer.GetUniqueId(), _name);
    }

    private void PeerConnected(long id)
    {
		//Dont do anything here, the client will send its info to server,
		// including its name.
        Logger.Log("(Server/Client) Peer Connected to server: " + id.ToString());
    }
	
    private void PeerDisconnected(long id)
    {
        Logger.Log("Player Disconnected: " + id.ToString());
		GameManager.Instance.Players.Remove((int)id); // TODO: should be a callback
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
		Logger.Log("CONNECTION FAILED");
    }


	#endregion
	

	#region RPCs

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_StartGame()
	{
		UIController.Instance.OnGameStarted?.Invoke();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_SendInfoToServer(int id, string name)
	{
		UpdatePlayerInfo(id, name);
		
		//Server sends back player info to everyone
		if(Multiplayer.IsServer()){
			foreach (var item in GameManager.Instance.Players)
			{
				Rpc("RPC_ReceiveInfoOnClients", item.Value.PeerID, item.Value.Name);
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
			Logger.Log("error cannot host! :" + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		Logger.Log("Waiting For Players!");
		
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
		Logger.Log("Joining Game!");
	}

	public void StartGame()
	{
		LoadMap();
		Rpc("RPC_StartGame");
	}

	private void UpdatePlayerInfo(int id, string name)
	{
		if(!GameManager.Instance.Players.ContainsKey(id)){
			PlayerInfo playerInfo = new PlayerInfo(){
				Name = name,
				PeerID = id
			};
			GameManager.Instance.Players[id] = playerInfo;
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
