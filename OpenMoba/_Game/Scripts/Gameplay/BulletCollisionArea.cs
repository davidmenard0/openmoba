using Godot;
using System;

public partial class BulletCollisionArea : Area3D
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

        if (body is Player)
        {
            Player p = (Player)body;
			p.TakeDamage(GetParent<Bullet>().Damage);

			
			var fx = GetNode<FXManager>("/root/Main/FXManager");
			fx.PlayVFX("hit_smoke", this.GlobalPosition);
			fx.PlayAudio("bullet_hit", this.GlobalPosition);

			GetParent().QueueFree();
        }
    }
}