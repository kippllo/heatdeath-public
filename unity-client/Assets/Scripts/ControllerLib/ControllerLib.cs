using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This is a wrapper controller class.
// PS4 Controller Help: https://www.reddit.com/r/Unity3D/comments/1syswe/ps4_controller_map_for_unity/
// Looking to this for making my own input system: https://docs.unity3d.com/2017.4/Documentation/ScriptReference/EventSystems.BaseInput.html     AND      https://docs.unity3d.com/2017.4/Documentation/ScriptReference/EventSystems.BaseInputModule.html
// Note: for some reason my unity is glitching and using "6th axis" for Win PS4 "right joystick Y axis". When it should be the "4th axis" as it is on mac.
// Make my own test input app in unity that will also allow the user to config the controller...
public static class ControllerLib {

	public static bool controllerConnected{
		get {
			string[] controllers = Input.GetJoystickNames();
			return controllers.Length > 0 && controllers[0] != "";
		}
	}

	public static bool anyKeyOrButtonPressed {
		get{
			return Input.anyKeyDown;
		}
	}
}
