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
        var vfx = GetNode<VFXManager>("/root/Main/VFXManager");
        vfx.PlayOnClients("hit_smoke", this.GlobalPosition);
        this.QueueFree();
    }

}
