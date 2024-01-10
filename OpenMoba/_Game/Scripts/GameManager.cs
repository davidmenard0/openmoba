using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


public partial class GameManager : Node
{
	#region Singleton
	//Singleton base class does not work because of this issue: https://github.com/godotengine/godot/issues/79519
	public static GameManager Instance { 
        get { return _instance; } 
        private set { _instance = value; } 
    }
    private static GameManager _instance;

    public override void _Ready() {
        if (_instance == null) {
            _instance = this as GameManager;
            Initialize();
        }
        else {
            this.QueueFree();
        }
    }
	#endregion

	//Game Messages
	
	//Triggered by playerVision when a collider enters/Exits a vision area.
	// Observer, Observee, Tnered/Exited
	public Action<Player, Player, bool> OnNodeVisionAreaTransition;
	//Triggered when a projectile hits or expires, used by Spawner to keep track of them
	public Action<Projectile> OnProjectileDespawn;

	public Color[] TeamColors = {new Color(1,0,0), new Color(0,0,1)};

	public bool IsClient = false;

	public PlayerObjectSpawner Spawner;
	//This list is only maintained on the server side
	public Dictionary<int, PlayerInfo> Players = new Dictionary<int, PlayerInfo>();
	private VisibilityManager _visibilityManager;
	

	protected void Initialize()
	{
		Balance.Load();

		Spawner = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
		Debug.Assert(Spawner != null, "ERROR: Cannot find PlayerOBjectSpawner in GameManager.");

		_visibilityManager = GetNode<VisibilityManager>("VisibilityManager");
		Debug.Assert(_visibilityManager != null, "ERROR: Cannot find VisibilityManager in GameManager.");
		_visibilityManager.Init();

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

	public Color GetNodeColor(Node node)
	{
		return TeamColors[GetNodeTeam(node)];
	}

    //Called on all clients when the player has initialized itself. 
    /*private void Client_AddPlayerToSpawner(Player p)
	{
		if(Multiplayer.IsServer()) return;
		Spawner.Players[p.OwnerID] = p;
	}

	private void Client_AddProjectileToSpawner(Projectile p)
	{
		if(Multiplayer.IsServer()) return;
		Spawner.Projectiles[p.OwnerID] = p;
	}*/

	////// Update the IDs and names on all clients ///////
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_HandshakeNewPlayer(int ownerID)
	{
		if(!Multiplayer.IsServer()) return;

		//Only server holds this info, so send it to clients
		var pi = GameManager.Instance.GetPlayerInfo(ownerID);
		RpcId(ownerID, "RPC_Client_HandshakeResponse", ownerID, pi.Name, pi.Team);
	}
}
