using Godot;
using System;

public partial class Main : Node
{
	public Node UI;
	public Node Map;

	public override void _Ready()
	{
		UI = GetNode<Node>("UI");
		Map = GetNode<Node>("Map");

		var mainmenu = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/UI/MainMenu.tscn").Instantiate();
		UI.AddChild(mainmenu);
	}
}
