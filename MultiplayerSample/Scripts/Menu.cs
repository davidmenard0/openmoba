using Godot;
using System;
using System.Diagnostics;

public partial class Menu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_create_pressed()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(Game.PORT);
		if(error != Error.Ok)
		{
			GD.Print("Error, cannot host! : " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Server Created");

		if(peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Disconnected){
			GD.Print("Failde to start server");
			return;
		}

		var game = GetNode<Game>("/root/Game");
		game.LoadMap();

		//TODO: We shouldn't have to call this. It *should* be the callback from 
		// Multiplayer.PeerConnected, but for some reason, Im not receiving the callback
		game.SpawnPlayer(peer.GetUniqueId());
	}
	public void _on_connect_pressed()
	{
		var peer = new ENetMultiplayerPeer();
		var error = peer.CreateClient("localhost", Game.PORT);
		if(error != Error.Ok)
		{
			GD.Print("Error, cannot connect! : " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Client Connected");
		var game = GetNode<Game>("/root/Game");
		game.LoadMap();

		//TODO: We shouldn't have to call this. It *should* be the callback from 
		// Multiplayer.PeerConnected, but for some reason, Im not receiving the callback
		game.SpawnPlayer(peer.GetUniqueId());
	}
}
