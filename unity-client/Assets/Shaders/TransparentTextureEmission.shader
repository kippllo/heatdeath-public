Shader "Custom/TransparentTextureEmission" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Cull Back
		CGPROGRAM
			#pragma surface surf Standard fullforwardshadows vertex:vert alpha:blend // Note: "alpha:blend" is what allows the transparent textures to show correctly!
			#pragma target 3.0


			struct Input {
				float2 uv_MainTex;
				float4 vertexColor;
			};

			sampler2D _MainTex;

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
				fixed4 c = tex2D (_MainTex, IN.uv_MainTex); //See: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-tex2d
				//o.Albedo = c.rgb;
				o.Alpha = c.a; //Grab the alpha from the texture file.
				o.Emission = c.rgb;
			}
		ENDCG
	}
	FallBack "Diffuse"
}
