//using System.IO.Compression;
//using static Keybored.BackendServer.Network.StringFormat;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Keybored.BackendServer.Files;
using Keybored.BackendServer.Logging;
using Keybored.BackendServer.Settings;



namespace Keybored.BackendServer.Network {

	/// <summary> This class sets up the HTTP paths that are used in the server's REST API. </summary>
	/// <remarks> <para>These HTTP paths are used for getting initial connection data like the <see cref="sendClientData.syncID"/>. And they are also used for more RESTful things like getting the online player count.</para>
	/// <para> The HTTP paths also provide a TCP method for downloading important data. (An example is downloading the map data. This data cannot trust UDP to deliver it). </para>
	/// </remarks>
	[Route("api")] // This defines the first part of the http url path.
	[Produces("application/json")] //Do I need this line??? Test later...
    public class HTTPServer_PathsController : ControllerBase {

		/// <summary> ASP.Net Console Logger </summary>
		/// <value> This parameter uses DI. </value>
		public ILogger<HTTPServer_PathsController> Logger { get; }
		
		/// <summary> The main UDP server object. </summary>
		/// <value> This parameter uses DI. </value>
		Master master;
		
		/// <summary> This Constructor uses DI. And should not be called by the user. </summary>
		/// <param name="masterIN"> Passed by DI. </param>
		/// <param name="logger"> Passed by DI. </param>
		public HTTPServer_PathsController(Master masterIN, ILogger<HTTPServer_PathsController> logger) {
			master = masterIN;
			Logger = logger;
		}
		
		/// <summary> HTTP path that returns the current number of player connections active on the server. </summary>
		/// <returns> This will return a JSON containing the data. </returns>
		/// <remarks> API URL: <c> HTTP GET: api/onlinePlayers </c> </remarks>
        [HttpGet("onlinePlayers")]
        public ActionResult<IEnumerable<string>> GetOnlinePlayers() {
			try{
				return Content("{ \"onlinePlayers\":" + master.onlinePlayers + " }", "application/json");
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				return Content("{ \"onlinePlayers\":" + -1 + " }", "application/json");
			}
        }

		/// <summary> HTTP path that returns the server version (<see cref="ServerSettings.clientVer"/>) and maintenanceMessage (<see cref="ServerSettings.maintenanceMessage"/>). </summary>
		/// <returns> This will return a JSON containing the data. </returns>
		/// <remarks> API URL: <c> HTTP GET: api/serverInfo </c> </remarks>
		[HttpGet("serverInfo")]
        public ActionResult<IEnumerable<string>> GetServerInfo() {
			try{
				return Content("{ \"clientVer\": \"" + ServerSettings.clientVer + "\", \"maintenanceMessage\": \"" + ServerSettings.maintenanceMessage + "\" }", "application/json");
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				return Content("{ \"clientVer\": " + -1.0f + ", \"maintenanceMessage\": \"" + "The Server is down! \nRhett really needs to fix this..." + "\" }", "application/json");
			}
        }


        /// <summary> HTTP path that game clients call to join a new match on the server. </summary>
		/// <returns> This will return a JSON containing the data. </returns>
		/// <remarks> API URL: <c> HTTP GET: api/requestID </c> </remarks>
        [HttpGet("requestID")]
        public ActionResult<IEnumerable<string>> Get() {
			try{
				Game game = master.getOpenGame();
				int syncID = game.addConnection(Request.HttpContext.Connection.RemoteIpAddress.ToString());
				int gameID = game.gameID;

				//This needs to be called after "game.addConnection()" so "master.onlinePlayers" is updated.
				//	Note: In the below line I am hashing the IP for a second time. This is because the "connection" object
				//		with the saved hash is hard to access. Maybe change this to later only refernce that connection object's hashed IP.
				master.logConnection(gameID, syncID, IPHash.hashIP(Request.HttpContext.Connection.RemoteIpAddress.ToString()) ); //I'm pretty sure "Request.HttpContext.Connection.LocalIpAddress.ToString()" is just the server's IP, so no need to log it!

				//WILL NEED to check on client if "gameID == -1" because that means no games were available to be joined.
				return Content("{ \"gameID\":" + gameID + ", \"syncID\": " + syncID + " }", "application/json");
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				return Content("{ \"gameID\":" + -1 + ", \"syncID\": " + 0 + " }", "application/json");
			}
        }

		/// <summary> HTTP path that returns client reconnection data. </summary>
		/// <returns> This will return a compressed <see cref="playerObj"/> JSON. </returns>
		/// <remarks> <para> Note: This POST accepts a compressed <see cref="sendClientData"/> JSON from the Client-side. </para>
		/// <para>API URL: <c> HTTP POST: api/reconnectData </c></para>
		/// </remarks>
        [HttpPost("reconnectData")]
		[Consumes("application/octet-stream")]
        public async Task<ActionResult<IEnumerable<byte[]>>> reconnectData() {
			try{
				sendClientData body = await getReqJson<sendClientData>(Request.Body);

				Game game = master.getGame(body.gameID);
				string reconnData = game.getReconnData(body.syncID);
				byte[] res = getResJson(reconnData);

				return File(res, "application/octet-stream");
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				return File(new byte[0], "application/octet-stream");
			}
        }

		/// <summary> HTTP path that check if a client's gameID and syncID are valid. </summary>
		/// <returns> This will return a compressed JSON in the form of <c>{success: true}</c>. </returns>
		/// <remarks> <para> Note: This POST accepts a compressed <see cref="sendClientData"/> JSON from the Client-side. </para>
		/// <para>API URL: <c> HTTP POST: api/testConnection </c></para>
		/// </remarks>
		[HttpPost("testConnection")]
		[Consumes("application/octet-stream")]
        public async Task<ActionResult<IEnumerable<byte[]>>> testConnection() {
			try{
				sendClientData body = await getReqJson<sendClientData>(Request.Body);
				Game game = master.getGame(body.gameID);
				
				bool isConnected = game.testConnection(body);
				string strSuccess = (isConnected) ? "true" : "false";
				string resObj = "{ success: " + strSuccess + "}";
				byte[] res = getResJson(resObj);

				return File(res, "application/octet-stream");
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				return File(new byte[0], "application/octet-stream");
			}
        }

		/// <summary> HTTP path that sends map data specific to a match back to the asking client. </summary>
		/// <returns> This will return a compressed <see cref="Keybored.BackendServer.GameLevel.MapData"/> JSON. </returns>
		/// <remarks> <para> Note: This POST accepts a compressed <see cref="sendClientData"/> JSON from the Client-side. </para>
		/// <para>API URL: <c> HTTP POST: api/SendMapData </c></para>
		/// </remarks>
		[HttpPost("SendMapData")]
		[Consumes("application/octet-stream")]
        public async Task<ActionResult<IEnumerable<byte[]>>> SendMapData() {
			try{
				sendClientData body = await getReqJson<sendClientData>(Request.Body);

				Game game = master.getGame(body.gameID);
				string mapData = await game.getMapData();
				byte[] res = getResJson(mapData);

				return File(res, "application/octet-stream");
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				return File(new byte[0], "application/octet-stream");
			}
        }
		

		
		//=================================================HTTP Helper Methods=================================================
		/// <summary> Just like the other <see cref="getReqJson(byte[])"/>, but use this one in HTTP path functions when the body is a Stream (<see cref="Stream"/>) instead of a byte[]. </summary>
		/// <remarks> <para> This one is mostly used when the data comes from HTTP. </para>
		/// <para> The other version is mostly used when the data comes from UDP. </para>
		/// </remarks>
		public static async Task<T> getReqJson<T>(Stream body){
			try{
				byte[] rawBody;
				using (var ms = new MemoryStream()) { //Help: https://weblog.west-wind.com/posts/2017/Sep/14/Accepting-Raw-Request-Body-Content-in-ASPNET-Core-API-Controllers?Page=2
					await body.CopyToAsync(ms); //Copy help part2: https://stackoverflow.com/questions/3212707/how-to-get-a-memorystream-from-a-stream-in-net
					rawBody = ms.ToArray();
				}
				
				byte[] decompressedBytes = FileData.Decompress(rawBody);
				string bodyStr = Encoding.ASCII.GetString(decompressedBytes);
				T bodyObj = JsonConvert.DeserializeObject<T>(bodyStr, new JsonSerializerSettings{
					TypeNameHandling = TypeNameHandling.None
				});
				
				return bodyObj;
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				T errObj = (T) new Object();
				return errObj;
			}
		}

			
		/// <summary> Gets the request JSON from a compressed byte array. </summary>
		/// <returns> This returns an object of type <typeparamref name="T"/> converted from the compressed JSON data <paramref name="body"/>. </returns>
		/// <typeparam name="T"> The type to convert the decompressed JSON to.</typeparam>
		/// <remarks> <para> NOTE: This version of the method is used when the UDP body is already a byte array! </para>
		/// <para>NOTE: Upon any conversion error this functions return a blank object of type <typeparamref name="T"/>. </para>
		/// </remarks>
		public static T getReqJson<T>(byte[] body){
			try{
				byte[] decompressedBytes = FileData.Decompress(body);
				string bodyStr = Encoding.ASCII.GetString(decompressedBytes);
				T bodyObj = JsonConvert.DeserializeObject<T>(bodyStr, new JsonSerializerSettings{
					TypeNameHandling = TypeNameHandling.None
				});
				
				return bodyObj;
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				T errObj = (T) new Object();
				return errObj;
			}
		}


		/// <summary> Readys a JSON string to be sent to the client. </summary>
		/// <returns> This returns a compressed byte array made from <paramref name="strObj"/> which is a string JSON object. </returns>
		/// <param name="strObj"> Should be a string JSON. But could alternatively be any string that needs to be compressed. </param>
		public static byte[] getResJson(string strObj){
			byte[] strBytes = Encoding.ASCII.GetBytes(strObj);
			byte[] compressedBytes = FileData.Compress(strBytes);
			return compressedBytes;
		}
		
		
    }
	
}
