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

    // the state
    public AgentState GetState() { return m_state; }

    private IEnumerator WaitUntilBehaviourComplete()
    {
        
        yield return new WaitUntil(() => m_state == AgentState.kSucceed || m_state == AgentState.kFail);
        RunEndAction();
    }
    public void MoveTo(GameObject destination, Action onSuccess, Action onFail)
    {
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
        //Set the success and fail actions
        m_success = onSuccess;
        m_fail = onFail;
        //Start the waiting behaviour
        StartCoroutine(WaitUntilBehaviourComplete());
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
}
