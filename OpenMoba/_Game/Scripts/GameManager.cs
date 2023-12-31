using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


public partial class GameManager : Singleton<GameManager>
{
	//Game Messages
	
	//Triggered by playerVision when a collider enters/Exits a vision area.
	// Observer, Observee, Tnered/Exited
	public Action<Player, Player, bool> OnNodeVisionAreaTransition;
	//Triggered when a projectile hits or expires, used by Spawner to keep track of them
	public Action<Projectile> OnProjectileDespawn;

	public Action<Player> Client_OnPlayerInit;
	public Action<Projectile> Client_OnProjectileInit;
	public PlayerObjectSpawner Spawner;

	//This list is only maintained on the server side
	public Dictionary<int, PlayerInfo> Players = new Dictionary<int, PlayerInfo>();

	private VisibilityManager _visibilityManager;
	

	protected override void Initialize()
	{
		Spawner = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
		Debug.Assert(Spawner != null, "ERROR: Cannot find PlayerOBjectSpawner in GameManager.");

		_visibilityManager = GetNode<VisibilityManager>("VisibilityManager");
		Debug.Assert(_visibilityManager != null, "ERROR: Cannot find VisibilityManager in GameManager.");
		_visibilityManager.Init();

		Client_OnPlayerInit += Client_AddPlayerToSpawner;
		Client_OnProjectileInit += Client_AddProjectileToSpawner;
	}

    public override void _ExitTree()
    {
        Client_OnPlayerInit -= Client_AddPlayerToSpawner;
		Client_OnProjectileInit -= Client_AddProjectileToSpawner;
    }

	public PlayerInfo GetPlayerInfo(int id)
	{
		return Players[id];
	}

	public int GetNodeTeam(Node node)
	{
		int id = -1;
		if(node is Player)
			id = ((Player)node).OwnerID;
		else if(node is Projectile)
			id = ((Projectile)node).OwnerID;
		return Players[id].Team;
	}

    //Called on all clients when the player has initialized itself. 
    private void Client_AddPlayerToSpawner(Player p)
	{
		if(Multiplayer.IsServer()) return;
		Spawner.Players[p.OwnerID] = p;
	}

	private void Client_AddProjectileToSpawner(Projectile p)
	{
		if(Multiplayer.IsServer()) return;
		Spawner.Projectiles[p.OwnerID] = p;
	}
}
