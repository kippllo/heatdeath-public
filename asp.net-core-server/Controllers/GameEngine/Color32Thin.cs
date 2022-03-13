//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using static gameBackendTest.GameObj.StringFormat;

using Keybored.BackendServer.Logging;


namespace Keybored.BackendServer.GameEngine {
    
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
	}

}