using Godot;
using System;
using System.Diagnostics;

public partial class InGameUI : Control
{
	private UIController ui;
	private Label _progressLabel;

    public override void _Ready()
    {
        ui = GetNode<UIController>("/root/Main/UI");
		_progressLabel = GetNode<Label>("ProgressLabel");
		Debug.Assert(_progressLabel != null, "ERROR: _progressLabel not found in InGameUI");

		ui.OnObjectiveProgressUpdate += UpdateProgress;

		UpdateProgress(0f);
    }

	public void UpdateProgress(float progress)
	{
		_progressLabel.Text = String.Format("{0:0.#} %", Mathf.Abs(progress * 100f) );
	}
}
