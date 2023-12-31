using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Projectile has a very long CollisionArea that is offset towards the bottom
/// so players on the high ground can hit players on the low ground
/// </summary>

public partial class Projectile : Node3D
{
	[Export]
	public float Speed = 20f;
	[Export]
	public float Damage = 1f;
    [Export]
	public float Lifetime = 1f;
	public Vector3 Direction;

    public int UID;
    public int OwnerID; //Dont hold the OwnerInfo, just the ID so we can do a lookup in GameManager
    private static int _idCounter = 0;


    public override void _Ready()
    {
        SetProcess(Multiplayer.IsServer());

        if(!Multiplayer.IsServer()) return;
        
        var server_sync = GetNode<MultiplayerSynchronizer>("ServerSynchronizer");
        Debug.Assert(server_sync != null, "ERROR: Couldn't find ServerSynchronizer node under Projectile");

        UID = _idCounter;
        _idCounter++;

        FXManager.Instance.PlayAudio("projectile_fire", this.GlobalPosition);

        StartLifeTimer();
    }

    public void Init(int ownerID)
    {
        OwnerID = ownerID;
    }

    public override void _PhysicsProcess(double delta)
    {
		if(!Multiplayer.IsServer()) return;

        this.GlobalPosition += Direction * Speed * (float)delta;
    }

    private async void StartLifeTimer()
    {
        if(!Multiplayer.IsServer()) return;

        await Task.Delay(Mathf.RoundToInt(Lifetime*1000));

        if(this != null) // Projectile might have hit player and already dies
            GameManager.Instance.Spawner.DespawnProjectile(this);
    }

}
