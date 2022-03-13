//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using static gameBackendTest.GameObj.StringFormat;

using Keybored.BackendServer.GameLevel;
using Keybored.BackendServer.Logging;



namespace Keybored.BackendServer.GameEngine {

    //I guess this class should actually be call "lineSeg"
	public class LineSegment {
		
		private float x1,y1,z1, x2,y2,z2;
		
		// Maybe add these later and find them in the constructor...
		private float _slope; //public read only
		public float slope {
			get {
				return _slope; //Cached in constructor!
			}
		}
		private float _YIntercept;
		public float YIntercept {
			get {
				return _YIntercept;
			}
		}

		//private Vector3Thin _Start;
		public Vector3Thin Start {
			get {
				return new Vector3Thin(x1, y1, z1);
			}
			set {
				x1 = value.x;
				y1 = value.y;
				z1 = value.z;
				_slope = (End.y - Start.y) / (End.x - Start.x); //Reset slope cache!
				_YIntercept = Start.y - (slope * Start.x);
			}
		}
		//private Vector3Thin _End;
		public Vector3Thin End {
			get {
				return new Vector3Thin(x2, y2, z2);
			}
			set {
				x2 = value.x;
				y2 = value.y;
				z2 = value.z;
				_slope = (End.y - Start.y) / (End.x - Start.x);
				_YIntercept = Start.y - (slope * Start.x);
			}
		}

		public LineSegment(): this(new Vector3Thin(), new Vector3Thin()) {
		}

		public LineSegment(Vector3Thin StartIN, Vector3Thin EndIN) {
			Start = StartIN;
			End = EndIN;

			_slope = (End.y - Start.y) / (End.x - Start.x);
			_YIntercept = Start.y - (slope * Start.x);
		}

		public static Vector3Thin intercept(LineSegment lineSeg1, LineSegment lineSeg2){
			
			// Cache these for speed...
			Vector3Thin Start1 = lineSeg1.Start;
			Vector3Thin End1 = lineSeg1.End;
			Vector3Thin Start2 = lineSeg2.Start;
			Vector3Thin End2 = lineSeg2.End;
			
			float slope1 = lineSeg1.slope; //(End1.y - Start1.y) / (End1.x - Start1.x);
			float slope2 = lineSeg2.slope; //(End2.y - Start2.y) / (End2.x - Start2.x);

			//Slope will be "NaN" if the two line points are the same, and "Infinity" if the two point make a vertical line...

			if(float.IsInfinity(slope1) && float.IsInfinity(slope2)) return null; //Two vertical lines will never intersect.
			if(float.IsNaN(slope1) || float.IsNaN(slope2)) return null; //One of the two lines does not have two different points. Both points of that line are identical.
			if(slope1 == slope2) return null; //The lines are parallel. (Or they are the same line)...

			if(float.IsInfinity(slope1) && !float.IsInfinity(slope2)) { //If the first line is a vertical line (undefined slope), and the second line is a normal line. Find their intercept based on that.
				return interceptOfVerticalLine(lineSeg2, lineSeg1.Start.x);
			}
			else if(!float.IsInfinity(slope1) && float.IsInfinity(slope2)) { //Else if the second line is a vertical line, find the intercept based on that.
				return interceptOfVerticalLine(lineSeg1, lineSeg2.Start.x);
			}

			// y = m*x + b
			// b = y / (m*x)
			float YIntercept1 = lineSeg1.YIntercept; //Start1.y - (slope1 * Start1.x);
			float YIntercept2 = lineSeg2.YIntercept; //Start2.y - (slope2 * Start2.x);

			// Solve both for X...
			// Note: I am setting both Y's equaL to each other and find the X where they both met that way...
			/* So:
				y = m*x + b

			   Becomes:
				m1*x + b1 = m2*x + b2

				m1*x = m2*x + b2 - b1

				m1*x - m2*x = b2 - b1
				
				(m1 - m2)*x = b2 - b1
				
				x = (b2 - b1) / (m1 - m2)
			*/
			float LinesInterceptX = (YIntercept2 - YIntercept1) / (slope1 - slope2);
			float LinesInterceptY = slope1*LinesInterceptX + YIntercept1;
			return new Vector3Thin(LinesInterceptX, LinesInterceptY, 0); //Z is 0 because this is a 2D line...
		}

		public static Vector3Thin interceptOfVerticalLine(LineSegment lineSeg, float XofVert){
			//float segSlope = lineSeg.slope; //(lineSeg.End.y - lineSeg.Start.y) / (lineSeg.End.x - lineSeg.Start.x);
			//float YIntercept = lineSeg.Start.y - (lineSeg.slope * lineSeg.Start.x);
			float vertLineInterceptY = lineSeg.slope*XofVert + lineSeg.YIntercept;
			return new Vector3Thin(XofVert, vertLineInterceptY);
		}

		public static bool DoIntersect(LineSegment lineSeg1, LineSegment lineSeg2, Vector3Thin negLim, Vector3Thin posLim){
			// Note: When passing "negLim" and "posLim", it is up to the outisde code to make sure "negLim" contain both the lowest X and lowest Y value.
			//		 This function assumes that "negLim" already have both of the lowest valued X and Y. And that "posLim" has the highest values for X and Y.

			//DebugTimer t_DoBoundingBoxesOverlap = new DebugTimer();
			if(!DoBoundingBoxesOverlap(lineSeg1, lineSeg2)) return false; //The line segments can't intersect if they aren't touching each other.
			//t_DoBoundingBoxesOverlap.stopTimer("t_DoBoundingBoxesOverlap: ");
			
			//DebugTimer t_intercept = new DebugTimer();
			Vector3Thin intersection = intercept(lineSeg1, lineSeg2);
			//t_intercept.stopTimer("t_intercept: ");

			//DebugTimer t_lastChecks = new DebugTimer();
			if(intersection == null || float.IsNaN(intersection.x) || float.IsNaN(intersection.y)) return false; // "intersection" will be null when the lines are parrallel...

			bool XWithinLim = intersection.x >= negLim.x && intersection.x <= posLim.x;
			bool YWithinLim = intersection.y >= negLim.y && intersection.y <= posLim.y;

			//t_lastChecks.stopTimer("t_lastChecks: ");

			return XWithinLim && YWithinLim;
		}

		public Vector3Thin[] FindBoundingBox(){
			return new Vector3Thin[2] {Start, End};
		}
		
		public Vector3Thin[] FindHighLowPoints(){
			// Index 0 is the lowest corner, and index 1 is the highest corner...
			Vector3Thin min = new Vector3Thin();
			Vector3Thin max = new Vector3Thin();

			min.x = (x1 <= x2) ? x1 : x2;
			min.y = (y1 <= y2) ? y1 : y2;

			max.x = (x1 > x2) ? x1 : x2; // Note: ">" instead of ">=" because "<=" was use in min. If "x1" and "x2" are equal, min defaults to "x1" and max defaults to "x2".
			max.y = (y1 > y2) ? y1 : y2;

			return new Vector3Thin[2] {min, max};
		}

		public static bool DoBoundingBoxesOverlap(LineSegment lineSeg1, LineSegment lineSeg2){
			/*
			

			//Check if any of the 4 corners of box2 are inside of box1...
			

			
			bool inX = box2Corner0.x >= BBox1[0].x && box2Corner0.x <= BBox1[1].x;
            bool inY = box2Corner0.y >= BBox1[0].y && box2Corner0.y <= BBox1[1].y;
			bool corner0Overlap = inX && inY;

			inX = box2Corner1.x >= BBox1[0].x && box2Corner1.x <= BBox1[1].x; //Reuse these check-vars...
            inY = box2Corner1.y >= BBox1[0].y && box2Corner1.y <= BBox1[1].y;
			bool corner1Overlap = inX && inY;

			inX = box2Corner2.x >= BBox1[0].x && box2Corner2.x <= BBox1[1].x; //Reuse these check-vars...
            inY = box2Corner2.y >= BBox1[0].y && box2Corner2.y <= BBox1[1].y;
			bool corner2Overlap = inX && inY;

			inX = box2Corner3.x >= BBox1[0].x && box2Corner3.x <= BBox1[1].x; //Reuse these check-vars...
            inY = box2Corner3.y >= BBox1[0].y && box2Corner3.y <= BBox1[1].y;
			bool corner3Overlap = inX && inY;

			return corner0Overlap || corner1Overlap || corner2Overlap || corner3Overlap; //If any corner is inside of the other's bounding box, the two boxes overlap.
			*/

			Vector3Thin[] BBox1 = lineSeg1.FindHighLowPoints();
			Vector3Thin[] BBox2 = lineSeg2.FindHighLowPoints();

			bool box1XinRange = BBox1[0].x >= BBox2[0].x && BBox1[1].x <= BBox2[1].x; // Min is greater than the other min, Max is less than the other max. That means our X range is within the X range of othe over box.
			bool box1YinRange = BBox1[0].y >= BBox2[0].y && BBox1[1].y <= BBox2[1].y;

			bool box2XinRange = BBox2[0].x >= BBox1[0].x && BBox2[1].x <= BBox1[1].x;
			bool box2YinRange = BBox2[0].y >= BBox1[0].y && BBox2[1].y <= BBox1[1].y;

			bool boxRangesOverlaps = (box1XinRange && box2YinRange) || (box2XinRange && box1YinRange);
			bool boxInsideOther = (box1XinRange && box1YinRange) || (box2XinRange && box2YinRange);
			//bool box1OverlapsBox2 = (box1XinRange && box2YinRange) || (box2XinRange && box1YinRange);
			//bool box2OverlapsBox1 = (box2XinRange && box1YinRange) || (box1XinRange && box2YinRange);

			if (boxRangesOverlaps || boxInsideOther) return true;


			Vector3Thin box1Corner0 = BBox1[0]; //Bot left Corner
			Vector3Thin box1Corner1 = new Vector3Thin(BBox1[0].x, BBox1[1].y); //Top left
			Vector3Thin box1Corner2 = new Vector3Thin(BBox1[1].x, BBox1[0].y); //Bot right
			Vector3Thin box1Corner3 = BBox1[1]; //Top Right

			bool box1Corner0Overlap = pointInBox(lineSeg2, box1Corner0);
			bool box1Corner1Overlap = pointInBox(lineSeg2, box1Corner1);
			bool box1Corner2Overlap = pointInBox(lineSeg2, box1Corner2);
			bool box1Corner3Overlap = pointInBox(lineSeg2, box1Corner3);
			bool box1InBox2 = box1Corner0Overlap || box1Corner1Overlap || box1Corner2Overlap || box1Corner3Overlap;


			Vector3Thin box2Corner0 = BBox2[0]; //Bot left Corner
			Vector3Thin box2Corner1 = new Vector3Thin(BBox2[0].x, BBox2[1].y); //Top left
			Vector3Thin box2Corner2 = new Vector3Thin(BBox2[1].x, BBox2[0].y); //Bot right
			Vector3Thin box2Corner3 = BBox2[1]; //Top Right

			bool box2Corner0Overlap = pointInBox(lineSeg1, box2Corner0);
			bool box2Corner1Overlap = pointInBox(lineSeg1, box2Corner1);
			bool box2Corner2Overlap = pointInBox(lineSeg1, box2Corner2);
			bool box2Corner3Overlap = pointInBox(lineSeg1, box2Corner3);
			bool box2InBox1 = box2Corner0Overlap || box2Corner1Overlap || box2Corner2Overlap || box2Corner3Overlap;

			return box1InBox2 || box2InBox1;
		}

		private static bool pointInBox(LineSegment lineSegBoundingBox, Vector3Thin pntToCheck){
			// Note: "lineSegBoundingBox" should be the line segment that will be made into a bounding box with ".FindHighLowPoints()".
			Vector3Thin[] BBox = lineSegBoundingBox.FindHighLowPoints();
			bool inX = pntToCheck.x >= BBox[0].x && pntToCheck.x <= BBox[1].x;
            bool inY = pntToCheck.y >= BBox[0].y && pntToCheck.y <= BBox[1].y;
			return inX && inY;
		}

		//Find a point on the line given an X value!
		public float findY(float x){
			//float slope = (End.y - Start.y) / (End.x - Start.x);
			//float YIntercept = Start.y - (slope * Start.x);
			float y = slope * x + YIntercept;
			return y;
		}

		public int slopeSign(){
			//float slope = (End.y - Start.y) / (End.x - Start.x); //cache these later!!!
			return Math.Sign(slope);
		}

		public bool lineHitsWall(MapData mapData) { //grid is a 2D collision grid!
			float distBetweenPnts = End.x - Start.x;

			Vector3Thin gridStartTransform = mapData.convertToGridPos(Start.RoundToInt());
			Vector3Thin gridEndTransform = mapData.convertToGridPos(End.RoundToInt());
			bool outsideOfMap = mapData.checkOutsideGrid(gridStartTransform) || mapData.checkOutsideGrid(gridEndTransform); //Maybe only chech if the end is out of the map later, so bot can move back inside the map if something weird happens.

			if(float.IsNaN(distBetweenPnts) || outsideOfMap) return true; //Don't let the bot move if the line points are on top of each other!
			
			int signStepX = Math.Sign(distBetweenPnts); //Probably need to use the sign of the distance!
			for(int dist=0; Math.Abs(dist)<Math.Abs(distBetweenPnts); dist+=signStepX){
				int x = GameMath.roundToInt(Start.x + dist);
				int y = GameMath.roundToInt(findY(x));

				/* LOG HERE!!!!!!!!!!!!! */
				Vector3Thin transPos = mapData.convertToGridPos(new Vector3Thin(x,y));

				try{
					if(mapData.grid[(int)transPos.x, (int)transPos.y]) {
						return true;
					}
				} catch(Exception err){
					FileLogger.logErrorToFile("pos: " + transPos.toJSON());
					FileLogger.logErrorToFile("gridStartTransform: " + gridStartTransform.toJSON());
					FileLogger.logErrorToFile("gridEndTransform: " + gridEndTransform.toJSON());
					FileLogger.logErrorToFile("mapData lims: " + mapData.width + ", " + + mapData.height);
					FileLogger.logErrorToFile(err.ToString());
				}
			}

			return false; //The line does not hit a wall on the map!
		}
	}

}