using Godot;
using System;
using System.Data.Common;
using System.Diagnostics;


public partial class Map : Node3D
{
	[Export]
	private PackedScene playerScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;

		var targets = GetNode<Node3D>("ObjectiveTargets");
		Debug.Assert(targets != null, "ERROR: Couldn't find ObjectiveTargets in the map.");

		var spawnPoints = FindChild("SpawnPoints").GetChildren();
		int i = 0;
		foreach (var p in GameManager.Players)
		{
			Player currentPlayer = playerScene.Instantiate<Player>();
			p.Team = i % 2;
			currentPlayer.Init(p);
			currentPlayer.SetMultiplayerAuthority(1, true);
			AddChild(currentPlayer);
			currentPlayer.GlobalPosition = ((Node3D)spawnPoints[p.Team]).GlobalPosition;
			i++;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
