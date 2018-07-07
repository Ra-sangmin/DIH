Shader "PAS/SpaceSkybox"
{
	Properties
	{
		_Exposure("Exposure", Range(0,1)) = 1
		_Cube("Environment Map", Cube) = "white" {}
		_SpaceColor("Space Color", Color) = (1,1,1,1)
		_SunColor("Sun Color", Color) = (1, 0.99, 0.87, 1)
		_SunIntensity("Sun Intensity", Range(0.0,20.0)) = 10.0
		_SunSize("Sun Size", Range(0.1,10)) = 1
	}

		CGINCLUDE

#include "UnityCG.cginc"

	struct appdata
	{
		float4 position : POSITION;
		float3 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float3 texcoord : TEXCOORD0;
	};

	half _Exposure;
	samplerCUBE _Cube;
	half3 _SunColor;
	half3 _SpaceColor;
	half _SunIntensity;
	half _SunSize;


	v2f vert(appdata v)
	{
		v2f o;
		o.position = UnityObjectToClipPos(v.position);
		o.texcoord = v.texcoord;
		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		float3 v = normalize(i.texcoord);

		float d = dot(v, _WorldSpaceLightPos0.xyz);
		float dotm = max(0, d);
		half3 c_sun = _SunIntensity * _SunColor * min(pow(dotm, 1000 / _SunSize), 1);
		half3 c_space = texCUBE(_Cube, i.texcoord) * _SpaceColor * _Exposure;
		return half4(c_space + c_sun, 0);
	}

		ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Skybox" "Queue" = "Background" }
			Pass
		{
			ZWrite Off
			Cull Off
			Fog { Mode Off }
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}