using Godot;
using System;

public partial class Objective : Node
{
	[Export]
	public float CaptureTime = 60f;

	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;

		Connect("body_entered", new Callable(this, "_OnBodyEntered"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!Multiplayer.IsServer()) return;
	}

	private void _OnBodyEntered(Node body)
    {
		GD.Print(body.Name);
        if (body is CharacterBody3D)
        {
            GD.Print("Character3D entered the Area3D");
            // Your logic for when a Character3D enters the area
        }
    }
}
