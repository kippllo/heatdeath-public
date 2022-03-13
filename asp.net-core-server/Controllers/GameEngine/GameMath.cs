using System;
using System.IO;
using System.Text;

namespace Keybored.BackendServer.GameEngine {

    public static class GameMath {

		public static readonly float PI = 3.14f;
        
        public static float lerp(float value1, float value2, float amount){
			//Clamp the amount.
			amount = clamp(amount, 0.0f, 1.0f);
			return (value2-value1)*amount +value1;
		}

		public static float clamp(float value, float min, float max){
			value = (value > max) ? max : value;
			value = (value < min) ? min : value;
			return value;
		}

		public static int roundToInt(float numb){
			int intNumb = (int)numb;
			float dec = numb - intNumb;
			if(dec >= 0.5f) intNumb++;
			return intNumb;
		}

		public static float Sign(float f){
			return (f >= 0) ? 1f : -1f;
		}

		public static float randomRange(float min, float max){ //min is inclusive, max in exclusive.
			Random rand = new Random();
			
			//Make the parameters are in the proper order
			float trueMin = (min <= max) ? min : max;
			float trueMax = (max >= min) ? max : min;
			min = trueMin;
			max = trueMax;

			int randInd = rand.Next((int)Math.Round(min), (int)Math.Round(max)); //A random int in range
			double randDouble = rand.NextDouble(); //A random double from 0.0 to less than 1.0.
			float randFloat = (float)randInd*(float)randDouble;
			return randFloat;
		}

		public static int randomRange(int min, int max){ //min is inclusive, max in exclusive.
			Random rand = new Random();
			
			//Make the parameters are in the proper order
			int trueMin = (min <= max) ? min : max;
			int trueMax = (max >= min) ? max : min;
			min = trueMin;
			max = trueMax;

			int randInd = rand.Next(min, max); //A random int in range
			return randInd;
		}

		// This method will point return the angle in degrees to turn to point the Z axis
		// of the given position towards the given target point.
		// Note: "posToTurn" should be the position of the object you want to turn, not its rotation!
		// UPDATE ON 2/14/20: This function NEEDS to leave the returned angle as a negative for the "PlayerBot.shootSpread()" function to work correctly!!! If you need this return value normalized, use "pointZAxisTowardsNormalized" instead!
		public static float pointZAxisTowards(Vector3Thin posToTurn, Vector3Thin targetPos){
			float translatedX = targetPos.x - posToTurn.x;
			float translatedY = targetPos.y - posToTurn.y;
			return (float)Math.Atan2(translatedY, translatedX) * (180f/GameMath.PI);
		}

		public static float pointZAxisTowardsNormalized(Vector3Thin posToTurn, Vector3Thin targetPos){
			float translatedX = targetPos.x - posToTurn.x;
			float translatedY = targetPos.y - posToTurn.y;
			float newAngle = (float)Math.Atan2(translatedY, translatedX) * (180f/GameMath.PI);
			return normalizeUnitCircleAngles(newAngle);
		}

		private static float normalizeUnitCircleAngles(float angle){
			if (angle < 360 && angle >= 0) {return angle;} //If in the bounds of 0 to 359 return the angles!
			angle = (angle >= 360) ? angle-360 : angle;
			angle = (angle < 0) ? angle+360 : angle;
			return normalizeUnitCircleAngles(angle);
		}

		private static Vector3Thin normalizeUnitCircleAngles(Vector3Thin vect){
			// If the angles are greater than 360, subtract 360.
			// If it is less than zero, add 360
			// Do this recursively.
			Vector3Thin reVect = new Vector3Thin(vect.x, vect.y, vect.z);

			if ( (reVect.x < 360 && reVect.x >= 0) && (reVect.y < 360 && reVect.y >= 0) && (reVect.z < 360  && reVect.z >= 0) ) {return reVect;} //If in the bounds of 0 to 359 return the angles!

			reVect.x = (reVect.x >= 360) ? reVect.x-360 : reVect.x;
			reVect.y = (reVect.y >= 360) ? reVect.y-360 : reVect.y;
			reVect.z = (reVect.z >= 360) ? reVect.z-360 : reVect.z;

			reVect.x = (reVect.x < 0) ? reVect.x+360 : reVect.x;
			reVect.y = (reVect.y < 0) ? reVect.y+360 : reVect.y;
			reVect.z = (reVect.z < 0) ? reVect.z+360 : reVect.z;

			return normalizeUnitCircleAngles(reVect);
		}

		public static float degreesToRadians(float angle){
			return angle * (GameMath.PI/180f);
		}

		public static float radiansTodegrees(float angle){
			return angle * (180f/GameMath.PI);
		}

		public static int quadrant(Vector3Thin Cart2D){
			// This function return an int that tells what cartesian quadrant a 2D point is in.
			bool xNeg = Cart2D.x < 0;
			bool yNeg = Cart2D.y < 0;

			if(!xNeg && !yNeg) return 1; //Both positive
			if(xNeg && !yNeg) return 2; //X negative, Y positive.
			if(xNeg && yNeg) return 3; //If both are negative the point is in quadrant 3.
			if(!xNeg && yNeg) return 4;

			return -1; //return "-1" if there was some sort of error...
		}

		public static Vector3Thin cartToPolar2D(Vector3Thin Cart2D){
			// Note: I'm storing polar inside of a "Vector3Thin" as : (Radius, Theta).
			//		 So, the x propery is radius and the y property is theta.
			// Note: The Z axis is ignored. This is a 2D only function!
			Vector3Thin rtnPolar = new Vector3Thin(0,0,0);

			rtnPolar.x = (float)Math.Sqrt( Math.Pow(Cart2D.x, 2) + Math.Pow(Cart2D.y, 2) );
			rtnPolar.y = (float)Math.Atan( Cart2D.y / Cart2D.x );

			// I am following the convertion help of this website: https://www.mathsisfun.com/polar-cartesian-coordinates.html
			//	They said to use the following chart to fix "Atan" not working on negative X or Y values...
			/*
				Quadrant 	Value of tan-1
				I 			Use the calculator value
				II 			Add 180° to the calculator value
				III 		Add 180° to the calculator value
				IV 			Add 360° to the calculator value
			*/
			int quad = quadrant(Cart2D);
			switch (quad){
                case 1:
                    break;
				case 2:
                    rtnPolar.y += GameMath.PI;
                    break;
				case 3:
                    rtnPolar.y += GameMath.PI;
                    break;
				case 4:
                    rtnPolar.y += 2*GameMath.PI;
                    break;
			}

			return rtnPolar;
		}

		public static Vector3Thin polarToCart2D(Vector3Thin Polar2D){
			// Note: "Polar2D" must have its x propery be radius
			//		 and y property be theta.
			Vector3Thin rtnCart = new Vector3Thin(0,0,0);
			rtnCart.x = Polar2D.x * (float)Math.Cos(Polar2D.y);
			rtnCart.y = Polar2D.x * (float)Math.Sin(Polar2D.y);
			return rtnCart;
		}

		// This function acts kinda like a lerp,
		// but moves the vector closers to the target by a set step (which is the "speed" parameter).
		// Since it is based off a step size, it will not be based on a percent like a lerp is.
		// This means it will return an even step for any distance!
        public static Vector3Thin MoveTowardsTarget(Vector3Thin start, Vector3Thin endTarget, float speed){
            // Move the X location the amount you want the bot to step.
            Vector3Thin localSpaceTarget = new Vector3Thin(endTarget.x - start.x, endTarget.y - start.y);
            Vector3Thin nextStep = GameMath.cartToPolar2D(localSpaceTarget);
            nextStep.x = speed; //Set the radius to the speed to control step size!

            Vector3Thin globalNextStep = GameMath.polarToCart2D(nextStep); //Back to cart pos!
            globalNextStep = new Vector3Thin(globalNextStep.x + start.x, globalNextStep.y + start.y); // Convert out of local space!
            return globalNextStep;
        }

		// Rotates a point around a relative origin.
        public static Vector3Thin rotatePointByLocalDegrees(Vector3Thin localOrigin, Vector3Thin pointToRot, float degrees){
            Vector3Thin localRotPos = new Vector3Thin(pointToRot.x - localOrigin.x, pointToRot.y - localOrigin.y, 0); //Z stay zero because this 2D only...
            Vector3Thin polarPos = GameMath.cartToPolar2D(localRotPos);
            polarPos.y += GameMath.degreesToRadians(degrees); //Rotate the theta position by degrees, but in radians.
            Vector3Thin localCartPos = GameMath.polarToCart2D(polarPos); //Convert back to cartesian.
            Vector3Thin globalCartPos = new Vector3Thin(localCartPos.x + localOrigin.x, localCartPos.y + localOrigin.y, 0); //Convert the local degree turn coordinates into world space coordinates. This is converting from local space relative this origin's position, to global based on the world origin.
            return globalCartPos;
        }
		
		// This function acts pretty much like a lerp between two vectors. I'm not sure why I made it...
        // Note: If "radiusReduce" is 0.8f then the radius will be reduced to 80% of its current length.
        public static Vector3Thin reducePointByLocalRadius(Vector3Thin localOrigin, Vector3Thin pointToReduce, float radiusReduce){
            Vector3Thin localRotPos = new Vector3Thin(pointToReduce.x - localOrigin.x, pointToReduce.y - localOrigin.y);
            
            Vector3Thin polarPos = GameMath.cartToPolar2D(localRotPos);
            polarPos.x *= radiusReduce; // radiusReduce is a percent...

            Vector3Thin localCartPos = GameMath.polarToCart2D(polarPos);
            Vector3Thin globalCartPos = new Vector3Thin(localCartPos.x + localOrigin.x, localCartPos.y + localOrigin.y);
            return globalCartPos;
        }

    }

}