using Godot;
using System;
using System.Diagnostics;

public partial class Objective : Node3D
{
[Export]
	public float CaptureTime = 60f;
	[Export]
	public Label3D ProgressLabel;

	private ObjectiveInfluenceArea _area;
	private float _captureProgress = 0f;
	private UIController UI;
	private Node3D[] _objectiveTargets;

	public override void _Ready()
	{
		UI = GetNode<UIController>("/root/Main/UI");
		if(!Multiplayer.IsServer()) return;
		
		_area = GetNode<ObjectiveInfluenceArea>("./InfluenceArea");
		Debug.Assert(_area != null, "ERROR: Could not find ObjectiveInfluenceArea under Objective.");
		Debug.Assert(ProgressLabel != null, "ERROR: ProgressLabel must be assigned in objective");

		//Objectives are part of the map because they might change in position based on the map
		_objectiveTargets = new Node3D[2];
		var targets = GetNode<Node3D>("../ObjectiveTargets");
		_objectiveTargets[0] = targets.GetChild<Node3D>(0);
		_objectiveTargets[1] = targets.GetChild<Node3D>(1);
		Debug.Assert(_objectiveTargets[0] != null, "ERROR: Map is missing ObjectiveTarget 0");
		Debug.Assert(_objectiveTargets[1] != null, "ERROR: Map is missing ObjectiveTarget 1");
	}

	public override void _Process(double delta)
	{
		if(!Multiplayer.IsServer()) return;

		double lastProgress = _captureProgress;
		_captureProgress += _area.PushCounter * (float)delta / CaptureTime;
		this.Transform = _objectiveTargets[0].Transform.InterpolateWith(_objectiveTargets[1].Transform, (1f+_captureProgress)*0.5f );
		
		if( Mathf.Abs(_captureProgress - lastProgress) > Mathf.Epsilon )
			Rpc("RPC_UpdateObjectiveProgress", _captureProgress);

		if(_captureProgress >= 1.0f)
		{
			GD.Print("TEAM 1 wins!");
		}
		else if(_captureProgress <= -1.0f)
		{
			GD.Print("Team2 wins!");
		}
	}

	//Clients only get progress updates
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_UpdateObjectiveProgress(float progress)
	{
		UI.OnObjectiveProgressUpdate?.Invoke(progress);
	}
}
