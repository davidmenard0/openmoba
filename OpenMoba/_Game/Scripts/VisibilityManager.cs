using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class VisibilityManager : Node
{
	//this keeps track of how many players from the opposing team see you
	private Dictionary<int, int> _teamVisibilityCounter = new Dictionary<int, int>();
	private Dictionary<int, int> _projectileVisibilityCounter = new Dictionary<int, int>();

	private PlayerObjectSpawner _spawner;

	
	public void Init()
	{
		_spawner = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
		Debug.Assert(_spawner != null, "ERROR: Cannot find PlayerOBjectSpawner in GameManager.");
		_spawner.Server_OnPlayerSpawn += OnNodeSpawn;
		_spawner.Server_OnProjectileSpawn += OnNodeSpawn;

		GameManager.Instance.OnNodeVisionAreaTransition += OnNodeVisibilityChange;
	}

    public override void _ExitTree()
    {
        GameManager.Instance.OnNodeVisionAreaTransition -= OnNodeVisibilityChange;
		_spawner.Server_OnPlayerSpawn -= OnNodeSpawn;
		_spawner.Server_OnProjectileSpawn += OnNodeSpawn;
    }

	private void OnNodeSpawn(Node3D n)
	{
		var sync = n.GetNode<MultiplayerSynchronizer>("ServerSynchronizer");
		sync.PublicVisibility = false;
		int team = GameManager.Instance.GetNodeTeam(n);
		SetNodeVisibilityForTeam(n, team, true);
	}


	private void OnNodeVisibilityChange(Player observer, Node3D node, bool visible)
	{
		if(!Multiplayer.IsServer()) return;

		int observer_team = GameManager.Instance.GetPlayerInfo(observer.OwnerID).Team;
		int observee_id = -1;

		if(node is Player)
			observee_id = ((Player)node).OwnerID;
		else if(node is Projectile)
			observee_id = ((Projectile)node).UID;
		

		if(!_teamVisibilityCounter.ContainsKey(observee_id))
			_teamVisibilityCounter[observee_id] = 0;

		_teamVisibilityCounter[observee_id] += visible ? 1 : -1;
		if(_teamVisibilityCounter[observee_id] < 0)
			_teamVisibilityCounter[observee_id] = 0; // cant have negative values

		bool spotted = _teamVisibilityCounter[observee_id] > 0;
		SetNodeVisibilityForTeam(node, observer_team, spotted);
	}

	private void SetNodeVisibilityForTeam(Node3D observee, int team, bool visibility)
	{
		//TODO: Take into consideration when the server is also client
		//TODO: Add visibility for Bullets

		int observee_id = -1;

		if(observee is Player)
			observee_id = ((Player)observee).OwnerID;
		else if(observee is Projectile)
			observee_id = ((Projectile)observee).UID;
		
		var observee_sync = observee.GetNode<MultiplayerSynchronizer>("ServerSynchronizer");
		
		foreach(var p in _spawner.Players)
		{
			// Only update if other_player is on the team that sees the peerID
			// Remember: observer_team is NOT the observee's team. It's the opposing team
			int p_team = GameManager.Instance.GetPlayerInfo(p.Value.OwnerID).Team;
			if(p_team == team)
			{
				// Team spots the node
				observee_sync.SetVisibilityFor(p.Value.OwnerID, visibility);
				/*if(observee is Player)
					RpcId(p.Value.OwnerID, "RPC_Client_UpdatePlayerVisibility", observee_id, visibility);
				else if(observee is Projectile)
					RpcId(p.Value.OwnerID, "RPC_Client_UpdateProjectileVisibility", observee_id, visibility);
				*/
			}
		}
	}


	//Note: This is only needed when the server is also a client. 
	// This way the server keeps the updates, but the visibility is set to false
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPC_Client_UpdatePlayerVisibility(int id, bool visible)
	{
		if(_spawner.Players.ContainsKey(id))
		{
			Logger.Log("Player " + Multiplayer.GetUniqueId() + " un/spotted player " + id + ". Setting visibility to " + visible);
			_spawner.Players[id].Visible = visible;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPC_Client_UpdateProjectileVisibility(int id, bool visible)
	{
		if(_spawner.Players.ContainsKey(id))
		{
			_spawner.Players[id].Visible = visible;
		}
	}
}
