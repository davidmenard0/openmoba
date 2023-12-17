using Godot;
using System;

public partial class UIController : Node
{
	//MainMenu
	public Action<string, bool> OnHostClicked; //name, spawnServerPlayer
	public Action<string> OnJoinClicked; //name
	public Action OnStartClicked;


	// Multiplayer events
	public Action OnGameStarted;

	//InGameEvents
	public Action<float> OnObjectiveProgressUpdate; //progress %

	//Main UI Scenes
	private Control _mainMenu;
	private Control _inGameUI;

	public override void _Ready()
	{
		OnGameStarted += GameStarted;

		_mainMenu = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/MainMenu.tscn").Instantiate<Control>();
		AddChild(_mainMenu);

		_inGameUI = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/InGameUI.tscn").Instantiate<Control>();
		_inGameUI.Hide();
		AddChild(_inGameUI);
	}

    public override void _ExitTree()
    {
        OnGameStarted -= GameStarted;
    }

    private void GameStarted()
    {
		_inGameUI.Show();
        _mainMenu.Hide();
    }

}
