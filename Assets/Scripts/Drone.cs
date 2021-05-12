using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
[CreateAssetMenu(fileName = "NewDrone", menuName = "ScriptableObjects/Drone", order = 1)]

public class Drone : ScriptableObject
{
    
    public string m_name;
    //Wants
    public WantBase m_food;
    public WantBase m_population;
    public WantBase m_energy;
    public WantBase m_desire;
    public List<Shop.Interest> m_orderOfDesires;
    private static System.Random rng = new System.Random();
    //this should be called by ANY drone of this type that cannot find ANY interst in the mall
    public void ReRollInterest()
    {
        //build a new list of interest
        var lenght = m_orderOfDesires.Count;
        List<Shop.Interest> newInterest = new List<Shop.Interest>();
        foreach (var item in Enum.GetValues(typeof(Shop.Interest)))
        {
            newInterest.Add((Shop.Interest)item);
        }
        m_orderOfDesires = newInterest;
        m_orderOfDesires = m_orderOfDesires.OrderBy(a => rng.Next()).ToList();
    }
   
}
[Serializable]
public class WantBase
{
    public float weight;
    public int m_fullSatisfied;
    public float m_cycleLength;
    public int m_dropPerCycle;
}

