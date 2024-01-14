using Godot;
using System;

public partial class ProjectileCollisionArea : Area3D
{	
	public override void _Ready()
	{
		if(!Multiplayer.IsServer()) return;
		Connect("body_entered", new Callable(this, "_OnBodyEntered"));
		Connect("body_exited", new Callable(this, "_OnBodyExited"));
	}

	private void _OnBodyEntered(Node body)
    {
		if(!Multiplayer.IsServer()) return;

        if (body is PlayerNode)
        {
            PlayerNode p = (PlayerNode)body;
			p.TakeDamage(Balance.Get("Projectile.BaseDamage"));
			var projectile = GetParent<Projectile>();
			projectile.QueueFree();
        }
    }

	private void _OnBodyExited(Node body)
    {
		if(!Multiplayer.IsServer()) return;
    }
}
