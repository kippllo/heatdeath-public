// Help:
// Basics:                	https://docs.unity3d.com/Manual/SL-SurfaceShaders.html
// SurfaceShaderExamples:	https://docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html
// Custom Light Function:	https://docs.unity3d.com/Manual/SL-SurfaceShaderLightingExamples.html
// ShaderLab Syntax:        https://docs.unity3d.com/Manual/SL-Shader.html
// Mutiple Pass Help:       https://forum.unity.com/threads/achieving-a-multi-pass-effect-with-a-surface-shader.96393/
// Toon Lighting Help:      https://docs.unity3d.com/Manual/SL-SurfaceShaderLightingExamples.html
// Shader Tutorial:         https://docs.unity3d.com/Manual/ShaderTut2.html
// Advanced Vert Shader:    https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
// ZTest Help:              https://docs.unity3d.com/Manual/SL-CullAndDepth.html
// Shader function Overview:https://docs.unity3d.com/Manual/SL-Pass.html

//Next work on the beans going through other beans! Try to stop them only at their outline: 
// To Do:
// 1. Fix bean mesh for the weird side Cull-Outline-Normals aren't there!

Shader "Custom/VertColorOutline" {
	Properties {
		_Color ("Outline Color", Color) = (1,1,1,1)
		_Outline ("Outline", Range(0,1)) = 0.5
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Cull Front //Only Render the back-facing normals, this way there will be an outline around the regular bean, but it won't cause the bean to render through other beans like the "ZTest Greater" method would.
		CGPROGRAM
			#pragma surface surf Standard vertex:vert //fullforwardshadows
			#pragma target 3.0

			struct Input {
				float4 vertexColor; //float4 for RGBA
			};

			//Initialize vars...
			half _Outline;
			fixed4 _Color;


			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_INSTANCING_BUFFER_END(Props)

			//Vert function, ran once for each vert?
			void vert (inout appdata_full v, out Input o) {
	            UNITY_INITIALIZE_OUTPUT(Input,o);
	            o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
	            v.vertex.xyz += v.normal * _Outline; //Move all verts outward in the direction of their normal.    //Help: https://docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html
	         }

			void surf (Input IN, inout SurfaceOutputStandard o) {
				////o.Albedo = _Color;
				o.Alpha = _Color.a; //IN.vertexColor.a;
				o.Emission = _Color;
			}
		ENDCG


		Cull Back //Reset the cull to render the front-facing normals in the beans just like regular.
		CGPROGRAM
			#pragma surface surf Standard fullforwardshadows vertex:vert
			#pragma target 3.0


			struct Input {
				float4 vertexColor;
			};

			half _Glossiness;
			half _Metallic;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)


			void vert (inout appdata_full v, out Input o)
	         {
	             UNITY_INITIALIZE_OUTPUT(Input,o);
	             o.vertexColor = v.color;
	         }

			void surf (Input IN, inout SurfaceOutputStandard o) {
				//o.Albedo = IN.vertexColor.rgb;
				o.Emission = IN.vertexColor;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = IN.vertexColor.a;
			}
		ENDCG
	}
	FallBack "Diffuse"
}
