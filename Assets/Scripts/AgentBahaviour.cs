using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AgentBahaviour : MonoBehaviour
{
    private AgentMovement movement;

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<AgentMovement>();
        if(movement == null)
        {
            Debug.LogError("This object does not have an AgentMovement Component");
        }
        Test();
    }

    private void Test()
    {
        var locations = MapData.GetMap().GetLocations(LocationType.kFood);
        if (locations.Count == 0)
        {
            Debug.LogError("No locations found");
            return;
        }
        GameObject location = FindClosestLocation(locations);
        movement.MoveTo(location, null, null);

    }

    private GameObject FindClosestLocation(List<GameObject> locations)
    {
        Assert.IsNotNull(locations);
        //find the closest
        var closest = locations[0];
        float distance = Vector3.Distance(transform.position, closest.transform.position);
        foreach (var location in locations)
        {
            var newDist = Vector3.Distance(transform.position, location.transform.position);
            if(distance >  newDist)
            {
                closest = location;
                distance = newDist;
            }
        }
        return closest;
    }
}
