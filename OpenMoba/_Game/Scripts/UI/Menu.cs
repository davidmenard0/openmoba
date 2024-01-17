using Godot;
using System;
using System.Diagnostics;

public partial class Menu : Control
{
    private Control _landing;
    private Lobby _lobby;

    public override void _Ready()
    {
        _landing = GetNodeOrNull<Control>("Landing");
        Debug.Assert(_landing != null, "ERROR: Cant find Landing page in Menu");
        _lobby = GetNodeOrNull<Lobby>("Lobby");
        Debug.Assert(_lobby != null, "ERROR: Cant find Lobby page in Menu");

        UIController.Instance.OnGameCreated += OpenLobby;
    }

    public override void _ExitTree()
    {
        UIController.Instance.OnGameCreated -= OpenLobby;
    }

    private void OpenLobby(string ip, int port)
    {
        _landing.Visible = false;
        _lobby.Visible = true;
    }

    public void GoToLandingPage()
    {
        _landing.Visible = true;
        _lobby.Visible = false;
    }

}
