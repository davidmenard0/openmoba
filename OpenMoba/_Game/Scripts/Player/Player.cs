using Godot;
using System;
using System.Diagnostics;

public partial class Player : CharacterBody3D
{
	public Action<bool> Client_OnInit; //isMine
	public Action<Player> OnDeath; //PlayerInfo

	[Export]
	const float Speed = 15f;
	[Export]
	public PackedScene ProjectileTemplate;
	[Export]
	public Node3D ProjectileSpawn;

	public bool IsMine = false;
	public bool IsInit = false;

	public int OwnerID; //Dont hold the OwnerInfo, just the ID so we can do a lookup in GameManager
	public PlayerCamera Camera;

	private float _health = 2f;
	private float _maxHealth;

	private PlayerInput _playerInput;
	private Node3D _clientAuthority;
	private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");


	public override void _Ready()
	{
		_playerInput = GetNode<PlayerInput>("ClientAuthority/PlayerInput");
		Camera = GetNode<PlayerCamera>("ClientAuthority/Camera3D");

		if(Multiplayer.IsServer())
		{
			//Process only on server
			SetPhysicsProcess(true);
			SetProcess(true);
			_maxHealth = _health;
		}

		// Dont init anything here! The Player doesn't know yet if 
		// its ID is the PeerID. 
		// Request playerID & name from server.
		// This is necessary because server is authority on everything
		RpcId(1, "RPC_Server_RequestInfo");
	}

    public override void _PhysicsProcess(double delta)
    {
		//Physics only on server
		if(!Multiplayer.IsServer()) return;

		var v = Velocity;
        if(!IsOnFloor())
		{
			v.Y -= (float) (_gravity * delta);
		}
			
		var input = _playerInput.InputVector;
		var direction = new Vector3(input.X, 0f, input.Y).Normalized();
		if(direction.LengthSquared() > Mathf.Epsilon)
		{
			v.X = direction.X * Speed;
			v.Z = direction.Z * Speed;
		}
		else
		{
			v.X = Mathf.MoveToward(v.X, 0f, Speed);
			v.Z = Mathf.MoveToward(v.Z, 0f, Speed);
		}

		Velocity = v;
		MoveAndSlide();
    }

	public void Server_Init(int ownerID)
	{
		OwnerID = ownerID;
	}

	public void Client_Init(PlayerInfo pi)
	{
		OwnerID = pi.PeerID;
		GetNode<Label3D>("IDLabel").Text = GameManager.Instance.GetPlayerInfo(OwnerID).Name;

		// A few things are client-authority:
		// Rotation of the player, camera, Input
		_playerInput = GetNode<PlayerInput>("ClientAuthority/PlayerInput");
		_playerInput.SetMultiplayerAuthority(OwnerID);
		_clientAuthority = GetNode<Node3D>("ClientAuthority");
		_clientAuthority.SetMultiplayerAuthority(OwnerID, true);

		IsInit = true;
		Client_OnInit?.Invoke(IsMine);
		GameManager.Instance.Client_OnPlayerInit?.Invoke(this);
	}


	public void Fire(int id)
	{
		if(!Multiplayer.IsServer()) return;

		//Double-check that the id is the one associated to this player. 
		//This should always be true, since the RPC is only called on the correct player
		// It prevents anyone from sending this RPC with malicious intent
		if(OwnerID != id)
		{
			Debug.Assert(OwnerID == id, "ERROR: Fire called on another Player than the one that fired");
			return;
		}

		var dir = -_clientAuthority.GlobalTransform.Basis.Z; //forward vector
		GameManager.Instance.Spawner.SpawnProjectile(this, ProjectileTemplate, ProjectileSpawn.GlobalPosition, dir);
	}

	public void TakeDamage(float dmg){
		if(!Multiplayer.IsServer()) return;

		_health -= dmg;
		if(_health <= 0f)
		{
			OnDeath?.Invoke(this);
			//Dont queuefree here, the PlayerSpawner will do that
			Logger.Log("Player died: " + OwnerID);
		}
	}
	
	////// Update the IDs and names on all clients ///////
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Server_RequestInfo()
	{
		if(!Multiplayer.IsServer()) return;

		//Only server holds this info, so send it to clients
		var pi = GameManager.Instance.GetPlayerInfo(OwnerID);
		Rpc("RPC_Client_RespondInfo", OwnerID, pi.Name, pi.Team);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_RespondInfo(int id, string name, int team)
	{
		//Dont server-check here because server might have spawned a player
				
		//Update client-side info
		PlayerInfo playerInfo = new PlayerInfo(){
			PeerID = id,
			Name = name,
			Team = team
		};

		if(id == Multiplayer.GetUniqueId())
		{
			GD.Print("Found local player: " + id);
			IsMine = true;
		}

		//Call after this --^ to make sure isMine is set
		Client_Init(playerInfo);
	}

}
