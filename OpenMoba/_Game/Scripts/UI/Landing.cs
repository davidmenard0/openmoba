using Godot;
using System;
using System.Diagnostics;

public partial class Landing : Control
{
    private Button _hostLANButton;
    private Button _hostInternetButton;
    private Button _joinButton;
    private LineEdit _nameInput;
    private LineEdit _ipInput;
    private LineEdit _portInput;

    public override void _Ready()
    {
        _hostLANButton = GetNodeOrNull<Button>("HostLANButton");
        Debug.Assert(_hostLANButton != null, "ERROR: Cant find HostLANButton in MainMenu");
        _hostInternetButton = GetNodeOrNull<Button>("HostInternetButton");
        Debug.Assert(_hostInternetButton != null, "ERROR: Cant find HostInternetButton in MainMenu");
        _joinButton = GetNodeOrNull<Button>("JoinButton");
        Debug.Assert(_joinButton != null, "ERROR: Cant find JoinButton in MainMenu");
        _nameInput = GetNodeOrNull<LineEdit>("NameInput");
        Debug.Assert(_nameInput != null, "ERROR: Cant find NameInput in MainMenu");
        _ipInput = GetNodeOrNull<LineEdit>("IPInput");
        Debug.Assert(_ipInput != null, "ERROR: Cant find IPInput in MainMenu");
        _portInput = GetNodeOrNull<LineEdit>("PortInput");
        Debug.Assert(_portInput != null, "ERROR: Cant find PortInput in MainMenu");

        _nameInput.Text = "Player" + new RandomNumberGenerator().RandiRange(0,9999);

        _hostLANButton.Pressed += OnHostLANPressed;
        _hostInternetButton.Pressed += OnHostInternetPressed;
        _joinButton.Pressed += OnJoinPressed;
    }

    public override void _ExitTree()
    {
        _hostLANButton.Pressed -= OnHostLANPressed;
        _hostInternetButton.Pressed -= OnHostInternetPressed;
        _joinButton.Pressed -= OnJoinPressed;
    }

    #region button callbacks

    public void OnHostLANPressed()
	{
		UIController.Instance.OnHostLANClicked?.Invoke(_nameInput.Text);
	}


    public void OnHostInternetPressed()
	{
		UIController.Instance.OnHostInternetClicked?.Invoke(_nameInput.Text);
	}

	public void OnJoinPressed()
	{
		UIController.Instance.OnJoinClicked?.Invoke(_nameInput.Text, _ipInput.Text, int.Parse(_portInput.Text));
	}

	#endregion
}
