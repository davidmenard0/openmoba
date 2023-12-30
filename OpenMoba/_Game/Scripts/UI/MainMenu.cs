using Godot;
using System;

public partial class MainMenu : Control
{

    public override void _Ready()
    {
    }

    public override void _ExitTree()
    {
    }

    #region button callbacks

    public void _on_host_button_down()
	{
		UIController.Instance.OnHostClicked?.Invoke(GetNode<LineEdit>("NameInput").Text, true);
	}

	public void _on_join_button_down()
	{
		UIController.Instance.OnJoinClicked?.Invoke(GetNode<LineEdit>("NameInput").Text);
	}

	public void _on_start_game_button_down()
	{
		UIController.Instance.OnStartClicked?.Invoke();
	}
	#endregion

}
