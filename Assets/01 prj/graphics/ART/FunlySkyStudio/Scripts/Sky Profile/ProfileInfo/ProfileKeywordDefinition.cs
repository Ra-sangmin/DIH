using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class ProfileKeywordDefinition : System.Object
  {
    public string shaderKeyword;
    public string name;
    public bool value;
    public string tooltip;
    public string dependsOnKeyword;
    public bool dependsOnValue;

    public ProfileKeywordDefinition(string shaderKeyword, bool value, string name, string tooltip)
    {
      this.shaderKeyword = shaderKeyword;
      this.name = name;
      this.value = value;
      this.tooltip = tooltip;
    }

    public ProfileKeywordDefinition(string shaderKeyword, bool value, string name, string dependsOnKeyword, bool dependsOnValue, string tooltip) {
      this.shaderKeyword = shaderKeyword;
      this.name = name;
      this.dependsOnKeyword = dependsOnKeyword;
      this.dependsOnValue = dependsOnValue;
      this.value = value;
      this.tooltip = tooltip;
    }

  }
}

