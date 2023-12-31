using Godot;
using System;

public partial class CaptureArea : Area3D
{
	public int PushCounter = 0;

	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;
		Connect("body_entered", new Callable(this, "_OnBodyEntered"));
		Connect("body_exited", new Callable(this, "_OnBodyExited"));
	}

	private void _OnBodyEntered(Node body)
    {
		if(!Multiplayer.IsServer()) return;
		
        if (body is Player)
        {
            Player p = (Player)body;
			if(GameManager.Instance.GetNodeTeam(body) == 0)
				PushCounter++;
			else
				PushCounter--;
        }
    }

	private void _OnBodyExited(Node body)
    {
		if(!Multiplayer.IsServer()) return;

        if (body is Player)
        {
            Player p = (Player)body;
			if(GameManager.Instance.GetNodeTeam(body) == 0)
				PushCounter--;
			else
				PushCounter++;
        }
    }
}
