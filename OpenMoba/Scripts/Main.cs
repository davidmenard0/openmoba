using Godot;
using System;

public partial class Main : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var mainmenu = ResourceLoader.Load<PackedScene>("res://Scenes/MainMenu.tscn").Instantiate();
		AddChild(mainmenu);
	}
}
