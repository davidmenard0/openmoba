using Godot;
using System;
using System.Diagnostics;
using System.Runtime;

public partial class PlayerAnimationController : AnimationPlayer
{
	[Export]
	private float AnimThreshold = 0.25f;
	private PlayerClient _playerClient;

	private Vector3 _lastPosition;
	private string _lastAnimation = "";

	public override void _Ready()
	{
		_playerClient = GetParent().GetParentOrNull<PlayerClient>();
		Debug.Assert(_playerClient != null, "ERROR: Can't find ClientAuthority node in PlayerCharacterController");

		_lastPosition = _playerClient.GlobalPosition;
	}

	public override void _Process(double delta)
	{		
		var aim = _playerClient.Basis;
		var forward = -aim.Z;
		var left = -aim.X;

		Vector3 dir = (_playerClient.GlobalPosition - _lastPosition).Normalized();

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

		_lastPosition = _playerClient.GlobalPosition;
	}
}
