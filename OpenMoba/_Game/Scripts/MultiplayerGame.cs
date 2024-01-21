using Godot;
using System;
using System.Xml.Schema;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// MultiplyerGame is to setup the game & connectiosn in the lobby
//  vs GameManager is to manage the in-game experience
public partial class MultiplayerGame : Node
{

	#region Singleton
	//Singleton base class does not work because of this issue: https://github.com/godotengine/godot/issues/79519
	public static MultiplayerGame Instance { 
        get { return _instance; } 
        private set { _instance = value; } 
    }
    private static MultiplayerGame _instance;

    public override void _Ready() {
        if (_instance == null) {
            _instance = this as MultiplayerGame;
            Initialize();
        }
        else {
            this.QueueFree();
        }
    }
	#endregion

	public Action<PlayerInfo> Client_OnNewPlayerJoined;
	public Action<int> Client_OnPlayerDisconnected; // PlayerID

	public bool IsClient = false;

	private int _connectedPort = 7777;
	private string _connectedIP = "127.0.0.1";

	private ENetMultiplayerPeer _peer;
	private string _name = "";
	private PortForwarding _portForwarding;
	
	protected void Initialize()
	{
		UIController.Instance.OnHostLANClicked += HostGame;
		UIController.Instance.OnHostInternetClicked += HostInternetGame;
		UIController.Instance.OnJoinClicked += JoinGame;
		UIController.Instance.OnStartPressed += StartGame;
		UIController.Instance.OnLeavePressed += LeaveGame;

		Multiplayer.ConnectedToServer += Client_ConnectedToServer;
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		Multiplayer.ServerDisconnected += Client_DisconnectedFromServer;

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
		UIController.Instance.OnStartPressed -= StartGame;
		UIController.Instance.OnLeavePressed -= LeaveGame;

		Multiplayer.ConnectedToServer -= Client_ConnectedToServer;
		Multiplayer.PeerConnected -= PeerConnected;
		Multiplayer.PeerDisconnected -= PeerDisconnected;
		Multiplayer.ConnectionFailed -= ConnectionFailed;
		Multiplayer.ServerDisconnected -= Client_DisconnectedFromServer;
    }

	private void HostGame(string name){
		_name = name;
		_peer = new ENetMultiplayerPeer();
		var error = _peer.CreateServer(_portForwarding.Port, 2);
		if(error != Error.Ok)
		{
			Logger.Log("ERROR cannot host game:" + error.ToString());
			return;
		}

		Multiplayer.MultiplayerPeer = _peer;
		Logger.Log("Waiting For Players!");
		
		//Spawn a server client
		IsClient = true;

		UIController.Instance.OnLocalPlayerJoinedGame?.Invoke(_portForwarding.LocalIP, _portForwarding.Port);

		RPC_Server_ReceiveNewPlayerInfo(1, _name);
	}

	private void HostInternetGame(string name){
		var worker = new BackgroundWorker();
		worker.DoWork += delegate{_portForwarding.SetupPortForwarding();};
		worker.RunWorkerCompleted += (sender, e) => {
			UIController.Instance.OnLocalPlayerJoinedGame?.Invoke(_portForwarding.PublicIP, _portForwarding.Port);
		};
		worker.RunWorkerAsync();
	}

	private void JoinGame(string name, string ip, int port)
	{
		_name = name;
		_connectedIP = ip;
		_connectedPort = port;
		_peer = new ENetMultiplayerPeer();
		var error = _peer.CreateClient(ip, port);
		if(error != Error.Ok)
		{
			Logger.Log("Error: cannot join game:" + error.ToString());
			return;
		}

		Multiplayer.MultiplayerPeer = _peer;
		Logger.Log("Joining Game!");
	}

	#region multiplayer callbacks

    private void Client_ConnectedToServer()
    {
		IsClient = true;
		UIController.Instance.OnLocalPlayerJoinedGame?.Invoke(_connectedIP,_connectedPort);

		//Called in clients when they connect to server. 
		// Send your name and ID to server.
        Logger.Log("(Client) Connected To Server. Sending player info.");
		RpcId(1, "RPC_Server_ReceiveNewPlayerInfo", Multiplayer.GetUniqueId(), _name);
		RpcId(1, "RPC_RequestPlayerListFromServer", Multiplayer.GetUniqueId());
    }

	private void Client_DisconnectedFromServer()
	{
		Multiplayer.MultiplayerPeer = null;
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
		Server_RemovePlayer((int)id);
    }

    private void ConnectionFailed()
    {
		Multiplayer.MultiplayerPeer = null;
		Logger.Log("CONNECTION FAILED");
    }


	#endregion
	

	#region Client RPCs

	//A new player has connected, so send the new player info to everyone
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Server_ReceiveNewPlayerInfo(int id, string name)
	{
		if(!Multiplayer.IsServer()) return;
		
		Server_AddNewPlayer(id, name);
		PlayerInfo p = GameManager.Instance.GetPlayerInfo(id);
		Server_AssignTeam(p);
		Rpc("RPC_Client_OnNewPlayerJoined", id, name, p.Team);
	}

	// A single client requests a list of already connected players from the server
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_RequestPlayerListFromServer(int id)
	{
		if(!Multiplayer.IsServer()) return;

		foreach(var p in GameManager.Instance.Players)
		{
			RpcId(id, "RPC_Client_OnNewPlayerJoined", p.Value.PeerID, p.Value.Name, p.Value.Team);
		}		
	}

	

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_OnNewPlayerJoined(int id, string name, int team)
	{
		if(!IsClient) return;

		PlayerInfo playerInfo = new PlayerInfo(){
			Name = name,
			PeerID = id,
			Team = team
		};
		Client_OnNewPlayerJoined?.Invoke(playerInfo);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_OnPlayerDisconnected(int id)
	{
		if(!IsClient) return;
		Multiplayer.MultiplayerPeer = null;
		Client_OnPlayerDisconnected?.Invoke(id);
	}

	#endregion

	private void Server_AssignTeam(PlayerInfo new_player)
	{
		if(!Multiplayer.IsServer()) return;
		
		int team1Count = 0;
		int team2Count = 0;
		foreach(var p in GameManager.Instance.Players)
		{
			if(p.Value.Team == 0) team1Count++;
			else if(p.Value.Team == 1) team2Count++;
		}

		if(team2Count >= team1Count) new_player.Team = 0;
		else new_player.Team = 1;
	}

	private void Server_AddNewPlayer(int id, string name)
	{
		if(!Multiplayer.IsServer()) return;

		if(!GameManager.Instance.Players.ContainsKey(id)){
			PlayerInfo playerInfo = new PlayerInfo(){
				Name = name,
				PeerID = id
			};
			GameManager.Instance.Players[id] = playerInfo;
		}
	}

	private void Server_RemovePlayer(int id)
	{
		if(!Multiplayer.IsServer()) return;

		if(GameManager.Instance.Players.ContainsKey(id)){
			GameManager.Instance.RemovePlayer(id);
			Rpc("RPC_Client_OnPlayerDisconnected", id);
		}
	}

	private void StartGame()
	{
		GameManager.Instance.StartGame("MainMap");
	}
	
    private void LeaveGame()
    {
        _peer.Close();
		Multiplayer.MultiplayerPeer = null;
		UIController.Instance.OnLocalPlayerLeftGame?.Invoke();
    }
}
