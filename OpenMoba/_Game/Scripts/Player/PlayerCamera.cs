using Godot;
using System;
using System.Diagnostics;

public partial class PlayerCamera : Camera3D
{
	private PlayerClient _playerClient;
	private PlayerNode _player;
	private Camera3D _camera;
	private Vector3 _offset;
	private AudioListener3D _listener;

	public override void _Ready()
	{
		_playerClient = GetParentOrNull<PlayerClient>();
		Debug.Assert(_playerClient != null, "ERROR: Cannot find PlayerClient in PlayerCamera.");

		_player = _playerClient.GetParentOrNull<PlayerNode>();
		Debug.Assert(_player != null, "ERROR: Cannot find Player in PlayerCamera.");

		_listener = GetNode<AudioListener3D>("AudioListener3D");
		Debug.Assert(_listener != null, "ERROR: Cant find AudioListener3D under camera" );

		//De-parent the camera so it doesn't turn with the player
		//Remember: CallDeferred takes the GDScript (not C#) naming conversion for function names. ("remove_child" instead of "RemoveChild")
		_playerClient.CallDeferred("remove_child", this);
		_player.CallDeferred("add_child", this);

		_offset = GlobalPosition - _player.GlobalPosition;
	}
    public void InitOwnership()
    {
		this.Visible = true;
		this.Current = true;
		_listener.MakeCurrent();
    }

    public override void _Process(double delta)
	{
		this.GlobalPosition = _player.GlobalPosition + _offset;
	}
}
