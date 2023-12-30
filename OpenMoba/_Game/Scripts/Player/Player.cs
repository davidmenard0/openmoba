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
	public PackedScene Bullet;
	[Export]
	public Node3D BulletSpawn;

	public bool IsMine = false;
	public bool IsInit = false;

	public PlayerInfo PlayerInfo;
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

	public void Server_Init(PlayerInfo pi)
	{
		PlayerInfo = pi;
	}

	public void Client_Init(PlayerInfo pi)
	{
		PlayerInfo = pi;
		GetNode<Label3D>("IDLabel").Text = pi.Name;

		// A few things are client-authority:
		// Rotation of the player, camera, Input
		_playerInput = GetNode<PlayerInput>("ClientAuthority/PlayerInput");
		_playerInput.SetMultiplayerAuthority(pi.PeerID);
		_clientAuthority = GetNode<Node3D>("ClientAuthority");
		_clientAuthority.SetMultiplayerAuthority(pi.PeerID, true);

		IsInit = true;
		Client_OnInit?.Invoke(IsMine);
		GameManager.Instance.Client_OnPlayerInit(this);		
	}


	public void Fire(int id)
	{
		if(!Multiplayer.IsServer()) return;

		//Double-check that the id is the one associated to this player. 
		//This should always be true, since the RPC is only called on the correct player
		// It prevents anyone from sending this RPC with malicious intent
		if(PlayerInfo.PeerID != id)
		{
			Debug.Assert(PlayerInfo.PeerID == id, "ERROR: Fire called on another Player than the one that fired");
			return;
		}

		var bullet = Bullet.Instantiate<Bullet>();
		this.GetParent().AddChild(bullet, true);
		bullet.GlobalPosition = BulletSpawn.GlobalPosition;
		bullet.Direction = -_clientAuthority.GlobalTransform.Basis.Z; //forward vector
	}

	public void TakeDamage(float dmg){
		if(!Multiplayer.IsServer()) return;

		_health -= dmg;
		if(_health <= 0f)
		{
			OnDeath?.Invoke(this);
			//Dont queuefree here, the PlayerSpawner will do that
			Logger.Log("Player died: " + PlayerInfo.PeerID);
		}
	}
	
	////// Update the IDs and names on all clients ///////
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Server_RequestInfo()
	{
		if(!Multiplayer.IsServer()) return;

		//Only server holds this info, so send it to clients
		Rpc("RPC_Client_RespondInfo", PlayerInfo.PeerID, PlayerInfo.Name, PlayerInfo.Team);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_RespondInfo(int id, string name, int team)
	{
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
