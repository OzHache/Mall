using System;
using UnityEngine;
public class Food : INeed
{
    private float m_max;
    private float m_current;
    private float m_saturation;
    public Food(int max)
    {
        //Set the default values
        m_max = max;
        m_current = max;
        m_saturation = max;
    }
    public void Initialize(int max)
    {
        //reset all values to max
        m_current = max;
        m_saturation = max;
    }
    /// <summary>
    /// Raw Affect to the current Value
    /// </summary>
    /// <param name="amount">amount to affect the value by, still depreceates saturation first</param>
    public bool Affect(int amount)
    {
        if (m_saturation > 0)
        {
            m_saturation -= amount;
            return false;
        }
        else
        {
            m_current -= amount;
            return true;
        }

    }

    /// <summary>
    /// Need is calculated Raw
    /// </summary>
    /// <returns>(internal) current / max</returns>
    public float CalculateNeed()
    {
        return m_current / m_max;
    }
    /// <summary>
    /// Detemines if the value should be decreased
    /// for food, when this is checked, saturation goes down and if it is below 0 then cycle should apply
    /// </summary>
    /// <returns> if should apply cycle degredation</returns>
    public bool CycleCondition(int amount)
    {
        if(m_saturation <= 0)
        {
            m_saturation -= amount;
            return true;
        }
        else
        {
            m_saturation -= amount;
            return false;
        }
    }

    /// <summary>
    /// Given a value to add to the current
    /// </summary>
    /// <param name="amount">amount to add to current</param>
    /// <returns>returns true ONLY if I am fully satisfied</returns>
    public bool Satisfied(int amount)
    {
        m_current = Mathf.Clamp(m_current + amount, 0, m_max);
        m_saturation = Mathf.Clamp(m_saturation + amount, 0, m_max);
        if (m_current == m_max)
        {
            m_saturation = m_max;
            m_saturation = m_max;
        }
        return m_current == m_max;
    }

    public void Reset()
    {
        m_current = m_max;
        m_saturation = m_max;
    }
}
