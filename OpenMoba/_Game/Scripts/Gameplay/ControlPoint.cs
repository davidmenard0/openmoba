using Godot;
using System;
using System.Diagnostics;

public partial class ControlPoint : Node
{
	[Export]
	private float CaptureTime = 10f;
	[Export]
	private float Income = 1;

	[Export]
	private Node3D Flag;
	[Export]
	private Color FlagColor = new Color(1,1,1); // Exported so we can use the multiplayerSynchronize to sync

	private float _flagTopY = 3.75f; // hardcoded
	
	private CaptureArea _area;
	private float _captureProgress = 0f;
	private int _capturedTeam = -1;
	private StandardMaterial3D _material;
	
	public override void _Ready()
	{
		Debug.Assert(Flag != null, "ERROR: Must assign flag in ControlPoint: " + this.Name);

		_area = GetNode<CaptureArea>("./CaptureArea");
		Debug.Assert(_area != null, "ERROR: Could not find CaptureArea under Objective.");
		
		_material = (StandardMaterial3D) Flag.GetChild<MeshInstance3D>(0).Mesh.SurfaceGetMaterial(0);

		SetPhysicsProcess(Multiplayer.IsServer());
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!Multiplayer.IsServer()) return;

		_captureProgress += _area.PushCounter * (float)delta / CaptureTime;
		_captureProgress = Mathf.Clamp(_captureProgress, -1f, 1f);

		var abs_progress = Mathf.Abs(_captureProgress);
		var flag_pos = Flag.Position;
		flag_pos.Y = abs_progress*_flagTopY;
		Flag.Position = flag_pos;

		if(_captureProgress > 0.01f)
		{
			FlagColor = GameManager.Instance.TeamColors[0];
		}
		else if(_captureProgress < -0.01f)
		{
			FlagColor = GameManager.Instance.TeamColors[1];
		}

		if(_captureProgress >= 1.0f)
		{
			if(_capturedTeam != 0)
			{
				Logger.Log("Team 0 captured point!");
			}
				
		}
		else if(_captureProgress <= -1.0f)
		{
			if(_capturedTeam != 1)
			{
				Logger.Log("Team 1 captured point!");
			}
		}
	}

    public override void _Process(double delta)
    {
		if(_material.AlbedoColor != FlagColor)
			_material.AlbedoColor = FlagColor;
    }
}
