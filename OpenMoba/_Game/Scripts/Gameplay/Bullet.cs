using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// Bullet has a very long CollisionArea that is offset towards the bottom
/// so players on the high ground can hit players on the low ground
/// </summary>

public partial class Bullet : Node3D
{
	[Export]
	public float Speed = 20f;
	[Export]
	public float Damage = 1f;
    [Export]
	public float Lifetime = 1f;
	public Vector3 Direction;


    public override void _Ready()
    {
        SetProcess(Multiplayer.IsServer());

        if(!Multiplayer.IsServer()) return;

        var fx = GetNode<FXManager>("/root/Main/FXManager");
        fx.PlayAudio("bullet_fire", this.GlobalPosition);

        StartLifeTimer();
    }

    public override void _PhysicsProcess(double delta)
    {
		if(!Multiplayer.IsServer()) return;

        this.GlobalPosition += Direction * Speed * (float)delta;
    }

    private async void StartLifeTimer()
    {
        await Task.Delay(Mathf.RoundToInt(Lifetime*1000));
        var fx = GetNode<FXManager>("/root/Main/FXManager");
        fx.PlayVFX("hit_smoke", this.GlobalPosition);
        fx.PlayAudio("bullet_expire", this.GlobalPosition);
        this.QueueFree();
    }

}
