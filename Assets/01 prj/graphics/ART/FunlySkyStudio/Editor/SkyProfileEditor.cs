using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Funly.SkyStudio;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Funly.SkyStudio
{
  [CustomEditor(typeof(SkyProfile))]
  public class SkyProfileEditor : Editor
  {
    private static string SHADER_NAME_PREFIX = "Funly/Sky Studio/Skybox";

    private SkyProfile m_Profile;
    private SkyBuilder m_Builder;
    private Texture2D m_SectionHeaderBg;
    private Dictionary<string, ProfileKeywordSection> m_KeywordToSection;
    private const float k_IconSize = 20.0f;
    private const int k_TitleSize = 12;
    private const int k_HeaderHeight = 20;
    private string m_SpherePointSelectionToken;

    // The setup window will set this value to force the first load rebuild.
    public static int forceRebuildProfileId;

    private void OnEnable()
    {
      serializedObject.Update();

      m_Profile = (SkyProfile)target;
      m_SectionHeaderBg = CreateColorImage(SectionColorForEditorSkin());

      // Make the sure the profile's features are in sync with shader material.
      ApplyKeywordsToMaterial();

      SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private void OnDisable()
    {
      SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
      //SpherePointGUI.RenderSpherePointSceneSelection(ref m_SpherePointSelectionToken);
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      m_Profile = (SkyProfile)target;

      // For new profiles we'll automatically build them.
      if (forceRebuildProfileId != -1 && forceRebuildProfileId == m_Profile.GetInstanceID()) {
        RebuildSkySystem();
        forceRebuildProfileId = -1;
      }

      if (RenderSkyboxMaterial() == false) {
        serializedObject.ApplyModifiedProperties();
        return;
      }

      // Cache a mapping of section key to section for shader keywords.
      if (m_KeywordToSection == null) {
        m_KeywordToSection = new Dictionary<string, ProfileKeywordSection>();
      }

      foreach (ProfileKeywordSection section in m_Profile.keywordDefinitions) {
        m_KeywordToSection[section.sectionKey] = section;
      }

      RenderFeatureSection();
      RenderTimelineList();
      RenderProfileDefinitions();

      TimeOfDayController tc = GameObject.FindObjectOfType<TimeOfDayController>();
      if (tc != null)
      {
        tc.UpdateSkyForCurrentTime();
      }

      serializedObject.ApplyModifiedProperties();
    }

    private Color SectionColorForEditorSkin()
    {
      if (EditorGUIUtility.isProSkin)
      {
        float gray = 190.0f / 225.0f;
        return new Color(gray, gray, gray, 1.0f);
      }
      else
      {
        float gray = 222.0f / 225.0f;
        return new Color(gray, gray, gray, 1.0f);
      }
    }

    private void RenderProfileDefinitions()
    {
      foreach (ProfileGroupSection groupSection in m_Profile.groupDefinitions)
      {
        if (groupSection.dependsOnKeyword != null &&
            groupSection.dependsOnValue != m_Profile.GetShaderKeywordValue(groupSection.dependsOnKeyword)) {
          continue;
        }

        RenderSection(groupSection.sectionKey); 
      }
    }

    private bool RenderSkyboxMaterial()
    {
      EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SkyboxMaterial"));
      
      if (SkyboxMaterial() == null)
      {
        EditorGUILayout.HelpBox("You need to assign a Funly Sky Studio skybox material" +
                                " before you can edit the sky profile.", MessageType.Info);
        return false;
      }

      if (SkyboxMaterial().shader.name.Contains(SHADER_NAME_PREFIX) == false)
      {
        EditorGUILayout.HelpBox("Skybox material has an unsupported shader. You need to use a shader" +
                                " in the Funly/Sky/ directory.", MessageType.Error);
        return false;
      }

      return true;
    }

    private void RebuildSkySystem()
    {
      if (m_Builder != null)
      {
        m_Builder.CancelBuild();
        m_Builder = null;
      }

      m_Builder = CreateSkyBuilder();

      if (m_Builder.IsComplete == false) {
        return;
      }

      m_Builder.BuildSkySystem();
    }

    private void RenderFeatureSection()
    {
      RenderSectionTitle("Features", "FeatureSectionIcon");

      string[] rebuildSkyKeywords = new string[]
      {
        ShaderKeywords.StarLayer1,
        ShaderKeywords.StarLayer2,
        ShaderKeywords.StarLayer3
      };

      if (m_KeywordToSection.ContainsKey(ProfileSectionKeys.FeaturesSectionKey) == false)
      {
        Debug.LogError("Shader definition is missing a features dictionary");
        return;
      }

      ProfileKeywordSection section = m_KeywordToSection[ProfileSectionKeys.FeaturesSectionKey];
      foreach (ProfileKeywordDefinition def in section.shaderKeywords)
      {
        bool valueChanged;
        RenderFeatureCheckbox(
          def.shaderKeyword,
          def.name,
          m_Profile.GetShaderKeywordValue(def.shaderKeyword),
          out valueChanged);

        if (valueChanged && rebuildSkyKeywords.Contains(def.shaderKeyword)) {
          RebuildSkySystem();
        }
      }
    }

    private bool RenderFeatureCheckbox(string keyword, string title, bool keywordValue, out bool valueChanged)
    {
      EditorGUI.BeginChangeCheck();
      bool value = EditorGUILayout.Toggle(title, keywordValue);
      if (EditorGUI.EndChangeCheck())
      {
        SetShaderKeyword(keyword, value);
        valueChanged = true;
      } else {
        valueChanged = false;
      }

      m_Profile.SetShaderKeywordValue(keyword, value);

      return value;
    }

    private Material SkyboxMaterial()
    {
      if (serializedObject.FindProperty("m_SkyboxMaterial") == null)
      {
        return null;
      }
      return serializedObject.FindProperty("m_SkyboxMaterial").objectReferenceValue as Material;
    }

    private void ApplyKeywordsToMaterial() {
      if (SkyboxMaterial() == null) {
        return;
      }

      ApplyKeywordsToMaterial(m_Profile, SkyboxMaterial());
    }

    public static void ApplyKeywordsToMaterial(SkyProfile profile, Material skyboxMaterial)
    {
      foreach (ProfileKeywordSection section in profile.keywordDefinitions)
      {
        foreach (ProfileKeywordDefinition definition in section.shaderKeywords)
        {
          SetShaderKeyword(
            definition.shaderKeyword,
            profile.GetShaderKeywordValue(definition.shaderKeyword),
            skyboxMaterial);
        }
      }
    }

    private void SetShaderKeyword(string keyword, bool value)
    {
      SetShaderKeyword(keyword, value, SkyboxMaterial());
    }

    private static void SetShaderKeyword(string keyword, bool value, Material skyboxMaterial)
    {      
      if (value)
      {
        skyboxMaterial.EnableKeyword(keyword);
      }
      else
      {
        skyboxMaterial.DisableKeyword(keyword);
      }
    }

    private void RenderTimelineList()
    {
      RenderSectionTitle("Timeline Animated Properties", "TimelineSectionIcon");

      EditorGUILayout.Space();

      List<ProfileGroupDefinition> onTimeline = m_Profile.GetGroupDefinitionsManagedByTimeline();
      List<ProfileGroupDefinition> offTimeline = m_Profile.GetGroupDefinitionsNotManagedByTimeline();

      int deleteIndex = -1;
      bool didSwapRows = false;
      int swapIndex1 = -1;
      int swapIndex2 = -1;

      if (onTimeline.Count == 0)
      {
        // Show definition message if no items added yet.
        EditorGUILayout.HelpBox("You can animate properties by adding them to the timeline.", MessageType.None);
      }
      else
      {
        EditorGUI.BeginChangeCheck();
        List<string> timelineTitles = GetTitlesForGroups(onTimeline);

        StringTableListGUI.RenderTableList(
          timelineTitles,
          out deleteIndex,
          out didSwapRows,
          out swapIndex1,
          out swapIndex2);

        // Check for table modification events (remove, reorder, etc.)
        if (EditorGUI.EndChangeCheck()) {
          if (deleteIndex != -1) {
            string deleteGroupKey = onTimeline[deleteIndex].propertyKey;

            IKeyframeGroup group = m_Profile.GetGroup(deleteGroupKey);
            if (SkyEditorUtility.IsGroupSelectedOnTimeline(group.id)) {
              TimelineSelection.Clear();

              // If we deleted a sphere point group make sure to hide the debug dots.
              if (group is SpherePointKeyframeGroup && m_Profile.skyboxMaterial != null) {
                m_Profile.skyboxMaterial.DisableKeyword(ShaderKeywords.RenderDebugPoints);
              }
            }

            m_Profile.timelineManagedKeys.Remove(deleteGroupKey);
            m_Profile.TrimGroupToSingleKeyframe(deleteGroupKey);
          } else if (didSwapRows) {
            string tmp = m_Profile.timelineManagedKeys[swapIndex2];
            m_Profile.timelineManagedKeys[swapIndex2] = m_Profile.timelineManagedKeys[swapIndex1];
            m_Profile.timelineManagedKeys[swapIndex1] = tmp;
            EditorUtility.SetDirty(m_Profile);
          }
        }
      }

      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (GUILayout.Button(new GUIContent("Open Timeline"))) {
        SkyTimelineWindow.ShowWindow();
      }

      if (GUILayout.Button(new GUIContent("Add to Timeline"))) {
        SkyGUITimelineMenu.ShowAddTimelinePropertyMenu(m_Profile, offTimeline);
      }
      EditorGUILayout.EndHorizontal();
    }

    // Render all properties for a section.
    public void RenderSection(string sectionKey, params string[] ignoreGroups)
    {
      ProfileGroupSection sectionInfo = m_Profile.GetSectionInfo(sectionKey);
      RenderSectionTitle(sectionInfo.sectionTitle, sectionInfo.sectionIcon);

      // Render shader keywords that need to be placed in the section.
      if (m_KeywordToSection.ContainsKey(sectionKey)) {
        ProfileKeywordSection keywordSection = m_KeywordToSection[sectionKey];

        foreach (ProfileKeywordDefinition def in keywordSection.shaderKeywords)
        {
          // Check for keyword dependencies.
          if (def.dependsOnKeyword != null) {
            if (m_Profile.GetShaderKeywordValue(def.dependsOnKeyword) != def.dependsOnValue) {
              continue;
            }
          }

          bool valueChanged;
          RenderFeatureCheckbox(
            def.shaderKeyword,
            def.name,
            m_Profile.GetShaderKeywordValue(def.shaderKeyword),
            out valueChanged);
        }
      }

      foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups)
      {
        bool shouldIgnore = false;
        foreach (string ignoreName in ignoreGroups)
        {
          if (groupInfo.groupName.Contains(ignoreName))
          {
            shouldIgnore = true;
            break;
          }
        }

        if (shouldIgnore)
        {
          continue;
        }

        RenderProfileGroup(groupInfo);
      }
    }

    // Render all properties in a group.
    public void RenderProfileGroup(ProfileGroupDefinition groupDefinition)
    {
      if (groupDefinition.dependsOnKeyword != null &&
          m_Profile.GetShaderKeywordValue(groupDefinition.dependsOnKeyword) != groupDefinition.dependsOnValue)
      {
        return;
      }

      bool valueChanged = false;
      if (groupDefinition.type == ProfileGroupDefinition.GroupType.Color)
      {
        valueChanged = RenderColorGroupProperty(groupDefinition);
      } else if (groupDefinition.type == ProfileGroupDefinition.GroupType.Number)
      {
        valueChanged = RenderNumericGroupProperty(groupDefinition);
      } else if (groupDefinition.type == ProfileGroupDefinition.GroupType.Texture)
      {
        valueChanged = RenderTextureGroupProperty(groupDefinition);
      } else if (groupDefinition.type == ProfileGroupDefinition.GroupType.SpherePoint)
      {
        valueChanged = RenderSpherePointPropertyGroup(groupDefinition);
      }

      // Check if this property needs to rebuild the sky.
      if (valueChanged && groupDefinition.rebuildType == ProfileGroupDefinition.RebuildType.Stars)
      {
        RebuildSkySystem();
      }

      if (valueChanged)
      {
        EditorUtility.SetDirty(m_Profile);
      }
    }

    // Render color property.
    public bool RenderColorGroupProperty(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();

      ColorKeyframeGroup group = m_Profile.GetGroup<ColorKeyframeGroup>(def.propertyKey);
      EditorGUILayout.PrefixLabel(new GUIContent(group.name, def.tooltip));
      bool valueChanged = false;

      if (m_Profile.IsManagedByTimeline(def.propertyKey))
      {
        RenderManagedOnTimlineMessage();
      }
      else
      {
        ColorKeyframe frame = group.GetKeyframe(0);

        EditorGUI.BeginChangeCheck();
        Color selectedColor = EditorGUILayout.ColorField(frame.color);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(m_Profile, "Changed color keyframe value");
          frame.color = selectedColor;
          valueChanged = true;
        }
      }

      EditorGUILayout.EndHorizontal();
      return valueChanged;
    }

    // Render numeric properties with a slider.
    public bool RenderNumericGroupProperty(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();
      NumberKeyframeGroup group = m_Profile.GetGroup<NumberKeyframeGroup>(def.propertyKey);
      EditorGUILayout.PrefixLabel(new GUIContent(group.name, def.tooltip));
      bool valueChanged = false;

      if (m_Profile.IsManagedByTimeline(def.propertyKey))
      {
        RenderManagedOnTimlineMessage();
      }
      else
      {
        NumberKeyframe frame = group.GetKeyframe(0);

        if (def.formatStyle == ProfileGroupDefinition.FormatStyle.Integer)
        {
          EditorGUI.BeginChangeCheck();
          int value = EditorGUILayout.IntField((int) frame.value);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(m_Profile, "Changed int keyframe value");
            frame.value = (int) Mathf.Clamp(value, group.minValue, group.maxValue);
            valueChanged = true;
          }
        }
        else
        {
          EditorGUI.BeginChangeCheck();
          float value = EditorGUILayout.Slider(frame.value, group.minValue, group.maxValue);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(m_Profile, "Changed float keyframe value");
            frame.value = value;
            valueChanged = true;
          }
        }

      }

      EditorGUILayout.EndHorizontal();

      return valueChanged;
    }

    // Render texture property.
    public bool RenderTextureGroupProperty(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();

      TextureKeyframeGroup group = m_Profile.GetGroup<TextureKeyframeGroup>(def.propertyKey);
      EditorGUILayout.PrefixLabel(new GUIContent(group.name + ":", def.tooltip));
      bool valueChanged = false;

      if (m_Profile.IsManagedByTimeline(def.propertyKey))
      {
        RenderManagedOnTimlineMessage();
      }
      else
      {
        TextureKeyframe frame = group.GetKeyframe(0);
        EditorGUI.BeginChangeCheck();
        Texture assignedTexture = (Texture) EditorGUILayout.ObjectField(frame.texture, typeof(Texture), true);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(m_Profile, "Changed texture keyframe value");
          frame.texture = assignedTexture;
          valueChanged = true;
        }
      }

      EditorGUILayout.EndHorizontal();
      return valueChanged;
    }

    private bool RenderSpherePointPropertyGroup(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();
      bool valueChanged = false;

      SpherePointKeyframeGroup group = m_Profile.GetGroup<SpherePointKeyframeGroup>(def.propertyKey);

      if (m_Profile.IsManagedByTimeline(def.propertyKey))
      {
        EditorGUILayout.PrefixLabel(new GUIContent(def.groupName, def.tooltip));
        RenderManagedOnTimlineMessage();
      }
      else
      {
        SpherePointKeyframe frame = group.GetKeyframe(0);

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent(group.name, def.tooltip));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        EditorGUI.indentLevel += 1;
        SpherePoint selectedPoint = SpherePointGUI.SpherePointField(
          frame.spherePoint, true, frame.id);
        EditorGUI.indentLevel -= 1;
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(m_Profile, "Changed sphere point");
          frame.spherePoint = selectedPoint;
        }

        EditorGUILayout.EndVertical();
      }

      EditorGUILayout.EndHorizontal();
      return valueChanged;
    }

    private void RenderManagedOnTimlineMessage()
    {
      GUIStyle style = new GUIStyle(GUI.skin.label);
      style.fontStyle = FontStyle.Italic;
      EditorGUILayout.LabelField("Managed on timeline", style);
    }

    private void RenderSectionTitle(string title, string iconName)
    {
      GUIStyle bgStyle = new GUIStyle();
      bgStyle.normal.background = m_SectionHeaderBg;
      bgStyle.margin = new RectOffset(0, 0, 20, 7);
      bgStyle.padding = new RectOffset(0, 0, 0, 0);

      GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
      titleStyle.normal.textColor = Color.black;
      titleStyle.fontStyle = FontStyle.Bold;
      titleStyle.fontSize = k_TitleSize;
      titleStyle.margin = new RectOffset(0, 0, 0, 0);
      titleStyle.padding = new RectOffset(0, 0, 3, 0);

      GUIStyle iconStyle = new GUIStyle();
      iconStyle.margin = new RectOffset(0, 0, 0, 0);
      iconStyle.padding = new RectOffset(0, 0, 0, 0);

      EditorGUILayout.BeginHorizontal(bgStyle, GUILayout.Height(k_HeaderHeight));
      
      Texture icon = SkyEditorUtility.LoadEditorResourceTexture(iconName);
      if (icon == null)
      {
        icon = SkyEditorUtility.LoadEditorResourceTexture("UnknownSectionIcon");
      }

      EditorGUILayout.LabelField(new GUIContent(icon), iconStyle, GUILayout.Width(k_IconSize), GUILayout.Height(k_IconSize));
      EditorGUILayout.LabelField(new GUIContent(title), titleStyle);

      GUILayout.FlexibleSpace();

      EditorGUILayout.EndHorizontal();
    }

    private Texture2D CreateColorImage(Color c)
    {
      Texture2D tex = new Texture2D(1, 1);
      tex.SetPixel(0, 0, c);
      tex.Apply();
    
      return tex;
    }

    private SkyBuilder CreateSkyBuilder()
    {
      SkyBuilder b = new SkyBuilder();
      b.starLayer1Enabled = m_Profile.GetShaderKeywordValue(ShaderKeywords.StarLayer1);
      b.starLayer2Enabled = m_Profile.GetShaderKeywordValue(ShaderKeywords.StarLayer2); ;
      b.starLayer3Enabled = m_Profile.GetShaderKeywordValue(ShaderKeywords.StarLayer3); ;

      b.starLayer1Density = m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1DensityKey).GetFirstValue();
      b.starLayer2Density = m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2DensityKey).GetFirstValue();
      b.starLayer3Density = m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3DensityKey).GetFirstValue();

      b.skyboxMaterial = m_Profile.skyboxMaterial;
      b.completionCallback += BuilderCompletion;

      return b;
    }

    private void BuilderCompletion(SkyBuilder builder, bool successful)
    {
      m_Builder.completionCallback -= BuilderCompletion;
      m_Builder = null;

      if (m_Profile)
      {
        EditorUtility.SetDirty(m_Profile);
      }

      TimeOfDayController tc = GameObject.FindObjectOfType<TimeOfDayController>();
      if (tc != null)
      {
        tc.UpdateSkyForCurrentTime();
      }
    }

    private List<string> GetTitlesForGroups(List<ProfileGroupDefinition> groups) {
      List<string> titles = new List<string>();

      foreach (ProfileGroupDefinition group in groups) {
        titles.Add(group.groupName);
      }

      return titles;
    }
  }
}



