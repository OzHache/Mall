using System;

public class Population : INeed
{
    private float m_max;
    private float m_current;
    public Population(int max)
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
        //always BUT the amount we use is based on the density of people around us
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
        throw new NotImplementedException();
    }
}
