using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public class ProfileGroupSection
  {
    public string sectionTitle;
    public string sectionIcon;
    public string sectionKey;
    public string dependsOnKeyword;
    public bool dependsOnValue;
    public ProfileGroupDefinition[] groups;

    public ProfileGroupSection(
      string sectionTitle, string sectionKey, string sectionIcon, string dependsOnKeyword,
      bool dependsOnValue, ProfileGroupDefinition[] groups)
    {
      this.sectionTitle = sectionTitle;
      this.sectionIcon = sectionIcon;
      this.sectionKey = sectionKey;
      this.groups = groups;
      this.dependsOnKeyword = dependsOnKeyword;
      this.dependsOnValue = dependsOnValue;
    }
  }
}
