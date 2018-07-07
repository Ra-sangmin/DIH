using System.IO;
using System.Collections;
using System.Collections.Generic;
using Funly.SkyStudio;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;

namespace Funly.SkyStudio
{
  public class SkySetupWindow : EditorWindow
  {

    private class ProfilePreset : IComparer<ProfilePreset>
    {
      public string guid;
      public string assetPath;
      public string name;
      public string menuName;

      public ProfilePreset(string guid, string assetPath, string name, string menuName)
      {
        this.guid = guid;
        this.assetPath = assetPath;
        this.name = name;
        this.menuName = menuName;
      }

      public int Compare(ProfilePreset x, ProfilePreset y)
      {
        return x.assetPath.CompareTo(y.assetPath);
      }
    }

    private ProfilePreset _selectedProfilePreset;
    private const string SKY_CONTROLLER_PREFAB = "SkySystemController";

    [MenuItem("Window/Funly Sky Studio/Setup Sky")]
    public static void ShowWindow()
    {
      TimelineSelection.Clear();

      EditorWindow window = EditorWindow.GetWindow<SkySetupWindow>();

      window.Show();
    }

    private void OnEnable()
    {
      name = "Setup Sky";
      titleContent = new GUIContent("Setup Sky");
    }

    private void OnGUI()
    {
      List<ProfilePreset> presets = LoadListOfPresets();

      EditorGUILayout.HelpBox("Setup a new sky system in the current scene. " +
                              "This will create a copy of the preset you select, and load it into your scene.",
        MessageType.Info);
      EditorGUILayout.Separator();

      RenderPresetPopup(presets);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      if (GUILayout.Button(new GUIContent("Create Sky System"))) {
        SetupSceneWithPreset(_selectedProfilePreset);
      }
    }

    private void RenderPresetPopup(List<ProfilePreset> presets)
    {
      int selectedIndex = 0;
      List<string> displayedPrests = new List<string>();

      // Build our list of presets to show in popup.
      for (int i = 0; i < presets.Count; i++) {
        ProfilePreset preset = presets[i];
        displayedPrests.Add(preset.menuName);

        // Check if this is the selected preset.
        if (_selectedProfilePreset != null && preset.assetPath == _selectedProfilePreset.assetPath) {
          selectedIndex = i;
        }
      }

      selectedIndex = EditorGUILayout.Popup("Sky Preset", selectedIndex, displayedPrests.ToArray());
      _selectedProfilePreset = presets[selectedIndex];
    }

    private List<ProfilePreset> LoadListOfPresets()
    {
      List<ProfilePreset> presets = new List<ProfilePreset>();

      string[] guids = AssetDatabase.FindAssets("t:SkyProfile");

      if (guids == null || guids.Length == 0) {
        return presets;
      }

      foreach (string guid in guids) {
        string presetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (presetPath == null) {
          Debug.LogError("Failed to get name for profile GUID: " + guid);
          continue;
        }
        string presetName = ObjectNames.NicifyVariableName(Path.GetFileNameWithoutExtension(presetPath));
        string menuName = Path.GetDirectoryName(presetPath) + "/" + presetName;

        string presetDirPrefix = "Assets/" + SkyEditorUtility.PACKAGE_DIR_NAME + "/Internal/Presets/";
        if (menuName.StartsWith(presetDirPrefix))
        {
          menuName = menuName.Remove(0, presetDirPrefix.Length);
        }
        else
        {
          menuName = "Your Project/" + menuName;
        }

        presets.Add(new ProfilePreset(guid, presetPath, presetName, menuName));
      }

      presets.Sort(delegate(ProfilePreset p1, ProfilePreset p2)
      {
        return p1.menuName.CompareTo(p2.menuName);
      });

      return presets;
    }

    private void SetupSceneWithPreset(ProfilePreset preset)
    {
      ClearSkyControllers();

      // Create new sky controller.
      GameObject skySystemPrefab = SkyEditorUtility.LoadEditorPrefab(SKY_CONTROLLER_PREFAB);
      if (skySystemPrefab == null) {
        Debug.LogError("Failed to locate sky controller prefab");
        return;
      }

      TimeOfDayController tc = Instantiate(skySystemPrefab).GetComponent<TimeOfDayController>();
      tc.name = SKY_CONTROLLER_PREFAB;

      string assetName = GetBestProfileName();
      AssetDatabase.CopyAsset(preset.assetPath, assetName);

      // Duplicate and create a new sky profile.
      SkyProfile profile = AssetDatabase.LoadAssetAtPath(assetName, typeof(SkyProfile)) as SkyProfile;
      if (profile == null) {
        Debug.LogError("Failed to duplicate profile");
        return;
      }

      Material skyboxMaterial = new Material(GetBestShaderForSkyProfile(profile));
      AssetDatabase.CreateAsset(skyboxMaterial, GetBestSkyboxMaterialName());
      profile.skyboxMaterial = skyboxMaterial;

      tc.skyProfile = profile;
      tc.skyProfile.skyboxMaterial = skyboxMaterial;
      tc.skyTime = .22f;

      SkyProfileEditor.ApplyKeywordsToMaterial(tc.skyProfile, skyboxMaterial);
      SkyProfileEditor.forceRebuildProfileId = profile.GetInstanceID();

      RenderSettings.skybox = skyboxMaterial;

      EditorUtility.SetDirty(skyboxMaterial);
      EditorUtility.SetDirty(tc.skyProfile);
      EditorUtility.SetDirty(tc);
      EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

      Selection.activeObject = tc.skyProfile;
    }

    private Shader GetBestShaderForSkyProfile(SkyProfile profile)
    {
      Shader skyShader = null;
      if (profile.shaderName != null)
      {
        skyShader = Shader.Find(profile.shaderName);
        if (skyShader == null)
        {
          Debug.LogError("Failed to located shader used in profile, loading default sky shader");
        }
      }

      if (skyShader == null)
      {
        skyShader = Shader.Find(SkyProfile.DefaultShaderName);
      }

      return skyShader;
    }

    private string GetBestProfileName()
    {
      return GetBestFileName("SkyProfile", ".asset", typeof(SkyProfile));
    }

    private string GetBestSkyboxMaterialName()
    {
      return GetBestFileName("SkyboxMaterial", ".mat", typeof(Material));
    }

    private string GetBestFileName(string fileName, string ext, System.Type fileType)
    {
      Scene scene = SceneManager.GetActiveScene();

      for (int i = 0; i < 100; i++) {
        string suffixName = null;
        if (i == 0) {
          suffixName = fileName + ext;
        } else {
          suffixName = fileName + "-" + i + ext;
        }

        string assetPath = "Assets/" + scene.name + suffixName;

        if (AssetDatabase.LoadAssetAtPath(assetPath, fileType) == null)
        {
          return assetPath;
        }
      }

      Debug.LogError("Failed to find name without collision, using default file name.");
      return "Assets/" + fileName + ext;
    }

    private void ClearSkyControllers()
    {
      TimeOfDayController[] skyControllers = GameObject.FindObjectsOfType<TimeOfDayController>();
      if (skyControllers != null) {
        foreach (TimeOfDayController timeController in skyControllers) {
          Debug.Log("Removing old sky controller from scene...");
          DestroyImmediate(timeController.gameObject);
        }
      }
    }
  }
}
