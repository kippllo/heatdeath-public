using UnityEngine;
using UnityEngine.UI;

namespace Keybored.Mobile {

    /// <summary>
    /// A unity UI touch screen joystick control.
    /// To use this correctly, be sure to set the UI prefab to start in the bottom-left hand corner as its "Anchor Preset"!
    /// After that you can move it to the desired position on screen. Just make sure the origin is the bottom-left corner!
    /// </summary>
    public class TouchJoystick : Graphic {

        public Color innerColor;
        public float outerThickness = 5f;
        public float innerThickness = 5f;
        public float innerRadius = 10f;

        /// <value> This var limits where touch will be detected in both a positive and negative distance from the UI's center position. </value>
        [SerializeField]
        private float touchZoneBound = 200f;
        /// <value> This var limits where touch will be rendered to output vars in both a positive and negative distance from the UI's center position. </value>
        [SerializeField]
        private float touchZoneDisplayBound = 100f;

        public float _UIScale = 1f;
        /// <value> This is used to scale the UI control different form screen resolutions. </value>
        public float UIScale {
            get{
                return _UIScale;
            }
            set {
                _UIScale = value;
                //Redraw all UI based on this new value
                SetAllDirty(); //For the outer cirlce

                if(joystickSprite){
                    redrawInnerCircle(); //For the inner cirlce
                }
                //Else there is no need to trigger a redraw because the inner joystick will be drawn with the 
                // correct "UIScale" in the start function.
            }
        }

        float horAxis;
        public float XAxis { //readonly!
            get {return horAxis;}
        }

        float vertAxis;
        public float YAxis {
            get {return vertAxis;}
        }

        DrawMesh meshCanvas;
        GameObject joystickSprite; //An object to hold the inner cirlce UI element.


        protected override void Start() {
            meshCanvas = new DrawMesh();
            meshCanvas.segmentCount = 30;

            //Only Gen the inner circle if the game is being played. Don't spawned it in the editor view.
            if(Application.isPlaying){ //Don't redraw the inner circle UI if it has already been drawn by "UIScale.set".
                joystickSprite = genPlayerTouchPosUI(); //Gen the inner jpystick UI GameObject.
            }
        }


        //This is where the UI mesh is drawn!
        protected override void OnPopulateMesh(VertexHelper vh) {
            //This will sometime run in the editor before "Start" is called so this is a work around for those errors.
            #if UNITY_EDITOR
                if(meshCanvas == null){
                    Start();
                }
            #endif
            
            meshCanvas.clear();
            
            Vector2 joystickCenter = Vector2.zero;
            float outerRadiusScaled = touchZoneDisplayBound*TouchLib.ScreenScale*UIScale;
            float outerThicknessScaled = outerThickness*TouchLib.ScreenScale*UIScale;
            meshCanvas.DrawCircleOutline(joystickCenter, outerRadiusScaled, outerThicknessScaled, color);

            // "vh.Clear();" is in part of the below function...
            meshCanvas.RenderToVertexHelper(ref vh);
        }


        //The "Graphic" class update method work the same way as the one in the "MonoBehaviour" class.
        void Update() {
            Vector2 joystickCenter = gameObject.GetComponent<RectTransform>().position;
            //Clear old pos!
            if(joystickSprite){
                joystickSprite.transform.position = joystickCenter; //Reset the joystick pos. Maybe lerp later...
            }

            //Reset the axis vars
            horAxis = 0;
            vertAxis = 0;
            Touch[] touches = TouchLib.getCurrentTouches();

            //Find the scaled vars...
            float touchZoneBoundScaled = touchZoneBound*TouchLib.ScreenScale*UIScale;
            float touchZoneDisplayBoundScaled = touchZoneDisplayBound*TouchLib.ScreenScale*UIScale;

            for (int i = 0; i < touches.Length; i++) {
                Vector2 screenPos = touches[i].position;
                float dist = TouchLib.distance(joystickCenter, screenPos);

                if(dist <= touchZoneBoundScaled){
                    //Get the reduced position of the touch, which is now inside of the given distance.
                    Vector2 zeroedPos = new Vector2(screenPos.x - joystickCenter.x, screenPos.y - joystickCenter.y); //Translate the point to be based on the origin of (0,0)
                    Vector2 fixedPos = reducePosToDist(Vector2.zero, zeroedPos, touchZoneDisplayBoundScaled);
                    fixedPos += joystickCenter; //Translate the position to be based on the correct origin/center once again.

                    //Set the axis based on the new user input.
                    horAxis = fixedPos.x - joystickCenter.x; //Distance from center position.
                    vertAxis = fixedPos.y - joystickCenter.y;
                    
                    //Grab the correct screen position
                    Vector2 userTouchPos = new Vector2(joystickCenter.x + horAxis, joystickCenter.y + vertAxis);

                    //Clamp the output value so this class will work like a regular real joystick.
                    horAxis = Mathf.Clamp(horAxis/touchZoneDisplayBoundScaled, -1, 1);
                    vertAxis = Mathf.Clamp(vertAxis/touchZoneDisplayBoundScaled, -1, 1);

                    //Render the joystick!
                    if(joystickSprite){
                        joystickSprite.transform.position = userTouchPos; //Just update the position of the touch joystick inner circle...
                    }
                }
            }
        }


        /// <summary>
        /// Linearly reduces a position to be confined to a max distance away from a given pos1.
        /// </summary>
        /// <returns>
        /// The reduced pos2, which is now inside of the max distance from pos1.
        /// </returns>
        public Vector2 reducePosToDist(Vector2 pos1, Vector2 pos2, float maxDist){
            float oldDist = TouchLib.distance(pos1, pos2);

            if(oldDist <= maxDist) { return pos2; } //Escape early if the point is already within the max distance.

            float reduction = oldDist / maxDist;
            Vector2 newPos = new Vector2( pos2.x/reduction, pos2.y/reduction );
            return newPos;
        }


        private GameObject genPlayerTouchPosUI(){
            GameObject drawUI = new GameObject("UserTouchPosUI");
            drawUI.transform.SetParent(transform);
            drawUI.transform.position = Vector2.zero;
            DrawUIBase drawUIBase = drawUI.AddComponent<DrawUIBase>();
            meshCanvas.clear();
            float innerRadiusScaled = innerRadius*TouchLib.ScreenScale*UIScale;
            float innerThicknessScaled = innerThickness*TouchLib.ScreenScale*UIScale;
            meshCanvas.DrawCircleOutline(Vector2.zero, innerRadiusScaled, innerThicknessScaled, innerColor); //Draw the mesh at (0,0) so the position is based on the UIElement's transform!
            drawUIBase.mesh = meshCanvas.RenderToMesh();
            return drawUI;
        }

        public void redrawInnerCircle(){
            if(Application.isPlaying){
                if(joystickSprite){
                    Destroy(joystickSprite);
                }
                joystickSprite = genPlayerTouchPosUI();
            }
        }
    }
}