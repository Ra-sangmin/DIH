using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
  /**
   * The Sky Profile manages all properties for a sky, and the keyframe values
   * for any skybox animations or transitions.
   **/
  [CreateAssetMenu()]
  public class SkyProfile : ScriptableObject
  {
    public const string DefaultShaderName = "Funly/Sky Studio/Skybox/3D Standard";

    // Reference to material paired with this profile for enabling shader features.
    [SerializeField]
    private Material m_SkyboxMaterial;
    public Material skyboxMaterial
    {
      get { return m_SkyboxMaterial; }
      set
      {
        if (value == null)
        {
          m_SkyboxMaterial = null;
          return;
        }
        
        if (m_SkyboxMaterial && m_SkyboxMaterial.shader.name != value.shader.name)
        {
          m_SkyboxMaterial = value;
          m_ShaderName = value.shader.name;
          ReloadDefinitions();
        }
        else
        {
          m_SkyboxMaterial = value;
        }
      }
    }

    // We cache the last valid shader name so we can create sky systems from sky profiles during the setup wizard.
    [SerializeField]
    private string m_ShaderName = DefaultShaderName;
    public string shaderName { get { return m_ShaderName; } }

    // Shader definition knows about the properties and keywords a profile needs.
    private IProfileShaderDefinition shaderDefinition;

    // List determines what shows up on the timeline editor, or is a single value.
    public List<string> timelineManagedKeys = new List<string>();

    // Groups of keyframes for sky properties.
    public KeyframeGroupDictionary keyframeGroups = new KeyframeGroupDictionary();

    // Shader keyword values;
    public BoolDictionary shaderKeywords = new BoolDictionary();

    [SerializeField]
#pragma warning disable
    private int m_ProfileVersion = 1;
#pragma warning restore

    // Keep a mapping of key to definition for fast lookups.
    private Dictionary<string, ProfileGroupDefinition> m_KeyToGroupInfo;
    
    // Definitions.
    public ProfileGroupSection[] groupDefinitions
    {
      get { return shaderDefinition != null ? shaderDefinition.profileDefinitions : null; }
    }

    public ProfileKeywordSection[] keywordDefinitions
    {
      get { return shaderDefinition != null ? shaderDefinition.profileKeywords : null; }
    }

    // Sky Properties.
    public TextureKeyframeGroup SkyCubemap
    {
      get { return GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.SkyCubemapKey); }
    }

    public ColorKeyframeGroup SkyUpperColor
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.SkyUpperColorKey); }
    }

    public ColorKeyframeGroup SkyMiddleColor
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.SkyMiddleColorKey); }
    }

    public ColorKeyframeGroup SkyLowerColor
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.SkyLowerColorKey); }
    }

    public NumberKeyframeGroup SkyMiddleColorPosition
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SkyMiddleColorPosition); }
    }

    public NumberKeyframeGroup HorizonTransitionStart
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.HorizonTrasitionStartKey); }
    }

    public NumberKeyframeGroup HorizonTransitionLength
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.HorizonTransitionLengthKey); }
    }

    public NumberKeyframeGroup StarTransitionStart
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.StarTransitionStartKey); }
    }

    public NumberKeyframeGroup StarTransitionLength
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.StarTransitionLengthKey); }
    }

    public NumberKeyframeGroup StarDistanceScale
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.HorizonStarScaleKey); }
    }

    // Clouds.
    public TextureKeyframeGroup CloudNoiseTexture
    {
      get { return GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.CloudNoiseTextureKey); }
    }

    public NumberKeyframeGroup CloudTextureTiling
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.CloudTextureTiling); }
    }

    public NumberKeyframeGroup CloudDensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.CloudDensityKey); }
    }

    public NumberKeyframeGroup CloudSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.CloudSpeedKey); }
    }

    public NumberKeyframeGroup CloudDirection
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.CloudDirectionKey); }
    }

    public NumberKeyframeGroup CloudHeight
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.CloudHeight); }
    }

    public ColorKeyframeGroup CloudColor1
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.CloudColor1Key); }
    }

    public ColorKeyframeGroup CloudColor2
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.CloudColor2Key); }
    }

    public NumberKeyframeGroup CloudFadePosition
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.CloudFadePositionKey); }
    }

    public NumberKeyframeGroup CloudFadeAmount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.CloudFadeAmountKey); }
    }

    public ColorKeyframeGroup FogColor
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.FogColorKey); }
    }

    public NumberKeyframeGroup FogDensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.FogDensityKey); }
    }

    public NumberKeyframeGroup FogHeight
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.FogLengthKey); }
    }

    // Sun Properties.
    public ColorKeyframeGroup SunColor
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.SunColorKey); }
    }

    public TextureKeyframeGroup SunTexture
    {
      get { return GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.SunTextureKey); }
    }

    public NumberKeyframeGroup SunSpriteRowCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunSpriteRowCount); }
    }

    public NumberKeyframeGroup SunSpriteColumnCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunSpriteColumnCount); }
    }

    public NumberKeyframeGroup SunSpriteItemCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunSpriteItemCount); }
    }

    public NumberKeyframeGroup SunSpriteAnimationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunSpriteAnimationSpeed); }
    }

    public NumberKeyframeGroup SunRotationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunRotationSpeedKey); }
    }

    public NumberKeyframeGroup SunSize
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunSizeKey); }  
    }

    public NumberKeyframeGroup SunEdgeFeathering
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunEdgeFeatheringKey); }
    }

    public NumberKeyframeGroup SunBloomIntensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunColorIntensityKey); }
    }

    public ColorKeyframeGroup SunLightColor
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.SunLightColorKey); }
    }

    public NumberKeyframeGroup SunLightIntensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.SunLightIntensityKey); }
    }

    public SpherePointKeyframeGroup SunPosition
    {
      get { return GetGroup<SpherePointKeyframeGroup>(ProfilePropertyKeys.SunPositionKey); }
    }

    // Moon Properties.
    public ColorKeyframeGroup MoonColor
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.MoonColorKey); }
    }

    public TextureKeyframeGroup MoonTexture
    {
      get { return GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.MoonTextureKey); }
    }

    public NumberKeyframeGroup MoonSpriteRowCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonSpriteRowCount); }
    }

    public NumberKeyframeGroup MoonSpriteColumnCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonSpriteColumnCount); }
    }

    public NumberKeyframeGroup MoonSpriteItemCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonSpriteItemCount); }
    }

    public NumberKeyframeGroup MoonSpriteAnimationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonSpriteAnimationSpeed); }
    }

    public NumberKeyframeGroup MoonRotationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonRotationSpeedKey); }
    }

    public NumberKeyframeGroup MoonSize
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonSizeKey); }
    }

    public NumberKeyframeGroup MoonEdgeFeathering
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonEdgeFeatheringKey); }
    }

    public NumberKeyframeGroup MoonBloomIntensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonColorIntensityKey); }
    }

    public ColorKeyframeGroup MoonLightColor
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.MoonLightColorKey); }
    }

    public NumberKeyframeGroup MoonLightIntensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonLightIntensityKey); }
    }

    public SpherePointKeyframeGroup MoonPosition
    {
      get { return GetGroup<SpherePointKeyframeGroup>(ProfilePropertyKeys.MoonPositionKey); }
    }

    public NumberKeyframeGroup MoonOrbitSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.MoonOrbitSpeed); }
    }

    // Star 1 Properties.
    public NumberKeyframeGroup Star1Size
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1SizeKey); }
    }

    public ColorKeyframeGroup Star1Color
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.Star1ColorKey); }
    }

    public TextureKeyframeGroup Star1Texture
    {
      get { return GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.Star1TextureKey); }
    }

    public NumberKeyframeGroup Star1SpriteRowCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1SpriteRowCount); }
    }

    public NumberKeyframeGroup Star1SpriteColumnCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1SpriteColumnCount); }
    }

    public NumberKeyframeGroup Star1SpriteItemCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1SpriteItemCount); }
    }

    public NumberKeyframeGroup Star1SpriteAnimationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1SpriteAnimationSpeed); }
    }

    public NumberKeyframeGroup Star1TwinkleAmount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1TwinkleAmountKey); }
    }

    public NumberKeyframeGroup Star1TwinkleSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1TwinkleSpeedKey); }
    }

    public NumberKeyframeGroup Star1RotationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1RotationSpeedKey); }
    }

    public NumberKeyframeGroup Star1EdgeFeathering
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1EdgeFeatheringKey); }
    }

    public NumberKeyframeGroup Star1BloomIntensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1ColorIntensityKey); }
    }

    public NumberKeyframeGroup Star1Density
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1DensityKey); }
    }

    // Star 2 Properties.
    public NumberKeyframeGroup Star2Size
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2SizeKey); }
    }

    public ColorKeyframeGroup Star2Color
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.Star2ColorKey); }
    }

    public TextureKeyframeGroup Star2Texture
    {
      get { return GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.Star2TextureKey); }
    }

    public NumberKeyframeGroup Star2SpriteRowCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2SpriteRowCount); }
    }

    public NumberKeyframeGroup Star2SpriteColumnCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2SpriteColumnCount); }
    }

    public NumberKeyframeGroup Star2SpriteItemCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2SpriteItemCount); }
    }

    public NumberKeyframeGroup Star2SpriteAnimationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2SpriteAnimationSpeed); }
    }

    public NumberKeyframeGroup Star2TwinkleAmount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2TwinkleAmountKey); }
    }

    public NumberKeyframeGroup Star2TwinkleSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2TwinkleSpeedKey); }
    }

    public NumberKeyframeGroup Star2RotationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2RotationSpeedKey); }
    }

    public NumberKeyframeGroup Star2EdgeFeathering
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2EdgeFeatheringKey); }
    }

    public NumberKeyframeGroup Star2BloomIntensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2ColorIntensityKey); }
    }

    public NumberKeyframeGroup Star2Density
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2DensityKey); }
    }

    // Star 3 Properties.
    public NumberKeyframeGroup Star3Size
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3SizeKey); }
    }

    public ColorKeyframeGroup Star3Color
    {
      get { return GetGroup<ColorKeyframeGroup>(ProfilePropertyKeys.Star3ColorKey); }
    }

    public TextureKeyframeGroup Star3Texture
    {
      get { return GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.Star3TextureKey); }
    }

    public NumberKeyframeGroup Star3SpriteRowCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3SpriteRowCount); }
    }

    public NumberKeyframeGroup Star3SpriteColumnCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3SpriteColumnCount); }
    }

    public NumberKeyframeGroup Star3SpriteItemCount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3SpriteItemCount); }
    }

    public NumberKeyframeGroup Star3SpriteAnimationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3SpriteAnimationSpeed); }
    }

    public NumberKeyframeGroup Star3TwinkleAmount
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3TwinkleAmountKey); }
    }

    public NumberKeyframeGroup Star3TwinkleSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3TwinkleSpeedKey); }
    }

    public NumberKeyframeGroup Star3RotationSpeed
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3RotationSpeedKey); }
    }

    public NumberKeyframeGroup Star3EdgeFeathering
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3EdgeFeatheringKey); }
    }

    public NumberKeyframeGroup Star3BloomIntensity
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3ColorIntensityKey); }
    }

    public NumberKeyframeGroup Star3Density
    {
      get { return GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3DensityKey); }
    }

    public SkyProfile()
    {
      // Build the profile definition table.
      ReloadFullProfile();
    }

    private void OnEnable()
    {
      ReloadFullProfile();
    }

    private void ReloadFullProfile()
    {
      ReloadDefinitions();
      MergeProfileWithDefinitions();
      RebuildKeyToGroupInfoMapping();
      ValidateTimelineGroupKeys();
    }

    private void ReloadDefinitions()
    {
      shaderDefinition = GetShaderInfoForMaterial(m_ShaderName);
    }

    private IProfileShaderDefinition GetShaderInfoForMaterial(string shaderName)
    {
      // We currently only support 1 shader.
      return new Standard3dShaderDefinition();
    }

    public void MergeProfileWithDefinitions()
    {
      MergeGroupsWithDefinitions();
      MergeShaderKeywordsWithDefinitions();
    }

    public void MergeGroupsWithDefinitions()
    {
      HashSet<string> validProperties = ProfilePropertyKeys.GetPropertyKeysSet();

      // Build our groups from the profile definition table.
      foreach (ProfileGroupSection section in groupDefinitions)
      {
        foreach (ProfileGroupDefinition groupInfo in section.groups)
        {
          // Filter out old groups that are no longer valid.
          if (!validProperties.Contains(groupInfo.propertyKey))
          {
            continue;
          }

          if (groupInfo.type == ProfileGroupDefinition.GroupType.Color) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false)
            {
              AddColorGroup(groupInfo.propertyKey, groupInfo.groupName, groupInfo.color);
            }
            else
            {
              keyframeGroups[groupInfo.propertyKey].name = groupInfo.groupName;
            }
          } else if (groupInfo.type == ProfileGroupDefinition.GroupType.Number) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false)
            {
              AddNumericGroup(groupInfo.propertyKey, groupInfo.groupName,
                groupInfo.minimumValue, groupInfo.maximumValue, groupInfo.value);
            }
            else
            {
              NumberKeyframeGroup numberGroup = keyframeGroups.GetGroup<NumberKeyframeGroup>(groupInfo.propertyKey);
              numberGroup.name = groupInfo.groupName;
              numberGroup.minValue = groupInfo.minimumValue;
              numberGroup.maxValue = groupInfo.maximumValue;
            }
          } else if (groupInfo.type == ProfileGroupDefinition.GroupType.Texture) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false)
            {
              AddTextureGroup(groupInfo.propertyKey, groupInfo.groupName, groupInfo.texture);
            }
            else
            {
              keyframeGroups[groupInfo.propertyKey].name = groupInfo.groupName;
            }
          } else if (groupInfo.type == ProfileGroupDefinition.GroupType.SpherePoint) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false) {
              AddSpherePointGroup(groupInfo.propertyKey, groupInfo.groupName, groupInfo.spherePoint);
            } else
            {
              keyframeGroups[groupInfo.propertyKey].name = groupInfo.groupName;
            }
          }
        }
      }
    }

    public Dictionary<string, ProfileGroupDefinition> GroupDefinitionDictionary() {
      ProfileGroupSection[] sections = ProfileDefinitionTable();

      Dictionary<string, ProfileGroupDefinition> dict = new Dictionary<string, ProfileGroupDefinition>();

      foreach (ProfileGroupSection sectionInfo in sections) {
        foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups) {
          dict.Add(groupInfo.propertyKey, groupInfo);
        }
      }

      return dict;
    }

    public ProfileGroupSection[] ProfileDefinitionTable()
    {
      return groupDefinitions;
    }

    private void AddNumericGroup(string propKey, string groupName, float min, float max, float value)
    {
      NumberKeyframeGroup group = new NumberKeyframeGroup(
        groupName, min, max, new NumberKeyframe(0, value));
      keyframeGroups[propKey] = group;
    }
    
    private void AddColorGroup(string propKey, string groupName, Color color)
    {
      ColorKeyframeGroup group = new ColorKeyframeGroup(
        groupName, new ColorKeyframe(color, 0));

      keyframeGroups[propKey] = group;
    }

    private void AddTextureGroup(string propKey, string groupName, Texture2D texture)
    {
      TextureKeyframeGroup group = new TextureKeyframeGroup(
        groupName, new TextureKeyframe(texture, 0));

      keyframeGroups[propKey] = group;
    }

    private void AddSpherePointGroup(string propKey, string groupName, SpherePoint point)
    {
      SpherePointKeyframeGroup group = new SpherePointKeyframeGroup(groupName, new SpherePointKeyframe(point, 0));

      keyframeGroups[propKey] = group;
    }

    public T GetGroup<T>(string propertyKey) where T : class
    {
      return keyframeGroups[propertyKey] as T;
    }

    public IKeyframeGroup GetGroup(string propertyKey)
    {
      return keyframeGroups[propertyKey];
    }

    public IKeyframeGroup GetGroupWithId(string groupId)
    {
      if (groupId == null) {
        return null;
      }

      foreach (string key in keyframeGroups)
      {
        IKeyframeGroup group = keyframeGroups[key];
        if (group.id == groupId) {
          return group;
        }
      }
      return null;
    }

    // This returns the groups that exist in the profile for easy iteration.
    public ProfileGroupSection[] GetProfileDefinitions()
    {
      return groupDefinitions;
    }

    public ProfileGroupSection GetSectionInfo(string sectionKey)
    {
      foreach (ProfileGroupSection section in groupDefinitions)
      {
        if (section.sectionKey == sectionKey)
        {
          return section;
        }
      }
      return null;
    }

    // Check if a group is managed by the timeline.
    public bool IsManagedByTimeline(string propertyKey)
    {
      return timelineManagedKeys.Contains(propertyKey);
    }

    public void ValidateTimelineGroupKeys()
    {
      List<string> removeKeys = new List<string>();

      HashSet<string> validProperties = ProfilePropertyKeys.GetPropertyKeysSet();

      foreach (string timelineKey in timelineManagedKeys)
      {
        if (!IsManagedByTimeline(timelineKey) || !validProperties.Contains(timelineKey))
        {
          removeKeys.Add(timelineKey);
        }
      }

      foreach (string removeKey in removeKeys)
      {
        if (timelineManagedKeys.Contains(removeKey))
        {
          timelineManagedKeys.Remove(removeKey);
        }
      }
    }

    public List<ProfileGroupDefinition> GetGroupDefinitionsManagedByTimeline() {
      List<ProfileGroupDefinition> groups = new List<ProfileGroupDefinition>();

      foreach (string groupKey in timelineManagedKeys)
      {
        ProfileGroupDefinition groupDefinition = GetGroupDefinitionForKey(groupKey);
        if (groupDefinition == null)
        {
          continue;
        }

        groups.Add(groupDefinition);
      }

      return groups;
    }

    public List<ProfileGroupDefinition> GetGroupDefinitionsNotManagedByTimeline()
    {
      List<ProfileGroupDefinition> groups = new List<ProfileGroupDefinition>();

      foreach (ProfileGroupSection sectionInfo in groupDefinitions)
      {
        foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups)
        {
          if (IsManagedByTimeline(groupInfo.propertyKey) == false && CanGroupBeOnTimeline(groupInfo))
          {
            groups.Add(groupInfo);
          }
        }
      }

      return groups;
    }

    public ProfileGroupDefinition GetGroupDefinitionForKey(string propertyKey)
    {
      ProfileGroupDefinition def = null;
      if (m_KeyToGroupInfo.TryGetValue(propertyKey, out def))
      {
        return def;
      }

      return null;
    }

    public void RebuildKeyToGroupInfoMapping()
    {
      m_KeyToGroupInfo = new Dictionary<string, ProfileGroupDefinition>();

      foreach (ProfileGroupSection sectionInfo in groupDefinitions) {
        foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups)
        {
          m_KeyToGroupInfo[groupInfo.propertyKey] = groupInfo;
        }
      }
    }

    public void TrimGroupToSingleKeyframe(string propertyKey)
    {
      IKeyframeGroup group = GetGroup(propertyKey);
      if (group == null)
      {
        return;
      }

      group.TrimToSingleKeyframe();
    }

    // Blacklist some groups from being on the timeline.
    public bool CanGroupBeOnTimeline(ProfileGroupDefinition definition)
    {
      if (definition.type == ProfileGroupDefinition.GroupType.Texture || 
         (definition.propertyKey.Contains("Star") && definition.propertyKey.Contains("Density")) || 
         definition.propertyKey.Contains("Sprite"))
      {
        return false;
      }
      else
      {
        return true;
      }
    }

    protected void MergeShaderKeywordsWithDefinitions()
    {
      foreach (ProfileKeywordSection section in shaderDefinition.profileKeywords)
      {
        foreach (ProfileKeywordDefinition definition in section.shaderKeywords)
        {
          if (shaderKeywords.dict.ContainsKey(definition.shaderKeyword) == false)
          {
            SetShaderKeywordValue(definition.shaderKeyword, definition.value);
          } 
        }
      }  
    }

    public bool GetShaderKeywordValue(string keywordName)
    {
      if (shaderKeywords.dict.ContainsKey(keywordName) == false)
      {
        return false;
      }
      return shaderKeywords[keywordName];
    }

    public void SetShaderKeywordValue(string keywordName, bool value)
    {
      shaderKeywords[keywordName] = value;
    }
  }
}

