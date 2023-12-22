using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class PlayerObjectSpawner : MultiplayerSpawner
{
    [Export]
    private PackedScene playerScene;
	private Node _spawnNode;
	private Array<Node> _spawnPoints;

	public void SpawnPlayers(Node spawnPoints)
    {
        _spawnNode = GetNode<Node>(SpawnPath);
		_spawnPoints = spawnPoints.GetChildren();
		int i = 0;
		foreach (var p in GameManager.Players)
		{
			int team = i % 2;
			p.Team = team;
			SpawnPlayer(p);
			i++;
		}
    }

	private void SpawnPlayer(PlayerInfo pi)
	{
		
		Player currentPlayer = playerScene.Instantiate<Player>();
		currentPlayer.OnDeath += OnPlayerDeath;
		currentPlayer.Init(pi);
		currentPlayer.SetMultiplayerAuthority(1, true);
		_spawnNode.AddChild(currentPlayer, true);
		
		var pos = ((Node3D)_spawnPoints[pi.Team]).GlobalPosition;
		currentPlayer.GlobalPosition = pos;
	}

	private async void OnPlayerDeath(Player p)
	{
		PlayerInfo pi = p.PlayerInfo;
		_spawnNode.RemoveChild(p);
		p.QueueFree();
		await Task.Delay(3000);
		SpawnPlayer(pi);
	}
}
