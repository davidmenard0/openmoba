using Godot;
using System;

public partial class Bullet : Node3D
{
	[Export]
	public float Speed = 20f;
	[Export]
	public float Damage = 1f;
	public Vector3 Direction;

    public override void _Ready()
    {
        SetProcess(Multiplayer.IsServer());
    }

    public override void _PhysicsProcess(double delta)
    {
		if(!Multiplayer.IsServer()) return;

        this.GlobalPosition += Direction * Speed * (float)delta;
    }

	
	private void _OnBodyEntered(Node body)
    {
		if(!Multiplayer.IsServer()) return;

        if (body is Player)
        {
            Player p = (Player)body;
			p.TakeDamage(Damage);
			this.QueueFree();
        }
    }
}
