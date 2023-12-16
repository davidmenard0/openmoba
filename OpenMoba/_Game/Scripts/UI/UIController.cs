using Godot;
using System;

public partial class UIController : Node
{
	//MainMenu
	public Action<string, bool> OnHostClicked; //name, spawnServerPlayer
	public Action<string> OnJoinClicked; //name
	public Action OnStartClicked;


	// Multiplayer events
	public Action OnGameStarted;


	//InGameEvents
	public Action<float> OnObjectiveProgressUpdate; //progress %
}
