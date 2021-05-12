using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MallZone : MonoBehaviour
{
    [SerializeField] Want.WantType m_want;
    [SerializeField] float m_serviceWait;
    [SerializeField] int m_serviceWorth;
    private List<AgentBehaviour> m_customers = new List<AgentBehaviour>();
    [SerializeField] protected Seat[] m_seats;

    protected AgentBehaviour[] m_reservations;

    internal void SeatedCustomer(AgentBehaviour agent)
    {
        agent.BeingServiced(m_want, true);
        if (m_customers.Contains(agent))
        {
            return;
        }
        else
        {
            m_customers.Add(agent);
        }
    }


    internal void UnseatedCustomer(AgentBehaviour agent)
    {
        
        if (agent != null)
        {
            foreach(var cust in m_reservations)
            {
                if (cust == agent)
                {
                    return;
                }
            }
            m_customers.Remove(agent);
            agent.BeingServiced(m_want, false);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ServiceCustomers());
        List<Seat> seats = new List<Seat>();
        //fill the seats
        for (var i = 0; i < transform.childCount; i++)
        {
            var seat = transform.GetChild(i).GetComponent<Seat>();
            if (seat != null)
                seats.Add(seat);
        }
        m_seats = new Seat[seats.Count];
        for (var i = 0; i < seats.Count; i++)
        {
            m_seats[i] = seats[i];
        }
        m_reservations = new AgentBehaviour[seats.Count];
    }

    public bool ResearveSeat(AgentBehaviour agent, out MallZone zone)
    {
        zone = this;
        //find first vacancy
        var reservationNumber = 0;
        for(; reservationNumber < m_reservations.Length; reservationNumber++)
        {
            if (m_reservations[reservationNumber] == null)
                break;
        }
        if (reservationNumber < m_reservations.Length)
        {
            //we have a vacancy
            m_reservations[reservationNumber] = agent;
            m_seats[reservationNumber].AssignPatron(agent.gameObject);
            return true;
        }
        return false;
    }
    public void CancelReservation(AgentBehaviour agent)
    {
        //find the reservation number
        var reservationNumber = 0;
        for(; reservationNumber<m_reservations.Length; reservationNumber++)
        {
            if (m_reservations[reservationNumber] == agent)
            {
                m_customers.Remove(agent);
                //we have found them so clear the reservation
                m_reservations[reservationNumber] = null;
                m_seats[reservationNumber].ClearPatron();
                return;
            }
        }
        if (agent.PrintDebugForAgent()) Debug.LogError(agent.gameObject.name + " does not have a reservation here " + gameObject.name);
        
        
    }
    protected IEnumerator ServiceCustomers()
    {
        while (true)
        {
            if (m_customers.Count == 0)
                yield return null;
            else { 
            var i = m_customers.Count -1;
                for (; i > 0; i--)
                {
                    var agent = m_customers[i];
                    if (agent.PrintDebugForAgent()) Debug.Log(gameObject.name + " Servicing " + agent.GetName() + " for " + m_want);

                    //if the agent is done here
                    if (agent.CustomerAction(m_want, m_serviceWorth))
                    {
                        //CancelReservation(agent);
                    }
                }
            }
            yield return new WaitForSeconds(m_serviceWait);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
    
        var agent = other.gameObject.GetComponent<AgentBehaviour>();
        if (agent != null)
        {
            //see if they have a reservation
            var seatNumber = 0;
            for(; seatNumber < m_reservations.Length; seatNumber++)
            {
                if(m_reservations[seatNumber] == agent)
                {
                    agent.GoToSeat(m_seats[seatNumber].gameObject);
                    return;
                }
            }
            //Debug.Log(agent.GetName() + " just passing through");
        }
    }





}
