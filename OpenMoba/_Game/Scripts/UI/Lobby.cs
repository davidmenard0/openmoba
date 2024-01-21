using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public partial class Lobby : Control
{
    private VBoxContainer _team1Players;
    private VBoxContainer _team2Players;
    private Label _ip;
    private Label _port;
    private Label _Team1PlayerLabelTemplate;
    private Label _Team2PlayerLabelTemplate;

    private Button _startButton;
    private Button _leaveButton;


    public override void _Ready()
    {
        _ip = GetNodeOrNull<Label>("IP");
        Debug.Assert(_ip != null, "ERROR: Cant find IP label in Lobby");
        _port = GetNodeOrNull<Label>("Port");
        Debug.Assert(_port != null, "ERROR: Cant find Port label in Lobby");

        _team1Players = GetNode("Team1").GetChild<VBoxContainer>(0);
        _team2Players = GetNode("Team2").GetChild<VBoxContainer>(0);

        _Team1PlayerLabelTemplate = _team1Players.GetChild<Label>(0);
        _Team1PlayerLabelTemplate.Visible = false;
        _Team2PlayerLabelTemplate = _team2Players.GetChild<Label>(0);
        _Team2PlayerLabelTemplate.Visible = false;

        _startButton = GetNodeOrNull<Button>("StartGameButton");
        Debug.Assert(_startButton != null, "ERROR: Cant find StartGameButton in Lobby");
        _leaveButton = GetNodeOrNull<Button>("LeaveGameButton");
        Debug.Assert(_leaveButton != null, "ERROR: Cant find LeaveGameButton in Lobby");
        _startButton.Pressed += OnStartGamePressed;
        _leaveButton.Pressed += OnLeaveGamePressed;
    }

    public override void _ExitTree()
    {
        _startButton.Pressed -= OnStartGamePressed;
        _leaveButton.Pressed -= OnLeaveGamePressed;
    }

    public void Initialize(string ip, int port)
    {
        _ip.Text = ip;
        _port.Text = port.ToString();
    }

    public void OnPlayerJoined(PlayerInfo info)
    {
        if(IsPlayerAlreadyAdded(info.PeerID)) return;

        Label newplayer;
        if(info.Team == 0)
        {
            newplayer = (Label)_Team1PlayerLabelTemplate.Duplicate();
            _team1Players.AddChild(newplayer);
        }
        else
        {
            newplayer = (Label)_Team2PlayerLabelTemplate.Duplicate();
            _team2Players.AddChild(newplayer);
        }
        newplayer.Name = info.PeerID.ToString();
        newplayer.Text = info.Name;
        newplayer.Visible = true;
    }

    public void OnPlayerLeft(int id)
    {
        foreach(var label in _team1Players.GetChildren())
        {
            if(label.Name == id.ToString())
            {
               _team1Players.RemoveChild(label);
               label.QueueFree();
            }
        }

        foreach(var label in _team2Players.GetChildren())
        {
            if(label.Name == id.ToString())
            {
               _team2Players.RemoveChild(label);
               label.QueueFree();
            }
        }
    }

    private bool IsPlayerAlreadyAdded(int id)
    {
        foreach(var label in _team1Players.GetChildren())
        {
            if(label.Name == id.ToString())
                return true;
        }

        foreach(var label in _team2Players.GetChildren())
        {
            if(label.Name == id.ToString())
                return true;
        }
        return false;
    }


    private void OnStartGamePressed()
    {
        UIController.Instance.OnStartPressed?.Invoke();
    }
    
    private void OnLeaveGamePressed()
    {
        UIController.Instance.OnLeavePressed?.Invoke();
    }

}
