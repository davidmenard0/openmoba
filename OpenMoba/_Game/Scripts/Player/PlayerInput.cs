using Godot;
using System;

public partial class PlayerInput : MultiplayerSynchronizer
{
	[Export]
	public Vector2 InputVector; //Sync via replication

	private Player _player;

    public override void _Ready()
    {
		_player = GetParent<Player>();
		//Only process input if puppet is the authority
        SetProcessInput(GetMultiplayerAuthority() == Multiplayer.GetUniqueId());
    }

    public override void _Input(InputEvent @event)
    {
		//Inputs only called on clients		
		InputVector = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");

		if(@event.IsActionPressed("fire"))
		{
			Rpc("RPC_Fire", Multiplayer.GetUniqueId());
		}

		//Control player rotation based on mouse position
		var mouse_pos = GetViewport().GetMousePosition();
		var from = _player.Camera.ProjectRayOrigin(mouse_pos);
		var to = from + _player.Camera.ProjectRayNormal(mouse_pos) * 100f;
		var cursorPos = new Plane(Vector3.Up, _player.Transform.Origin.Y).IntersectsRay(from, to);
		if(cursorPos.HasValue)
		{
			//Y is always at the height of the player. He's always looking straight flat
			var pos = cursorPos.Value;
			pos.Y = _player.GlobalPosition.Y;
			_player.LookAt(pos);
		}

    }

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Fire(int id)
	{
		if(!Multiplayer.IsServer()) return;
		_player.Fire(id);
	}

}
