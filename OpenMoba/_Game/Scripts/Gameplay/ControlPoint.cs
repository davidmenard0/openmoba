using Godot;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public partial class ControlPoint : Node
{
	private enum STATE {UNCAPTURED, DISPUTED, CAPTURED}

	[Export]
	private float CaptureTime = 10f;
	[Export]
	private int Income = 1; 
	[Export]
	private int IncomePeriod = 1;

	[Export]
	private Node3D Flag;
	[Export]
	private Color FlagColor = new Color(1,1,1); // Exported so we can use the multiplayerSynchronize to sync

	private float _flagTopY = 3.75f; // hardcoded
	
	private STATE _state = STATE.UNCAPTURED;
	private CaptureArea _area;
	private float _captureProgress = 0f;
	private int _capturedTeam = -1;
	private StandardMaterial3D _flagMaterial;
	private CancellationTokenSource _cancelTokenSource = null;
	
	public override void _Ready()
	{
		Debug.Assert(Flag != null, "ERROR: Must assign flag in ControlPoint: " + this.Name);

		_area = GetNode<CaptureArea>("./CaptureArea");
		Debug.Assert(_area != null, "ERROR: Could not find CaptureArea under Objective.");
		
		_flagMaterial = (StandardMaterial3D) Flag.GetChild<MeshInstance3D>(0).Mesh.SurfaceGetMaterial(0);
		
		SetPhysicsProcess(Multiplayer.IsServer());
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!Multiplayer.IsServer()) return;

		//Update Capture Progress
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

		// Update Capture State
		if(_state == STATE.CAPTURED)
		{
			if(_capturedTeam == 0 && _area.PushCounter > 0)
			{
				_state = STATE.DISPUTED;
				StopIncomeTimer();
			}
			if(_capturedTeam == 1 && _area.PushCounter < 0)
			{
				_state = STATE.DISPUTED;
				StopIncomeTimer();
			}
		}

		// Check if captured
		if(_captureProgress >= 1.0f && _capturedTeam != 0)
			Capture(0);
		else if(_captureProgress <= -1.0f && _capturedTeam != 1)
			Capture(1);
	}

    public override void _Process(double delta)
    {
		if(_flagMaterial.AlbedoColor != FlagColor)
			_flagMaterial.AlbedoColor = FlagColor;
    }

	private void Capture(int team)
	{
		_state = STATE.CAPTURED;
		_capturedTeam = team;
		StartIncomeTimer();
		Rpc("RPC_Client_NotifyCapture", team, GameManager.Instance.TeamColors[team]);
		Logger.Log(String.Format("Team {0} captured point!", team));
	}

	private async void StartIncomeTimer()
	{
		if(_cancelTokenSource == null)
		{
			_cancelTokenSource = new CancellationTokenSource();
			try
			{
				await DoIncomeTimer(_cancelTokenSource.Token);
			}
			catch(OperationCanceledException)
			{
			}
			finally
			{
				_cancelTokenSource.Cancel();
				_cancelTokenSource = null;
			}
		}
	}

	private async Task DoIncomeTimer(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		while(true)
		{
			await Task.Delay(IncomePeriod*1000);
			GameManager.Instance.GiveResourceToTeam(_capturedTeam, Income);
		}
	}

	private void StopIncomeTimer()
	{
		if(_cancelTokenSource != null)
		{
			_cancelTokenSource.Cancel();
			_cancelTokenSource = null;
		}
	}


	////////// Client functions //////////////
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_NotifyCapture(int team, Color color)
	{
		//TODO: Add some client-side effect to notify the capture
	}
}
