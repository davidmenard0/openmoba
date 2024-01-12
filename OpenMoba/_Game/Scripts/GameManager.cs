using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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


	//Triggered by playerVision when a collider enters/Exits a vision area.
	// Observer, Observee, Tnered/Exited
	public Action<Player, Player, bool> OnNodeVisionAreaTransition;
	
	public int[] TeamIncome = {0,0};

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
		if(!Multiplayer.IsServer()) return -1;

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

	public void GiveResourceToTeam(int team, int resource)
	{
		if(!Multiplayer.IsServer()) return;

		if(resource < 0) return; // cant give negative resources. Use another function

		var players_in_team = Players.Select(p=>p.Value).Where(x => x.Team == team);
		foreach(var p in players_in_team)
		{
			p.Resources += resource;
			RpcId(p.PeerID, "Client_OnResourceChange", p.Resources);
		}
		Logger.Log(String.Format("Giving {0} resources to team {1}", resource, team));
	}


	////////////////////// Client functions /////////////////////

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Client_OnResourceChange(int new_resource)
	{
		UIController.Instance.OnResourceChange?.Invoke(new_resource);
	}
}
