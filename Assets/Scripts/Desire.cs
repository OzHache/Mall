using System;
using UnityEngine;

public class Desire : INeed
{
    private float m_max;
    private float m_current;
    public Desire(int max)
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
        //always , that is why we are at the mall
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
