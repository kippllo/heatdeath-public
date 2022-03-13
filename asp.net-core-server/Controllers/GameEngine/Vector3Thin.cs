//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using static gameBackendTest.GameObj.StringFormat;

namespace Keybored.BackendServer.GameEngine {

    public class Vector3Thin {
		public float x,y,z;

		public Vector3Thin(): this(0,0,0) {
		}

		public Vector3Thin(float xIN, float yIN): this(xIN,yIN,0)  {}

		public Vector3Thin(float xIN, float yIN, float zIN) {
			/*
			x = (float)Math.Round(xIN, 2);
			y = (float)Math.Round(yIN, 2);
			z = (float)Math.Round(zIN, 2);
			*/
			
			// Testing with out rounding...
			// It messed up bot's pathfinding...
			x = xIN;
			y = yIN;
			z = zIN;
		}

		public static float distance(Vector3Thin p1, Vector3Thin p2){
			//Formula Help: https://www.varsitytutors.com/hotmath/hotmath_help/topics/distance-formula-in-3d
			double dist = Math.Sqrt( Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2) + Math.Pow(p2.z - p1.z, 2) );
			return (float)dist;
		}

		public static Vector3Thin lerp(Vector3Thin pos1, Vector3Thin pos2, float amount){
			Vector3Thin lerpVect = new Vector3Thin();
			lerpVect.x = GameMath.lerp(pos1.x, pos2.x, amount);
			lerpVect.y = GameMath.lerp(pos1.y, pos2.y, amount);
			lerpVect.z = GameMath.lerp(pos1.z, pos2.z, amount);
			return lerpVect;
		}

		public string toJSON(string qLevel = "\""){ //qLevel must a StringFormat quote level! 
			return "{" + qLevel + "x" + qLevel + ":" + x + "," + qLevel + "y" + qLevel + ":" + y + "," + qLevel + "z" + qLevel + ":" + z + "}";
		}

		public Vector3Thin Clone(){
			return new Vector3Thin(x, y, z);
		}

		public static Vector3Thin Undefined{
			get{
				return new Vector3Thin(-9999, -9999, -9999);
			}
		}

		public bool IsUndefined(){
			return x == -9999 && y == -9999 && z == -9999;
		}

		// Returns true if the two positions have the same coordinates.
		public static bool Same(Vector3Thin pos1, Vector3Thin pos2){
			return pos1.x == pos2.x && pos1.y == pos2.y && pos1.z == pos2.z;
		}
		
		// When being used as a key in a dictionary, "class.Equals" must be overriden!
		// Note: Structs can sometimes get away without overriding ".Equals".
		// Help: https://stackoverflow.com/questions/16472159/using-a-class-versus-struct-as-a-dictionary-key
		//		 https://docs.microsoft.com/en-us/dotnet/api/system.object.equals?view=netframework-4.8
		public override bool Equals(Object obj){
			/*
			if(obj ==null || GetType() != obj.GetType()) return false;

			Vector3Thin other = (Vector3Thin)obj;
			return x == other.x && y == other.y && z == other.z;
			*/

			return obj is Vector3Thin && this == (Vector3Thin)obj;
		}
		public static bool operator ==(Vector3Thin pos1, Vector3Thin pos2){
			if(Object.ReferenceEquals(pos1, null) && !Object.ReferenceEquals(pos2, null)) return false; // pos1 is null, but pos2 is not.
			if(!Object.ReferenceEquals(pos1, null) && Object.ReferenceEquals(pos2, null)) return false; // pos1 is not null, but pos2 is.
			if(Object.ReferenceEquals(pos1, null) && Object.ReferenceEquals(pos2, null)) return true; // Both are null return true, they are equal...
			return pos1.x == pos2.x && pos1.y == pos2.y && pos1.z == pos2.z;
		}
		public static bool operator !=(Vector3Thin pos1, Vector3Thin pos2){
			return !(pos1 == pos2);
		}
		public override int GetHashCode(){ //https://docs.microsoft.com/en-us/dotnet/api/system.object.equals?view=netframework-4.8
			// If this is too slow, try round each float to 2 decimal points and multiplying by 100.
			// That will convert them into ints.
			/* Then use:
				int hCode = x ^ y ^ z;
				return hCode.GetHashCode(); //Maybe even skip this line and just return the above...
			*/
			return Tuple.Create(x,y,z).GetHashCode();
		}

		public Vector3Thin RoundToInt(){
			return new Vector3Thin( GameMath.roundToInt(x), GameMath.roundToInt(y), GameMath.roundToInt(z) );
		}
	}


	class Vector3ThinEqualityComparer : EqualityComparer<Vector3Thin>{ //help: https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.equalitycomparer-1?view=netframework-4.8
		public override bool Equals(Vector3Thin pos1, Vector3Thin pos2){
			return pos1 == pos2;
		}
		public override int GetHashCode(Vector3Thin pos){
			return pos.GetHashCode();
		}
	}

}