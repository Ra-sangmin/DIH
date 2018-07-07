Shader "PAS/AtmosphericScattering"
{
	Properties
	{
		_LookupTable("Lookup Table", 2D) = "white" {}
		_StepCount("Step Count", Int) = 15

		_SunIntencity("Sun Intencity", Range(0,100)) = 10
		_SunColor("Sun Color", Color) = (1,1,1,1)
		_SunDirection("Sun Direction", Vector) = (0,0,-1)

		_RayScatterCof("Ray Scatter Cof",Vector) = (0.18, 1.35, 3.31, 1.0)
		_MeiScatterCof("Mei Scatter Cof", Float) = 2

		_OuterRadius("OuterRadius", Float) = 100
		_InnerRadius("InnerRadius", Float) = 66.6
		_RadiusRange("RadiusRange",Float) = 33.3
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
		LOD 100
		Blend One One
		Cull Front
		Lighting Off
		ZWrite Off
		ZTest Always
		Fog{ Mode Off }

		Pass
		{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"

		sampler2D_float _CameraDepthTexture;
		sampler2D_float _LookupTable;

		int _StepCount;

		float _SunIntencity;
		float3 _SunDirection;
		float3 _SunColor;

		float4 _RayScatterCof;
		float _MeiScatterCof;
		float _OuterRadius;
		float _InnerRadius;
		float _RadiusRange;

		struct appdata
		{
			float4 vertex : POSITION;
			float3 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float4 uv : TEXCOORD0;
			float3 pos : TEXCOORD1;
			float3 viewRay : TEXCOORD2;
			float3 worldRay : TEXCOORD3;
			float3 worldPosition : TEXCOORD4;
		};
			
		float SphereIn(float3 ro, float3 rd, float3 sp, float sr)
		{
			float3  d = ro - sp;
			float b = dot(rd, d);
			float c = dot(d, d) - sr*sr;
			float t = b*b - c;
			return -b - sqrt(t);
		}

		float SphereOut(in float3 ro, in float3 rd, in float3 sp, float sr)
		{
			float3  d = ro - sp;
			float b = dot(rd, d);
			float c = dot(d, d) - sr*sr;
			float t = b*b - c;
			return -b + sqrt(t);
		}

		float GetHeight(float distance)
		{
			return (distance - _InnerRadius) / _RadiusRange;
		}
		float4 SampleLookupA(float3 position, float3 ray, float height, float distance)
		{
			float angle = (dot(position, ray) / distance) * 0.5 + 0.5;
			return tex2D(_LookupTable, float2(angle, height));
		}
		float4 SampleLookup(float3 position, float3 ray)
		{
			float distance = length(position);
			float height = GetHeight(distance);
			float angle = (dot(position, ray) / distance) * 0.5 + 0.5;
			return tex2D(_LookupTable, float2(angle, height));
		}

		float3 SkyColor(const float3 origin, const float3 ray, const float tMin, const float tMax)
		{
			const float PI = 3.14159265359;
			const float3 betaR = _RayScatterCof.xyz * _RayScatterCof.w;
			const float3 betaM = float3(_MeiScatterCof, _MeiScatterCof, _MeiScatterCof);

			const float segmentLength = (tMax - tMin) / _StepCount;
			float3 sumR = float3(0, 0, 0);
			float3 sumM = float3(0, 0, 0);

			float mu = dot(ray, _SunDirection);

			const float phaseR = 3.0 / (16.0 * PI) * (1 + mu * mu);
			const float g = 0.76;
			const float phaseM = 3.0 / (8.0 * PI) * ((1.0 - g * g) * (1.0 + mu * mu)) / ((2.0 + g * g) * pow(1.0 + g * g - 2.0 * g * mu, 1.5));

			float3 samplePos = origin + ray * (tMin + segmentLength * 0.5);
			float3 sampleRay = ray * segmentLength;
			float opticalDepthR = 0;
			float opticalDepthM = 0;
			for (int i = 0; i < _StepCount; i++)
			{
				float4 sunLookup = SampleLookup(samplePos, _SunDirection);
				float hr = sunLookup.y * segmentLength / _OuterRadius;
				float hm = sunLookup.w * segmentLength / _OuterRadius;
				opticalDepthR += hr;
				opticalDepthM += hm;
				
				float3 attenuation = exp(-betaR * (opticalDepthR + sunLookup.x + betaM * (opticalDepthM + sunLookup.z)));
				sumR += hr * attenuation;
				sumM += hm * attenuation;

				samplePos += sampleRay;
			}

			return (sumR * betaR * phaseR + sumM * betaM * phaseM) * _SunIntencity * _SunColor;
		}


		v2f vert (appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = ComputeScreenPos(o.vertex);
			o.pos = mul(unity_ObjectToWorld, v.vertex);
			o.viewRay = -WorldSpaceViewDir(v.vertex);
			o.worldRay = mul(UNITY_MATRIX_MV, v.vertex).xyz * float3(-1, -1, 1);
			o.worldRay = lerp(o.worldRay, v.texcoord, v.texcoord.z != 0);
			o.worldPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
			return o;
		}
			
		fixed4 frag (v2f i) : SV_Target
		{
			i.worldRay = i.worldRay * (_ProjectionParams.z / i.worldRay.z);
			float2 uv = i.uv.xy / i.uv.w;
			float depth = tex2D(_CameraDepthTexture, uv);
			depth = Linear01Depth(depth);
			float3 csPos = i.worldRay * depth;
			float3 wsPos = mul(unity_CameraToWorld, float4(csPos, 1)).xyz;

			float wsDepth = length(csPos);
			float spDepth = length(i.viewRay);
			i.viewRay /= spDepth;

			float rayIn = max(SphereIn(_WorldSpaceCameraPos, i.viewRay, i.worldPosition, _OuterRadius),0);
			float rayOut = min(spDepth, wsDepth);

			clip(rayOut - rayIn);

			return fixed4(SkyColor(_WorldSpaceCameraPos - i.worldPosition, i.viewRay, rayIn, rayOut), 1);
		}
		ENDCG
		}
	}
}
