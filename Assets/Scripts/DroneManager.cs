using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    private static DroneManager s_instance;
    public static DroneManager GetInstance()
    {
        if (s_instance != null)
        {
            return s_instance;
        }
        else
        {
            //If for some reason I have not set up the UI managert yet, find it
            s_instance = FindObjectOfType<DroneManager>();
            if (s_instance == null)
            {
                Debug.LogError("No Drone manager found in the scene");
                return null;
            }
            return s_instance;
        }
    }

    [SerializeField] private List<Drone> m_droneTypes;
    [SerializeField] private GameObject m_dronePrefab;
    [SerializeField] private int m_numberOfDrones;
    [SerializeField] Transform m_spawnPosition;

    //Spawn rates
    [SerializeField] private float m_spawnInSeconds;
    [SerializeField] private float m_initialSpawnDelay;
    //This does not need to be exposed
    [SerializeField] private int m_currentNumberOfDrones; 
    private List<GameObject> m_dronePool = new List<GameObject>();
    private List<GameObject> m_activeDrones = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SmartPooling());
        StartCoroutine(SmartSpawning());
    }


    IEnumerator SmartPooling()
    {
        
        while ((m_activeDrones.Count + m_dronePool.Count) < m_numberOfDrones)
        {
            //Choose a drone type randomly
            int index = Random.Range(0, m_droneTypes.Count);
            var drone = Instantiate(m_dronePrefab);
            drone.name = m_droneTypes[index].m_name + " " + m_currentNumberOfDrones;
            drone.GetComponent<AgentBehaviour>().SetDrone(m_droneTypes[index]);
            //pool the drone
            drone.gameObject.SetActive(false);
            m_dronePool.Add(drone);
            m_currentNumberOfDrones++;
            yield return null;
        }
    }

    IEnumerator SmartSpawning()
    {
        yield return new WaitForSeconds(m_initialSpawnDelay);
        while (true)
        {
            if(m_activeDrones.Count < m_numberOfDrones)
            {
                //move a drone from pooled to active
                if(m_dronePool.Count > 0)
                {
                    //Move the drone from pooled to Acive
                    var drone = m_dronePool[0];                         
                    m_activeDrones.Add(drone);
                    m_dronePool.RemoveAt(0);
                    drone.transform.SetPositionAndRotation(m_spawnPosition.position, m_spawnPosition.rotation);
                    //Activate the Drone
                    drone.SetActive(true);
                    drone.GetComponent<AgentBehaviour>().Initialize();

                }
            }
            yield return new WaitForSeconds(m_spawnInSeconds);

        }
    }

    public void ExitDrone(GameObject obj)
    {
        //confirm this is a drone
        var agent = obj.GetComponent<AgentBehaviour>();
        if ( agent != null)
        {
            //turn off this object
            obj.SetActive(false);
            m_activeDrones.Remove(obj);
            m_dronePool.Add(obj);
            //tell the ui to drop the drone
            UIManager.GetInstance().RemoveDrone(agent);
        }
    }
}
