using System;
using UnityEngine;

public class Energy : INeed
{
    private float m_max;
    private float m_current;
    public Energy(int max)
    {
        //Set the default values
        m_max = max;
        m_current = max;
    }
    public bool Affect(int amount)
    {
        m_current -= amount;
        return true;

    }

    public float CalculateNeed()
    {
        return m_current / m_max;
    }

    public bool CycleCondition(int amount)
    {
        //Need to see if we are currently shoping. Otherwise we don't lose energy
        return true;
    }

    public void Initialize(int max)
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        m_current = m_max;
    }

    public bool Satisfied(int amount)
    {
        m_current = Mathf.Clamp(m_current + amount, 0, m_max);
        return m_current == m_max;
    }
}
