using Godot;
using System;
using System.Diagnostics;

public partial class InGameUI : Control
{
	private Label _progressLabel;
	private ColorRect _team1Progress;
	private ColorRect _team2Progress;
	private Label _respawnLabel;
	private Label _resourceLabel;

	private float _maxWidth;

    public override void _Ready()
    {
		UIController.Instance.OnGameStarted += InitializeGame;
    }
    public override void _ExitTree()
	{
		EndGame();
		UIController.Instance.OnGameStarted -= InitializeGame;
	}

    private void InitializeGame()
	{
		_progressLabel = GetNode<Label>("ProgressLabel");
		Debug.Assert(_progressLabel != null, "ERROR: _progressLabel not found in InGameUI");

		_team1Progress = GetNode<ColorRect>("ProgressRect/Team1");
		Debug.Assert(_team1Progress != null, "ERROR: Could not find Team1Progress rect in InGameUI");
		_team2Progress = GetNode<ColorRect>("ProgressRect/Team2");
		Debug.Assert(_team2Progress != null, "ERROR: Could not find Team2Progress rect in InGameUI");
		
		_respawnLabel = GetNode<Label>("RespawnLabel");
		Debug.Assert(_respawnLabel != null, "ERROR: Couldn't find RespawnLabel in InGameUI");

		_resourceLabel = GetNode<Label>("ResourceLabel");
		Debug.Assert(_resourceLabel != null, "ERROR: Couldn't find ResourceLabel in InGameUI");

		_maxWidth = _team1Progress.Size.X;

		UIController.Instance.OnObjectiveProgressUpdate += UpdateProgress;
		UIController.Instance.OnResourceChange += OnResourceChange;
		
		UpdateProgress(0f);
	}

	private void EndGame()
	{
		UIController.Instance.OnObjectiveProgressUpdate -= UpdateProgress;
		UIController.Instance.OnResourceChange -= OnResourceChange;
	}


	public void UpdateProgress(float progress)
	{
		_progressLabel.Text = String.Format("{0:0.#} %", Mathf.Abs(progress * 100f) );

		var size1 = _team1Progress.Size;
		var size2 = _team2Progress.Size;
		if(progress > 0f)
		{
			size1.X = _maxWidth*Mathf.Abs(progress);
			size2.X = 0f;
		}
		else
		{
			size1.X = 0f;
			size2.X = _maxWidth*Mathf.Abs(progress);
		}
		
		_team1Progress.Size = size1;
		_team2Progress.Size = size2;
	}

	public async void ExecuteRespawnTimer(float timer)
	{
		_respawnLabel.Show();
		float t = timer;
		long ms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		while(t > 0f)
		{	
			long new_ms = DateTimeOffset.Now.ToUnixTimeMilliseconds(); 
			float delta = (new_ms - ms)/1000f;
			ms = new_ms;
			t -= delta;
			int sec = Mathf.CeilToInt(t);
			_respawnLabel.Text = sec.ToString();
			await ToSignal(GetTree(), "process_frame");
		}
		_respawnLabel.Hide();
	}

    private void OnResourceChange(int resource)
    {
        _resourceLabel.Text = resource.ToString();
    }
}
