using Godot;
using Godot.Collections;
using System;

public partial class PlayerObjectSpawner : MultiplayerSpawner
{
    [Export]
    private PackedScene playerScene;

	public void SpawnPlayers(Array<Node> spawnPoints)
    {
        Node spawnNode = GetNode<Node>(SpawnPath);
		int i = 0;
		foreach (var p in GameManager.Players)
		{
			Player currentPlayer = playerScene.Instantiate<Player>();
			p.Team = i % 2;
			currentPlayer.Init(p);
			currentPlayer.SetMultiplayerAuthority(1, true);
			spawnNode.AddChild(currentPlayer, true);
			currentPlayer.GlobalPosition = ((Node3D)spawnPoints[p.Team]).GlobalPosition;
			i++;
		}
    }
}
