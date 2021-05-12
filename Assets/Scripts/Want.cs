
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Want: MonoBehaviour
{
    public enum WantType { k_energy, k_food, k_desire, k_population, k_sentinel}
    //Managed by the base Want Class
    protected bool m_isFulfilling = false;
    protected int m_dropPerCycle;
    protected float m_secondsPerCycle;
    protected Action m_needUpdate;
    private float m_weight;
    //explosed to child classes
    protected int m_wantMax;
    protected int m_wantLevel;
    protected INeed m_need;
    
    /// <summary>
    /// The Calculated need 0 is extream 1 or more is fully satisfied
    /// </summary>
    /// <returns></returns>
    public float GetNeed()
    {
        //root is 0-1 with 0 being full need and 1 being no need
        var root = m_need.CalculateNeed();
        //weight is a growing value that can be added the more there is need based on how much need
        var weight = Mathf.Clamp((1 - root) * m_weight, 0, m_weight);
        //need is root - weight which draws it down purportional to the need gap
        var need = Mathf.Clamp(root - weight, 0, 1);
        return need;


    }
    //This is the RAW wantlevel and does not include any modifiers use for UI)
    public int GetWantValue() { return m_wantLevel;}

    /// <summary>
    /// Set up the need
    /// </summary>
    /// <param name="max">Maximum level when completly fulfilled </param>
    /// <param name="dropPerCycle">How much satisfaction is lost per cycle</param>
    /// <param name="cycleLength">How long each cycle is</param>
    /// <param name="needUpdate">Action to perform when a cycle is updated (hook up to UI)</param>
    public void SetNeed(WantBase wantBase, Action needUpdate, WantType type)
    {
        //build a wantbase from the drone
        m_wantLevel = wantBase.m_fullSatisfied;
        m_wantMax = wantBase.m_fullSatisfied;
        m_dropPerCycle = wantBase.m_dropPerCycle;
        m_secondsPerCycle = wantBase.m_cycleLength;
        m_needUpdate += needUpdate;
        m_weight = wantBase.weight;
        //build the Need behind the want :)
        BuildNeed(type);
    }

    protected virtual void BuildNeed(WantType type)
    {
        switch (type)
        {
            case WantType.k_desire:                 m_need = new Desire(m_wantMax);
                break;
            case WantType.k_energy:                 m_need = new Energy(m_wantMax);
                break;
            case WantType.k_food:                   m_need = new Food(m_wantMax);
                break;
            //case WantType.k_population:             m_need = new Population(m_wantMax);
                //break;
        }
    }

    public void Fullfilling() => m_isFulfilling = true;
    public void StopFulfilling() => m_isFulfilling = false;
    public void  Initialize() { 
        StartCoroutine(Cycle()); 
    }

    public virtual bool AffectWant(int amount)
    {
        m_wantLevel = Mathf.Clamp(m_wantLevel + amount, 0, m_wantMax);
        bool satisfied = m_need.Satisfied(amount);
        m_needUpdate.Invoke();
        return satisfied;
    }

    internal virtual bool IsFulfilling()
    {
        return m_isFulfilling;
    }

    protected virtual IEnumerator Cycle()
    {
        while (true)
        {
            //slowly drop the need value
            yield return new WaitForSeconds(m_secondsPerCycle);
            if (!m_isFulfilling)
            {
                if (m_need.CycleCondition(m_dropPerCycle))
                {
                    m_need.Affect(m_dropPerCycle);
                    m_wantLevel -= m_dropPerCycle;
                    m_needUpdate.Invoke();
                }
            }
        }
    }

    internal void Reset()
    {
        m_wantLevel = m_wantMax;
        if (m_need != null)
            m_need.Reset();
    }

    internal float GetWeight()
    {
        return m_weight;
    }
}
