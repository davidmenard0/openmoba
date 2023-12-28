using Godot;
using System;
using System.Diagnostics;
using System.Runtime;

public partial class PlayerAnimationController : AnimationPlayer
{
	[Export]
	private float AnimThreshold = 0.25f;
	private Player _player;
	private Node3D _playerClientAuthority;
	private PlayerInput _input;
	//AnimationNodeStateMachinePlayback _animStatemachine;

	public override void _Ready()
	{
		_input = GetParent().GetParent().GetNode<PlayerInput>("PlayerInput");
		Debug.Assert(_input != null, "ERROR: Can't find PlayerInput in PlayerCharacterController");

		_player = GetParent().GetParent().GetParent<Player>();
		Debug.Assert(_player != null, "ERROR: Can't find Node3D playerroot in PlayerCharacterController");

		_playerClientAuthority = GetParent().GetParent<Node3D>();
		Debug.Assert(_playerClientAuthority != null, "ERROR: Can't find ClientAuthority node in PlayerCharacterController");

	}

	public override void _Process(double delta)
	{
		//Only animate on the client
		if(!_player.IsMine) return;

		//_animStatemachine = (AnimationNodeStateMachinePlayback)this.Get("parameters/StateMachine/playback");

		var aim = _playerClientAuthority.Basis;
		var forward = -aim.Z;
		var left = -aim.X;

		Vector3 input3D = new Vector3(_input.InputVector.X, 0f, _input.InputVector.Y);

		float forward_comp = input3D.Dot(forward);
		float left_comp = input3D.Dot(left);

		

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

		/*if(forward_comp > AnimThreshold && left_comp > AnimThreshold)
			_animStatemachine.Travel("Forward Left");
		else if(forward_comp > AnimThreshold && left_comp < -AnimThreshold)
			_animStatemachine.Travel("Forward Right");
		else if(forward_comp < -AnimThreshold && left_comp > AnimThreshold)
			_animStatemachine.Travel("Back Left");
		else if(forward_comp < -AnimThreshold && left_comp < -AnimThreshold)
			_animStatemachine.Travel("Back Right");
		else if(forward_comp > AnimThreshold)
			_animStatemachine.Travel("Forward");
		else if(forward_comp < -AnimThreshold)
			_animStatemachine.Travel("Back");
		else if(left_comp > AnimThreshold)
			_animStatemachine.Travel("Left");
		else if(left_comp < -AnimThreshold)
			_animStatemachine.Travel("Right");
		else
			_animStatemachine.Travel("Idle");*/
	}
}
