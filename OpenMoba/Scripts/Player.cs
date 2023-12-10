using Godot;
using System;

//TODO: Separate client and server code. 
// We have a fully server-authoritative game, so client should be input-only

public partial class Player : CharacterBody3D
{
	const float GRAVITY = 30f;
	const float SPEED = 15f;
	const float JUMP_VELOCITY = 10f;

	public int PeerID
	{
		get {return _peerID;}
		set 
		{ 
			_peerID = value;
			Name = _peerID.ToString();
			GetNode<Label3D>("Label3D").Text = _peerID.ToString();
			SetMultiplayerAuthority(_peerID);
		}
	}
	private int _peerID;

	public override void _Ready()
	{
		GetNode<Camera3D>("Camera3D").Current = PeerID == Multiplayer.GetUniqueId();
		var isLocal = IsMultiplayerAuthority();
		SetProcessInput(isLocal);
		SetPhysicsProcess(isLocal);
		SetProcess(isLocal);

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Visible ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
		}
	}

    public override void _PhysicsProcess(double delta)
    {
		var v = Velocity;
        if(!IsOnFloor())
		{
			v.Y -= (float) (GRAVITY * delta);
		}

		if(Input.IsActionJustPressed("move_jump") && IsOnFloor() )
		{
			v.Y = JUMP_VELOCITY;
		}

		var input_dir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		var direction = (Transform.Basis * new Vector3(input_dir.X, 0f, input_dir.Y)).Normalized();
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
        if(Input.MouseMode == Input.MouseModeEnum.Visible)
			return;
		
		if(@event is InputEventMouseMotion)
		{
			InputEventMouseMotion e = (InputEventMouseMotion)@event;
			var r = Rotation;
			r.Y += e.Relative.X * -0.001f;
			Rotation = r;
		}
    }
}
