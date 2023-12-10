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
		var peer = new ENetMultiplayerPeer();
		peer.CreateServer(1337);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Server Created");

		var game = GetNode<Game>("/root/Game");
		game.LoadMap();
	}
	public void _on_connect_pressed()
	{
		var peer = new ENetMultiplayerPeer();
		peer.CreateClient("localhost", 1337);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Client Connected");
		
		var game = GetNode<Game>("/root/Game");
		game.LoadMap();
	}
}
