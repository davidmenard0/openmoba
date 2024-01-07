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
		_spawner.Server_OnPlayerSpawn += Server_OnPlayerNodeSpawn;
		_spawner.Server_OnProjectileSpawn += Server_OnPlayerNodeSpawn;

		GameManager.Instance.OnNodeVisionAreaTransition += OnNodeVisibilityChange;
	}

    public override void _ExitTree()
    {
        GameManager.Instance.OnNodeVisionAreaTransition -= OnNodeVisibilityChange;
		_spawner.Server_OnPlayerSpawn -= Server_OnPlayerNodeSpawn;
		_spawner.Server_OnProjectileSpawn += Server_OnPlayerNodeSpawn;
    }

	private void Server_OnPlayerNodeSpawn(Node3D n)
	{
		if(!Multiplayer.IsServer()) return;

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
		Dictionary<int,int> counter = null;

		if(node is Player)
		{
			observee_id = ((Player)node).OwnerID;
			counter = _teamVisibilityCounter;
		}
		else if(node is Projectile)
		{
			observee_id = ((Projectile)node).UID;
			counter = _projectileVisibilityCounter;
		}

		if(!counter.ContainsKey(observee_id))
				counter[observee_id] = 0;

		counter[observee_id] += visible ? 1 : -1;
		if(counter[observee_id] < 0)
			counter[observee_id] = 0; // cant have negative values
		
		bool spotted = counter[observee_id] > 0;
		SetNodeVisibilityForTeam(node, observer_team, spotted);
	}

	private void SetNodeVisibilityForTeam(Node3D observee, int team, bool visibility)
	{
		if(!Multiplayer.IsServer()) return;

		var observee_sync = observee.GetNode<MultiplayerSynchronizer>("ServerSynchronizer");
		
		foreach(var p in _spawner.Players)
		{
			// Only update if other_player is on the team that sees the peerID
			// Remember: observer_team is NOT the observee's team. It's the opposing team
			int p_team = GameManager.Instance.GetPlayerInfo(p.Value.OwnerID).Team;
			if(p_team == team)
			{
				//TODO: Visibility only sets... well... the visibility of the node.
				//This means the position of each player is still being sent to each client. 
				// This can lead to client-side map hacks

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
	/*[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
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
	}*/
}
