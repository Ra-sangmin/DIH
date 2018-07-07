using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public class Standard3dShaderDefinition : BaseShaderDefinition
  {
    public const float MaxStarSize = .2f;
    public const float MaxStarDensity = 1.0f;
    public const float MinEdgeFeathering = 0.0001f;
    public const float MinStarFadeBegin = -.999f;
    public const float MaxSpriteItems = 100000;
    public const float MinRotationSpeed = -10.0f;
    public const float MaxRotationSpeed = 10.0f;

    public Standard3dShaderDefinition()
    {
      shaderName = "Funly/Sky Studio/Skybox/3D Standard";
    }

    protected override ProfileKeywordSection[] ProfileShaderKeywords()
    {
      return new[]
      {
        new ProfileKeywordSection("Features", ProfileSectionKeys.FeaturesSectionKey, new []
        {
          new ProfileKeywordDefinition(ShaderKeywords.Gradient, true, "Gradient Background",
            "Enables gradient background feature in shader as an alternative to a cubemap background."),
          new ProfileKeywordDefinition(ShaderKeywords.Sun, false, "Sun",
            "Enables sun feature in skybox shader."),
          new ProfileKeywordDefinition(ShaderKeywords.Moon, false, "Moon",
            "Enables moon feature in skybox shader."),
          new ProfileKeywordDefinition(ShaderKeywords.Clouds, false, "Clouds",
            "Enables cloud feature in the skybox shader."),
          new ProfileKeywordDefinition(ShaderKeywords.Fog, false, "Fog",
            "Enables fog feature in the skybox shader."),
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer1, false, "Star Layer 1",
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer2, false, "Star Layer 2",
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer3, false, "Star Layer 3",
            "Enables a layer of stars in the shader. Use less star layers for better performance.")
        }),
        new ProfileKeywordSection("Sun", ProfileSectionKeys.SunSectionKey, new []
        {
          new ProfileKeywordDefinition(ShaderKeywords.SunCustomTexture, false, "Use Custom Texture",
            "Enables a custom texture to be used for the sun."),
          new ProfileKeywordDefinition(ShaderKeywords.SunSpriteSheet, false, "Texture Is Sprite Sheet Animation", ShaderKeywords.SunCustomTexture, true,
            "If enabled the sun texture will be used as a sprite sheet animation."),
          new ProfileKeywordDefinition(ShaderKeywords.SunAlphaBlend, false, "Use Alpha Blending", ShaderKeywords.SunCustomTexture, true,
            "Enables alpha blending of the sun texture into the background. If disabled additive blending will be used."),
          new ProfileKeywordDefinition(ShaderKeywords.SunRotation, false, "Animate Sun Rotation", ShaderKeywords.SunCustomTexture, true,
            "If enabled the sun texture will rotate using the rotation speed property"),
        }),
        new ProfileKeywordSection("Moon", ProfileSectionKeys.MoonSectionKey, new []
        {
          new ProfileKeywordDefinition(ShaderKeywords.MoonCustomTexture, false, "Use Custom Texture",
            "Enables a custom texture to be used for the moon."),
          new ProfileKeywordDefinition(ShaderKeywords.MoonSpriteSheet, false, "Texture Is Sprite Sheet Animation", ShaderKeywords.MoonCustomTexture, true,
            "If enabled the moon texture will be used as a sprite sheet animation."),
          new ProfileKeywordDefinition(ShaderKeywords.MoonAlphaBlend, false, "Use Alpha Blending", ShaderKeywords.MoonCustomTexture, true,
            "Enables alpha blending of the moon texture into the background. If disabled additive blending will be used."),
          new ProfileKeywordDefinition(ShaderKeywords.MoonRotation, false, "Animate Moon Rotation", ShaderKeywords.MoonCustomTexture, true,
            "If enabled the moon texture will rotate using the rotation speed property"),
        }),
        new ProfileKeywordSection("Global Fog", ProfileSectionKeys.FogSectionKey, new []
        {
            new ProfileKeywordDefinition(ShaderKeywords.GlobalFog, false, "Use Global Fog", 
            "If true, and global fog is enabled in lighting settings, this will render fog into the skybox. " +
            "This typically is only useful for really dark stormy scenes, since it can make the skybox very dark."), 
        }), 
        new ProfileKeywordSection("Star Layer 1", ProfileSectionKeys.Star1SectionKey, new []
        {
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer1CustomTexture, false, "Use Custom Texture",
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer1SpriteSheet, false, "Texture Is Sprite Sheet Animation", ShaderKeywords.StarLayer1CustomTexture, true,
            "If enabled star texture will be used as a sprite sheet animation."),
        }),
        new ProfileKeywordSection("Star Layer 2", ProfileSectionKeys.Star2SectionKey, new []
        {
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer2CustomTexture, false, "Use Custom Texture",
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer2SpriteSheet, false, "Texture Is Sprite Sheet Animation", ShaderKeywords.StarLayer2CustomTexture, true,
            "If enabled star texture will be used as a sprite sheet animation."),
        }),
        new ProfileKeywordSection("Star Layer 3", ProfileSectionKeys.Star3SectionKey, new []
        {
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer3CustomTexture, false, "Use Custom Texture",
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          new ProfileKeywordDefinition(ShaderKeywords.StarLayer3SpriteSheet, false, "Texture Is Sprite Sheet Animation", ShaderKeywords.StarLayer3CustomTexture, true,
            "If enabled star texture will be used as a sprite sheet animation."),
        }),
      };
    }

    // Override this to return a different set of shader options.
    protected override ProfileGroupSection[] ProfileDefinitionTable()
    {
      return new[] {
        // Sky Section.
        new ProfileGroupSection("Sky", ProfileSectionKeys.SkySectionKey, "SkySectionIcon", null, false, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Sky Cubemap", ProfilePropertyKeys.SkyCubemapKey, null, ProfileGroupDefinition.RebuildType.None, ShaderKeywords.Gradient, false,
            "Image used as background for the skybox."),

          ProfileGroupDefinition.ColorGroupDefinition(
            "Sky Upper Color", ProfilePropertyKeys.SkyUpperColorKey, ColorHelper.ColorWithHex(0x2C2260),
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.Gradient, true,
            "Top color of the sky when using a gradient background."),

          ProfileGroupDefinition.ColorGroupDefinition(
            "Sky Middle Color", ProfilePropertyKeys.SkyMiddleColorKey, Color.white,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.Gradient, true,
            "Middle color of the sky when using the gradient background."),

          ProfileGroupDefinition.ColorGroupDefinition(
            "Sky Lower Color", ProfilePropertyKeys.SkyLowerColorKey, ColorHelper.ColorWithHex(0xE3C882),
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.Gradient, true,
            "Bottom color of the sky when using a gradient background."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Sky Middle Color Balance", ProfilePropertyKeys.SkyMiddleColorPosition, 0, 1, .5f,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.Gradient, true,
            "Shift the middle color closer to lower color or closer upper color to alter the gradient balance."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Horizon Position", ProfilePropertyKeys.HorizonTrasitionStartKey, -1, 1, -.3f,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.Gradient, true,
            "This vertical position controls where the gradient background will begin."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Sky Gradient Length", ProfilePropertyKeys.HorizonTransitionLengthKey, 0, 2, 1,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.Gradient, true,
            "The length of the background gradient fade from the bottom color to the top color."),

          ProfileGroupDefinition.NumberGroupDefinition("Star Start", ProfilePropertyKeys.StarTransitionStartKey, -1, 1, .2f,
            "Vertical position where the stars will begin fading in from. Typically this is just above the horizon."),

          ProfileGroupDefinition.NumberGroupDefinition("Star Transition Length", ProfilePropertyKeys.StarTransitionLengthKey, 0, 2, .5f,
            "The length of the fade-in where stars go from invisible to visible."),

          ProfileGroupDefinition.NumberGroupDefinition("Star Distance Scale", ProfilePropertyKeys.HorizonStarScaleKey, .01f, 1, .7f,
            "Scale value applied to stars closers to the horizon for distance effect."),
        }),

        // Clouds.
        new ProfileGroupSection("Clouds", ProfileSectionKeys.CloudSectionKey, "CloudSectionIcon", ShaderKeywords.Clouds, true, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Cloud Noise Texture", ProfilePropertyKeys.CloudNoiseTextureKey, null,
            "Noise texture that's used for generating dynamic clouds."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Density", ProfilePropertyKeys.CloudDensityKey, 0, 1, .25f,
            "Density controls the amount of clouds in the scene."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Texture Tiling", ProfilePropertyKeys.CloudTextureTiling, .1f, 20.0f, .55f,
            "Tiling changes the scale of the texture and how many times it will repeat in the sky. A higher number will increase visible resolution."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Speed", ProfilePropertyKeys.CloudSpeedKey, 0, 1, .1f,
            "Speed that the clouds move at as a percent from 0 to 1."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Direction", ProfilePropertyKeys.CloudDirectionKey, 0, Mathf.PI * 2, 1,
            "Direction that the clouds move in as an angle in radians between 0 and 2PI."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Height", ProfilePropertyKeys.CloudHeight, 0, 1, .7f,
            "Height (or altitude) of the clouds in the scene."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Color 1", ProfilePropertyKeys.CloudColor1Key, Color.white,
            "Primary color of the cloud features."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Color 2", ProfilePropertyKeys.CloudColor2Key, Color.gray,
            "Secondary color of the clouds between the primary features."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Fade-Out Distance", ProfilePropertyKeys.CloudFadePositionKey, 0, 1, .7f,
            "Distance at which the clouds will begin to fade away towards the horizon."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Fade-Out Amount", ProfilePropertyKeys.CloudFadeAmountKey, 0, 1, .75f,
            "This is the amount of fade out that will happen to clouds as they reach the horizon."),
        }),

        // Fog.
        new ProfileGroupSection("Fog", ProfileSectionKeys.FogSectionKey, "FogSectionIcon", ShaderKeywords.Fog, true, new []
        {
          ProfileGroupDefinition.ColorGroupDefinition("Fog Color", ProfilePropertyKeys.FogColorKey, Color.white,
            "Color of the fog at the horizon."),

          ProfileGroupDefinition.NumberGroupDefinition("Fog Density", ProfilePropertyKeys.FogDensityKey, 0, 1, .12f,
            "Density, or thickness, of the fog to display at the horizon."),

          ProfileGroupDefinition.NumberGroupDefinition("Fog Height", ProfilePropertyKeys.FogLengthKey, .03f, 1, .1f,
            "The height of the fog as it extends from the horizon upwards into the sky"),
        }), 

        // Sun section.
        new ProfileGroupSection("Sun", ProfileSectionKeys.SunSectionKey, "SunSectionIcon", ShaderKeywords.Sun, true, new[]
        {
          ProfileGroupDefinition.ColorGroupDefinition("Sun Color", ProfilePropertyKeys.SunColorKey, ColorHelper.ColorWithHex(0xFFE000),
            "Color of the sun."),

          ProfileGroupDefinition.TextureGroupDefinition("Sun Texture", ProfilePropertyKeys.SunTextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.SunCustomTexture, true,
            "Texture used for the sun."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Sprite Columns", ProfilePropertyKeys.SunSpriteColumnCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.SunSpriteSheet, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Sprite Rows", ProfilePropertyKeys.SunSpriteRowCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.SunSpriteSheet, true,
            "Number of rows in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Sprite Item Count", ProfilePropertyKeys.SunSpriteItemCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.SunSpriteSheet, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Sprite Animation Speed", ProfilePropertyKeys.SunSpriteAnimationSpeed,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.SunSpriteSheet, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Rotation Speed", ProfilePropertyKeys.SunRotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 1,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.SunRotation, true,
            "Speed value for sun texture rotation animation."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Size", ProfilePropertyKeys.SunSizeKey, 0, 1, .1f,
            "Size of the sun."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Edge Feathering", ProfilePropertyKeys.SunEdgeFeatheringKey, MinEdgeFeathering, 1, .8f,
            "Percent amount of gradient fade-in from the sun edges to it's center point."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Bloom Intensity", ProfilePropertyKeys.SunColorIntensityKey, 1, 10, 1,
            "Value that's multiplied against the suns color to intensify bloom effects."),

          ProfileGroupDefinition.ColorGroupDefinition("Sun Light Color", ProfilePropertyKeys.SunLightColorKey, Color.white,
            "Color of the directional light coming from the sun."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Light Intensity", ProfilePropertyKeys.SunLightIntensityKey, 0, 5, 1,
            "Intensity of the directional light coming from the sun."),

          ProfileGroupDefinition.SpherePointGroupDefinition("Sun Position", ProfilePropertyKeys.SunPositionKey, 0, 0,
            "Position of the sun in the skybox expressed as a horizontal and vertical rotation.")
        }),

        // Moon section.
        new ProfileGroupSection("Moon", ProfileSectionKeys.MoonSectionKey, "MoonSectionIcon", ShaderKeywords.Moon, true, new[]
        {
          ProfileGroupDefinition.ColorGroupDefinition("Moon Color", ProfilePropertyKeys.MoonColorKey, ColorHelper.ColorWithHex(0x989898),
            "Color of the moon."),

          ProfileGroupDefinition.TextureGroupDefinition("Moon Texture", ProfilePropertyKeys.MoonTextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.MoonCustomTexture, true,
            "Texture used for the moon"),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Sprite Columns", ProfilePropertyKeys.MoonSpriteColumnCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.MoonSpriteSheet, true,
            "Number of columns in the moon sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Sprite Rows", ProfilePropertyKeys.MoonSpriteRowCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.MoonSpriteSheet, true,
            "Number of rows in the moon sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Sprite Item Count", ProfilePropertyKeys.MoonSpriteItemCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.MoonSpriteSheet, true,
            "Number of columns in the moon sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Sprite Animation Speed", ProfilePropertyKeys.MoonSpriteAnimationSpeed,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.MoonSpriteSheet, true,
            "Frames per second to flip through the sprite images."), 

          ProfileGroupDefinition.NumberGroupDefinition("Moon Rotation Speed", ProfilePropertyKeys.MoonRotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 1,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.MoonRotation, true,
            "Speed value for moon texture rotation animation."),
          
          ProfileGroupDefinition.NumberGroupDefinition("Moon Size", ProfilePropertyKeys.MoonSizeKey, 0, 1, .08f,
            "Size of the moon."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Edge Feathering", ProfilePropertyKeys.MoonEdgeFeatheringKey, MinEdgeFeathering, 1, .1f,
            "Percentage of fade-in from edge to the center of the moon."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Bloom Intensity", ProfilePropertyKeys.MoonColorIntensityKey, 1, 10, 1,
            "Value multiplied with the moon color to help intensify bloom filters."),

          ProfileGroupDefinition.ColorGroupDefinition("Moon Light Color", ProfilePropertyKeys.MoonLightColorKey, Color.white,
            "Color of the directional light coming from the moon."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Light Intensity", ProfilePropertyKeys.MoonLightIntensityKey, 0, 5, 1,
            "Intensity of the directional light coming from the moon."),

          ProfileGroupDefinition.SpherePointGroupDefinition("Moon Position", ProfilePropertyKeys.MoonPositionKey, 0, 0,
            "Position of the moon in the skybox expressed as a horizontal and vertical rotation.")
        }),

        // Star 1 section.
        new ProfileGroupSection("Star Layer 1", ProfileSectionKeys.Star1SectionKey, "StarSectionIcon", ShaderKeywords.StarLayer1, true, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Star 1 Texture", ProfilePropertyKeys.Star1TextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.StarLayer1CustomTexture, true,
            "Texture used for the star image."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Sprite Columns", ProfilePropertyKeys.Star1SpriteColumnCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer1SpriteSheet, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Sprite Rows", ProfilePropertyKeys.Star1SpriteRowCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer1SpriteSheet, true,
            "Number of rows in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Sprite Item Count", ProfilePropertyKeys.Star1SpriteItemCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer1SpriteSheet, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Sprite Animation Speed", ProfilePropertyKeys.Star1SpriteAnimationSpeed,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer1SpriteSheet, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Star 1 Rotation Speed", ProfilePropertyKeys.Star1RotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 0,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.StarLayer1CustomTexture, true,
            "Speed the star rotates at."),
          
          ProfileGroupDefinition.ColorGroupDefinition("Star 1 Color", ProfilePropertyKeys.Star1ColorKey, Color.white,
            "Color of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Size", ProfilePropertyKeys.Star1SizeKey, 0, MaxStarSize, .005f,
            "Size of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Density", ProfilePropertyKeys.Star1DensityKey, 0, MaxStarDensity, .02f,
            ProfileGroupDefinition.RebuildType.Stars, null, false,
            "Density of the stars in this layer."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Twinkle Amount", ProfilePropertyKeys.Star1TwinkleAmountKey, 0, 1, .7f,
            "Percentage amount of twinkle animation."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Twinkle Speed", ProfilePropertyKeys.Star1TwinkleSpeedKey, 0, 10, 5,
            "Speed at which the star twinkles at."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Edge Feathering", ProfilePropertyKeys.Star1EdgeFeatheringKey, 0.0001f, 1, .4f,
            "Percentage of fade-in from the stars edges to the center."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Bloom Intensity", ProfilePropertyKeys.Star1ColorIntensityKey, 1, 10, 1,
            "Value multiplied with the star color to intensify bloom filters."),
        }),

        // Star 2 section.
        new ProfileGroupSection("Star Layer 2", ProfileSectionKeys.Star2SectionKey, "StarSectionIcon", ShaderKeywords.StarLayer2, true, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Star 2 Texture", ProfilePropertyKeys.Star2TextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.StarLayer2CustomTexture, true,
            "Texture used for the star image."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Sprite Columns", ProfilePropertyKeys.Star2SpriteColumnCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer2SpriteSheet, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Sprite Rows", ProfilePropertyKeys.Star2SpriteRowCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer2SpriteSheet, true,
            "Number of rows in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Sprite Item Count", ProfilePropertyKeys.Star2SpriteItemCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer2SpriteSheet, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Sprite Animation Speed", ProfilePropertyKeys.Star2SpriteAnimationSpeed,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer2SpriteSheet, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Star 2 Rotation Speed", ProfilePropertyKeys.Star2RotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 0,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.StarLayer2CustomTexture, true,
            "Speed the star rotates at."),
          
          ProfileGroupDefinition.ColorGroupDefinition("Star 2 Color", ProfilePropertyKeys.Star2ColorKey, Color.white,
            "Color of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Size", ProfilePropertyKeys.Star2SizeKey, 0, MaxStarSize, .005f,
            "Size of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Density", ProfilePropertyKeys.Star2DensityKey, 0, MaxStarDensity, .01f,
            ProfileGroupDefinition.RebuildType.Stars, null, false,
            "Density of the stars in this layer."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Twinkle Amount", ProfilePropertyKeys.Star2TwinkleAmountKey, 0, 1, .7f,
            "Texture used for the star image."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Twinkle Speed", ProfilePropertyKeys.Star2TwinkleSpeedKey, 0, 10, 5,
            "Speed at which the star twinkles at."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Edge Feathering", ProfilePropertyKeys.Star2EdgeFeatheringKey, MinEdgeFeathering, 1, .4f,
            "Percentage of fade-in from the stars edges to the center."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Bloom Intensity", ProfilePropertyKeys.Star2ColorIntensityKey, 1, 10, 1,
            "Value multiplied with the star color to intensify bloom filters."),
        }),

        // Star 3 section.
        new ProfileGroupSection("Star Layer 3", ProfileSectionKeys.Star3SectionKey, "StarSectionIcon", ShaderKeywords.StarLayer3, true, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Star 3 Texture", ProfilePropertyKeys.Star3TextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.StarLayer3CustomTexture, true,
            "Texture used for the star image."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Sprite Columns", ProfilePropertyKeys.Star3SpriteColumnCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer3SpriteSheet, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Sprite Rows", ProfilePropertyKeys.Star3SpriteRowCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer3SpriteSheet, true,
            "Number of rows in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Sprite Item Count", ProfilePropertyKeys.Star3SpriteItemCount,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer3SpriteSheet, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Sprite Animation Speed", ProfilePropertyKeys.Star3SpriteAnimationSpeed,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ShaderKeywords.StarLayer3SpriteSheet, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Star 3 Rotation Speed", ProfilePropertyKeys.Star3RotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 0,
            ProfileGroupDefinition.RebuildType.None, ShaderKeywords.StarLayer3CustomTexture, true,
            "Speed the star rotates at."),
          
          ProfileGroupDefinition.ColorGroupDefinition("Star 3 Color", ProfilePropertyKeys.Star3ColorKey, Color.white,
            "Color of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Size", ProfilePropertyKeys.Star3SizeKey, 0, MaxStarSize, .005f,
            "Size of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Density", ProfilePropertyKeys.Star3DensityKey, 0, MaxStarDensity, .01f,
            ProfileGroupDefinition.RebuildType.Stars, null, false,
            "Density of the stars in this layer."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Twinkle Amount", ProfilePropertyKeys.Star3TwinkleAmountKey, 0, 1, .7f,
            "Speed at which the star twinkles at."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Twinkle Speed", ProfilePropertyKeys.Star3TwinkleSpeedKey, 0, 10, 5,
            "Speed at which the star twinkles at."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Edge Feathering", ProfilePropertyKeys.Star3EdgeFeatheringKey, MinEdgeFeathering, 1, .4f,
            "Percentage of fade-in from the stars edges to the center."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Bloom Intensity", ProfilePropertyKeys.Star3ColorIntensityKey, 1, 10, 1,
            "Value multiplied with the star color to intensify bloom filters."),
        })
      };
    }
  }
}
