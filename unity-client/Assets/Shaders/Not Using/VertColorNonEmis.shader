Shader "Custom/VertColorNonEmis" {
	Properties {
	}
	SubShader {
		//Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "ForceNoShadowCasting"="True"}
		LOD 200

		CGPROGRAM
		
		//For whatever reason I could not get this shader to stop have a small amount of shadows/light from the "Directional Light" in the scene. So, to fix this I added the map to a "culling mask" layer in the "Directional Light".
		#pragma surface surf Standard vertex:vert //alpha:blend //#pragma surface surf Standard vertex:vert alpha:blend noshadow noambient nolightmap noshadowmask novertexlights nodynlightmap nodirlightmap nometa

		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float4 vertexColor;
		};

		void vert (inout appdata_full v, out Input o)
         {
             UNITY_INITIALIZE_OUTPUT(Input,o);
             o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
         }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = IN.vertexColor;
			o.Alpha = IN.vertexColor.a;
			//o.Emission = IN.vertexColor; //Use this line to make them glow!
		}
		ENDCG
	}
	FallBack "Diffuse"
}
