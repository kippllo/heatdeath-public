using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Threading.Tasks;
using Keybored.BackendServer.Settings;


namespace Keybored.BackendServer.Network {

    public class UDPController{

        UdpClient udpListen;

        public UDPController(){
            //Setup the UDP client to listen on the server settings JSON port and any incoming IP address.
            IPEndPoint host = new IPEndPoint(IPAddress.Any, ServerSettings.port);
		    udpListen = new UdpClient(host);

            udpListen.Client.Ttl = 32; //Can set other socket options like this...

            //Maybe look into the below and the Cleint-side version of this class has a helpful link talking about this reopening port stuff!
            //Reopen the socket and make sure it is listening with enough space for all the connections!
            //udpSocketNew.Listen( ServerSettings.simultaneousRounds*ServerSettings.connectionsLimit ); //Default is 100...
            //udpListen.Client = udpSocketNew; //Make sure the "udpListen" is bound to the current socket (this might not be needed...)
            
        }

        /*public async Task<IPEndPoint> ListenForClientHandshake() {
            UdpReceiveResult udpAsyncResults = await udpListen.ReceiveAsync();
		    //Byte[] serverPackage = udpAsyncResults.Buffer;
            IPEndPoint clientConnData = udpAsyncResults.RemoteEndPoint;
            return clientConnData;
        }*/

        public async Task<UdpReceiveResult> Listen() {
            UdpReceiveResult udpAsyncResults = await udpListen.ReceiveAsync();
            return udpAsyncResults; //This will have the connData and the data byte array in it.
        }
        

        public async Task<int> Send(IPEndPoint connData, Byte[] data){ //This can be used to send data to a remote end point in the "UdpReceiveResult.RemoteEndPoint" captured by the "Listen" method...
            int sendReturn = await udpListen.SendAsync(data, data.Length, connData);
            return sendReturn;
        }

        public int SendBlocking(IPEndPoint connData, Byte[] data){
            int sendReturn = udpListen.Send(data, data.Length, connData);
            return sendReturn;
        }


        public void Close(){
            udpListen.Close(); //Maybe close each time the serverCtrl is reset.
            udpListen.Dispose();
            udpListen = null; //Note: This object must be discarded and a new "UDPController" must be instantiated.
        }

    }
}