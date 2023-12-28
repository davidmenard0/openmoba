using Godot;
using System;
using System.Diagnostics;

public partial class PlayerCamera : Camera3D
{
	private Node3D _parent;
	private Player _player;
	private Camera3D _camera;
	private Vector3 _offset;
	private AudioListener3D _listener;

	public override void _Ready()
	{
		_parent = GetParent<Node3D>();
		_player = _parent.GetParent<Player>();
		_player.OnInit += Init;

		_listener = GetNode<AudioListener3D>("AudioListener3D");
		Debug.Assert(_listener != null, "ERROR: Cant find AudioListener3D under camera" );

		//De-parent the camera so it doesn't turn with the player
		//Remember: CallDeferred takes the GDScript naming conversion for function names. ("remove_child" instead of "RemoveChild")
		_parent.CallDeferred("remove_child", this);
		_player.CallDeferred("add_child", this);

		_offset = GlobalPosition - _player.GlobalPosition;
	}

    private void Init(bool isMine)
    {
		if(isMine)
		{
        	this.Current = true;
			_listener.MakeCurrent();
		}
    }

    public override void _Process(double delta)
	{
		this.GlobalPosition = _player.GlobalPosition + _offset;
	}
}
