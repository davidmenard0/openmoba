using Godot;
using System;
using System.Diagnostics;

public partial class PlayerClient : Node3D
{
	public Action Client_OnOwnershipConfirmation;
	
	public bool IsMine;
	public PlayerCamera Camera;

	private int _ownerID = -1;

	private PlayerNode _player;

    public override void _Ready()
    {
		// Players are all spawned on server, so when it get mirrored
		// on clinet, handshake to confirm ownership
		if(!GameManager.Instance.IsClient) return;

        _player = GetParentOrNull<PlayerNode>();
		Debug.Assert(_player != null, "ERROR: Cannot find Player in PlayerClient.");

		Camera = GetNodeOrNull<PlayerCamera>("PlayerCamera");
		Debug.Assert(Camera != null, "ERROR: Cannot find Camera in PlayerClient.");

		// Client asks the Server for a handshake.
		_player.RpcId(1, "RPC_Server_Handshake", Multiplayer.GetUniqueId());
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RPC_Client_ConfirmOwnership(int id, string name)
	{
		if(!GameManager.Instance.IsClient) return;
		
		if(_ownerID == Multiplayer.GetUniqueId())
		{
			Debug.Assert(id == Multiplayer.GetUniqueId(), "ERROR: CLient got confirmation from server with a different MultiplayerUniqueID");	
			return;
		}
		
		_ownerID = id;
		GetNode<Label3D>("IDLabel").Text = name;
		GD.Print("Found local player: " + _ownerID);
		IsMine = true;

		//Dont forget to set ownership on both client and server
		SetMultiplayerAuthority(_ownerID, true);

		Camera.InitOwnership();
	}
}
