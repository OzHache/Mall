using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : MonoBehaviour
{
    private GameObject patron = null;

    public void AssignPatron(GameObject obj)
    {
        patron = obj;
    }
    public void ClearPatron()
    {
        patron = null;
    }
    //real simple

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == patron)
        {
            var agent = other.GetComponent<AgentBehaviour>();
            if (agent != null)
                transform.parent.GetComponent<MallZone>().SeatedCustomer(agent);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var agent = other.GetComponent<AgentBehaviour>();
        if (agent != null)
            transform.parent.GetComponent<MallZone>().UnseatedCustomer(agent);
    }
}
