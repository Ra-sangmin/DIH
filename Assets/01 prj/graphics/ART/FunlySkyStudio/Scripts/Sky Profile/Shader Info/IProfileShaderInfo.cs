using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public interface IProfileShaderDefinition
  {
    // Name of shader as defined in the shader file.
    string shaderName { get; }

    // Keywords used in this shader.
    ProfileKeywordSection[] profileKeywords { get; }

    // List of sections and properties supported by this shader.
    ProfileGroupSection[] profileDefinitions { get; }
  }
}

