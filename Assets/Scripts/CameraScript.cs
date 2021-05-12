using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private static CameraScript s_instance;
    public static CameraScript GetCamera()
    {
        if (s_instance == null)
        {
            s_instance = FindObjectOfType<CameraScript>();
        }
        return s_instance;
    }
    private Vector3 m_offset = new Vector3(0.0f,9.0f, 0.0f);
    private Quaternion m_rotation;
    private void Start()
    {
        m_rotation = transform.rotation;
    }

    public void Activate()
    {
        transform.localPosition = m_offset;
    }
    private void Update()
    {
        transform.rotation = m_rotation;
       /* if(transform.localPosition != m_offset)
        {
            transform.localPosition = m_offset;
        }*/
    }
}
