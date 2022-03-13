using System.Net.Sockets;
using System.Net;
using System;
using System.Threading.Tasks;

//using UnityEngine; //Only using for "debug.log", remove later...

public class UDPController {

	public UdpClient udpListen;
	//WAS: UdpClient udpListen;

	public UDPController(){
		//Setup the UDP client to listen on the server settings JSON port and any incoming IP address.
		////IPEndPoint host = new IPEndPoint(IPAddress.Any, ClientSettings.defaultPort);
		////udpListen = new UdpClient(host);
		
		// TO DO:
		// 1. Later when I'm doing more refactoring, make a constructor that will take a string IP address and int port number and connect to host with these. That will be important for LAN play.
		// 2. Make this class "Send()" and "Listen()" work as non-async and start using those in the "serverCtrl.cs" script.
		IPAddress ipAdd = Dns.GetHostAddresses(ClientSettings.defaultIP)[0];
		IPEndPoint host = new IPEndPoint(ipAdd, ClientSettings.defaultPort);
		udpListen = new UdpClient();
		udpListen.Connect(host); //Note: Calling "Connect()" will only work on the Client. When this is a LAN server use the old/server method to accept all IP address packets instead!

		//Can set other socket options like this: udpListen.Client.Ttl = 32;

		
		/* See: https://stackoverflow.com/questions/687868/sending-and-receiving-udp-packets-between-two-programs-on-the-same-computer
		 * If you want to run multiple clients on the same PC or on the same PC as the server
		 * Consider using following code instead:
		 * (Note: When running two clients on the same PC, both will send, but only one will receive.)  */
		/*
		IPEndPoint host = new IPEndPoint(IPAddress.Any, ClientSettings.defaultPort);
		udpListen = new UdpClient();
		udpListen.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		udpListen.Client.Bind(host);
		*/
	}

	public async Task<Byte[]> Listen() {
		UdpReceiveResult udpAsyncResults = await udpListen.ReceiveAsync();
		Byte[] serverPackage = udpAsyncResults.Buffer;

		return serverPackage;
	}
	

	public async Task<int> Send(IPEndPoint connData, Byte[] data){ //This can be used to send data to a remote end point in the "UdpReceiveResult.RemoteEndPoint" captured by the "Listen" method...
		int sendReturn = await udpListen.SendAsync(data, data.Length, connData);
		return sendReturn;
	}


	public void Close(){
		udpListen.Close(); //Maybe close each time the serverCtrl is reset.
		udpListen.Dispose(); //NOTE: I could set an "udpDisposed" bool after this line. This would allow me to check in the UDP send function to make sure I won't get an "ObjectDisposedException" error every time after the purge...
		udpListen = null; //Note: This object must be discarded and a new "UDPController" must be instantiated.
	}

	public void Purge() {
		//Close the old listener...
		Close();

		//Start a new one...
		IPEndPoint host = new IPEndPoint(IPAddress.Any, ClientSettings.defaultPort);
		udpListen = new UdpClient(host);

		// Debug Remove the below code later...
		//Consider using "bind" to link to same local port again: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.bind?view=netframework-4.8
		//Debug.Log("Local: "+ udpListen.Client.LocalEndPoint.ToString());
	}

	/* This function, "UDPReady", does not work, remove it later....
	public bool UDPReady {
		get{
			return udpListen != null && udpListen.Client != null;
		}
	}*/

}
