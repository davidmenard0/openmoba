using Godot;
using System;
using System.Diagnostics;

public partial class Player : CharacterBody3D
{
	[Export]
	public PackedScene Bullet;
	[Export]
	public Node3D BulletSpawn;

	const float GRAVITY = 30f;
	const float SPEED = 15f;

	public PlayerInfo PlayerInfo;
	public PlayerCamera Camera;

	private PlayerInput _playerInput;
	private bool _isMine = false;

	public void Init(PlayerInfo pi){
		PlayerInfo = pi;
		GetNode<Label3D>("IDLabel").Text = pi.Name;
	}

	public override void _Ready()
	{
		_playerInput = GetNode<PlayerInput>("PlayerInput");

		if(!Multiplayer.IsServer()) return;

		RpcId(PlayerInfo.PeerID, "RPC_InitPlayer", PlayerInfo.PeerID);

		SetPhysicsProcess(IsMultiplayerAuthority());
		SetProcess(IsMultiplayerAuthority());
	}

    public override void _PhysicsProcess(double delta)
    {
		//Physics only on server
		if(!Multiplayer.IsServer()) return;

		var v = Velocity;
        if(!IsOnFloor())
		{
			v.Y -= (float) (GRAVITY * delta);
		}

		var input = _playerInput.InputVector;
		var direction = new Vector3(input.X, 0f, input.Y).Normalized();
		if(direction.LengthSquared() > Mathf.Epsilon)
		{
			v.X = direction.X * SPEED;
			v.Z = direction.Z * SPEED;
		}
		else
		{
			v.X = Mathf.MoveToward(v.X, 0f, SPEED);
			v.Z = Mathf.MoveToward(v.Z, 0f, SPEED);
		}

		Velocity = v;
		MoveAndSlide();
    }

	public void Fire(int id)
	{
		//Double-check that the id is the one associated to this player. 
		//This should always be true, since the RPC is only called on the correct player
		if(PlayerInfo.PeerID != id)
		{
			Debug.Assert(PlayerInfo.PeerID == id, "ERROR: Fire called on another Player than the one that fired");
			return;
		}

		var bullet = Bullet.Instantiate<Bullet>();
		this.GetParent().AddChild(bullet);
		bullet.GlobalPosition = BulletSpawn.GlobalPosition;
		bullet.Direction = -this.GlobalTransform.Basis.Z; //forward vector
	}

	//Sending PeerID here just to double-check, because its an RPCID call anyways
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_InitPlayer(int id)
	{
		if(id == Multiplayer.GetUniqueId())
		{
			GD.Print("Puppet Peer ID received: " + id);

			_isMine = true;
			Camera = GetNode<PlayerCamera>("Camera3D");
		}
	}

}
