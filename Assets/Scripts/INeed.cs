using System;

public interface INeed
{
    //trigger an Affect to the need
    abstract public bool Affect(int amount);
    //Returns if the cycle should apply the condition
    abstract public bool CycleCondition(int amount);

    abstract public float CalculateNeed();

    abstract public void Initialize(int max);
    abstract public bool Satisfied(int amount);
    abstract void Reset();
}
