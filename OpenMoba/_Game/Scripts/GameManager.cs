using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


public partial class GameManager : Node
{

	public List<PlayerInfo> Players = new List<PlayerInfo>();

	private Dictionary<int, int> _teamVisibilityCounter = new Dictionary<int, int>();

	private PlayerObjectSpawner _spawner;

	public override void _Ready()
	{
		_spawner = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
	}

	public void TeamSeesPlayer(int team, int peerID, bool visible)
	{
		if(!Multiplayer.IsServer()) return;

		if(!_teamVisibilityCounter.ContainsKey(peerID))
			_teamVisibilityCounter[peerID] = 0;

		_teamVisibilityCounter[peerID] += visible ? 1 : -1;
		if(_teamVisibilityCounter[peerID] < 0)
			_teamVisibilityCounter[peerID] = 0; // cant have negative values

		foreach(var p in _spawner.ServerPlayers)
		{
			// Only update if other_player is on the team that sees the peerID
			// Remember: team is NOT the peerID's team. It's the opposing team
			if(p.PlayerInfo.Team == team)
			{
				if(_teamVisibilityCounter[peerID] > 0)
				{
					RpcId(p.PlayerInfo.PeerID, "RPC_Client_UpdateVisibility", peerID, true);
				}
				else
				{
					RpcId(p.PlayerInfo.PeerID, "RPC_Client_UpdateVisibility", peerID, false);
				}
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPC_Client_UpdateVisibility(int id, bool visible)
	{
		var spawner = GetNode<PlayerObjectSpawner>("/root/Main/PlayerObjectSpawner");
		foreach(var p in _spawner.ClientPlayers)
		{
			if(p.PlayerInfo.PeerID == id)
			{
				GD.Print("Player " + Multiplayer.GetUniqueId() + " setting player " + id + " to " + visible);
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
