// Sky Studio
// Author: Jason Ederle

Shader "Hidden/Funly/Sky Studio/Computation/Stars"
{
  Properties
  {
  }
  SubShader
  {
    Tags{"RenderType" = "Opaque"} LOD 100

        Pass
    {
      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "SkyMathUtilities.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      float _StarDensity;
      float _ImageWidth;
      float _ImageHeight;
      int _NumStarPoints;
      float4 _RandomSeed;

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;

        return o;
      }

      float RangedRandom(float2 randomSeed, float minValue, float maxValue) {
        float dist = maxValue - minValue;
        float percent = rand(randomSeed);

        return minValue + (dist * percent);
      }

      float3 RandomUnitSpherePoint(float2 randomSeed)
      {
        float z = RangedRandom(randomSeed * .81239f, -1.0f, 1.0f);
        float a = RangedRandom(randomSeed * .12303f, 0.0f, UNITY_TWO_PI);

        float r = sqrt(1.0f - z * z);

        float x = r * cos(a);
        float y = r * sin(a);

        return normalize(float3(x, y, z));
      }
      
      // Returns normalized point on sphere, with w noise.
      float4 GenerateNextStarPoint(float i, float2 seed)
      {
        float noise = RangedRandom(seed * i * .98347f, .2f, .8f);

        float3 randomPoint = RandomUnitSpherePoint(seed * i);

        return float4(randomPoint.x, randomPoint.y, randomPoint.z, noise);
      }

      // Get position of the closest star point.
      float4 GetClosestStarPoint(float2 uv)
      {
        float4 closestStarPoint = float4(0, 0, 0, 0);
        float2 fragSphericalCoord = ConvertUVToSphericalCoordinate(uv);
        float3 fragPoint = SphericalCoordinateToDirection(fragSphericalCoord);
        float shortestDistance = 0;

        for (int i = 0; i < _NumStarPoints; i++)
        {
          float4 randomStarPoint = GenerateNextStarPoint(i + 1, _RandomSeed.xyz);
          float currentStarDistance = distance(randomStarPoint.xyz, fragPoint);
          if (i == 0 || currentStarDistance < shortestDistance)
          {
            closestStarPoint = randomStarPoint;
            shortestDistance = currentStarDistance;
          }
        }

        return closestStarPoint;
      }

      float4 frag(v2f i) : SV_Target
      {
        float4 starPosition = GetClosestStarPoint(i.uv);
        float2 sphericalCoord = DirectionToSphericalCoordinate(starPosition);
        float2 percents = ConvertSphericalCoordinateToPercentage(sphericalCoord);

        float4 starData = float4(
            percents.x,       // Azimuth rotation percent.
            percents.y,       // Altitude rotation percent.
            starPosition.w,   // Noise.
            1.0f);            // Unused.

        return starData;
      }
      ENDCG
    }
  }
}
