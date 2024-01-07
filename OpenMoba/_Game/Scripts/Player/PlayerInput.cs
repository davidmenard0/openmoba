using Godot;
using System;

public partial class PlayerInput : MultiplayerSynchronizer
{
	[Export]
	public Vector2 InputVector; //Sync via replication

	private Player _player;
	private PlayerClient _playerClient;

    public override void _Ready()
    {
		_playerClient = GetParent<PlayerClient>();
		_player = _playerClient.GetParent<Player>();
		_playerClient.Client_OnInit += Init;
		
		// Dont do this here! (And leave it commented as a warning
		// at this point, the client might not have received its playerID yet
		// So IsMine might not be initialized
        // SetProcessInput(_player.IsMine);
    }

    private void Init(bool isMine)
    {
        SetProcess(isMine);
		SetProcessInput(isMine);
    }


    public override void _ExitTree()
    {
		_playerClient.Client_OnInit -= Init;
    }

    public override void _Process(double delta)
    {
		if(!_playerClient.IsMine) return;

        //Inputs only called on clients		
		InputVector = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");

		if(Input.IsActionJustPressed("fire"))
		{
			RpcId(1,"Server_Fire", Multiplayer.GetUniqueId());
		}

		//Control player rotation based on mouse position
		var mouse_pos = GetViewport().GetMousePosition();
		var from = _playerClient.Camera.ProjectRayOrigin(mouse_pos);
		var dir = _playerClient.Camera.ProjectRayNormal(mouse_pos);
		var bullet_spawn_height = _player.ProjectileSpawn.GlobalPosition.Y; //intersect at the spawn height to be precise
		var cursor_raycast = new Plane(Vector3.Up, bullet_spawn_height).IntersectsRay(from, dir);
		if(cursor_raycast.HasValue)
		{
			//Y is always at the height of the player. He's always looking straight flat
			var pos = cursor_raycast.Value;
			pos.Y = _player.GlobalPosition.Y;
			_playerClient.LookAt(pos);
		}
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void Server_Fire(int id)
	{
		if(!Multiplayer.IsServer()) return;
		_player.Fire(id);
	}

}
