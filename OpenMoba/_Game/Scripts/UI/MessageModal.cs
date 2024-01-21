using Godot;
using System;
using System.Diagnostics;

public partial class MessageModal : ColorRect
{
	private Label _msgLabel;
	
	public override void _Ready()
	{
		_msgLabel = GetNodeOrNull<Label>("MessageLabel");
		Debug.Assert(_msgLabel != null, "ERROR: Cant find MessageLabel in MessageModal");
	}

	public void SetMessage(string msg)
	{
		_msgLabel.Text = msg;
	}
}
