using Godot;
using System;
using System.Data.Common;
using System.Diagnostics;

public partial class Player : CharacterBody3D
{
	public Action<bool> Client_OnInit; //isMine
	public Action<Player> OnDeath; //PlayerInfo

	[Export]
	public PackedScene ProjectileTemplate;
	[Export]
	public Node3D ProjectileSpawn;

	public bool IsMine = false;
	public bool IsInit = false;

	public int OwnerID = -1; //Dont hold the OwnerInfo, just the ID so we can do a lookup in GameManager
	public PlayerCamera Camera;

	private float _health;
	private float _maxHealth;

	private PlayerInput _playerInput;
	private Node3D _clientAuthority;
	private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");


	public override void _Ready()
	{
		_playerInput = GetNode<PlayerInput>("ClientAuthority/PlayerInput");
		Camera = GetNode<PlayerCamera>("ClientAuthority/Camera3D");

		if(Multiplayer.IsServer())
		{
			//Process only on server
			SetPhysicsProcess(true);
			SetProcess(true);
			_maxHealth = Balance.Get("Player.BaseHealth");
			_health = _maxHealth;

			Server_Init();
		}

		if(GameManager.Instance.IsClient)
		{
			// Client asks the Server for a handshake.
			RpcId(1, "RPC_Server_Handshake", Multiplayer.GetUniqueId());
		}
	}

	public void Server_Init()
	{
		if(!Multiplayer.IsServer()) return;

		Debug.Assert(OwnerID != -1, "ERROR: Player spawned on server witout an OwnerID");

		PlayerInfo pi = GameManager.Instance.GetPlayerInfo(OwnerID);
		RpcId(OwnerID, "RPC_Client_HandshakeResponseInfo", OwnerID, pi.Name);
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

		var dir = -_clientAuthority.GlobalTransform.Basis.Z; //forward vector
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
			// A few things are client-authority:
			// Rotation of the player, camera, Input
			// Dont forget to set ownership on both client and server
			_clientAuthority = GetNode<Node3D>("ClientAuthority");
			_clientAuthority.SetMultiplayerAuthority(requesterID, true);

			//Server return the requested info, confirming ownership
			RpcId(requesterID, "RPC_Client_ConfirmOwnership", pi.PeerID, pi.Name);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_ConfirmOwnership(int id, string name)
	{
		if(!GameManager.Instance.IsClient) return;
		
		OwnerID = id;
		GetNode<Label3D>("IDLabel").Text = name;

		if(OwnerID == Multiplayer.GetUniqueId())
		{
			GD.Print("Found local player: " + OwnerID);
			IsMine = true;

			//Dont forget to set ownership on both client and server
			_clientAuthority = GetNode<Node3D>("ClientAuthority");
			_clientAuthority.SetMultiplayerAuthority(OwnerID, true);
		}

		IsInit = true;

		Client_OnInit?.Invoke(IsMine);
		GameManager.Instance.Client_OnPlayerInit?.Invoke(this);
	}

}
