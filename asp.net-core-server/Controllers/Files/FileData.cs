using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Keybored.BackendServer.GameLevel;
using Keybored.BackendServer.Logging;


namespace Keybored.BackendServer.Files {

    public static class FileData {

		private static string mapFilepath = "./map";
		private static Dictionary<string, MapData> mapCache = new Dictionary<string, MapData>(); //Filepath is the key, and the class instance is the value;



		public static async Task<MapData> readMapFileFullPath(string fullPathFilename){ //Reads a map file from a full local or global file path, not from the default folder.
			try{
				MapData mapData;
				if( mapCache.TryGetValue(fullPathFilename, out mapData) ){ //Help: https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.containskey?view=netframework-4.8#System_Collections_Generic_Dictionary_2_ContainsKey__0_
					//mapData is now set correctly from the function above.
					
					//	Note: If I ever want a destructible environment or one that somehow changes each round
					//		I will want to clone "mapData" right here and return that object clone!
					
					return mapData; //return the cached value.
				}
				else if( File.Exists(fullPathFilename) ){ //Else try to read the data from the file.
					mapData = await ReadFile<MapData>(fullPathFilename);
					mapData.genGrid(); //Generate the bullet-collision grid. Caching it here will save time per round!
					
					//Add the mapData to the cache var!
					mapCache[fullPathFilename] = mapData;

					return mapData;
				} else { //If there is no cache and there is no file, return the default map.
					return MapData.DefaultMap();
				}
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				return MapData.DefaultMap();
			}
		}


		public static async Task<MapData> readRandomMap(){
			try{
				if( Directory.Exists(mapFilepath) ){
					var fileNames = Directory.EnumerateFiles(mapFilepath); //Help: https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=netframework-4.8#System_IO_Directory_EnumerateFiles_System_String_

					string[] fileNamesArr = fileNames.Cast<string>().ToArray(); //Help: https://stackoverflow.com/questions/268671/best-way-to-convert-ilist-or-ienumerable-to-array
					Random rand = new Random();
					int randInd = rand.Next(0, fileNamesArr.Length); //This is a random range function...

					return await readMapFileFullPath( fileNamesArr[randInd] ); //Read a random map from the saved files!
				} else {
					//Create a default map and save it to a file if there are no current map files.
					MapData mapData = MapData.DefaultMap();
					SaveFile<MapData>(mapData, mapFilepath, "map_0.dat"); //Save the map...
					return mapData;
				}
			} catch(Exception err){
				if(err.GetType() == typeof(IndexOutOfRangeException)){
					FileLogger.logErrorToFile("There are no map files in the map folder! Using a default one...\n" + err.ToString());
				} else {
					FileLogger.logErrorToFile(err.ToString());
				}
				return MapData.DefaultMap();
			}
		}


		public static byte[] Compress(byte[] data) { //help: https://stackoverflow.com/questions/39191950/how-to-compress-a-byte-array-without-stream-or-system-io
			MemoryStream output = new MemoryStream();
			using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
			{
				dstream.Write(data, 0, data.Length);
			}
			return output.ToArray();
		}
		public static byte[] Decompress(byte[] data) {
			MemoryStream input = new MemoryStream(data);
			MemoryStream output = new MemoryStream();
			using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
			{
				dstream.CopyTo(output);
			}
			return output.ToArray();
		}


		// Saves any object to a file as a compressed JSON.
		// Note: dir should be like "./map" not "./map/"
		// Maybe adda bool return type to see if the file write was successful...
		public static async void SaveFile<T>(T obj, string dir, string filename){
			try{
				if(!Directory.Exists(dir)){
					Directory.CreateDirectory(dir);
				}

				string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
				byte[] strBytes = Encoding.ASCII.GetBytes(json);
				byte[] compressedBytes = Compress(strBytes);

				await File.WriteAllBytesAsync(dir + "/" + filename, compressedBytes);
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
			}
		}

		// Reads any compressed JSON object file and return the object as the correct type.
		public static async Task<T> ReadFile<T>(string fullPathFilename){
			try{
				if( File.Exists(fullPathFilename) ){
					byte[] readData = await File.ReadAllBytesAsync(fullPathFilename);

					byte[] bytesDecompressed = Decompress(readData);
					string json = Encoding.ASCII.GetString(bytesDecompressed);
					T objData = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings{
						TypeNameHandling = TypeNameHandling.None
					});

					return objData;
				} else { //If the file cannot be found, return a default object.
					return (T)new System.Object();
				}
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
				
				//If the read fails return a new default instance of the object.
				T errObj = (T) new System.Object(); //Cast a standard object into the correctt type.
				return errObj;
			}
		}

    }

}