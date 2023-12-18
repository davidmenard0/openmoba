using Godot;
using System;
using System.Diagnostics;

public partial class Player : CharacterBody3D
{
	public Action<bool> OnInit; //isMine

	[Export]
	private float Health = 10f;
	[Export]
	
	const float Speed = 15f;
	[Export]
	public PackedScene Bullet;
	[Export]
	public Node3D BulletSpawn;
	public bool IsMine = false;


	public PlayerInfo PlayerInfo;
	public PlayerCamera Camera;

	private PlayerInput _playerInput;
	private float _gravity;

	public void Init(PlayerInfo pi){
		PlayerInfo = pi;
		_gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		GetNode<Label3D>("IDLabel").Text = pi.Name;
	}

	public override void _Ready()
	{
		_playerInput = GetNode<PlayerInput>("PlayerInput");
		Camera = GetNode<PlayerCamera>("Camera3D");

		// Dont init anythign here! The Player doesn't know yet if 
		// its ID is the PeerID. 
		// Request playerID & name from server.
		// This is necessary because server is authority on everything
		Rpc("RPC_Server_RequestInfo");

		if(Multiplayer.IsServer())
		{
			//Process only on server
			SetPhysicsProcess(true);
			SetProcess(true);
		}
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

		if(PlayerInfo.PeerID != Multiplayer.GetUniqueId())
			GD.Print(_playerInput.InputVector);
			
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
		bullet.Direction = -this.GlobalTransform.Basis.Z; //forward vector
	}

	public void TakeDamage(float dmg){
		if(!Multiplayer.IsServer()) return;

		Health -= dmg;
		if(Health <= 0f)
		{
			GD.Print("Player died!");
			//Notify client who died
		}
	}

	
	////// Update the IDs and names on all clients ///////
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Server_RequestInfo()
	{
		//Only server holds this info, so send it to clients
		Rpc("RPC_Client_RespondInfo", PlayerInfo.PeerID, PlayerInfo.Name);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_RespondInfo(int id, string name)
	{
		//Update client-side info
		PlayerInfo playerInfo = new PlayerInfo(){
			PeerID = id,
			Name = name
		};
		Init(playerInfo);

		if(id == Multiplayer.GetUniqueId())
		{
			GD.Print("Found local player: " + id);
			IsMine = true;
		}
	}

}
