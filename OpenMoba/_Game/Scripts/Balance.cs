using System;
using Godot;
using Godot.Collections;

public partial class Balance : Node
{

    //This is a Godot.Collections.Dictionary, not a System.Collections.Generic Dictionary
    private static Dictionary BalanceValues;

    public static void Load()
    {
        var balance_file = FileAccess.Open("res://balance.json", FileAccess.ModeFlags.Read);
        StringName content = balance_file.GetAsText();

        var json = new Json();
        var error = json.Parse(content);
        if(error == Error.Ok)
        {
            BalanceValues = (Dictionary) json.Data;
        }
        else
        {
            GD.Print("JSON Parse Error: ", json.GetErrorMessage(), " in balance parser at line ", json.GetErrorLine());
        }
    }


    /// Accessors. Static just for shortening Balance.Instance.Get to Balance.Get
    public static float Get(string key)
    {
        return (float)BalanceValues[key];
    }
}