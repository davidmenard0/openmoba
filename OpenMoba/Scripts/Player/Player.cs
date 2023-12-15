using Godot;
using System;

public partial class Player : CharacterBody3D
{
	const float GRAVITY = 30f;
	const float SPEED = 15f;

	public PlayerInfo PlayerInfo;

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
		var direction = (Transform.Basis * new Vector3(input.X, 0f, input.Y)).Normalized();
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

	//Sending PeerID here just to double-check, because its an RPCID call anyways
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_InitPlayer(int id)
	{
		if(id == Multiplayer.GetUniqueId())
		{
			GD.Print("Puppet Peer ID received: " + id);

			_isMine = true;
			GetNode<Camera3D>("Camera3D").Current = true;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

}
