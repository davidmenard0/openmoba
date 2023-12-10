using Godot;
using GodotPlugins.Game;
using System;
using System.Data;
using System.Xml.Resolvers;

public partial class Game : Node
{
	const int PORT = 1337;

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

		Multiplayer.PeerConnected += SpawnPlayer;
		Multiplayer.PeerDisconnected += RemovePlayer;
	}


    // Called every frame. 'delta' is the elapsed time since the previous frame.
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

    private void SpawnPlayer(long id)
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
