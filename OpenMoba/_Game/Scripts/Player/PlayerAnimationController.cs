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
		
		if(forward_comp > AnimThreshold && left_comp > AnimThreshold)
			this.Play("ForwardLeft/mixamo_com");
		else if(forward_comp > AnimThreshold && left_comp < -AnimThreshold)
			this.Play("ForwardRight/mixamo_com");
		else if(forward_comp < -AnimThreshold && left_comp > AnimThreshold)
			this.Play("BackLeft/mixamo_com");
		else if(forward_comp < -AnimThreshold && left_comp < -AnimThreshold)
			this.Play("BackRight/mixamo_com");
		else if(forward_comp > AnimThreshold)
			this.Play("Forward/mixamo_com");
		else if(forward_comp < -AnimThreshold)
			this.Play("Back/mixamo_com");
		else if(left_comp > AnimThreshold)
			this.Play("Left/mixamo_com");
		else if(left_comp < -AnimThreshold)
			this.Play("Right/mixamo_com");
		else
			this.Play("Idle/mixamo_com");

		_lastPosition = _playerClientAuthority.GlobalPosition;
	}
}
