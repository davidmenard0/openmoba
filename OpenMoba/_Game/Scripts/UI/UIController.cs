using Godot;
using System;

public partial class UIController : Node
{
	public static UIController Instance { 
        get { return _instance; } 
        private set { _instance = value; } 
    }
    private static UIController _instance;

    public override void _Ready() {
        if (_instance == null) {
            _instance = this as UIController;
            Initialize();
        }
        else {
            this.QueueFree();
        }
    }

	//MainMenu
	public Action<string, bool> OnHostClicked; //name, spawnServerPlayer
	public Action<string> OnJoinClicked; //name
	public Action OnStartClicked;


	// Multiplayer events
	public Action OnGameStarted;

	//InGameEvents
	public Action<float> OnObjectiveProgressUpdate; //progress %
	public Action<float> OnLocalPlayerRespawn; // respawn timer

	//Main UI Scenes
	private Control _mainMenu;
	private InGameUI _inGameUI;

	protected void Initialize()
	{
		OnGameStarted += GameStarted;
		OnLocalPlayerRespawn += LocalPlayerRespawn;

		_mainMenu = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/MainMenu.tscn").Instantiate<Control>();
		AddChild(_mainMenu);

		_inGameUI = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/InGameUI.tscn").Instantiate<InGameUI>();
		_inGameUI.Hide();
		AddChild(_inGameUI);
	}

    public override void _ExitTree()
    {
        OnGameStarted -= GameStarted;
		OnLocalPlayerRespawn -= LocalPlayerRespawn;
    }

    private void GameStarted()
    {
		_inGameUI.Show();
        _mainMenu.Hide();
    }

	private void LocalPlayerRespawn(float timer)
	{
		_inGameUI.ExecuteRespawnTimer(timer);
	}
}
