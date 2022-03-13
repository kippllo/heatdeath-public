//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using static gameBackendTest.GameObj.StringFormat;
//using Keybored.BackendServer.Logging;


namespace Keybored.BackendServer.GameEngine {

    public class cube{
		public Vector3Thin pos;
		public float x {
			get{
				return pos.x;
			}
			set{
				pos.x = value;
			}
		}
		public float y {
			get{
				return pos.y;
			}
			set{
				pos.y = value;
			}
		}
		public float z {
			get{
				return pos.z;
			}
			set{
				pos.z = value;
			}
		}


		public Vector3Thin size;
		public float sizeX {
			get{
				return size.x;
			}
			set{
				size.x = value;
			}
		}
		public float sizeY {
			get{
				return size.y;
			}
			set{
				size.y = value;
			}
		}
		public float sizeZ {
			get{
				return size.z;
			}
			set{
				size.z = value;
			}
		}

		public Color32Thin[] vertColors;
		public void setAllVertColor(byte r, byte g, byte b, byte a){
			vertColors = new Color32Thin[8];
			Color32Thin c = new Color32Thin(r,g,b,a);
			vertColors[0] = c;
			vertColors[1] = c;
			vertColors[2] = c;
			vertColors[3] = c;
			vertColors[4] = c;
			vertColors[5] = c;
			vertColors[6] = c;
			vertColors[7] = c;
		}

		public cube(){
			pos = new Vector3Thin(0,0,0);
			size = new Vector3Thin(1,1,1);
			setAllVertColor(255,255,255,255);
		}

		public cube(float x, float y, float z, float sX, float sY, float sZ, byte r, byte g, byte b, byte a){
			pos = new Vector3Thin(x,y,z);
			size = new Vector3Thin(sX,sY,sZ);
			setAllVertColor(r,g,b,a);
		}

		public cube(float x, float y, float z, float sX, float sY, float sZ, Color32Thin vertColorIN){
			pos = new Vector3Thin(x,y,z);
			size = new Vector3Thin(sX,sY,sZ);
			setAllVertColor(vertColorIN.r, vertColorIN.g, vertColorIN.b, vertColorIN.a);
		}

		//Return true if the passed position is touching this cube.
		public bool checkCubeCollision(Vector3Thin chkObjPos){ //checkWallCollision
            float wallMinX = pos.x - sizeX/2; //Cubes are center after they are spawned, so check them as centered here...
            float wallMaxX = pos.x + sizeX/2;
            
            float wallMinY = pos.y - sizeY/2;
            float wallMaxY = pos.y + sizeY/2;

            float wallMinZ = pos.z - sizeZ/2;
            float wallMaxZ = pos.z + sizeZ/2;

            bool inX = chkObjPos.x >= wallMinX && chkObjPos.x <= wallMaxX;
            bool inY = chkObjPos.y >= wallMinY && chkObjPos.y <= wallMaxY;
            bool inZ = chkObjPos.z >= wallMinZ && chkObjPos.z <= wallMaxZ;
            
            return inX && inY && inZ;
        }
	}

}