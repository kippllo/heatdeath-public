using UnityEngine;
using System; //For math...

//Thin classes for small network packet bandwidth size.
public class Vector3Thin {
	public float x,y,z;

	public Vector3Thin(): this(0,0,0) { //Call the other consturctor.
	}

	public Vector3Thin(float xIN, float yIN, float zIN) {
		x = (float)Math.Round(xIN, 2); //Round to just two decimal points to save data in packet size!
		y = (float)Math.Round(yIN, 2);
		z = (float)Math.Round(zIN, 2);
	}

	public Vector3 ToVector3() { //Could name it "ToFat" lol!
		return new Vector3(x, y, z);
	}

	public void position(float xIN, float yIN, float zIN) {
		x = (float)Math.Round(xIN, 2);
		y = (float)Math.Round(yIN, 2);
		z = (float)Math.Round(zIN, 2);
	}

	public static Vector3Thin FromVector3(Vector3 vect){
		return new Vector3Thin(vect.x, vect.y, vect.z);
	}

	public static float distance(Vector3Thin p1, Vector3Thin p2){
		//Formula Help: https://www.varsitytutors.com/hotmath/hotmath_help/topics/distance-formula-in-3d
		double dist = Math.Sqrt( Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2) + Math.Pow(p2.z - p1.z, 2) );
		return (float)dist;
	}
}


public class Color32Thin {
	public byte r,g,b,a;

	public Color32Thin(): this(255,255,255,255) {
	}

	public Color32Thin(byte rIN, byte gIN, byte bIN, byte aIN) {
		r = rIN;
		g = gIN;
		b = bIN;
		a = aIN;
	}

	public Color32 ToColor32() {
		return new Color32(r,g,b,a);
	}

	public static Color32Thin FromColor32(Color32 col){
		return new Color32Thin(col.r, col.g, col.b, col.a);
	}
}