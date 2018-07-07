using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public abstract class BaseShaderDefinition : IProfileShaderDefinition
  {
    public string shaderName { get; protected set; }

    // Definition of shader parameters.
    private ProfileGroupSection[] m_ProfileDefinitions;
    public ProfileGroupSection[] profileDefinitions
    {
      get { return m_ProfileDefinitions ?? (m_ProfileDefinitions = ProfileDefinitionTable()); }
    }

    // Shader keywords.
    [SerializeField]
    private ProfileKeywordSection[] m_ProfileKeywords;
    public ProfileKeywordSection[] profileKeywords
    {
      get { return m_ProfileKeywords ?? (m_ProfileKeywords = ProfileShaderKeywords()); }
    }

    // Override and return shader keyword info.
    protected abstract ProfileKeywordSection[] ProfileShaderKeywords();
    
    // Override and return shader property info.
    protected abstract ProfileGroupSection[] ProfileDefinitionTable();
  }
}

