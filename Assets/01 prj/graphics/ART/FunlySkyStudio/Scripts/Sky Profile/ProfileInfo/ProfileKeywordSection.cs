using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class ProfileKeywordSection
  {
    public string sectionTitle;
    public string sectionKey;
    public string sectionIcon;
    public ProfileKeywordDefinition[] shaderKeywords;

    public ProfileKeywordSection(string sectionTitle, string sectionKey, ProfileKeywordDefinition[] shaderKeywords)
    {
      this.sectionTitle = sectionTitle;
      this.sectionKey = sectionKey;
      this.shaderKeywords = shaderKeywords;
    }
  }
}
