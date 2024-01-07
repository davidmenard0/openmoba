using Godot;
using System;

public partial class PlayerClient : Node3D
{
	public Action<bool> Client_OnInit; //isMine
	
	public bool IsMine;
	public PlayerCamera Camera;

	private int _ownerID = -1;

	private Player _player;

    public override void _Ready()
    {
		// Players are all spawned on server, so when it get mirrored
		// on clinet, handshake to confirm ownership
		if(!GameManager.Instance.IsClient) return;

        _player = GetParent<Player>();
		Camera = GetNode<PlayerCamera>("PlayerCamera");

		// Client asks the Server for a handshake.
		_player.RpcId(1, "RPC_Server_Handshake", Multiplayer.GetUniqueId());
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_ConfirmOwnership(int id, string name)
	{
		if(!GameManager.Instance.IsClient) return;
		
		_ownerID = id;
		GetNode<Label3D>("IDLabel").Text = name;

		if(_ownerID == Multiplayer.GetUniqueId())
		{
			GD.Print("Found local player: " + _ownerID);
			IsMine = true;

			//Dont forget to set ownership on both client and server
			SetMultiplayerAuthority(_ownerID, true);
		}

		Client_OnInit?.Invoke(IsMine);
		GameManager.Instance.Client_OnPlayerInit?.Invoke(_player);
	}
}
