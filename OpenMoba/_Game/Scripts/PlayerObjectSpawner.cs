using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class PlayerObjectSpawner : MultiplayerSpawner
{
    [Export]
    private PackedScene playerScene;
	[Export]
    private float PlayerRespawnTime = 3f;
	private Node _spawnNode;
	private Array<Node> _spawnPoints;
	private UIController UI;

	public void SpawnPlayers(Node spawnPoints)
    {
		UI = GetNode<UIController>("/root/Main/UI");
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

	private async void SpawnPlayer(PlayerInfo pi)
	{
		RpcId(pi.PeerID, "RPC_Client_NotifyPlayerSpawn", PlayerRespawnTime);
		await Task.Delay(Mathf.RoundToInt(PlayerRespawnTime*1000f));

		Player currentPlayer = playerScene.Instantiate<Player>();
		currentPlayer.OnDeath += OnPlayerDeath;
		currentPlayer.Init(pi);
		currentPlayer.SetMultiplayerAuthority(1, true);
		_spawnNode.AddChild(currentPlayer, true);
		
		var pos = ((Node3D)_spawnPoints[pi.Team]).GlobalPosition;
		currentPlayer.GlobalPosition = pos;
	}

	private void OnPlayerDeath(Player p)
	{
		PlayerInfo pi = p.PlayerInfo;
		_spawnNode.RemoveChild(p);
		p.QueueFree();
		SpawnPlayer(pi);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_NotifyPlayerSpawn(float deathTimer)
	{
		UI.OnLocalPlayerRespawn?.Invoke(deathTimer);
	}
}
