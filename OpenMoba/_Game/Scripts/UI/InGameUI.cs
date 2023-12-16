using Godot;
using System;

public partial class InGameUI : Node
{
	private UIController ui;

    public override void _Ready()
    {
        ui = GetNode<UIController>("/root/Main/UI");
		ui.OnObjectiveProgressUpdate += UpdateProgress;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void UpdateProgress(float progress)
	{

	}
}
