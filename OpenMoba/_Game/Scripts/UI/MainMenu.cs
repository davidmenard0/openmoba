using Godot;
using System;

public partial class MainMenu : Control
{
    private UIController ui;

    public override void _Ready()
    {
        ui = GetNode<UIController>("/root/Main/UI");
		ui.OnGameStarted += OnGameStarted;
    }

    public override void _ExitTree()
    {
        ui.OnGameStarted -= OnGameStarted;
    }

    #region button callbacks

    public void _on_host_button_down()
	{
		ui.OnHostClicked?.Invoke(GetNode<LineEdit>("NameInput").Text, true);
	}

	public void _on_join_button_down()
	{
		ui.OnJoinClicked?.Invoke(GetNode<LineEdit>("NameInput").Text);
	}

	public void _on_start_game_button_down()
	{
		ui.OnStartClicked?.Invoke();
	}
	#endregion

	private void OnGameStarted()
	{
		Hide();
	}
}
