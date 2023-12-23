using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// The VFXManager is used to manager spawning particles on clients
/// It's useful to be able to Free things (like bullets) and independently spawn 
/// their VFX after. This way, you don't need to keep the Node in a sleeping 
/// state while its VFX plays. Same for Audio (See AudioManager)
/// </summary>

public partial class VFXManager : Node
{
	private Dictionary<string, PackedScene> _particles;

	public override void _Ready()
	{
		_particles = new Dictionary<string, PackedScene>();
		_particles["hit_smoke"] = ResourceLoader.Load<PackedScene>("res://_Game/Assets/VFX/smoke_hit_particle.tscn");
	}

	public override void _Process(double delta)
	{
	}

	public void PlayOnClients(string name, Vector3 pos)
	{
		Rpc("RPC_Client_PlayVFX", name, pos);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private async void RPC_Client_PlayVFX(string name, Vector3 pos)
	{
		GpuParticles3D instance = _particles[name].Instantiate<GpuParticles3D>();
		AddChild(instance);
		instance.GlobalPosition = pos;
		instance.Emitting = true;
		await Task.Delay(Mathf.RoundToInt(instance.Lifetime*1000));
		RemoveChild(instance);
		instance.QueueFree();
	}
}
