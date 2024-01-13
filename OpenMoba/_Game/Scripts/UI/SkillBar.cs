using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class SkillBar : HBoxContainer
{
	[Export]
	private PackedScene Skilltemplate;

	private Control[] _skills;

    public override void _Ready()
	{
		Debug.Assert(Skilltemplate != null, "ERROR: Skilltemplate not assigned in SkillBar");
		UIController.Instance.OnGameStarted += InitializeGame;
	}

    public override void _ExitTree()
    {
		EndGame();
		UIController.Instance.OnGameStarted += InitializeGame;
    }

	private void InitializeGame()
	{

		_skills = new Control[Balance.GetInt("Game.MaxSkills")];
		for(int i = 0; i < _skills.Length; i++)
		{
			var newskill = Skilltemplate.Instantiate<Control>();
			_skills[i] = newskill;
			AddChild(newskill, true);
			newskill.Visible = false;
		}

		UIController.Instance.OnNewSkill += OnNewSkill;
	}

	private void EndGame()
	{
        UIController.Instance.OnNewSkill -= OnNewSkill;
	}
	
    private void OnNewSkill(int skillSlot)
    {
		_skills[skillSlot].Visible = true;	
    }
}
