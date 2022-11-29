Shader "Custom/Volumetric/VolumetricParticlesFake" {
	Properties {
		[HDR] _MainColor ("Base Color", Vector) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Emission ("Emission", Range(0, 1)) = 0
		_FresnelP ("Fresnel Power", Range(0.1, 4)) = 1
		_FresnelO ("Fresnel Offset", Range(-0.9, -0.5)) = -0.7
		_InvFade ("Soft Particles Factor", Range(0.01, 10)) = 1
		_LightBase ("LightBase", Float) = 0.3
		_LightPass ("LightPass", Float) = 0.7
		_Offset ("Offset", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}