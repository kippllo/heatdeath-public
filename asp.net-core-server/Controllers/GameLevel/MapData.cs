//=================================================Client-side classes=================================================
//These may or may not be used here on the server-side, but are meant to mirror the client-side class structure.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using static gameBackendTest.GameObj.StringFormat;

using Keybored.BackendServer.GameEngine;


namespace Keybored.BackendServer.GameLevel {

	public class MapData {
		public int width;
		public int height;
		public cube[] cubes;
		public Vector3Thin[] spawnPoints;
		

		[JsonIgnore]
		public bool[,] grid; //This the 2D collision grid...

		public static MapData DefaultMap(){
			MapData mapData = new MapData();
			mapData.width = 25;
			mapData.height = 25;
			mapData.cubes = new cube[3];
			mapData.cubes[0] = new cube(5,5,0, 1,1,1, 255,0,0,255);
			mapData.cubes[1] = new cube(6,5,0, 1,1,1, 0,255,0,255);
			mapData.cubes[2] = new cube(7,5,0, 1,1,1, 0,0,255,255);

			mapData.spawnPoints = new Vector3Thin[1];
			mapData.spawnPoints[0] = new Vector3Thin(0,0,0);
			
			return mapData;
		}

		// Returns true if the passed position is outside of this map's borders.
		// Note all map's have an origin of (0,0,0)
		public bool checkOutsideBorder(Vector3Thin chkObjPos){
            float mapBorderMinX = -width/2;
            float mapBorderMaxX = width/2;
            float mapBorderMinY = -height/2;
            float mapBorderMaxY = height/2;
            
            bool outsideOfX = chkObjPos.x < mapBorderMinX || chkObjPos.x > mapBorderMaxX;
            bool outsideOfY = chkObjPos.y < mapBorderMinY || chkObjPos.y > mapBorderMaxY;

            return outsideOfX || outsideOfY;
        }

		// This function is expensive, but only run once per when the map is first read!!!
		public void genGrid(){
			grid = new bool[width, height];
			for(int c=0; c<cubes.Length; c++){
				cube wall = cubes[c];

				// Grab all positions in the wall and mark them as a wall in the grid map.
				// Note: this only a 2D check!
				int wallMinX = (int)(wall.pos.x - wall.sizeX/2);
				int wallMaxX = (int)(wall.pos.x + wall.sizeX/2);
				int wallMinY = (int)(wall.pos.y - wall.sizeY/2);
				int wallMaxY = (int)(wall.pos.y + wall.sizeY/2);

				for(int x=wallMinX; x<=wallMaxX; x++){ // "<=wallMax" because the wall limits should be included.
					for(int y=wallMinY; y<=wallMaxY; y++){
						Vector3Thin zeroBasedGridPos = new Vector3Thin(x +width/2, y +height/2); //Transform the world position into a zero-based grid position.
						if(zeroBasedGridPos.x < width && zeroBasedGridPos.y < height){ //Don't check wall positions that are on or outside of the map borders.
							grid[(int)zeroBasedGridPos.x, (int)zeroBasedGridPos.y] = true;
						}
					}
				}
			}
		}

		public string printGrid(){
			string strGrid = "";
			//So it prints right in the string, start with the max Y, but lowest X.
			for(int h=height-1; h>-1; h--){
				for(int w=0; w<width; w++){
					strGrid += (grid[w,h]) ? "X" : "_";
				}
				strGrid += "\n";
			}

			return strGrid;
		}

		// Converts any regular world position into a zero-based collision grid position.
		public Vector3Thin convertToGridPos(Vector3Thin pos){
			return new Vector3Thin(GameMath.roundToInt(pos.x) + width/2, GameMath.roundToInt(pos.y) + height/2);
		}

		// Returns true if the positions is outside of the 2D collision grid index range.
		// Which would mean the position is outside of the map borders.
		// NOTE: The passed "pos" must already be tranformed into a collision grid zero-based index by using "convertToGridPos()".
		public bool checkOutsideGrid(Vector3Thin pos){
			bool outsideMapX = pos.x > width-1 || pos.x < 0;
			bool outsideMapY = pos.y > height-1 || pos.y < 0; 
			return outsideMapX || outsideMapY;
		}

		// This function check is a position has a 2D collision with the walls of this map.
		// The check is based on this map's 2D collision grid.
		// Note: pos doesn't need to be rounded before it is passed to this function!
        public bool posHitsWall(Vector3Thin pos){
			pos = pos.RoundToInt();
            pos = convertToGridPos(pos);
            bool outsideBorder = checkOutsideGrid(pos);
            bool hitsWall = (outsideBorder) ? true : grid[(int)pos.x, (int)pos.y];
            return hitsWall;
        }
	}

}