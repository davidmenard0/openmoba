using Godot;
using System.Collections;
using System.Collections.Generic;

public class Logger
{
    //Use logger to print so its easier later to add proper logging
    public static void Log(string s)
    {
        GD.Print(s);
    }
}