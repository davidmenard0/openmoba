using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// The FXManager is used to manage spawning particles & audio on clients
/// It's useful to be able to Free things (like bullets) and independently spawn 
/// their VFX/Audio after. This way, you don't need to keep the Node in a sleeping 
/// state while its VFX/Audio plays.
/// 
/// Note: We could use a MultiplayerSpawner for this as well, 
/// but this gives us more control over loading adn client-side control
/// </summary>

public partial class FXManager : Singleton<FXManager>
{
	private Dictionary<string, PackedScene> _particles;
	private Dictionary<string, PackedScene> _audio;

	protected override void Initialize()
	{
		_particles = new Dictionary<string, PackedScene>();
		_particles["hit_smoke"] = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/VFX/smoke_hit_particle.tscn");

		
		_audio = new Dictionary<string, PackedScene>();
		_audio["projectile_expire"] = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/Audio/projectile_expire.tscn");
		_audio["projectile_hit"] = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/Audio/projectile_hit.tscn");
		_audio["projectile_fire"] = ResourceLoader.Load<PackedScene>("res://_Game/Scenes/Audio/projectile_fire.tscn");
	}

	public override void _Process(double delta)
	{
	}

	public void PlayVFX(string name, Vector3 pos)
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

	public void PlayAudio(string name, Vector3 pos)
	{
		Rpc("RPC_Client_PlayAudio", name, pos);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private async void RPC_Client_PlayAudio(string name, Vector3 pos)
	{
		AudioStreamPlayer3D instance = _audio[name].Instantiate<AudioStreamPlayer3D>();
		AddChild(instance);
		instance.GlobalPosition = pos;
		instance.Play();
		await Task.Delay(Mathf.RoundToInt(instance.Stream.GetLength()*1000));
		RemoveChild(instance);
		instance.QueueFree();
	}
}
