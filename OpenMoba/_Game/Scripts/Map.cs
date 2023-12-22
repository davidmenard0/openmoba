using Godot;
using System;
using System.Data.Common;
using System.Diagnostics;


public partial class Map : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;

		var targets = GetNode<Node3D>("ObjectiveTargets");
		Debug.Assert(targets != null, "ERROR: Couldn't find ObjectiveTargets in the map.");

		var spawnPoints = FindChild("SpawnPoints");
		Debug.Assert(spawnPoints != null, "ERROR: Could not find SpawnPoints in map.");

		PlayerObjectSpawner ps = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
		Debug.Assert(ps != null, "ERROR: Could not find PlayerObjectSpawner in map.");
		ps.SpawnPlayers(spawnPoints);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
