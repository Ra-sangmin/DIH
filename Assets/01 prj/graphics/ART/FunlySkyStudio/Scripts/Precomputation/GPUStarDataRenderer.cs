using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public class GPUStarDataRenderer : BaseStarDataRenderer
  {
    private const int k_MaxStars = 2000;

    public override IEnumerator ComputeStarData()
    {
      SendProgress(0);

      RenderTexture rt = RenderTexture.GetTemporary(
        (int)imageSize,
        (int)imageSize,
        0,
        RenderTextureFormat.ARGB32,
        RenderTextureReadWrite.Linear);

      rt.filterMode = FilterMode.Point;
      rt.wrapMode = TextureWrapMode.Clamp;
      rt.name = layerId;

      RenderTexture previousRenderTexture = RenderTexture.active;

      Material mat = new Material(new Material(Shader.Find("Hidden/Funly/Sky Studio/Computation/Stars"))) {
        hideFlags = HideFlags.HideAndDontSave
      };

      Vector4 randomSeed = new Vector4(
        Random.Range(0.0f, 1.0f),
        Random.Range(0.0f, 1.0f),
        Random.Range(0.0f, 1.0f),
        Random.Range(0.0f, 1.0f));
      
      int numStars = Mathf.FloorToInt(Mathf.Clamp01(density) * k_MaxStars);

      mat.SetFloat("_StarDensity", density);
      mat.SetFloat("_ImageWidth", imageSize);
      mat.SetFloat("_ImageHeight", imageSize);
      mat.SetFloat("_NumStarPoints", numStars);
      mat.SetVector("_RandomSeed", randomSeed);
      
      Graphics.Blit(null, rt, mat);

      Texture2D tex = ConvertToTexture2D(rt);

      RenderTexture.active = previousRenderTexture;
      rt.Release();

      SendCompletion(tex, true);

      yield break;
    }

    private Texture2D ConvertToTexture2D(RenderTexture rt)
    {
      Texture2D tex = new Texture2D((int)imageSize, (int)imageSize, TextureFormat.RGBA32, false);
      tex.name = layerId;
      tex.filterMode = FilterMode.Point;
      tex.wrapMode = TextureWrapMode.Clamp;
      tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
      tex.Apply(false);

      return tex;
    }
     
    StarPoint NearestStarPoint(Vector3 spot, List<StarPoint> starPoints)
    {
      StarPoint nearbyPoint = new StarPoint(Vector3.zero, 0, 0, 0);

      if (starPoints == null) {
        return nearbyPoint;
      }

      float nearbyDistance = -1.0f;

      for (int i = 0; i < starPoints.Count; i++) {
        StarPoint starPoint = starPoints[i];
        float distance = Vector3.Distance(spot, starPoint.position);
        if (nearbyDistance == -1.0f || distance < nearbyDistance) {
          nearbyPoint = starPoint;
          nearbyDistance = distance;
        }
      }

      return nearbyPoint;
    }
  }
}

