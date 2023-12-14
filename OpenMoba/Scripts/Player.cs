using Godot;
using System;

public partial class Player : CharacterBody3D
{
	const float GRAVITY = 30f;
	const float SPEED = 15f;
	const float JUMP_VELOCITY = 10f;

	public PlayerInfo _playerInfo;

	public int Team
	{
		get{return _team;}
		set{_team = value;}
	}
	private int _team;

	public Vector2 InputVector
	{
		get{return _inputDirection;}
		set{_inputDirection = value;}
	}
	private Vector2 _inputDirection;

	public override void _Ready()
	{
		bool isLocal = _playerInfo.PeerID == Multiplayer.GetUniqueId();
		GetNode<Camera3D>("Camera3D").Current = isLocal;
		
		//local authority only true if running a local server
		var authority = IsMultiplayerAuthority();
		var isServer = Multiplayer.IsServer();

		SetProcessInput(true);
		SetPhysicsProcess(IsMultiplayerAuthority());
		SetProcess(IsMultiplayerAuthority());

		if(isLocal)
		{
			GD.Print("MP Unique ID: " + Multiplayer.GetUniqueId());
			GD.Print("Assigned Peer ID: " + _playerInfo.PeerID);
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
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

		if(Input.IsActionJustPressed("move_jump") && IsOnFloor() )
		{
			v.Y = JUMP_VELOCITY;
		}

		var direction = (Transform.Basis * new Vector3(InputVector.X, 0f, InputVector.Y)).Normalized();
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

    public override void _Input(InputEvent @event)
    {
		if(Input.IsActionJustPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Visible ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
		}

        if(Input.MouseMode == Input.MouseModeEnum.Visible)
			return;
		
		if(@event is InputEventMouseMotion)
		{
			InputEventMouseMotion e = (InputEventMouseMotion)@event;
			var r = Rotation;
			r.Y += e.Relative.X * -0.001f;
			Rotation = r;
		}

		InputVector = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
    }

	public void SetUpPlayer(string name){
		GetNode<Label3D>("IDLabel").Text = name;
	}
}
