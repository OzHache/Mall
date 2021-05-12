using System.Collections;
using System;
using UnityEngine;
using UnityEngine.AI;
//State of the agent
public enum AgentState { kRunning, kSucceed, kFail, kIdle }

[RequireComponent(typeof(NavMeshAgent))]
public class AgentMovement : MonoBehaviour
{
    [SerializeField]private NavMeshAgent m_agent;
    private AgentState m_state = AgentState.kIdle;
    private Action m_success;
    private Action m_fail;
    private float m_startingDistance = 0.5f;
    private float m_checkDistance;

    private void Start()
    {
        StartCoroutine(WaitUntilBehaviourComplete());
    }
    public bool IsMoving() { return !m_agent.isStopped; }

    // the state
    public AgentState GetState() { return m_state; }

    private IEnumerator WaitUntilBehaviourComplete()
    {

        //start high, go lower as you get there, and then when you are sitting, have resting high avoidance priority
        
        while (true)
        {
            //If we have effectvely stopped
            
            if (Vector3.Distance(m_agent.destination, transform.position) < m_agent.stoppingDistance || m_agent.isStopped)
            {
                m_agent.avoidancePriority = 0;
                yield return null;
            }
            // if calculated distance is greater than our crow flies distance, ignore it
            //Or we have not found a location yet to go to
            else if (m_agent.remainingDistance > m_startingDistance || m_startingDistance == 0 )
            {
                m_agent.avoidancePriority = 40;
            }
            
            //set the prioity of this agent based on how far it is to thier destination. 
            //further is worth more
            else
            {
                float newPriority = (m_agent.remainingDistance / m_startingDistance);
                int intPriority = (int)(100 * newPriority);
                m_agent.avoidancePriority = intPriority;
                //real quick ray trace
                NavMeshHit hit;
                m_agent.Raycast(transform.forward, out hit);
                if(hit.distance < m_agent.radius + m_checkDistance)
                {
                    //recalculate the path
                    //m_agent.SetDestination(m_agent.destination);
                }

            }
            
            yield return null;

        }
    }
    public void MoveTo(GameObject destination, Action onSuccess, Action onFail)
    {
        
        m_agent.isStopped = true;
        //set the state to running
        m_state = AgentState.kRunning;
        m_agent.SetDestination(destination.transform.position);
        if(m_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            if (onFail != null)
            {
                onFail.Invoke();
                return;
            }
        }
        m_agent.isStopped = false;
        //Set the success and fail actions
        m_success = onSuccess;
        m_fail = onFail;
        //Start the waiting behaviour
        m_startingDistance = Vector3.Distance(transform.position, destination.transform.position);
        //StartCoroutine(WaitUntilBehaviourComplete());
    }


    /// <summary>
    /// run the ending action that corresponds to the end state
    /// </summary>
    private void RunEndAction()
    {
        if (m_state == AgentState.kFail && m_success != null)
            m_success.Invoke();
        else if (m_state == AgentState.kFail && m_fail != null)
            m_fail.Invoke();

        //clear the actions saved;
        m_success = null;
        m_fail = null;
    }

    /// <summary>
    /// Use the Agent to determine the distance
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>travel distance</returns>
    internal float GetDistance(Vector3 obj)
    {
        float distance = float.MaxValue;
        var currentDestination = m_agent.destination;
        m_agent.SetDestination(obj);
        if (m_agent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            distance = m_agent.remainingDistance;
        }
        return distance;
    }

    internal bool IsLeaving(GameObject exit)
    {
        return Vector3.Distance(m_agent.destination, exit.transform.position) < 0.5f;
    }
}
