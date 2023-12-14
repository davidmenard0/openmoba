using Godot;
using System;
using System.Diagnostics;

public partial class Objective : Node
{
	[Export]
	public float CaptureTime = 60f;
	[Export]
	public Label3D ProgressLabel;

	private int _pushCount = 0;
	private float _captureProgress = 0f;

	private Label3D _label;

	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;

		Debug.Assert(ProgressLabel != null, "ERROR: ProgressLabel must be assigned in objective");

		Connect("body_entered", new Callable(this, "_OnBodyEntered"));
		Connect("body_exited", new Callable(this, "_OnBodyExited"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!Multiplayer.IsServer()) return;

		_captureProgress += _pushCount * (float)delta / CaptureTime;
		ProgressLabel.Text = Mathf.Abs(_captureProgress * 100f).ToString(".#") + '%';

		if(_captureProgress >= 1.0f)
		{
			GD.Print("TEAM 1 wins!");
		}
		else if(_captureProgress <= -1.0f)
		{
			GD.Print("Team2 wins!");
		}
	}

	private void _OnBodyEntered(Node body)
    {
        if (body is CharacterBody3D)
        {
            Player p = (Player)body;
			if(p.Team == 0)
				_pushCount++;
			else
				_pushCount--;
        }
    }

	private void _OnBodyExited(Node body)
    {
        if (body is CharacterBody3D)
        {
            Player p = (Player)body;
			if(p.Team == 0)
				_pushCount--;
			else
				_pushCount++;
        }
    }
}
