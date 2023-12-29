using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PlayerObjectSpawner : MultiplayerSpawner
{
    [Export]
    private PackedScene playerScene;
	[Export]
    private float PlayerRespawnTime = 3f;

	//These two could be the same list, but its clearer if we keep them separated
	public List<Player> ServerPlayers = new List<Player>();
	public List<Player> ClientPlayers = new List<Player>();

	private Node _spawnNode;
	private Array<Node> _spawnPoints;
	private UIController UI;
	private GameManager GM;

    public override void _Ready()
    {
        UI = GetNode<UIController>("/root/Main/UI");
		GM = GetNode<GameManager>("/root/Main/GameManager");
    }

    public void SpawnPlayers(Node spawnPoints)
    {
		if(!Multiplayer.IsServer()) return;

        _spawnNode = GetNode<Node>(SpawnPath);
		_spawnPoints = spawnPoints.GetChildren();
		int i = 0;
		foreach (var p in GM.Players)
		{
			int team = i % 2;
			p.Team = team;
			SpawnPlayer(p);
			i++;
		}
    }

	private async void SpawnPlayer(PlayerInfo pi)
	{
		if(!Multiplayer.IsServer()) return;
		
		RpcId(pi.PeerID, "RPC_Client_NotifyPlayerSpawn", PlayerRespawnTime);
		await Task.Delay(Mathf.RoundToInt(PlayerRespawnTime*1000f));

		Player currentPlayer = playerScene.Instantiate<Player>();
		currentPlayer.OnDeath += OnPlayerDeath;
		currentPlayer.SetMultiplayerAuthority(1, true);
		currentPlayer.Server_Init(pi);
		_spawnNode.AddChild(currentPlayer, true);
		
		var pos = ((Node3D)_spawnPoints[pi.Team]).GlobalPosition;
		currentPlayer.GlobalPosition = pos;

		ServerPlayers.Add(currentPlayer);
	}

	private void OnPlayerDeath(Player p)
	{
		if(!Multiplayer.IsServer()) return;
		
		PlayerInfo pi = p.PlayerInfo;
		_spawnNode.RemoveChild(p);
		p.QueueFree();
		SpawnPlayer(pi);

		ServerPlayers.Remove(p);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_NotifyPlayerSpawn(float deathTimer)
	{
		UI.OnLocalPlayerRespawn?.Invoke(deathTimer);
	}
}
