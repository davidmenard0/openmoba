using Godot;
using System.Collections;
using System.Collections.Generic;

public class Logger
{
    public static void Log(string s)
    {
        GD.Print(s);
    }
}