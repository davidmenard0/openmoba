using Godot;
using System;
using System.Xml.Schema;
using System.Linq;


public partial class MultiplayerController : Control
{
	[Export]
	private int port = 8910;

	[Export]
	private string address = "127.0.0.1";

	private ENetMultiplayerPeer peer;
	
	public override void _Ready()
	{
		
		Multiplayer.ConnectedToServer += Client_ConnectedToServer;
		Multiplayer.PeerConnected += Server_PeerConnected;
		Multiplayer.PeerDisconnected += Server_PeerDisconnected;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		if(OS.GetCmdlineArgs().Contains("--server"))
		{
			HostGame(false);
		}
	}

	#region multiplayer callbacks

	
    private void Client_ConnectedToServer()
    {
		//Called in peers when they connect to server. 
		// Send your name and ID to server.
        GD.Print("(Client) Connected To Server.");
		RpcId(1, "SendPlayerInformation", GetNode<LineEdit>("NameInput").Text, Multiplayer.GetUniqueId());
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


	#region button callbacks

	public void _on_host_button_down()
	{
		HostGame(true);
	}

	public void _on_join_button_down()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address, port);

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game!");
	}

	public void _on_start_game_button_down()
	{
		Rpc("StartGame");
	}
	#endregion

	
	private void HostGame(bool spawnServerPlayer = false){
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
			SendPlayerInformation(GetNode<LineEdit>("NameInput").Text, 1);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame()
	{
		var scene = ResourceLoader.Load<PackedScene>("res://Scenes/MainMap.tscn").Instantiate<Node>();
		GetTree().Root.AddChild(scene);
		this.Hide();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendPlayerInformation(string name, int id)
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
				Rpc("SendPlayerInformation", item.Name, item.PeerID);
			}
		}
	}
}
