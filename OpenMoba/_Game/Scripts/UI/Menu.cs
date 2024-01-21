using Godot;
using System;
using System.Diagnostics;

public partial class Menu : Control
{
    public Lobby Lobby;
    private Control _landing;

    public override void _Ready()
    {
        _landing = GetNodeOrNull<Control>("Landing");
        Debug.Assert(_landing != null, "ERROR: Cant find Landing page in Menu");
        Lobby = GetNodeOrNull<Lobby>("Lobby");
        Debug.Assert(Lobby != null, "ERROR: Cant find Lobby page in Menu");

        UIController.Instance.OnLocalPlayerJoinedGame += OpenLobby;
        UIController.Instance.OnLocalPlayerLeftGame += GoToLandingPage;
    }

    public override void _ExitTree()
    {
        UIController.Instance.OnLocalPlayerJoinedGame -= OpenLobby;
        UIController.Instance.OnLocalPlayerLeftGame -= GoToLandingPage;
    }

    private void OpenLobby(string ip, int port)
    {
        _landing.Visible = false;
        Lobby.Visible = true;
        Lobby.Initialize(ip,port);
    }

    public void GoToLandingPage()
    {
        _landing.Visible = true;
        Lobby.Visible = false;
    }

}
