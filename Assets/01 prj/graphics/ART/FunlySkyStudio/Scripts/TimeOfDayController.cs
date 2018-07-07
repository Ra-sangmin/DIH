using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Funly.SkyStudio
{
  // This controller manages time and updating the skybox material with the proper configuration
  // values for the current time of day. This loads sky data from your sky profile and sky timeline.
  public class TimeOfDayController : MonoBehaviour
  {
    // Get access to the most recently created TimeOfDayController.
    public static TimeOfDayController instance { get; private set; }

    [Tooltip("Sky profile defines the skyColors configuration for times of day. " +
      "This script will animate between those skyColors values based on the time of day.")]
    [SerializeField]
    private SkyProfile m_SkyProfile;
    public SkyProfile skyProfile
    {
      get { return m_SkyProfile; }
      set {
        m_SkyProfile = value;
        m_SkyMaterialController = null;
        UpdateSkyForCurrentTime();
      }
    }

    [Tooltip("Time is expressed in a fractional number of days that have completed.")]
    [SerializeField]
    private float m_SkyTime = 0;
    public float skyTime
    {
      get { return m_SkyTime; }
      set {
        m_SkyTime = Mathf.Abs(value);
        UpdateSkyForCurrentTime();
      }
    }

    [Tooltip("Automatically advance time at fixed speed.")]
    public bool automaticTimeIncrement;

    // Use the Sky Material controller to directly manipulate the skybox values programatically.
    private SkyMaterialController m_SkyMaterialController;
    public SkyMaterialController SkyMaterial { get { return m_SkyMaterialController; } }

    [Tooltip("Speed at which to advance time by if in automatic increment is enabled.")]
    [Range(0, 1)]
    public float automaticIncrementSpeed = .01f;

    [Tooltip("Sun orbit.")]
    public OrbitingBody sunOrbit;

    [Tooltip("Moon orbit.")]
    public OrbitingBody moonOrbit;

    [Tooltip("If true we'll invoke DynamicGI.UpdateEnvironment() when skybox changes. This is an expensive operation.")]
    public bool updateGlobalIllumination = false;

    // Callback invoked whenever the time of day changes.
    public delegate void TimeOfDayDidChange(TimeOfDayController tc, float timeOfDay);
    public event TimeOfDayDidChange timeChangedCallback;

    // Current progress value through a day cycle (value 0-1).
    public float timeOfDay
    {
      get { return m_SkyTime - ((int)m_SkyTime); }
    }

    public int daysElapsed
    {
      get { return (int)m_SkyTime; }
    }

    void Awake() {
      instance = this;
    }

    private void OnEnabled()
    {
      skyTime = m_SkyTime;
    }

    private void OnValidate()
    {
      if (gameObject.activeInHierarchy == false)
      {
        return;
      }
      skyTime = m_SkyTime;
      skyProfile = m_SkyProfile;
    }

    private void Update()
    {
      if (automaticTimeIncrement && Application.isPlaying)
      {
        skyTime += automaticIncrementSpeed * Time.deltaTime;
      }
    }

    public void UpdateGlobalIllumination()
    {
      DynamicGI.UpdateEnvironment();
    }

    private void SynchronizeAllShaderKeywords()
    {
      SynchronizedShaderKeyword(ShaderKeywords.Gradient);
      SynchronizedShaderKeyword(ShaderKeywords.Moon);
      SynchronizedShaderKeyword(ShaderKeywords.MoonCustomTexture);
      SynchronizedShaderKeyword(ShaderKeywords.MoonSpriteSheet);
      SynchronizedShaderKeyword(ShaderKeywords.MoonAlphaBlend);
      SynchronizedShaderKeyword(ShaderKeywords.MoonRotation);
      SynchronizedShaderKeyword(ShaderKeywords.Sun);
      SynchronizedShaderKeyword(ShaderKeywords.SunCustomTexture);
      SynchronizedShaderKeyword(ShaderKeywords.SunSpriteSheet);
      SynchronizedShaderKeyword(ShaderKeywords.SunAlphaBlend);
      SynchronizedShaderKeyword(ShaderKeywords.SunRotation);
      SynchronizedShaderKeyword(ShaderKeywords.Clouds);
      SynchronizedShaderKeyword(ShaderKeywords.Fog);
      SynchronizedShaderKeyword(ShaderKeywords.GlobalFog);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer1);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer2);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer3);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer1CustomTexture);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer2CustomTexture);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer3CustomTexture);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer1SpriteSheet);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer2SpriteSheet);
      SynchronizedShaderKeyword(ShaderKeywords.StarLayer3SpriteSheet);
    }

    private void SynchronizedShaderKeyword(string shaderKeyword)
    {
      if (skyProfile == null || skyProfile.skyboxMaterial == null)
      {
        return;
      }

      if (skyProfile.GetShaderKeywordValue(shaderKeyword))
      {
        if (!skyProfile.skyboxMaterial.IsKeywordEnabled(shaderKeyword))
        {
          skyProfile.skyboxMaterial.EnableKeyword(shaderKeyword);
        }
      }
      else
      {
        if (skyProfile.skyboxMaterial.IsKeywordEnabled(shaderKeyword))
        {
          skyProfile.skyboxMaterial.DisableKeyword(shaderKeyword);
        }
      }
    }
    public void UpdateSkyForCurrentTime()
    {
      if (skyProfile == null)
      {
        Debug.LogError("Your scene has a sky controller but no sky profile is assigned. " +
          "Create a sky profile using one of the supplied templates in the presets directory, " +
          "or create a new sky profile from 'Assets > Create > Sky Profile' and assign it to the sky controller.");
        return;
      }

      if (skyProfile.skyboxMaterial == null)
      {
        Debug.LogError("Your sky profile is missing a reference to the skybox material.");
        return;
      }

      if (m_SkyMaterialController == null) {
        m_SkyMaterialController = new SkyMaterialController();
      }

      m_SkyMaterialController.SkyboxMaterial = skyProfile.skyboxMaterial;

      if (RenderSettings.skybox == null ||
          RenderSettings.skybox.GetInstanceID() != skyProfile.skyboxMaterial.GetInstanceID())
      {
        RenderSettings.skybox = skyProfile.skyboxMaterial;
      }

      SynchronizeAllShaderKeywords();

      // Sky.
      m_SkyMaterialController.SkyColor = skyProfile.SkyUpperColor.ColorForTime(timeOfDay);
      m_SkyMaterialController.BackgroundCubemap = skyProfile.SkyCubemap.TextureForTime(timeOfDay) as Cubemap;
      m_SkyMaterialController.HorizonColor = skyProfile.SkyLowerColor.ColorForTime(timeOfDay);
      m_SkyMaterialController.SkyMiddleColor = skyProfile.SkyMiddleColor.ColorForTime(timeOfDay);
      m_SkyMaterialController.GradientFadeBegin = skyProfile.HorizonTransitionStart.NumericValueAtTime(timeOfDay);
      m_SkyMaterialController.GradientFadeLength = skyProfile.HorizonTransitionLength.NumericValueAtTime(timeOfDay);
      m_SkyMaterialController.SkyMiddlePosition = skyProfile.SkyMiddleColorPosition.NumericValueAtTime(timeOfDay);
      m_SkyMaterialController.StarFadeBegin = skyProfile.StarTransitionStart.NumericValueAtTime(timeOfDay);
      m_SkyMaterialController.StarFadeLength = skyProfile.StarTransitionLength.NumericValueAtTime(timeOfDay);
      m_SkyMaterialController.HorizonDistanceScale = skyProfile.StarDistanceScale.NumericValueAtTime(timeOfDay);

      // Clouds.
      if (skyProfile.GetShaderKeywordValue(ShaderKeywords.Clouds))
      {
        m_SkyMaterialController.CloudTexture = skyProfile.CloudNoiseTexture.TextureForTime(timeOfDay);
        m_SkyMaterialController.CloudTextureTiling = skyProfile.CloudTextureTiling.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.CloudDensity = skyProfile.CloudDensity.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.CloudSpeed = skyProfile.CloudSpeed.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.CloudDirection = skyProfile.CloudDirection.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.CloudHeight = skyProfile.CloudHeight.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.CloudColor1 = skyProfile.CloudColor1.ColorForTime(timeOfDay);
        m_SkyMaterialController.CloudColor2 = skyProfile.CloudColor2.ColorForTime(timeOfDay);
        m_SkyMaterialController.CloudFadePosition = skyProfile.CloudFadePosition.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.CloudFadeAmount = skyProfile.CloudFadeAmount.NumericValueAtTime(timeOfDay);
      }

      // Fog.
      if (skyProfile.GetShaderKeywordValue(ShaderKeywords.Fog))
      {
        m_SkyMaterialController.FogColor = skyProfile.FogColor.ColorForTime(timeOfDay);
        m_SkyMaterialController.FogDensity = skyProfile.FogDensity.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.FogHeight = skyProfile.FogHeight.NumericValueAtTime(timeOfDay);
      }

      // Sun.
      if (skyProfile.GetShaderKeywordValue(ShaderKeywords.Sun) && sunOrbit)
      {
        sunOrbit.spherePoint = skyProfile.SunPosition.SpherePointForTime(timeOfDay);

        m_SkyMaterialController.SunDirection = sunOrbit.BodyGlobalDirection;
        m_SkyMaterialController.SunColor = skyProfile.SunColor.ColorForTime(timeOfDay);
        m_SkyMaterialController.SunTexture = skyProfile.SunTexture.TextureForTime(timeOfDay);
        m_SkyMaterialController.SunSize = skyProfile.SunSize.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.SunEdgeFeathering = skyProfile.SunEdgeFeathering.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.SunBloomFilterBoost = skyProfile.SunBloomIntensity.NumericValueAtTime(timeOfDay);

        if (skyProfile.GetShaderKeywordValue(ShaderKeywords.SunRotation)) {
          m_SkyMaterialController.SunRotationSpeed = skyProfile.SunRotationSpeed.NumericValueAtTime(timeOfDay);
        }

        if (skyProfile.GetShaderKeywordValue(ShaderKeywords.SunSpriteSheet)) {
          m_SkyMaterialController.SetSunSpriteDimensions(
            (int)skyProfile.SunSpriteColumnCount.NumericValueAtTime(timeOfDay),
            (int)skyProfile.SunSpriteRowCount.NumericValueAtTime(timeOfDay));
          m_SkyMaterialController.SunSpriteItemCount = (int)skyProfile.SunSpriteItemCount.NumericValueAtTime(timeOfDay);
          m_SkyMaterialController.SunSpriteAnimationSpeed = skyProfile.SunSpriteAnimationSpeed.NumericValueAtTime(timeOfDay);
        }

        if (sunOrbit.BodyLight) {
          if (!sunOrbit.BodyLight.enabled)
          {
            sunOrbit.BodyLight.enabled = true;
          }
          RenderSettings.sun = sunOrbit.BodyLight;
          sunOrbit.BodyLight.color = skyProfile.SunLightColor.ColorForTime(timeOfDay);
          sunOrbit.BodyLight.intensity = skyProfile.SunLightIntensity.NumericValueAtTime(timeOfDay);
        }
      } else if (sunOrbit && sunOrbit.BodyLight)
      {
        sunOrbit.BodyLight.enabled = false;
      }
      
      // Moon.
      if (skyProfile.GetShaderKeywordValue(ShaderKeywords.Moon) && moonOrbit)
      {
        moonOrbit.spherePoint = skyProfile.MoonPosition.SpherePointForTime(timeOfDay);

        m_SkyMaterialController.MoonDirection = moonOrbit.BodyGlobalDirection;
        m_SkyMaterialController.MoonColor = skyProfile.MoonColor.ColorForTime(timeOfDay);
        m_SkyMaterialController.MoonTexture = skyProfile.MoonTexture.TextureForTime(timeOfDay);
        m_SkyMaterialController.MoonSize = skyProfile.MoonSize.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.MoonEdgeFeathering = skyProfile.MoonEdgeFeathering.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.MoonBloomFilterBoost = skyProfile.MoonBloomIntensity.NumericValueAtTime(timeOfDay);

        if (skyProfile.GetShaderKeywordValue(ShaderKeywords.MoonRotation)) {
          m_SkyMaterialController.MoonRotationSpeed = skyProfile.MoonRotationSpeed.NumericValueAtTime(timeOfDay);
        }

        if (skyProfile.GetShaderKeywordValue(ShaderKeywords.MoonSpriteSheet))
        {
          m_SkyMaterialController.SetMoonSpriteDimensions(
            (int)skyProfile.MoonSpriteColumnCount.NumericValueAtTime(timeOfDay),
            (int)skyProfile.MoonSpriteRowCount.NumericValueAtTime(timeOfDay));
          m_SkyMaterialController.MoonSpriteItemCount = (int) skyProfile.MoonSpriteItemCount.NumericValueAtTime(timeOfDay);
          m_SkyMaterialController.MoonSpriteAnimationSpeed = skyProfile.MoonSpriteAnimationSpeed.NumericValueAtTime(timeOfDay);
        }

        if (moonOrbit.BodyLight)
        {
          if (!moonOrbit.BodyLight.enabled)
          {
            moonOrbit.BodyLight.enabled = true;
          }
          moonOrbit.BodyLight.color = skyProfile.MoonLightColor.ColorForTime(timeOfDay);
          moonOrbit.BodyLight.intensity = skyProfile.MoonLightIntensity.NumericValueAtTime(timeOfDay);
        }
      } else if (moonOrbit && moonOrbit.BodyLight)
      {
        moonOrbit.BodyLight.enabled = false;
      }
      
      // Star Layer 1.
      if (skyProfile.GetShaderKeywordValue(ShaderKeywords.StarLayer1))
      {
        m_SkyMaterialController.StarLayer1Color = skyProfile.Star1Color.ColorForTime(timeOfDay);
        m_SkyMaterialController.StarLayer1MaxRadius = skyProfile.Star1Size.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer1Texture = skyProfile.Star1Texture.TextureForTime(timeOfDay);
        m_SkyMaterialController.StarLayer1TwinkleAmount = skyProfile.Star1TwinkleAmount.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer1TwinkleSpeed = skyProfile.Star1TwinkleSpeed.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer1RotationSpeed = skyProfile.Star1RotationSpeed.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer1EdgeFeathering = skyProfile.Star1EdgeFeathering.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer1BloomFilterBoost = skyProfile.Star1BloomIntensity.NumericValueAtTime(timeOfDay);

        if (skyProfile.GetShaderKeywordValue(ShaderKeywords.StarLayer1SpriteSheet))
        {
          m_SkyMaterialController.StarLayer1SpriteItemCount = (int)skyProfile.Star1SpriteItemCount.NumericValueAtTime(timeOfDay);
          m_SkyMaterialController.StarLayer1SpriteAnimationSpeed = (int) skyProfile.Star1SpriteAnimationSpeed.NumericValueAtTime(timeOfDay);
          m_SkyMaterialController.SetStarLayer1SpriteDimensions(
            (int)skyProfile.Star1SpriteColumnCount.NumericValueAtTime(timeOfDay),
            (int)skyProfile.Star1SpriteRowCount.NumericValueAtTime(timeOfDay));
        }
      }

      // Star Layer 2.
      if (skyProfile.GetShaderKeywordValue(ShaderKeywords.StarLayer2))
      {
        m_SkyMaterialController.StarLayer2Color = skyProfile.Star2Color.ColorForTime(timeOfDay);
        m_SkyMaterialController.StarLayer2MaxRadius = skyProfile.Star2Size.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer2Texture = skyProfile.Star2Texture.TextureForTime(timeOfDay);
        m_SkyMaterialController.StarLayer2TwinkleAmount = skyProfile.Star2TwinkleAmount.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer2TwinkleSpeed = skyProfile.Star2TwinkleSpeed.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer2RotationSpeed = skyProfile.Star2RotationSpeed.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer2EdgeFeathering = skyProfile.Star2EdgeFeathering.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer2BloomFilterBoost = skyProfile.Star2BloomIntensity.NumericValueAtTime(timeOfDay);

        if (skyProfile.GetShaderKeywordValue(ShaderKeywords.StarLayer2SpriteSheet)) {
          m_SkyMaterialController.StarLayer2SpriteItemCount = (int)skyProfile.Star2SpriteItemCount.NumericValueAtTime(timeOfDay);
          m_SkyMaterialController.StarLayer2SpriteAnimationSpeed = (int)skyProfile.Star2SpriteAnimationSpeed.NumericValueAtTime(timeOfDay);
          m_SkyMaterialController.SetStarLayer2SpriteDimensions(
            (int)skyProfile.Star2SpriteColumnCount.NumericValueAtTime(timeOfDay),
            (int)skyProfile.Star2SpriteRowCount.NumericValueAtTime(timeOfDay));
        }
      }

      // Star Layer 3.
      if (skyProfile.GetShaderKeywordValue(ShaderKeywords.StarLayer3))
      {
        m_SkyMaterialController.StarLayer3Color = skyProfile.Star3Color.ColorForTime(timeOfDay);
        m_SkyMaterialController.StarLayer3MaxRadius = skyProfile.Star3Size.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer3Texture = skyProfile.Star3Texture.TextureForTime(timeOfDay);
        m_SkyMaterialController.StarLayer3TwinkleAmount = skyProfile.Star3TwinkleAmount.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer3TwinkleSpeed = skyProfile.Star3TwinkleSpeed.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer3RotationSpeed = skyProfile.Star3RotationSpeed.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer3EdgeFeathering = skyProfile.Star3EdgeFeathering.NumericValueAtTime(timeOfDay);
        m_SkyMaterialController.StarLayer3BloomFilterBoost = skyProfile.Star3BloomIntensity.NumericValueAtTime(timeOfDay);

        if (skyProfile.GetShaderKeywordValue(ShaderKeywords.StarLayer3SpriteSheet)) {
          m_SkyMaterialController.StarLayer3SpriteItemCount = (int)skyProfile.Star3SpriteItemCount.NumericValueAtTime(timeOfDay);
          m_SkyMaterialController.StarLayer3SpriteAnimationSpeed = (int)skyProfile.Star3SpriteAnimationSpeed.NumericValueAtTime(timeOfDay);
          m_SkyMaterialController.SetStarLayer3SpriteDimensions(
            (int)skyProfile.Star3SpriteColumnCount.NumericValueAtTime(timeOfDay),
            (int)skyProfile.Star3SpriteRowCount.NumericValueAtTime(timeOfDay));
        }
      }

      if (updateGlobalIllumination)
      {
        UpdateGlobalIllumination();
      }

      // Notify delegate after we've completed the sky modifications.
      if (timeChangedCallback != null) {
        timeChangedCallback(this, timeOfDay);
      }
    }
  }
}
