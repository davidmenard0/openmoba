using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


public partial class GameManager : Singleton<GameManager>
{
	//Observer, Observee, visible
	public Action<Player, Player, bool> OnPlayerVisibilityChange;

	public Action<Player> Client_OnPlayerInit;

	public List<PlayerInfo> Players = new List<PlayerInfo>();

	private VisibilityManager _visibilityManager;
	
	private PlayerObjectSpawner _spawner;

	protected override void Initialize()
	{
		_spawner = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
		Debug.Assert(_spawner != null, "ERROR: Cannot find PlayerOBjectSpawner in GameManager.");

		_visibilityManager = GetNode<VisibilityManager>("VisibilityManager");
		Debug.Assert(_visibilityManager != null, "ERROR: Cannot find VisibilityManager in GameManager.");
		_visibilityManager.Init();

		Client_OnPlayerInit += AddPlayerToSpawner;
	}

    public override void _ExitTree()
    {
        Client_OnPlayerInit -= AddPlayerToSpawner;
    }


    //Called on all clients when the player has initialized itself. 
    public void AddPlayerToSpawner(Player p)
	{
		_spawner.ClientPlayers.Add(p);
	}
}
