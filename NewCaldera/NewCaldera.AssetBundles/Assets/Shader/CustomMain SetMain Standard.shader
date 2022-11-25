Shader "Custom/Main Set/Main Standard" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
		_Dither ("Dither", Range(0, 1)) = 0
		_DoubleFaced ("_DoubleFaced", Range(0, 1)) = 2
		_NormStr ("Normal Strength", Range(-4, 4)) = 1
		[NoScaleOffset] [Normal] _NormTex ("Normal", 2D) = "bump" {}
		[HDR] _SpecColor ("Spec Color(RGB), Smooth (A)", Vector) = (1,1,1,0.5)
		_SmoothMin ("_SmoothMin", Range(0, 1)) = 0
		_SmoothMax ("_SmoothMax", Range(0, 1)) = 1
		_OccStr ("Occlusion Strength", Range(0, 2)) = 1
		[NoScaleOffset] _GenTex ("Spec (R), Gloss (G), Occlusion (B)", 2D) = "white" {}
		[NoScaleOffset] _SpecColorTex ("Spec (RGB)", 2D) = "white" {}
		[HDR] _EmissionColor ("Emission Color", Vector) = (0,0,0,0)
		[NoScaleOffset] _EmissionTex ("_EmissionTex", 2D) = "black" {}
		_EmitAnimSettings ("EmitAnimSettings U,V,Freq,Min", Vector) = (0,0,0,1)
		_EmitScroll ("Emit Scroll", Range(0, 1)) = 0
		_EmitPulse ("Emit Pulse", Range(0, 1)) = 0
		[NoScaleOffset] _DetMaskTex ("Detail Mask", 2D) = "white" {}
		[HDR] _DetColor ("Detail Color, Strength(A)", Vector) = (1,1,1,1)
		_DetTiling ("Detail Tiling (x,y)", Vector) = (1,1,0,0)
		[NoScaleOffset] _DetTex ("Detail Texture", 2D) = "white" {}
		_DetNormStr ("Detail Normal Strength", Range(-4, 4)) = 1
		[NoScaleOffset] [Normal] _DetNormTex ("Detail Normal", 2D) = "bump" {}
		_VPRTex ("Vertex Paint Texture R", 2D) = "" {}
		[HDR] _VPRTexColor ("Vertex Paint Texture Color R", Vector) = (0.5,0.5,0.5,0.5)
		_VPRTexSettings ("Vertex Paint Settings R", Vector) = (0.5,0.5,2,2)
		_VPRSpecColor ("Vertex Paint Spec Color R", Vector) = (0.5,0.5,0.5,0.5)
		_VPRNormTex ("NPTexR nrm map", 2D) = "bump" {}
		_VPRNormStr ("VPTexR nrm scale", Range(-4, 4)) = 1
		_VPRUnderAuto ("VP Under AutoTex", Float) = 0
		_VPRTiling ("Vertex Paint Spec Tiling R", Float) = 1
		_AutoTex ("Auto Texture", 2D) = "" {}
		[HDR] _AutoTexColor ("Auto Texture Color", Vector) = (0.5,0.5,0.5,0.5)
		_AutoTexSettings ("AutoTex Settings", Vector) = (0.5,2,0.5,2)
		_AutoTexHideEmission ("AutoTex Hide Emission", Range(0, 1)) = 0
		_AutoSpecColor ("Auto Spec Color", Vector) = (0.5,0.5,0.5,0.5)
		_AutoNormTex ("AutoTex nrm map", 2D) = "bump" {}
		_AutoNormStr ("AutoTex nrm scale", Range(-4, 4)) = 1
		_AutoTexTiling ("Auto Tex Tiling", Float) = 1
		_SnowEnabled ("Snow Enabled", Range(0, 1)) = 0
		[HideInInspector] _SpecMode ("__specMode", Float) = 0
		[HideInInspector] _Mode ("__mode", Float) = 0
		[HideInInspector] _SrcBlend ("__src", Float) = 1
		[HideInInspector] _DstBlend ("__dst", Float) = 0
		[HideInInspector] _ZWrite ("__zw", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Hidden/MainStandardLight"
	//CustomEditor "MainStandardGUI"
}