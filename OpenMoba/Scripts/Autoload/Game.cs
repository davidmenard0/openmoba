using Godot;
using GodotPlugins.Game;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Xml.Resolvers;

public partial class Game : Node
{
	public const int PORT = 8023;

	private Node _main;
	private Node _players;
	private Node _map;
	private Node _menu;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_main = GetTree().Root.GetNode("Main");
		_players = _main.GetNode("Players");

		_menu = ResourceLoader.Load<PackedScene>("res://scenes/menu.tscn").Instantiate();
		_main.AddChild(_menu);

		//TODO: This isn't working for some reason, so instead, we just call it in Menu.cs for now
		// until we can figure it out
		Multiplayer.PeerConnected += SpawnPlayer;
		Multiplayer.PeerDisconnected += RemovePlayer;

		//TODO: Same as above, this isn't working
		//This is a signal, no errors
		/*var error = Multiplayer.Connect("peer_connected", new Callable(this, nameof(this.SpawnPlayer)));
		if(error != Error.Ok)
		{
			GD.Print("Error: Peer not connected");
		}*/
	}

    public override void _Process(double delta)
	{
	}

	public void LoadMap()
	{
		if(_map != null)
			_map.QueueFree();
		if(_menu != null)
			_menu.QueueFree();

		_map = ResourceLoader.Load<PackedScene>("res://scenes/map.tscn").Instantiate();
		_main.AddChild(_map);
	}

    public void SpawnPlayer(long id)
    {
        Player player = (Player)ResourceLoader.Load<PackedScene>("res://scenes/player.tscn").Instantiate();
		player.PeerID = Mathf.RoundToInt(id);
		_players.AddChild(player);
    }

	private void _OnPeerConnected(long id)
    {
        Player player = (Player)ResourceLoader.Load<PackedScene>("res://scenes/player.tscn").Instantiate();
		player.PeerID = Mathf.RoundToInt(id);
		_players.AddChild(player);
    }

    private void RemovePlayer(long id)
    {
        if( !_players.HasNode(id.ToString()) )
			return;
		_players.GetNode(id.ToString()).QueueFree();
    }


}
