using Godot;
using System;
using System.Diagnostics;

public partial class ControlPoint : Node
{
	[Export]
	private float CaptureTime = 10f;

	[Export]
	private Node3D Flag;
	private float _flagTopY = 3.75f; // hardcoded
	
	private CaptureArea _area;
	private float _captureProgress = 0f;
	
	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;
		
		Debug.Assert(Flag != null, "ERROR: Must assign flag in ControlPoint: " + this.Name);

		_area = GetNode<CaptureArea>("./CaptureArea");
		Debug.Assert(_area != null, "ERROR: Could not find CaptureArea under Objective.");
		
	}

	public override void _PhysicsProcess(double delta)
	{
		double lastProgress = _captureProgress;
		_captureProgress += _area.PushCounter * (float)delta / CaptureTime;
		_captureProgress = Mathf.Clamp(_captureProgress, -1f, 1f);

		var abs_progress = Mathf.Abs(_captureProgress);
		var flag_pos = Flag.Position;
		flag_pos.Y = abs_progress*_flagTopY;
		Flag.Position = flag_pos;

		if(_captureProgress >= 1.0f)
		{
			GD.Print("yay!");
		}
	}
}
