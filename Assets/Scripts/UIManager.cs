using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Singleton
    private static UIManager s_instance;
    public static UIManager GetInstance()
    {
        if(s_instance != null)
        {
            return s_instance;
        }
        else
        {
            //If for some reason I have not set up the UI managert yet, find it
            s_instance = FindObjectOfType<UIManager>();
            if(s_instance == null)
            {
                Debug.LogError("No UI manager found in the scene");
                return null;
            }
            return s_instance;
        }
    }

    [SerializeField] GameObject m_droneUIPrefab;
    [SerializeField] RectTransform m_characterPanel;
    private Dictionary<AgentBehaviour, GameObject> agentToUI = new Dictionary<AgentBehaviour, GameObject>();
    // Start is called before the first frame update

    private void Awake()
    {
        s_instance = this;
    }
    public void AddDrone(AgentBehaviour agent)
    {
        //see if there is already a ui for this agent
        if (agentToUI.ContainsKey(agent))
        {
            return;
        }
        else
        {
            var obj = Instantiate(m_droneUIPrefab, m_characterPanel);
            obj.GetComponent<Button>().onClick.AddListener(()=>FindObjectOfType<CameraScript>().Activate());
            agentToUI.Add(agent, obj);
            obj.GetComponent<DroneUI>().SetUp(agent);
        }
    }

    public void RemoveDrone(AgentBehaviour agent)
    {
        //ensure that this has not already been removed
        if (agentToUI.ContainsKey(agent))
        {
            var ui = agentToUI[agent];
            //set the ui To be destoryed
            Destroy(ui, 1.0f);
            agentToUI.Remove(agent);
        }
    }
    //Pass along information to the DroneUI to be updated
    public void UpdateUI(AgentBehaviour agent, Want.WantType wantType, int newAmount)
    {
        if (agentToUI.ContainsKey(agent))
        {
            agentToUI[agent].GetComponent<DroneUI>().UpdateSlider(wantType, newAmount);
        }
    }
}
