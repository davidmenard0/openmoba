using Godot;
using System;

public partial class PlayerInput : MultiplayerSynchronizer
{
	[Export]
	public Vector2 InputVector; //Sync via replication

    public override void _Ready()
    {
		//Only process input if puppet is the authority
        SetProcessInput(GetMultiplayerAuthority() == Multiplayer.GetUniqueId());
    }

    public override void _Input(InputEvent @event)
    {
		//Inputs only called on puppets

		if(Input.IsActionJustPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Visible ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
		}

        if(Input.MouseMode == Input.MouseModeEnum.Visible)
			return;
		
		InputVector = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
    }

}
