using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PlayerObjectSpawner : MultiplayerSpawner
{
	public Action<Player> Server_OnPlayerSpawn;
	public Action Server_OnPlayerDespawn;

	public Action<Projectile> Server_OnProjectileSpawn;
	public Action Server_OnProjectileDespawn;

	[Export]
	private PackedScene PlayerTemplate;

	//Remember to update these on Server and Clients
	public Dictionary<int, Player> Players = new Dictionary<int, Player>();

	public Dictionary<int, Projectile> Projectiles = new Dictionary<int, Projectile>();

	private Node _spawnNode;
	private List<Node> _playerSpawnPoints = new List<Node>();

	public override void _Ready()
	{
	}

	public void SpawnPlayers(Node spawnPoints)
	{
		if(!Multiplayer.IsServer()) return;

		_spawnNode = GetNode<Node>(SpawnPath);

		_playerSpawnPoints.Add(spawnPoints.GetChild(0));
		_playerSpawnPoints.Add(spawnPoints.GetChild(1));
		int i = 0;
		foreach (var p in GameManager.Instance.Players)
		{
			int team = i % 2;
			p.Value.Team = team;
			SpawnPlayer(p.Value.PeerID);
			i++;
		}
	}

	private async void SpawnPlayer(int id)
	{
		if(!Multiplayer.IsServer()) return;
		
		RpcId(id, "RPC_Client_NotifyPlayerSpawn", Balance.Get("Game.RespawnTimer"));
		await Task.Delay(Mathf.RoundToInt(Balance.Get("Game.RespawnTimer")*1000f));

		Player currentPlayer = PlayerTemplate.Instantiate<Player>();
		currentPlayer.OnDeath += OnPlayerDeath;
		currentPlayer.OwnerID = id;
		_spawnNode.AddChild(currentPlayer, true);
		
		int team = GameManager.Instance.GetPlayerInfo(id).Team;
		var pos = ((Node3D)_playerSpawnPoints[team]).GlobalPosition;
		currentPlayer.GlobalPosition = pos;

		Players[id] = currentPlayer;

		Server_OnPlayerSpawn?.Invoke(currentPlayer);
	}

	private void OnPlayerDeath(Player p)
	{
		if(!Multiplayer.IsServer()) return;
		
		_spawnNode.RemoveChild(p);
		p.QueueFree();
		Server_OnPlayerDespawn?.Invoke();
		SpawnPlayer(p.OwnerID);

		Players.Remove(p.OwnerID);
	}

	public void SpawnProjectile(Player owner, PackedScene projectileTemplate, Vector3 position, Vector3 direction)
	{
		if(!Multiplayer.IsServer()) return;

		var projectile = projectileTemplate.Instantiate<Projectile>();
		_spawnNode.AddChild(projectile, true);
		projectile.Init(owner.OwnerID, position, direction);
		projectile.GlobalPosition = position;
		projectile.Direction = direction;

		Projectiles[projectile.UID] = projectile;

		Server_OnProjectileSpawn?.Invoke(projectile);
	}

	public void DespawnProjectile(Projectile p)
	{
		if(!Multiplayer.IsServer()) return;
		
		if(p == null) return; //Projectile might have dies in several ways

		FXManager.Instance.PlayVFX("hit_smoke", p.GlobalPosition);
		FXManager.Instance.PlayAudio("projectile_hit", p.GlobalPosition);

		_spawnNode.RemoveChild(p);
		p.QueueFree();

		Projectiles.Remove(p.OwnerID);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_NotifyPlayerSpawn(float deathTimer)
	{
		UIController.Instance.OnLocalPlayerRespawn?.Invoke(deathTimer);
	}
}
