using Godot;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Objective : Node3D
{
	[Export]
	public Label3D ProgressLabel;

	private CaptureArea _area;
	private float _captureProgress = 0f;
	private Node3D[] _objectiveTargets;

	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;
		
		_area = GetNode<CaptureArea>("./CaptureArea");
		Debug.Assert(_area != null, "ERROR: Could not find CaptureArea under Objective.");
		Debug.Assert(ProgressLabel != null, "ERROR: ProgressLabel must be assigned in objective");

		//Objectives are part of the map because they might change in position based on the map
		_objectiveTargets = new Node3D[2];
		var targets = GetNode<Node3D>("../ObjectiveTargets");
		_objectiveTargets[0] = targets.GetChild<Node3D>(0);
		_objectiveTargets[1] = targets.GetChild<Node3D>(1);
		Debug.Assert(_objectiveTargets[0] != null, "ERROR: Map is missing ObjectiveTarget 0");
		Debug.Assert(_objectiveTargets[1] != null, "ERROR: Map is missing ObjectiveTarget 1");
	}

    public override void _PhysicsProcess(double delta)
    {
		if(!Multiplayer.IsServer()) return;

		ProcessProgress(delta);
        StickToFloor();
    }

    private void ProcessProgress(double delta)
    {
        double lastProgress = _captureProgress;
		_captureProgress += _area.PushCounter * (float)delta / Balance.Get("Game.CaptureTime");
		_captureProgress = Mathf.Clamp(_captureProgress, -1f, 1f);

		var pos = _objectiveTargets[0].GlobalPosition.Lerp(_objectiveTargets[1].GlobalPosition, (1f+_captureProgress)*0.5f );
		pos.Y = GlobalPosition.Y; // dont change the Y, it's controled by a raycast in _PhysicsProcess
		this.GlobalPosition = pos;
		
		if( Mathf.Abs(_captureProgress - lastProgress) > Mathf.Epsilon )
			Rpc("RPC_UpdateObjectiveProgress", _captureProgress);

		if(_captureProgress >= 1.0f)
		{
			Logger.Log("TEAM 1 wins!");
		}
		else if(_captureProgress <= -1.0f)
		{
			Logger.Log("Team2 wins!");
		}
    }

    private void StickToFloor()
	{
		var spaceState = GetWorld3D().DirectSpaceState;
		Vector3 raycast_to = this.GlobalPosition + Vector3.Down*10f;
		var query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, raycast_to);
		var result = spaceState.IntersectRay(query);
		if(result.Count > 0)
		{
			this.GlobalPosition = (Vector3)result["position"] + Vector3.Up;
		}
	}

    //Clients get progress updates
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_UpdateObjectiveProgress(float progress)
	{
		UIController.Instance.OnObjectiveProgressUpdate?.Invoke(progress);
	}
}
