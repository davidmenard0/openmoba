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
		if(!Multiplayer.IsServer()) return;

		if(body is Player)
		{
			var other_player = (Player) body;
			if(other_player.PlayerInfo.Team != _player.PlayerInfo.Team)
			{
				GameManager.Instance.OnPlayerVisibilityChange?.Invoke(_player, other_player, true);
				Logger.Log("Player " + _player.PlayerInfo.PeerID + " sees " + other_player.PlayerInfo.PeerID);
			}
		}
    }

	private void _OnBodyExited(Node body)
    {
		if(!Multiplayer.IsServer()) return;

		if(body is Player)
		{
			var other_player = (Player) body;
			if(other_player.PlayerInfo.Team != _player.PlayerInfo.Team)
			{
				GameManager.Instance.OnPlayerVisibilityChange?.Invoke(_player, other_player, false);
				
				Logger.Log("I dont see you :( ");
			}
		}
    }
}
