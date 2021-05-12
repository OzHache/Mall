
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationWant : Want
    {
    //radius of x to check for nearby people
    [SerializeField] float m_areaToPoll = 10.0f;
    [SerializeField] int m_willIgnore = 2;
    [SerializeField] int m_distanceRecovery = 1;
    //Functions from base
    //GetNeed           -- Base can handle
    //GetWantValue      --Base can handle
    //Set Need          -- Base can handle
    //Fullfilling will not be used
    //stop fulfilling will not be used
    //Affect want will not be used
    //Reset             --Base can Handle
    //GetWeight         --base can handle
    protected override void BuildNeed(WantType type)
    {
        m_need = new Population(m_wantMax);
    }
    protected override IEnumerator Cycle()
    {
        while (true)
        {
            //slowly drop the need value
            yield return new WaitForSeconds(m_secondsPerCycle);
            if (!m_isFulfilling)
            {
                if (m_need.CycleCondition(m_dropPerCycle))
                {
                    //on a given cycle, look to see how many people are in my area of concern
                    List<Collider> collidersInRange = new List<Collider>(Physics.OverlapSphere(transform.position, m_areaToPoll));
                    //get only the people but ignore myself
                    collidersInRange.RemoveAll(item => !item.CompareTag("Player")||  item.gameObject == gameObject);
                    //sort by highest to lowest
                    collidersInRange.Sort(delegate (Collider a, Collider b)
                    {
                        if (a == null && b == null) return 0;
                        else if (a == null) return -1;
                        else if (b == null) return 1;
                        else return Vector3.Distance(a.transform.position, transform.position).CompareTo(Vector3.Distance(b.transform.position, transform.position));
                    });

                    if (collidersInRange.Count > 0)
                    {
                        //remove anyone I cannot see
                        for (var i = collidersInRange.Count - 1; i >= 0; i--)
                        {
                            //using the distance to this object
                            var distanceToCol = Vector3.Distance(transform.position, collidersInRange[i].transform.position);
                            var hits = Physics.RaycastAll(transform.position, collidersInRange[i].transform.position - transform.position);
                            foreach (var item in hits)
                            {
                                if (item.collider.CompareTag("Wall"))
                                {
                                    //if there is a wall that is closer, than I cannot see them
                                    if (item.distance < distanceToCol)
                                    {
                                        collidersInRange.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //with m_areaToPoll being a 0 and a distance of 0 being one, add all the possible distances together
                    float totalImpact = 0.0f;
                    //starting after the x closest objects ignoring those I am willing to ignore
                    for(var i = m_willIgnore; i < collidersInRange.Count; i++)
                    { 
                        //add the total impact with a 1 being right on top and a 0 being at the max distance away
                        totalImpact += (m_areaToPoll - Vector3.Distance(transform.position, collidersInRange[i].transform.position)) / m_areaToPoll; 
                    }
                    if(totalImpact == 0)
                    {
                        //start recovering 
                        totalImpact = -m_distanceRecovery;
                    }
                    int ceilingImpact = Mathf.CeilToInt(totalImpact);
                    m_need.Affect(ceilingImpact);
                    m_wantLevel -= ceilingImpact;
                    m_needUpdate.Invoke();
                }
            }
        }
    }

    internal override bool IsFulfilling()
    {
        return false;
    }

}