using Godot;
using System.Collections;
using System.Collections.Generic;

//Singleton base class does not work because of this issue: https://github.com/godotengine/godot/issues/79519
public partial class Singleton<T> : Node where T : Node 
{
    public static T Instance { 
        get { return _instance; } 
        private set { _instance = value; } 
    }
    private static T _instance;

    public override void _Ready() {
        if (_instance == null) {
            _instance = this as T;
            Initialize();
        }
        else {
            this.QueueFree();
        }
    }

    protected virtual void Initialize() {

    }
}