using Godot;
using System;

public partial class PlayerCamera : Camera3D
{
	private Node3D _parent;
	private Player _player;
	private Camera3D _camera;
	private Vector3 _offset;

	public override void _Ready()
	{
		_parent = GetParent<Node3D>();
		_player = _parent.GetParent<Player>();
		_player.OnInit += Init;
		//De-parent the camera so it doesn't turn with the player
		//Remember: CallDeferred takes the GDScript naming conversion for function names. ("remove_child" instead of "RemoveChild")
		_parent.CallDeferred("remove_child", this);
		_player.CallDeferred("add_child", this);

		_offset = GlobalPosition - _player.GlobalPosition;
	}

    private void Init(bool isMine)
    {
		if(isMine)
        	this.Current = true;
    }

    public override void _Process(double delta)
	{
		this.GlobalPosition = _player.GlobalPosition + _offset;
	}
}
