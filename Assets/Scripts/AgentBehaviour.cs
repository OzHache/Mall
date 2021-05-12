using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


public class AgentBehaviour : MonoBehaviour
{
    private static System.Random rng = new System.Random();

    private AgentMovement m_movement;
    private string m_name;
    private Drone m_drone;
    private Transform m_startTransform;
    //List of wants by type
    private Dictionary<Want.WantType, Want> m_wants = new Dictionary<Want.WantType, Want>();
    //List of Wants by need
    Dictionary<Want.WantType, float> m_wantsByNeed = new Dictionary<Want.WantType, float>();
    //Current Need
    private Want.WantType m_currentNeed = Want.WantType.k_desire;
    //List of Wants I cannot satisfy
    List<Want.WantType> m_cannotSatisfy = new List<Want.WantType>();

    //Shop Interest desire
    private Dictionary<Shop.Interest, bool> m_satisfiedInterest = new Dictionary<Shop.Interest, bool>();
    private List<Shop.Interest> m_orderOfInterest = new List<Shop.Interest>();
    [SerializeField]private bool m_printDebug = false;
    [SerializeField] private bool m_printDebugWarning = false;
    [SerializeField] private bool m_printDebugError = false;
    public bool PrintDebugForAgent() { return m_printDebug; }

    //ONLY TRUE IF WE ARE LEAVING 
    private bool m_isStaying = true;
    //Allows us to ignore other urgent needs until this one is handled
    private bool m_handleUrgentNeed = false;
    private MallZone m_reservation;

    /// <summary>
    /// Used by the Drone UI to Populate the UI
    /// </summary>
    /// <returns></returns>
    public Dictionary<Want.WantType, Want> GetWants()
    {
        var wants = m_wants;
        return wants;
    }

    //UI Support for getting the name
    internal string GetName() { return m_name; }

    /// <summary>
    /// Initial call from the drone Manager to have the Drone set up on the Agent Behaviour
    /// </summary>
    /// <param name="drone">Data Pack of Behaviours</param>
    public void SetDrone(Drone drone)
    {
        m_drone = drone;
        m_name = drone.m_name;
        //set Up Desire Interest
        m_orderOfInterest = m_drone.m_orderOfDesires;
        foreach(var interest in m_orderOfInterest)
        {
            m_satisfiedInterest.Add(interest, false);
        }
    }

    internal void GoToSeat(GameObject gameObject)
    {
        m_movement.MoveTo(gameObject, null, null);
    }

    /// <summary>
    /// Call to initialize the Agent Bahaviour from the Drone Manager
    /// </summary>
    public void Initialize()
    {
        StopAllCoroutines();
        
        //Set the start position
        m_startTransform = transform;
        //Reset the Drone wants
        ResetDrone();
        //ensure there is a movement agent
        m_movement = GetComponent<AgentMovement>();
        if (m_movement == null)
        {
            if(m_printDebugError) Debug.LogError("This object does not have an AgentMovement Component");
        }
        m_isStaying = true;
        m_movement.StopAllCoroutines();
        StartCoroutine(StartAI());
    }

    /// <summary>
    /// Called ONCE to intialize the player, This will iterate and update the UI
    /// Will stop once the player is leaving
    /// </summary>
    /// <returns>This is a coroutine</returns>
    private IEnumerator StartAI()
    {
        StartCoroutine(HeartBeat());
        //On the first pass we will always find a new behaviour
        bool first = true;
        foreach (var want in m_wants)
        {
            //Skip this step if m_wants by need exist (Respawn
            if (m_wantsByNeed.ContainsKey(want.Key))
            {
                continue;
            }
            m_wantsByNeed.Add(want.Key, want.Value.GetNeed());
        }
        //clear my list of wants I cannot satisfy, You know, things change.
        m_cannotSatisfy.Clear();
        m_cannotSatisfy.Add(Want.WantType.k_population);
        //while the Agent is not leaving
        while (m_isStaying)
        {
            //Update all wants with the calculated need
            foreach (var need in m_wants)
            {
                var type = need.Key;
                m_wantsByNeed[type] = m_wants[type].GetNeed();
                if(m_printDebug) Debug.Log(gameObject.name + " " + type.ToString() + " has Value " + m_wantsByNeed[type].ToString());
            }
            foreach (var type in m_wantsByNeed)
            {
                //if any value is set to 1 then we leave
                if (m_wants[type.Key].GetWantValue() == 0)
                {
                    //Leave
                    if(m_printDebugError)Debug.LogError(gameObject.name + " left because need " + type.Key.ToString() + "  was not satisfied");
                    Leave();
                    yield break;                                ///STOP ALL AI BEHAVIOURS AND LEAVE

                }
                if(type.Value < 0.5 && !m_handleUrgentNeed)    ///THIS IS A CRITICAL NEED as long as I don't have another
                {
                    if(m_printDebugWarning)Debug.LogWarning("CriticalNeed" + gameObject.name + "For want " + type.Key.ToString() + " = " + type.Value.ToString());
                    if (m_cannotSatisfy.Contains(type.Key))
                        continue;                              //Sorry, this need cannot be met
                    m_currentNeed = type.Key;
                    m_handleUrgentNeed = true;
                    FindLocationForNeed();
                    if (m_isStaying == false)                   //Catch just in case
                        yield break;
                }
            }
            //on first pass always find a new behaviour, after this we only look for a new behaviour when it is critical, urgent or we have satisfied our current need
            if (first)
            {
                first = false;
                FindNewBehaviour();     //FIRST REFERCENCE FROM THE AI
            }
            //if we are not moving
            /*if (!m_movement.IsMoving())
            {
                bool stalled = true;
                //see if our wants are being met
                foreach(var want in m_wants)
                {
                    if(want.Value.IsFulfilling())
                        stalled = false;
                }
                if (stalled)
                    //Catch if our Agent is stuck for some reason :) some reason .....I'm looking at you Susie
                    FindLocationForNeed();
            }*/
            //nominal delay helps to spread the load, over time this will create a cadance so behaviours are not updated together
            yield return new WaitForSeconds(1.0f);
        }
        
        if(m_printDebug) Debug.Log("Agent has exited AI behaviour");
    }

    /// <summary>
    /// If at any time I have a need at zero, direct this agent to leave
    /// </summary>
    /// <returns>Null</returns>
    private IEnumerator HeartBeat()
    {
        while (true)
        {
            foreach (var want in m_wants)
            {
                m_isStaying = false;
                if (want.Value.GetWantValue() == 0)
                    if (m_printDebugWarning) Debug.LogWarning(gameObject.name + " Is leaving under heart Beat for " + want.Key.ToString());
                if (!m_movement.IsLeaving(GameObject.FindGameObjectWithTag("Exit")))
                {
                    Leave();
                }
            }
        }
    }
    /// <summary>
    /// Behaviour is determined by finding the lowest value and setting that as the current need. 
    /// Behaviours are calculated by the wants based on needs
    /// </summary>
    /// <returns>Want for the behaviour to perform</returns>
    private Want.WantType DetermineBehaviour()
    {
        //key value pair to represent the lowest value
        KeyValuePair<Want.WantType, float> lowest = new KeyValuePair<Want.WantType, float>(Want.WantType.k_sentinel, 10.0f);

        foreach(var want in m_wantsByNeed)
            if (want.Value < lowest.Value)
                //replace the lowest
                if(!m_cannotSatisfy.Contains(want.Key))
                    lowest = want;

        //make a list of all the wants within 10% of this one and
        float benchMark = lowest.Value * 0.1f;
        List<KeyValuePair<Want.WantType, float>> considering = new List<KeyValuePair<Want.WantType, float>>();
        foreach (var want in m_wantsByNeed)
        {
            //skip wants I cannot satisfy
            if (m_cannotSatisfy.Contains(want.Key))
                continue;
            if (Mathf.Abs(want.Value - lowest.Value) <= benchMark)
            {
                if(m_printDebug) Debug.Log(gameObject.name + " is considering " + want.Key + " with value " + want.Value);
                considering.Add(want);
            }
        }
        //if I am down to one, early out
        if(considering.Count == 1)
        {
            return considering[0].Key;
        }
        //factor in weight
        foreach(var want in m_wantsByNeed)
        {
            if (m_cannotSatisfy.Contains(want.Key))
                continue;
            var weight = m_wants[want.Key].GetWeight();
            KeyValuePair<Want.WantType, float> check;
            foreach( var consideration in considering)
            {
                if(consideration.Key == want.Key)
                {
                    check = consideration;
                    break;
                }
            }
            //update the value
            KeyValuePair<Want.WantType, float> weightedValue = new KeyValuePair<Want.WantType, float>(check.Key, check.Value - (check.Value * weight));
            //add and remove to update the values
            considering.Remove(check);
            considering.Add(weightedValue);
        }
        lowest = considering[1];
        for(int i = 0; i < considering.Count; i++)
        {
            if(considering[i].Value < lowest.Value)
            {
                lowest = considering[i];
            }
        }



        if(m_printDebug) Debug.Log(gameObject.name + " lowest = " + lowest.Key + " at " + lowest.Value);
        return lowest.Key;
    }

    /// <summary>
    /// Leave call for when we are fully not satisfied
    /// called if there are no shops we like and if any need falls to 0
    /// </summary>
    private void Leave()
    {
        if (m_reservation != null)
            m_reservation.CancelReservation(this);
        //Stop the AI
        m_isStaying = false;
        StopAllCoroutines();
        //Go to the exit
        var location = GameObject.FindGameObjectWithTag("Exit");
        m_movement.MoveTo(location, null, null);
        
    }

    /// <summary>
    /// Find a location based on the need, 
    /// Two Routes
    ///  :For desires, find a location
    ///  :For any other want, find closest
    /// </summary>
    private void FindLocationForNeed()
    {
        //if for some reason we are here and our current need is Sentinel then set current to desire
        if (m_currentNeed == Want.WantType.k_sentinel)
            m_currentNeed = Want.WantType.k_desire;

        if(m_printDebug) Debug.Log(gameObject.name + " Looking for a location for" + m_currentNeed.ToString());
        //if I am looking for a desire Spot
        if(m_currentNeed == Want.WantType.k_desire)
        {
            GameObject obj;
            //find a location for 
            bool found = FindLocationForDesire(out obj);
            //if we have found a location, go there
            if (!found)
            {
                //this means there is no place I am interest in. So I will leave;
                if(m_printDebug) Debug.Log("Cannot find a location so I am leaving " + m_name + "" + gameObject.name);
                
                Leave();
                return;
            }
            if(m_printDebug) Debug.Log(gameObject.name +" is moving to " + obj.name);
            m_movement.MoveTo(obj, null, null);
            return;
        }

        //For all other searches, locate a location for this type
        var locations = MapData.GetMap().GetLocations(m_currentNeed);
        //If there are no locations for this need then seach for 
        if (locations == null || locations.Count == 0)
        {
            if(m_printDebug) Debug.LogError(gameObject.name + " Cannot find a location to satisfy " + m_currentNeed.ToString());
            m_cannotSatisfy.Add(m_currentNeed);
            //Find a new behaviour
            FindNewBehaviour();     //WHEN I CANNOT SATISFY THIS DESIRE
            return;
        }
        //look through the locations and find the closest one
        if(m_printDebug) Debug.Log("looking for closest location " + m_currentNeed.ToString());
        GameObject location = FindClosestLocation(locations);
        if(location == null)
        {
            if(m_printDebug) Debug.LogError(gameObject.name + " Cannot find a location to satisfy " + m_currentNeed.ToString());
            m_cannotSatisfy.Add(m_currentNeed);
            //Find a new behaviour
            FindNewBehaviour();     //WHEN I CANNOT SATISFY THIS NEED
            return;
        }
        m_movement.MoveTo(location, null, null);
    }

    /// <summary>
    /// When trying to satisfy desires, look for a location that matches my top interest first and then if it is satisfied, go to the next
    /// </summary>
    /// <param name="obj">location that will satisfy Desire and one of my interest</param>
    /// <returns>If a location that fulfilles desire and one of my interest</returns>
    private bool FindLocationForDesire(out GameObject obj)
    {
        obj = null;
        //keep track of interests that are doomed to fail so they can be removed
        List<Shop.Interest> removeInterest = new List<Shop.Interest>();
        //if I have no interests left in the mall to satisfy
        if (m_orderOfInterest.Count == 0)
        {
            //early out, leave will be called in the calling method
            return false;
        }
        //Label for searching will be called again in the event that I failed to find a interest that was not already satisfied
        SearchLoop:
        for (int i = 0; i < m_orderOfInterest.Count; i++)
        {
            //look to see if we have already satisfied this interest
            if (m_satisfiedInterest[m_orderOfInterest[i]])
                continue;

            //first look for this interest Location
            if(FindLocationForInterest(m_orderOfInterest[i], out obj))
            {
                if(m_printDebug) Debug.Log(gameObject.name + " satisfying" + m_orderOfInterest[i] + " with" + obj.name);
                m_satisfiedInterest[m_orderOfInterest[i]] = true;
                return true;
            }
            else //Failed to find a location for this interest
            {
                if(m_printDebug) Debug.Log(gameObject.name + " Could could not satisfy " + m_orderOfInterest[i]);
                removeInterest.Add(m_orderOfInterest[i]);
            }
        }
        //clean up interest
        foreach (var interest in removeInterest) 
        {
            m_satisfiedInterest.Remove(interest);
            m_orderOfInterest.Remove(interest);
        }
        //at this point if I have already satisfied all possibilities but have not found a location, as long as I have satisfied interest, reset and start the search again
        if(m_satisfiedInterest.Count > 0)
        {
            //reset all interst to not satisfied
            foreach(var interest in m_satisfiedInterest.Keys.ToList())
                m_satisfiedInterest[interest] = false;
            // now try the search again
            goto SearchLoop;
        }
        
        //Failed to find any location
        if(m_printDebug) Debug.LogError(gameObject.name + " Could not find any intersting places");
        //leave will be called from the calling method
        return false;
    }

    /// <summary>
    /// Look for a shop that satisfies a specfic interest
    /// </summary>
    /// <param name="interest">The interst from my desires</param>
    /// <param name="obj">Location that I will hopefully find</param>
    /// <returns>If I found a location that matches my interest</returns>
    private bool FindLocationForInterest(Shop.Interest interest, out GameObject obj)
    {
        obj = null;
        //get all locations for Desire
        var locations = MapData.GetMap().GetLocations(Want.WantType.k_desire);
        //list of all possible locations to be populated if they match my interest
        List<GameObject> possibleLocations = new List<GameObject>();

        //try and find a location that is highest in my order of interest that available

        foreach (var location in locations)
        {
            //get the shop and add it if it matches this interest
            var shop = location.GetComponent<Shop>();
            if (shop != null && shop.GetInterest() == interest)
                possibleLocations.Add(location);
        }
        if (possibleLocations.Count > 0)    //I have found valid locations
        {
            m_satisfiedInterest[interest] = true;
            //Get a random location so I don't occilate between the few choices I may have
            //shuffle the possible locations 
            possibleLocations = possibleLocations.OrderBy(a =>rng.Next()).ToList();
            for(var i = 0; i < possibleLocations.Count; i++)
            {
                //see if this location is accepting reservations
                if (m_reservation != null)
                {
                    if(m_printDebug) Debug.Log(gameObject.name +" Cancel reservation with " + m_reservation);
                    m_reservation.CancelReservation(this);
                    
                }
                m_reservation = null;

                MallZone reservation;
                if (possibleLocations[i].GetComponent<MallZone>().ResearveSeat(this,  out reservation))
                {
                    obj = possibleLocations[i];
                    if(m_printDebug) Debug.Log(gameObject.name + " Made reservation with " + reservation);

                    m_reservation = reservation;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Find the closest location
    /// </summary>
    /// <param name="locations"> list of possible locations</param>
    /// <returns>closest Game Object</returns>
    private GameObject FindClosestLocation(List<GameObject> locations)
    {
        if (locations == null || locations.Count == 0)
            return null;

        //sort location by crow flies distance
        locations.Sort(delegate (GameObject a, GameObject b)
            {
                if (a == null && b == null) return 0;
                else if (a == null) return -1;
                else if (b == null) return 1;
                else return Vector3.Distance(a.transform.position, transform.position).CompareTo(Vector3.Distance(b.transform.position, transform.position));
            });
        for (var index = 0; index < locations.Count - 1; index++)
        {
            //see if this location accepts reservations
            //get the mallZone on this location
            var location = locations[index].GetComponent<MallZone>();
            if (location != null)
            {
                if (m_reservation != null)
                {
                    m_reservation.CancelReservation(this);
                    if(m_printDebug) Debug.Log(gameObject.name + " Canceled reservation with " + m_reservation);
                }
                m_reservation = null;
                MallZone reservation;
                if (locations[index].GetComponent<MallZone>().ResearveSeat(this, out reservation))
                {
                    if(m_printDebug) Debug.Log(gameObject.name + " Made reservation with " + reservation);
                    m_reservation = reservation;
                    return locations[index];
                }

            }
            else
            {
                if(m_printDebug) Debug.LogError(locations[index].name + " does not have a MallZone");
            }
        }
            if(m_printDebug) Debug.LogError("no location found");
            return null;
    }

    /// <summary>
    /// Reset all wants and needs on the drone
    /// </summary>
    internal void ResetDrone()
    {
        //Get a new set of interest
        m_drone.ReRollInterest();

        //This should only run the first time, subsequent respawns will skip
        if (m_wants.Count == 0)
        {
            //build each want
            //Food
            var foodType = Want.WantType.k_food;
            m_wants.Add(foodType, gameObject.AddComponent<Want>());
            m_wants[foodType].SetNeed(
                m_drone.m_food ,
                () => UIManager.GetInstance().UpdateUI(
                    this , 
                    foodType , 
                    m_wants[foodType].GetWantValue()) ,
                    foodType
                );
            //Energy
            var energyType = Want.WantType.k_energy;
            m_wants.Add(energyType, gameObject.AddComponent<Want>());
            m_wants[energyType].SetNeed(
                m_drone.m_energy,
                () => UIManager.GetInstance().UpdateUI(
                    this , 
                    energyType ,
                    m_wants[energyType].GetWantValue()) ,
                    energyType
                );
            //Desire
            var desireType = Want.WantType.k_desire;
            m_wants.Add(desireType, gameObject.AddComponent<Want>());
            m_wants[desireType].SetNeed(
                m_drone.m_desire ,
                () => UIManager.GetInstance().UpdateUI(
                    this ,
                    desireType , 
                    m_wants[desireType].GetWantValue()) ,
                    desireType
                );
            //Population
            var populationType = Want.WantType.k_population;
            m_wants.Add(populationType, gameObject.AddComponent<PopulationWant>());
            m_wants[populationType].SetNeed(
                m_drone.m_population,
                () => UIManager.GetInstance().UpdateUI(
                    this , 
                    populationType , 
                    m_wants[populationType].GetWantValue()) ,
                    populationType
                );
        }
        foreach(var want in m_wants)
        {
            want.Value.Reset();
            want.Value.Initialize();
        }
        //reposition the Agent
        if (m_startTransform != null)
        {
            transform.position = m_startTransform.position;
            transform.rotation = m_startTransform.rotation;
        }
        //Add this drone to the UI
        UIManager.GetInstance().AddDrone(this);
    }

    /// <summary>
    /// Triggered by shops near me
    /// </summary>
    /// <param name="type"> what they are selling</param>
    /// <param name="amount"> satisfaction for me</param>
    public bool CustomerAction(Want.WantType type, int amount)
    {
        //if this is what I want
        if (m_currentNeed == type)
        {
            if (m_wants[type].AffectWant(amount))
            { //Trigger if I am fully satisfied
                if(m_printDebug) Debug.LogError(gameObject.name + " is fully satisfied and will look for something");
                FindNewBehaviour();
                return true; //we are done with this location
            }
            else
            {
                return false;// we are not yet fully satisfied
            }
        }
        else
        {
            //if this is not the need we need, find the place we need
            FindLocationForNeed();
            return true;    //this will automatically cancle the reservation
        }
    }

    /// <summary>
    /// Calculate all possible needs from each want and choose the lowest
    /// </summary>
    private void FindNewBehaviour()
    {
        if (!m_isStaying)
        {
            return;
        }
        if(m_printDebug) Debug.Log("Look for a new Behaviour " + m_name + " " + gameObject.name);
        Want.WantType lowestNeed = DetermineBehaviour();
        //if want type is Sentinel then leave
        if (lowestNeed == Want.WantType.k_sentinel)
        {
            Leave();
            return;
        }

        //see if that is the Need I am workin currently
        //if (lowestNeed != m_currentNeed)
        {
            if(m_printDebug) Debug.Log(gameObject.name + ": replace current want " + m_currentNeed.ToString() + " with " + lowestNeed.ToString());
            m_currentNeed = lowestNeed;
            //Standard location to call this Method
            if (m_handleUrgentNeed)
                m_handleUrgentNeed = false;
            FindLocationForNeed();  
        }
    }
    
    /// <summary>
    /// Called from the shops If I am being serviced
    /// </summary>
    /// <param name="type">what they are selling</param>
    /// <param name="isServiced">if they are trying to offer me service</param>
    public void BeingServiced(Want.WantType type, bool isServiced)
    {
        if (isServiced && type == m_currentNeed)
            m_wants[type].Fullfilling();
        else
            m_wants[type].StopFulfilling();
    }
}
