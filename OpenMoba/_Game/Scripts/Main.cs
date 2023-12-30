using Godot;
using System;

public partial class Main : Node
{
	public Node Map;

	public override void _Ready()
	{
		Map = GetNode<Node>("Map");
	}
}
