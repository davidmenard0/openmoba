using Godot;
using System;
using System.Data.Common;


public partial class Map : Node3D
{
	[Export]
	private PackedScene playerScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var spawnPoints = FindChild("SpawnPoints").GetChildren();
		int index = 0;
		foreach (var item in GameManager.Players)
		{
			Player currentPlayer = playerScene.Instantiate<Player>();
			currentPlayer._playerInfo = item;
			currentPlayer.SetUpPlayer(item.Name);
			AddChild(currentPlayer);
			int team = index % 2;
			currentPlayer.GlobalPosition = ((Node3D)spawnPoints[team]).GlobalPosition;
			index++;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
