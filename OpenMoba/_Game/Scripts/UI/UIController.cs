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
	//Also called when creating game
	public Action<string, int> OnLocalPlayerJoinedGame; // ip, port
	public Action OnLocalPlayerLeftGame;
	public Action OnStartPressed;
	public Action OnLeavePressed;


	// Multiplayer events
	public Action OnGameStarted;


	//InGameEvents
	public Action<float> OnObjectiveProgressUpdate; //progress %
	public Action<float> OnLocalPlayerRespawn; // respawn timer
	public Action<int> OnResourceChange; // resources
	public Action<int> OnNewSkill; //skillslot

	//Main UI Scenes
	private Menu _menu;
	private InGameUI _inGameUI;
	private Lobby _lobby;
	
	private MessageModal _messageModal;

	protected void Initialize()
	{
		
		OnHostLANClicked += ShowCreatingGameModal;
		OnHostInternetClicked += ShowCreatingGameModal;
		OnJoinClicked += ShowJoiningGameModal;

		OnGameStarted += GameStarted;
		OnLocalPlayerJoinedGame += HideModal;
		OnLocalPlayerRespawn += LocalPlayerRespawn;

		_menu = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/Menu.tscn").Instantiate<Menu>();
		AddChild(_menu);

		_inGameUI = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/InGameUI.tscn").Instantiate<InGameUI>();
		_inGameUI.Hide();
		AddChild(_inGameUI);

		_messageModal = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/MessageModal.tscn").Instantiate<MessageModal>();
		AddChild(_messageModal);
		_messageModal.Visible = false;

		OnLocalPlayerJoinedGame += DoLocalPlayerJoinedGame;
		OnLocalPlayerLeftGame += DoLocalPlayerLeftGame;
	}


    public override void _ExitTree()
    {
		OnHostLANClicked += ShowCreatingGameModal;
		OnHostInternetClicked += ShowCreatingGameModal;
		OnJoinClicked += ShowJoiningGameModal;
		
		OnGameStarted += GameStarted;
		OnLocalPlayerJoinedGame += HideModal;
        OnLocalPlayerRespawn += LocalPlayerRespawn;

		OnLocalPlayerJoinedGame -= DoLocalPlayerJoinedGame;
		OnLocalPlayerLeftGame -= DoLocalPlayerLeftGame;
 	}

	public void DoLocalPlayerJoinedGame(string ip, int port)
	{
		MultiplayerGame.Instance.Client_OnNewPlayerJoined += DoPlayerJoinedGame;
		MultiplayerGame.Instance.Client_OnPlayerDisconnected += DoPlayerLeftGame;
	}

	public void DoLocalPlayerLeftGame()
	{
		MultiplayerGame.Instance.Client_OnNewPlayerJoined -= DoPlayerJoinedGame;
		MultiplayerGame.Instance.Client_OnPlayerDisconnected -= DoPlayerLeftGame;
	}

    private void GameStarted()
    {
		_inGameUI.Show();
        _menu.Hide();
    }

	private void LocalPlayerRespawn(float timer)
	{
		_inGameUI.ExecuteRespawnTimer(timer);
	}

    private void ShowCreatingGameModal(string name)
    {
		_messageModal.SetMessage("Creating Game...");
        _messageModal.Visible = true;
    }

	private void ShowJoiningGameModal(string name, string ip, int port)
    {
		_messageModal.SetMessage("Joining Game...");
        _messageModal.Visible = true;
    }
	
    private void HideModal(string ip, int port)
    {
        _messageModal.Visible = false;
    }

	private void HideModal()
    {
        _messageModal.Visible = false;
    }

	private void DoPlayerJoinedGame(PlayerInfo info)
    {
        _menu.Lobby.OnPlayerJoined(info);
    }

	private void DoPlayerLeftGame(int id)
    {
        _menu.Lobby.OnPlayerLeft(id);
    }

}
