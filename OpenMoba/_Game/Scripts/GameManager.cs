using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


public partial class GameManager : Node
{

	public List<PlayerInfo> Players = new List<PlayerInfo>();

	//this keeps track of how many players from the opposing team see you
	private Dictionary<int, int> _teamVisibilityCounter = new Dictionary<int, int>();

	private PlayerObjectSpawner _spawner;

	public override void _Ready()
	{
		_spawner = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
		_spawner.Server_OnPlayerSpawn += OnPlayerSpawn;
	}

	public void OnPlayerVisibilityChange(Player observer, Player observee, bool visible)
	{
		if(!Multiplayer.IsServer()) return;

		int observer_team = observer.PlayerInfo.Team;
		int observee_id = observee.PlayerInfo.PeerID;

		if(!_teamVisibilityCounter.ContainsKey(observee_id))
			_teamVisibilityCounter[observee_id] = 0;

		_teamVisibilityCounter[observee_id] += visible ? 1 : -1;
		if(_teamVisibilityCounter[observee_id] < 0)
			_teamVisibilityCounter[observee_id] = 0; // cant have negative values

		bool spotted = _teamVisibilityCounter[observee_id] > 0;
		SetPlayerVisibilityForTeam(observee, observer_team, spotted);
	}

	private void OnPlayerSpawn(Player p)
	{
		var sync = p.GetNode<MultiplayerSynchronizer>("ServerSynchronizer");
		sync.PublicVisibility = false;
		SetPlayerVisibilityForTeam(p, p.PlayerInfo.Team, true);
	}

	private void SetPlayerVisibilityForTeam(Player observee, int team, bool visibility)
	{
		//TODO: Take into consideration when the server is also client
		//TODO: Add visibility for Bullets

		int observee_id = observee.PlayerInfo.PeerID;
		var observee_sync = observee.GetNode<MultiplayerSynchronizer>("ServerSynchronizer");
		
		foreach(var p in _spawner.ServerPlayers)
		{
			// Only update if other_player is on the team that sees the peerID
			// Remember: observer_team is NOT the observee's team. It's the opposing team
			if(p.PlayerInfo.Team == team)
			{
				// Team spots the observee
				if(visibility)
				{
					observee_sync.SetVisibilityFor(p.PlayerInfo.PeerID, true);
					RpcId(p.PlayerInfo.PeerID, "RPC_Client_UpdateVisibility", observee_id, true);
				}
				else // Team no longer see observee
				{
					observee_sync.SetVisibilityFor(p.PlayerInfo.PeerID, false);
					RpcId(p.PlayerInfo.PeerID, "RPC_Client_UpdateVisibility", observee_id, false);
				}
			}
		}
	}

	//Note: This is only needed when the server is also a client. 
	// This way the server keeps the updates, but the visibility is set to false
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPC_Client_UpdateVisibility(int id, bool visible)
	{
		var spawner = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
		foreach(var p in _spawner.ClientPlayers)
		{
			if(p.PlayerInfo.PeerID == id)
			{
				GD.Print("Player " + Multiplayer.GetUniqueId() + " un/spotted player " + id + ". Setting visibility to " + visible);
				p.Visible = visible;
			}
		}
	}

	//Called on all clients when the player has initialized itself. 
	public void Client_OnPlayerInit(Player p)
	{
		_spawner.ClientPlayers.Add(p);
	}
}
