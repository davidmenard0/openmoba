using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

public partial class PlayerVision : Area3D
{
	private Player _player;

	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;

		_player = GetParent<Player>();
		Debug.Assert(_player != null, "ERROR: can't find Player in player vision Area");

		_player.Client_OnInit += PlayerInit;

		Connect("body_entered", new Callable(this, "_OnBodyEntered"));
		Connect("body_exited", new Callable(this, "_OnBodyExited"));
	}

	public override void _ExitTree()
	{
		if(_player != null)
			_player.Client_OnInit -= PlayerInit;
	}

    public override void _Process(double delta)
	{
	}

    private void PlayerInit(bool isMine)
    {
        if(!Multiplayer.IsServer()) return;
    }

	private void _OnBodyEntered(Node body)
    {
		ProcessCollision(body, true);
    }

	private void _OnBodyExited(Node body)
    {
		ProcessCollision(body, false);
    }

	private void ProcessCollision(Node body, bool entered)
	{
		if(!Multiplayer.IsServer()) return;

		if(body is Player)
		{
			var other_player = (Player) body;
			var this_team = GameManager.Instance.GetPlayerInfo(_player.OwnerID);
			var other_team = GameManager.Instance.GetPlayerInfo(other_player.OwnerID);
			if(this_team != other_team)
			{
				Logger.Log("Player " + _player.OwnerID + " sees " + other_player.OwnerID);
				GameManager.Instance.OnNodeVisionAreaTransition?.Invoke(_player, other_player, entered);
			}
		}
	}
}
