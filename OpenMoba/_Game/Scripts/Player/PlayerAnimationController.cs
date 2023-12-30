using Godot;
using System;
using System.Diagnostics;
using System.Runtime;

public partial class PlayerAnimationController : AnimationPlayer
{
	[Export]
	private float AnimThreshold = 0.25f;
	private Node3D _playerClientAuthority;
	private PlayerInput _input;

	private Vector3 _lastPosition;
	private string _lastAnimation = "";

	public override void _Ready()
	{
		_input = GetParent().GetParent().GetNode<PlayerInput>("PlayerInput");
		Debug.Assert(_input != null, "ERROR: Can't find PlayerInput in PlayerCharacterController");

		_playerClientAuthority = GetParent().GetParent<Node3D>();
		Debug.Assert(_playerClientAuthority != null, "ERROR: Can't find ClientAuthority node in PlayerCharacterController");

		_lastPosition = _playerClientAuthority.GlobalPosition;
	}

	public override void _Process(double delta)
	{		
		var aim = _playerClientAuthority.Basis;
		var forward = -aim.Z;
		var left = -aim.X;

		Vector3 dir = (_playerClientAuthority.GlobalPosition - _lastPosition).Normalized();

		float forward_comp = dir.Dot(forward);
		float left_comp = dir.Dot(left);
		
		string anim = "Idle/mixamo_com";
		if(forward_comp > AnimThreshold && left_comp > AnimThreshold)
			anim = "ForwardLeft/mixamo_com";
		else if(forward_comp > AnimThreshold && left_comp < -AnimThreshold)
			anim = "ForwardRight/mixamo_com";
		else if(forward_comp < -AnimThreshold && left_comp > AnimThreshold)
			anim = "BackLeft/mixamo_com";
		else if(forward_comp < -AnimThreshold && left_comp < -AnimThreshold)
			anim = "BackRight/mixamo_com";
		else if(forward_comp > AnimThreshold)
			anim = "Forward/mixamo_com";
		else if(forward_comp < -AnimThreshold)
			anim = "Back/mixamo_com";
		else if(left_comp > AnimThreshold)
			anim = "Left/mixamo_com";
		else if(left_comp < -AnimThreshold)
			anim = "Right/mixamo_com";

		if(anim != _lastAnimation)
			this.Play(anim);
		_lastAnimation = anim;

		_lastPosition = _playerClientAuthority.GlobalPosition;
	}
}
