using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DroneUI : MonoBehaviour
{
    //access to setting up the camera pip
    private static Camera s_pipCamera;

    //Setting up the UI
    [SerializeField]private AgentBehaviour m_agent;
    //UI Elements
    [SerializeField] private Slider m_food;
    [SerializeField] private Slider m_energy;
    [SerializeField] private Slider m_population;
    [SerializeField] private Slider m_desire;
    [SerializeField] private TextMeshProUGUI m_name;
    private Dictionary<Want.WantType, Slider> m_wantsSlider;

    private void Start()
    {
        
    }

    //setup;
    public void SetUp( AgentBehaviour agent)
    {
        //set up the sliders
        m_wantsSlider = new Dictionary<Want.WantType, Slider>()
        {
            { Want.WantType.k_food, m_food } ,
            { Want.WantType.k_energy, m_energy },
            { Want.WantType.k_population, m_population },
            { Want.WantType.k_desire, m_desire }
        };
        m_agent = agent;
        m_name.text = agent.GetName();
        foreach(var want in agent.GetWants())
        {
            SetUpSlider(want.Key, want.Value.GetWantValue());
        }
    }
    private void SetUpSlider(Want.WantType wantType, int amount)
    {
        m_wantsSlider[wantType].maxValue = amount;
        m_wantsSlider[wantType].value = amount;
    }

    public void UpdateSlider(Want.WantType wantType, int amount)
    {
        m_wantsSlider[wantType].value = amount;
    }


    public void SetPIP()
    {
        if (s_pipCamera == null)
        {
            foreach (var camera in Camera.allCameras)
            {
                if (camera.CompareTag("PIP"))
                {
                    s_pipCamera = camera;
                }
            }
        }
        if(s_pipCamera == null)
        {
            return;
        }
        else
        {
            if(s_pipCamera.transform.parent == m_agent.transform)
            {
                //disable the camera
                s_pipCamera.transform.SetParent(null);
                s_pipCamera.gameObject.SetActive(false);
                return;
            }
            s_pipCamera.gameObject.SetActive(true);
            s_pipCamera.transform.SetParent(m_agent.transform);
            s_pipCamera.GetComponent<CameraScript>().Activate();
        }
    }


}
