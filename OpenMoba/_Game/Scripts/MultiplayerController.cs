using Godot;
using System;
using System.Xml.Schema;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;


public partial class MultiplayerController : Node
{
	private int port = 7777;
	private string address = "127.0.0.1";

	private ENetMultiplayerPeer peer;
	private string _name = "";
	private PortForwarding _portForwarding;
	
	public override void _Ready()
	{
		UIController.Instance.OnHostLANClicked += HostGame;
		UIController.Instance.OnHostInternetClicked += HostInternetGame;
		UIController.Instance.OnJoinClicked += JoinGame;
		UIController.Instance.OnStartClicked += StartGame;

		Multiplayer.ConnectedToServer += Client_ConnectedToServer;
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectionFailed += ConnectionFailed;

		_portForwarding = GetNodeOrNull<PortForwarding>("PortForwarding");
		Debug.Assert(_portForwarding != null, "ERROR: Cant find PortForwarding under MultiplayerControler");
		

		if(OS.GetCmdlineArgs().Contains("--server"))
		{
			HostInternetGame("");
		}
	}

    public override void _ExitTree()
    {
        UIController.Instance.OnHostLANClicked -= HostGame;
		UIController.Instance.OnHostInternetClicked -= HostInternetGame;
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
		GameManager.Instance.IsClient = true;

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

	private void HostGame(string name){
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
		
		//Spawn a server client
		GameManager.Instance.IsClient = true;
		RPC_SendInfoToServer(1, _name);

		UIController.Instance.OnGameCreated?.Invoke(_portForwarding.LocalIP, port );
	}

	private void HostInternetGame(string name){
		var worker = new BackgroundWorker();
		worker.DoWork += delegate{_portForwarding.SetupPortForwarding(port);};
		worker.RunWorkerCompleted += (sender, e) => {
			UIController.Instance.OnGameCreated(_portForwarding.PublicIP, _portForwarding.Port);
		};
		worker.RunWorkerAsync();
	}

	private void JoinGame(string name, string ip, int port)
	{
		_name = name;
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateClient(ip, port);
		if(error != Error.Ok)
		{
			Logger.Log("Error: cannot join :" + error.ToString());
			return;
		}

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		Logger.Log("Joining Game!");
	}

	public void StartGame()
	{
		GameManager.Instance.StartGame("MainMap");
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
	

	#region Client RPCs


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
}
