using Godot;
using System;
using System.Text;
using System.Xml.Schema;

public partial class PortForwarding : Node
{
	public Action<string, int> OnPortForwardingComplete; // ip, port

	public string PublicIP;
	public const int SERVER_PORT = 7777;

	Upnp _upnp;
	public override void _Ready()
	{
		SetupPotForwarding();
	}

    public override void _ExitTree()
    {
        _upnp.DeletePortMapping(SERVER_PORT, "UDP");
		_upnp.DeletePortMapping(SERVER_PORT, "TCP");
    }

    public override void _Process(double delta)
	{
	}

	private void SetupPotForwarding()
	{
		_upnp = new Upnp();
		Upnp.UpnpResult error = (Upnp.UpnpResult) _upnp.Discover();

		if(error != Upnp.UpnpResult.Success ) // 0 = Success
		{
			GD.Print("errooor.");
			//EmitSignal("upnp_error", (int)error);
			return;
		}

		if( _upnp.GetGateway() != null)
		{
			GD.Print("UPnP Gateway found: ", _upnp.GetGateway().DescriptionUrl);
			if(_upnp.GetGateway().IsValidGateway() )
			{
				int err1 = _upnp.AddPortMapping(SERVER_PORT, SERVER_PORT, "openmoba", "UDP");
				int err2 = _upnp.AddPortMapping(SERVER_PORT, SERVER_PORT, "openmoba", "TCP");

				if(err1 != 0)
				{
					GD.Print("Error setting up UCP mapping");
				}
				
				if(err2 != 0)
				{
					GD.Print("Error setting up UCP mapping");
				}

				//EmitSignal("upnp_completed", 0);
				GD.Print("cool!");
			}
		} 

		PublicIP = _upnp.QueryExternalAddress();
		GD.Print(PublicIP);

		OnPortForwardingComplete?.Invoke(PublicIP, SERVER_PORT);
	}
}
