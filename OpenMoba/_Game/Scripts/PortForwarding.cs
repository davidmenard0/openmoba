using Godot;
using System;
using System.Text;
using System.Xml.Schema;

public partial class PortForwarding : Node
{
	public Action<string, int> OnPortForwardingComplete; // ip, port

	public string LocalIP;
	public string PublicIP;
	public int Port = -1;

	Upnp _upnp;
	public override void _Ready()
	{
		FindLocalIP();
		_upnp = new Upnp();
	}

    public override void _ExitTree()
    {
		if(Port != -1)
		{
			_upnp.DeletePortMapping(Port, "UDP");
			_upnp.DeletePortMapping(Port, "TCP");
		}
    }

	private void FindLocalIP()
	{
		if(OS.HasFeature("windows") && OS.HasEnvironment("COMPUTERNAME"))
			LocalIP =  IP.ResolveHostname(OS.GetEnvironment("COMPUTERNAME"),IP.Type.Ipv4);
		else if(OS.HasFeature("x11") && OS.HasEnvironment("HOSTNAME")) // linux
			LocalIP =  IP.ResolveHostname(OS.GetEnvironment("HOSTNAME"),IP.Type.Ipv4);
		else if(OS.HasFeature("OSX") && OS.HasEnvironment("HOSTNAME"))
			LocalIP =  IP.ResolveHostname(OS.GetEnvironment("HOSTNAME"),IP.Type.Ipv4);

		Logger.Log("Local IP: " + LocalIP);
	}

	public void SetupPortForwarding(int port)
	{
		Upnp.UpnpResult error = (Upnp.UpnpResult) _upnp.Discover();

		if(error != Upnp.UpnpResult.Success ) // 0 = Success
		{
			Logger.Log("Error setting up Port Forwarding");
			return;
		}

		if( _upnp.GetGateway() != null)
		{
			GD.Print("UPnP Gateway found: ", _upnp.GetGateway().DescriptionUrl);
			if(_upnp.GetGateway().IsValidGateway() )
			{
				int err1 = _upnp.AddPortMapping(port, port, "openmoba", "UDP");
				int err2 = _upnp.AddPortMapping(port, port, "openmoba", "TCP");

				if(err1 != 0)
				{
					Logger.Log("Error setting up UCP mapping");
					return;
				}
				
				if(err2 != 0)
				{
					Logger.Log("Error setting up UCP mapping");
					return;
				}

				Port = port;
				Logger.Log("Done setting up port forwarding");
			}
		} 

		PublicIP = _upnp.QueryExternalAddress();
		Logger.Log("External IP: " + PublicIP);

		OnPortForwardingComplete?.Invoke(PublicIP, port);
	}
}
