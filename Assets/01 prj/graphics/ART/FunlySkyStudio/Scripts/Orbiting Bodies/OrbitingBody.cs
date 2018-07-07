using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public class OrbitingBody : MonoBehaviour
  {
    // Position of the orbiting body.
    private SpherePoint m_SpherePoint = new SpherePoint(0, 0);
    public SpherePoint spherePoint
    {
      get { return m_SpherePoint; }
      set
      {
        if (m_SpherePoint == null)
        {
          m_SpherePoint = new SpherePoint(0, 0);
        }
        else
        {
          m_SpherePoint = value;
        }

        m_CachedWorldDirection = m_SpherePoint.GetWorldDirection();
        LayoutOribit();
      }
    }

    // Direction to orbiting body.
    private Vector3 m_CachedWorldDirection = Vector3.right;
    public Vector3 BodyGlobalDirection { get { return m_CachedWorldDirection; } }

    private Light m_BodyLight;
    public Light BodyLight
    {
      get {
        if (m_BodyLight == null) {
          m_BodyLight = transform.GetComponentInChildren<Light>();
          if (m_BodyLight != null)
          {
            // Reset in case it was rotated from older prefab or developer.
            m_BodyLight.transform.localRotation = Quaternion.identity;
          }
        }

        return m_BodyLight;
      }
    }

    public void LayoutOribit()
    {
      transform.position = Vector3.zero;
      transform.rotation = Quaternion.identity;
      transform.forward = BodyGlobalDirection * -1.0f;
    }

    void OnValidate()
    {
      LayoutOribit();
    }
  }
}
