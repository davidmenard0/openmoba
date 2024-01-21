using Godot;
using System;
using System.Data.Common;
using System.Diagnostics;

public partial class PlayerNode : CharacterBody3D
{
	public Action<PlayerNode> OnDeath; //PlayerInfo, respawn

	[Export]
	public PackedScene ProjectileTemplate;
	[Export]
	public Node3D ProjectileSpawn;

	public bool IsInit = false;

	public int OwnerID = -1; //Dont hold the OwnerInfo, just the ID so we can do a lookup in GameManager

	private float _health;
	private float _maxHealth;

	private PlayerInput _playerInput;
	private PlayerClient _playerClient;
	private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");


	public override void _Ready()
	{
		_playerInput = GetNodeOrNull<PlayerInput>("PlayerClient/PlayerInput");
		Debug.Assert(_playerInput != null, "ERROR: Cannot find PlayerInput under player.");

		if(!Multiplayer.IsServer()) return;
		
		//Process only on server
		SetPhysicsProcess(true);
		SetProcess(true);
		_maxHealth = Balance.Get("Player.BaseHealth");
		_health = _maxHealth;

		Server_Init();
	}

	public void Server_Init()
	{
		if(!Multiplayer.IsServer()) return;

		Debug.Assert(OwnerID != -1, "ERROR: Player spawned on server witout an OwnerID");
	}
	

	public override void _PhysicsProcess(double delta)
	{
		//Physics only on server
		if(!Multiplayer.IsServer()) return;

		var v = Velocity;
		if(!IsOnFloor())
		{
			v.Y -= (float) (_gravity * delta);
		}
			
		var input = _playerInput.InputVector;
		var direction = new Vector3(input.X, 0f, input.Y).Normalized();
		if(direction.LengthSquared() > Mathf.Epsilon)
		{
			v.X = direction.X * Balance.Get("Player.MoveSpeed");
			v.Z = direction.Z * Balance.Get("Player.MoveSpeed");
		}
		else
		{
			v.X = Mathf.MoveToward(v.X, 0f, Balance.Get("Player.MoveSpeed"));
			v.Z = Mathf.MoveToward(v.Z, 0f, Balance.Get("Player.MoveSpeed"));
		}

		Velocity = v;
		MoveAndSlide();
	}


	public void Fire(int id)
	{
		if(!Multiplayer.IsServer()) return;

		//Double-check that the id is the one associated to this player. 
		//This should always be true, since the RPC is only called on the correct player
		// It prevents anyone from sending this RPC with malicious intent
		if(OwnerID != id)
		{
			Debug.Assert(OwnerID == id, "ERROR: Fire called on another Player than the one that fired");
			return;
		}

		var dir = -_playerClient.GlobalTransform.Basis.Z; //forward vector
		GameManager.Instance.Spawner.SpawnProjectile(this, ProjectileTemplate, ProjectileSpawn.GlobalPosition, dir);
	}

	public void TakeDamage(float dmg){
		if(!Multiplayer.IsServer()) return;

		_health -= dmg;
		if(_health <= 0f)
		{
			OnDeath?.Invoke(this);
			//Dont queuefree here, the PlayerSpawner will do that
			Logger.Log("Player died: " + OwnerID);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Server_Handshake(int requesterID)
	{
		if(!Multiplayer.IsServer()) return;

		PlayerInfo pi = GameManager.Instance.GetPlayerInfo(requesterID);
		if(this.OwnerID == requesterID)
		{
			//Find the plaeyrClient here, because when server is alsoa  client,
			// the "client" _Ready() is called before the server _Ready()
			_playerClient = GetNode<PlayerClient>("PlayerClient");
			Debug.Assert(_playerClient != null, "ERROR: Cannot find PlayerClient under player.");
		
			// A few things are client-authority:
			// Rotation of the player, camera, Input
			// Dont forget to set ownership on both client and server
			_playerClient.SetMultiplayerAuthority(requesterID, true);

			//Server return the requested info, confirming ownership
			_playerClient.RpcId(requesterID, "RPC_Client_ConfirmOwnership", pi.PeerID, pi.Name);
		}
	}

}
