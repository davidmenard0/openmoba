using Godot;
using System;

public partial class UIController : Node
{
	#region Singleton
	//Singleton base class does not work because of this issue: https://github.com/godotengine/godot/issues/79519
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
	#endregion

	//Menu Naviagtion
	public Action<string> OnHostLANClicked; //name
	public Action<string> OnHostInternetClicked; //name
	public Action<string, string, int> OnJoinClicked; //name, ip, port
	public Action OnStartClicked;
	public Action<string, int> OnGameCreated; // ip, port


	// Multiplayer events
	public Action OnGameStarted;

	//InGameEvents
	public Action<float> OnObjectiveProgressUpdate; //progress %
	public Action<float> OnLocalPlayerRespawn; // respawn timer
	public Action<int> OnResourceChange; // resources
	public Action<int> OnNewSkill; //skillslot

	//Main UI Scenes
	private Control _mainMenu;
	private InGameUI _inGameUI;
	private Lobby _lobby;
	
	private Control _creatingGameModal;

	protected void Initialize()
	{
		OnGameStarted += GameStarted;
		OnLocalPlayerRespawn += LocalPlayerRespawn;
		OnHostLANClicked += ShowCreatingGameModal;
		OnHostInternetClicked += ShowCreatingGameModal;
		OnGameCreated += HideCreatingGameModal;

		_mainMenu = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/Menu.tscn").Instantiate<Control>();
		AddChild(_mainMenu);

		_inGameUI = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/InGameUI.tscn").Instantiate<InGameUI>();
		_inGameUI.Hide();
		AddChild(_inGameUI);

		_creatingGameModal = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/CreatingGamePanel.tscn").Instantiate<Control>();
		AddChild(_creatingGameModal);
		_creatingGameModal.Visible = false;

	}


    public override void _ExitTree()
    {
        OnGameStarted -= GameStarted;
		OnLocalPlayerRespawn -= LocalPlayerRespawn;
		OnHostLANClicked -= ShowCreatingGameModal;
		OnHostInternetClicked -= ShowCreatingGameModal;
		OnGameCreated -= HideCreatingGameModal;
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

    private void ShowCreatingGameModal(string name)
    {
        _creatingGameModal.Visible = true;
    }
	
    private void HideCreatingGameModal(string ip, int port)
    {
        _creatingGameModal.Visible = false;
    }

}
