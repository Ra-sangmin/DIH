using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Keys for accessing timeline keyframe groups.
  public abstract class ProfilePropertyKeys
  {
    // Sky.
    public const string SkyCubemapKey = "SkyCubemapKey";
    public const string SkyUpperColorKey = "SkyUpperColorKey";
    public const string SkyMiddleColorKey = "SkyMiddleColorKey";
    public const string SkyLowerColorKey = "SkyLowerColorKey";
    public const string SkyMiddleColorPosition = "SkyMiddleColorPosition";
    public const string HorizonTrasitionStartKey = "HorizonTransitionStartKey";
    public const string HorizonTransitionLengthKey = "HorizonTransitionLengthKey";
    public const string HorizonStarScaleKey = "HorizonStarScaleKey";
    public const string StarTransitionStartKey = "StarTransitionStartKey";
    public const string StarTransitionLengthKey = "StarTransitionLengthKey";

    // Sun.
    public const string SunColorKey = "SunColorKey";
    public const string SunTextureKey = "SunTextureKey";
    public const string SunSizeKey = "SunSizeKey";
    public const string SunRotationSpeedKey = "SunRotationSpeedKey";
    public const string SunEdgeFeatheringKey = "SunEdgeFeatheringKey";
    public const string SunColorIntensityKey = "SunColorIntensityKey";
    public const string SunLightColorKey = "SunLightColorKey";
    public const string SunLightIntensityKey = "SunLightIntensityKey";
    public const string SunPositionKey = "SunPositionKey";
    public const string SunSpriteRowCount = "SunSpriteRowCountKey";
    public const string SunSpriteColumnCount = "SunSpriteColumnCountKey";
    public const string SunSpriteItemCount = "SunSpriteItemCount";
    public const string SunSpriteAnimationSpeed = "SunSpriteAnimationSpeed";

    // Moon.
    public const string MoonColorKey = "MoonColorKey";
    public const string MoonTextureKey = "MoonTextureKey";
    public const string MoonSizeKey = "MoonSizeKey";
    public const string MoonRotationSpeedKey = "MoonRotationSpeedKey";
    public const string MoonEdgeFeatheringKey = "MoonEdgeFeatheringKey";
    public const string MoonColorIntensityKey = "MoonColorIntensityKey";
    public const string MoonLightColorKey = "MoonLightColorKey";
    public const string MoonLightIntensityKey = "MoonLightIntensityKey";
    public const string MoonPositionKey = "MoonPositionKey";
    public const string MoonOrbitSpeed = "MoonOrbitSpeed";
    public const string MoonSpriteRowCount = "MoonSpriteRowCountKey";
    public const string MoonSpriteColumnCount = "MoonSpriteColumnCountKey";
    public const string MoonSpriteItemCount = "MoonSpriteItemCount";
    public const string MoonSpriteAnimationSpeed = "MoonSpriteAnimationSpeed";

    // Star layer 1.
    public const string Star1SizeKey = "Star1SizeKey";
    public const string Star1DensityKey = "Star1DensityKey";
    public const string Star1TextureKey = "Star1TextureKey";
    public const string Star1ColorKey = "Star1ColorKey";
    public const string Star1TwinkleAmountKey = "Star1TwinkleAmountKey";
    public const string Star1TwinkleSpeedKey = "Star1TwinkleSpeedKey";
    public const string Star1RotationSpeedKey = "Star1RotationSpeed";
    public const string Star1EdgeFeatheringKey = "Star1EdgeFeathering";
    public const string Star1ColorIntensityKey = "Star1ColorIntensityKey";
    public const string Star1SpriteRowCount = "Star1SpriteRowCountKey";
    public const string Star1SpriteColumnCount = "Star1SpriteColumnCountKey";
    public const string Star1SpriteItemCount = "Star1SpriteItemCount";
    public const string Star1SpriteAnimationSpeed = "Star1SpriteAnimationSpeed";

    // Star layer 2.
    public const string Star2SizeKey = "Star2SizeKey";
    public const string Star2DensityKey = "Star2DensityKey";
    public const string Star2TextureKey = "Star2TextureKey";
    public const string Star2ColorKey = "Star2ColorKey";
    public const string Star2TwinkleAmountKey = "Star2TwinkleAmountKey";
    public const string Star2TwinkleSpeedKey = "Star2TwinkleSpeedKey";
    public const string Star2RotationSpeedKey = "Star2RotationSpeed";
    public const string Star2EdgeFeatheringKey = "Star2EdgeFeathering";
    public const string Star2ColorIntensityKey = "Star2ColorIntensityKey";
    public const string Star2SpriteRowCount = "Star2SpriteRowCountKey";
    public const string Star2SpriteColumnCount = "Star2SpriteColumnCountKey";
    public const string Star2SpriteItemCount = "Star2SpriteItemCount";
    public const string Star2SpriteAnimationSpeed = "Star2SpriteAnimationSpeed";

    // Star layer 3.
    public const string Star3SizeKey = "Star3SizeKey";
    public const string Star3DensityKey = "Star3DensityKey";
    public const string Star3TextureKey = "Star3TextureKey";
    public const string Star3ColorKey = "Star3ColorKey";
    public const string Star3TwinkleAmountKey = "Star3TwinkleAmountKey";
    public const string Star3TwinkleSpeedKey = "Star3TwinkleSpeedKey";
    public const string Star3RotationSpeedKey = "Star3RotationSpeed";
    public const string Star3EdgeFeatheringKey = "Star3EdgeFeathering";
    public const string Star3ColorIntensityKey = "Star3ColorIntensityKey";
    public const string Star3SpriteRowCount = "Star3SpriteRowCountKey";
    public const string Star3SpriteColumnCount = "Star3SpriteColumnCountKey";
    public const string Star3SpriteItemCount = "Star3SpriteItemCount";
    public const string Star3SpriteAnimationSpeed = "Star3SpriteAnimationSpeed";

    // Clouds.
    public const string CloudNoiseTextureKey = "CloudNoiseTextureKey";
    public const string CloudDensityKey = "CloudDensityKey";
    public const string CloudSpeedKey = "CloudSpeedKey";
    public const string CloudDirectionKey = "CloudDirectionKey";
    public const string CloudHeight = "CloudHeightKey";
    public const string CloudColor1Key = "CloudColor1Key";
    public const string CloudColor2Key = "CloudColor2Key";
    public const string CloudFadePositionKey = "CloudFadePositionKey";
    public const string CloudFadeAmountKey = "CloudFadeAmountKey";
    public const string CloudTextureTiling = "CloudTextureTiling";

    // Fog.
    public const string FogDensityKey = "FogDensityKey";
    public const string FogColorKey = "FogColorKey";
    public const string FogLengthKey = "FogLengthKey";


    public static HashSet<string> GetPropertyKeysSet()
    {
      FieldInfo[] fields = typeof(ProfilePropertyKeys).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      HashSet<string> validPropertyKeys = new HashSet<string>();

      foreach (FieldInfo field in fields) {
        if (!field.IsLiteral) {
          continue;
        }

        string propKey = field.GetValue(null) as string;
        validPropertyKeys.Add(propKey);
      }

      return validPropertyKeys;
    }
  }
}
