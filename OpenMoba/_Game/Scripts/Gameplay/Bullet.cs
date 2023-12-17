using Godot;
using System;

public partial class Bullet : Node3D
{
	[Export]
	public float Speed = 20f;
	public Vector3 Direction;

    public override void _PhysicsProcess(double delta)
    {
        this.GlobalPosition += Direction * Speed * (float)delta;
    }
}
