using Godot;
using System;

public partial class Main : Node
{
	
	public override void _Ready()
	{
		var mainmenu = ResourceLoader.Load<PackedScene>("res://Scenes/UI/MainMenu.tscn").Instantiate();
		AddChild(mainmenu);
	}

	public void ChangeScenes()
	{
		GD.Print("derpy");
	}
}
