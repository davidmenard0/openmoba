using Godot;
using System;

public partial class PlayerCamera : Camera3D
{
	private Player _player;
	private Camera3D _camera;
	private Vector3 _offset;

	public override void _Ready()
	{
		_player = GetParent<Player>();
		//De-parent the camera so it doesn't turn with the player
		//Remember: CallDeferred takes the GDScript naming conversion for function names. ("remove_child" instead of "RemoveChild")
		_player.CallDeferred("remove_child", this);
		_player.GetParent().CallDeferred("add_child", this);
		this.Current = true;

		_offset = GlobalPosition - _player.GlobalPosition;
	}

	public override void _Process(double delta)
	{
		this.GlobalPosition = _player.GlobalPosition + _offset;
	}
}
