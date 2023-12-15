using Godot;
using System;

public partial class MainMenu : Control
{
    private MultiplayerController _multiplayerController;

    public override void _Ready()
    {
        _multiplayerController = GetNode<MultiplayerController>("/root/Main/MultiplayerController");
		GD.Print("derpina");
    }

    #region button callbacks

	public void _on_host_button_down()
	{
		_multiplayerController.HostGame(GetNode<LineEdit>("NameInput").Text, true);
	}

	public void _on_join_button_down()
	{
        _multiplayerController.JoinGame(GetNode<LineEdit>("NameInput").Text);
	}

	public void _on_start_game_button_down()
	{
		_multiplayerController.StartGame();
		this.Hide();
	}
	#endregion
}
