using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  public abstract class SkyEditorUtility
  {
    public const float KEY_GRIP_HEIGHT = 18.0f;
    public const float KEY_GRIP_WIDTH = 18.0f;
    public const string KEY_GRIP_ACTIVE = "TriangleGripActive";
    public const string KEY_GRIP_INACTIVE = "TriangleGripInactive";

    public const string PACKAGE_DIR_NAME = "FunlySkyStudio";

    public static Dictionary<string, Texture2D> _imageCache = new Dictionary<string, Texture2D>();

    // Keyframe time is pretty much zero.
    public static bool IsKeyFrameAtStart(IBaseKeyframe keyframe)
    {
      return keyframe.time >= 0 && keyframe.time < .00001f;
    }

    // Keyframe time is pretty much 1.0f.
    public static bool IsKeyFrameAtEnd(IBaseKeyframe keyframe)
    {
      return keyframe.time >= .99999f;
    }

    public static float GetWidthBetweenTimes(Rect rect, float fromTime, float toTime)
    {
      if (toTime < fromTime) {
        toTime = 1.0f;
      }

      return GetXPositionForPercent(rect, toTime) - GetXPositionForPercent(rect, fromTime);
    }

    // Get the xPosition that's a percent inside the rect.
    public static float GetXPositionForPercent(Rect rect, float percent)
    {
      float distance = rect.width * percent;
      return rect.x + distance;
    }

    public static float GetYPositionForPercent(Rect rect, float percent)
    {
      return rect.y + (rect.height * percent);
    }

    // Get the percentage this xPosition is inside the rect.
    public static float GetPercentForXPosition(Rect rect, float xPosition)
    {
      return Mathf.Clamp01((xPosition - rect.x) / rect.width);
    }

    public static float GetPercentForYPosition(Rect rect, float yPosition)
    {
      return Mathf.Clamp01((yPosition - rect.y) / rect.height);
    }

    public static void CancelKeyframeDrag() {
      TimelineSelection.selectedControlUUID = null;
    }

    // Cancel drag operations, useful for when mouse leaves window.
    public static void CancelTimelineDrags() {
      TimelineSelection.selectedControlUUID = null;
      TimelineSelection.isDraggingTimeline = false;
    }

    public static bool IsKeyframeSelected(IBaseKeyframe keyframe) {
      if (keyframe == null || TimelineSelection.selectedControlUUID == null ||
          TimelineSelection.selectedControlUUID != keyframe.id) {
        return false;
      }
      return true;
    }

    // Sticks to bottom of rect, and can slide horizontally only.
    public static void DrawHorizontalKeyMarker(
      Rect fullSliderRect, BaseKeyframe keyFrame, UnityEngine.Object undoObject, out bool didSingleClick, out bool isDragging, out bool keyFrameTimeChanged)
    {
      Rect markerRect = new Rect(
        SkyEditorUtility.GetXPositionForPercent(fullSliderRect, keyFrame.time) - (KEY_GRIP_WIDTH / 2),
        fullSliderRect.y + fullSliderRect.height - (KEY_GRIP_HEIGHT) / 2.0f,
        KEY_GRIP_WIDTH,
        KEY_GRIP_HEIGHT);
      
      bool wasDragging = TimelineSelection.selectedControlUUID != null && TimelineSelection.selectedControlUUID == keyFrame.id;
      bool isMouseOverControl = markerRect.Contains(Event.current.mousePosition);

      didSingleClick = false;
      isDragging = wasDragging;
      keyFrameTimeChanged = false;

      // Single Click.
      if (Event.current.isMouse) {
        // Check for single click, with no drag.
        if (Event.current.type == EventType.MouseUp && TimelineSelection.selectedControlUUID == null && isMouseOverControl) {
          Event.current.Use();
          didSingleClick = true;
        }

        // Start slide.
        if (TimelineSelection.selectedControlUUID == null && isMouseOverControl && Event.current.type == EventType.MouseDrag) {
          TimelineSelection.selectedControlUUID = keyFrame.id;
          Event.current.Use();
          isDragging = true;
        }

        // End Slide.
        if (wasDragging && Event.current.type == EventType.MouseUp) {
          TimelineSelection.selectedControlUUID = null;
          Event.current.Use();
          isDragging = false;
        }

        // If we're dragging this keyframe grip, move it's position.
        if (isDragging || wasDragging) {
          // Update key frame time value and reposition rectangle.
          Undo.RecordObject(undoObject, "Keyframe time position changed.");
          keyFrame.time = SkyEditorUtility.GetPercentForXPosition(fullSliderRect, Event.current.mousePosition.x);
          keyFrameTimeChanged = true;
          isDragging = true;

          // Position the marker rect.
          markerRect.x = SkyEditorUtility.GetXPositionForPercent(fullSliderRect, keyFrame.time) - (KEY_GRIP_WIDTH / 2);
          Event.current.Use();
        }
      }

      bool showAsActive = IsKeyframeActiveInInspector(keyFrame) || isDragging;

      // Draw the marker at this location.
      SkyEditorUtility.DrawKeyMarker(markerRect, showAsActive);
    }

    public static void DrawNumericKeyMarker(Rect fullSliderRect, NumberKeyframe keyFrame, NumberKeyframeGroup group, 
      UnityEngine.Object undoObject, out bool didSingleClick, out bool isDragging, out bool keyFrameTimeChanged)
    {
      Rect markerRect = new Rect(
        SkyEditorUtility.GetXPositionForPercent(fullSliderRect, keyFrame.time) - (KEY_GRIP_WIDTH / 2),
        GetYPositionForPercent(fullSliderRect, 1 - group.ValueToPercent(keyFrame.value)),
        KEY_GRIP_WIDTH,
        KEY_GRIP_HEIGHT);

      bool wasDragging = TimelineSelection.selectedControlUUID != null && TimelineSelection.selectedControlUUID == keyFrame.id;
      bool isMouseOverControl = markerRect.Contains(Event.current.mousePosition);

      didSingleClick = false;
      keyFrameTimeChanged = false;
      isDragging = wasDragging;

      // Single Click.
      if (Event.current.isMouse) {
        // Check for single click, with no drag.
        if (Event.current.type == EventType.MouseUp && TimelineSelection.selectedControlUUID == null && isMouseOverControl) {
          didSingleClick = true;
          Event.current.Use();
        }

        // Start slide.
        if (TimelineSelection.selectedControlUUID == null && isMouseOverControl && Event.current.type == EventType.MouseDrag) {
          TimelineSelection.selectedControlUUID = keyFrame.id;

          // Find the position of the current value and record the offset so we can drag the keygrip relative from here.
          Vector2 valuePosition = new Vector2(
            GetXPositionForPercent(fullSliderRect, keyFrame.time),
            GetYPositionForPercent(fullSliderRect, 1 - group.ValueToPercent(keyFrame.value)));

          TimelineSelection.startingMouseOffset = valuePosition - Event.current.mousePosition;

          isDragging = true;
          Event.current.Use();
        }

        // End Slide.
        if (wasDragging && Event.current.type == EventType.MouseUp) {
          TimelineSelection.selectedControlUUID = null;
          isDragging = false;
          Event.current.Use();
        }

        // If we're dragging this keyframe grip, move it's position.
        if (wasDragging || isDragging) {
          // Update key frame time value and reposition rectangle.
          Undo.RecordObject(undoObject, "Keyframe time and value changed.");

          Vector2 adjustedMousePosition = Event.current.mousePosition + TimelineSelection.startingMouseOffset;

          keyFrame.time = GetPercentForXPosition(fullSliderRect, adjustedMousePosition.x);

          float adjustedValuePercent = 1 - GetPercentForYPosition(fullSliderRect, adjustedMousePosition.y);
          keyFrame.value = group.PercentToValue(adjustedValuePercent);
          keyFrameTimeChanged = true;
          isDragging = true;

          // Position the marker rect.
          markerRect.x = SkyEditorUtility.GetXPositionForPercent(fullSliderRect, keyFrame.time) - (KEY_GRIP_WIDTH / 2);
          markerRect.y = GetYPositionForPercent(fullSliderRect, 1 - group.ValueToPercent(keyFrame.value));
          Event.current.Use();
        }
      }

      bool showAsActive = IsKeyframeActiveInInspector(keyFrame) || isDragging;

      // Draw the marker at this location.
      SkyEditorUtility.DrawKeyMarker(markerRect, showAsActive);
    }

    // True if the keyframe is shown in the inspector window.
    public static bool IsKeyframeActiveInInspector(BaseKeyframe keyFrame) {
			if (KeyframeInspectorWindow.inspectorEnabled && KeyframeInspectorWindow.GetActiveKeyframeId() == keyFrame.id)
			{
        return true;
      } else {
        return false;
      }
    }

    // True if group is selected on timeline.
    public static bool IsGroupSelectedOnTimeline(string groupId) {
      if (TimelineSelection.selectedGroupUUID != null && TimelineSelection.selectedGroupUUID == groupId) {
        return true;
      } else {
        return false;
      }
    }

    public static void DrawKeyMarker(Rect rect, bool isActive)
    {
      if (Event.current.type != EventType.Repaint)
      {
        return;
      }

      Texture2D gripTexture = null;
      if (isActive) {
        gripTexture = GetActiveKeyframeMarkerTexture();
      } else {
        gripTexture = GetInactiveKeyframeMarkerTexture();
      }

      GUI.DrawTexture(rect, gripTexture, ScaleMode.ScaleAndCrop, true);
    }

    public static Texture2D GetActiveKeyframeMarkerTexture()
    {
      return LoadEditorResourceTexture(KEY_GRIP_ACTIVE);
    }

    public static Texture2D GetInactiveKeyframeMarkerTexture()
    {
      return LoadEditorResourceTexture(KEY_GRIP_INACTIVE);
    }

    public static Texture2D LoadEditorResourceTexture(string textureName) {
      return LoadEditorResourceTexture(textureName, true);
    }

    public static Texture2D LoadEditorResourceTexture(string textureName, bool useCache)
    {
      if (useCache && _imageCache.ContainsKey(textureName))
      {
        return _imageCache[textureName];
      }

      string[] guids = AssetDatabase.FindAssets(textureName);

      if (guids.Length == 0) {
        Debug.LogError("Failed to located editor keyframe texture, asset is in unknown location.");
        return null;
      }

      foreach (string guid in guids)
      {
        // Return first image located inside our asset directory.
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (assetPath.Contains(PACKAGE_DIR_NAME))
        {
          Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

          if (useCache) {
            _imageCache.Add(textureName, tex);
          }

          return tex;
        }
      }

      return null;
    }

    public static GameObject LoadEditorPrefab(string prefabName)
    {
      string[] guids = AssetDatabase.FindAssets(prefabName);

      if (guids.Length == 0) {
        Debug.LogError("Failed to located editor keyframe texture, asset is in unknown location.");
        return null;
      }

      foreach (string guid in guids) {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (assetPath.Contains(PACKAGE_DIR_NAME)) {
          return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }
      }

      return null;
    }

    public static void WriteTextureToFile(RenderTexture renderTexture, string filename)
    {
      Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height);
      tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
      tex.Apply();

      WriteTextureToFile(tex, filename);
    }

    public static void WriteTextureToFile(Texture2D tex, string filename)
    {
      File.WriteAllBytes(filename, tex.EncodeToPNG());
    }
  }
}

