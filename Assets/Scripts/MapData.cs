using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapData : MonoBehaviour, INotify
{
    private static MapData s_instance;
    //list of locations 
    private List<GameObject> m_rests = new List<GameObject>();
    private List<GameObject> m_foods = new List<GameObject>();
    private List<GameObject> m_shops = new List<GameObject>();
    //Valid Tags we will be looking for
    const string kRest = "Rest";
    const string kFood = "Food";
    const string kShop = "Shop";
    private Dictionary<Want.WantType, string> locationToTag = new Dictionary<Want.WantType, string>
    {
        { Want.WantType.k_food, "Food" },
        { Want.WantType.k_energy, "Rest" },
        { Want.WantType.k_desire, "Shop" }
    };

    public static MapData GetMap() 
    { 
        if (s_instance == null)
        {
            var map = FindObjectOfType<MapData>();
            if (map == null)
                Debug.LogError("No Map Found");
            else
                return s_instance = map;
        }
        return s_instance; 
    }

    #region LoadMapData
    private void Awake()
    {
        //set up the objects lists
        m_rests.AddRange(GameObject.FindGameObjectsWithTag(kRest));
        m_foods.AddRange(GameObject.FindGameObjectsWithTag(kFood));
        m_shops.AddRange(GameObject.FindGameObjectsWithTag(kShop));

        //add the notify components
        AttachNotifier(m_rests);
        AttachNotifier(m_foods);
        AttachNotifier(m_shops);
    }

    private void AttachNotifier(List<GameObject> objects)
    {
        foreach(var obj in objects)
        {
            if(obj.GetComponent<StatusNotifier>() == null)
            {
                var notifier = obj.AddComponent<StatusNotifier>();
                notifier.SetWhoToNotify(this);
            }
        }
    }
    #endregion LoadMapData

    #region MapManagement

    //Calls to notify the Map when an object is activated or deativated
    public void Notify(GameObject obj)
        {
            //Manage the list associated with the obj tag
            switch (obj.tag)
            {
                case kFood:
                    ListManagement(m_foods, obj);
                    break;
                case kRest:
                    ListManagement(m_rests, obj);
                    break;
                case kShop:
                    ListManagement(m_shops, obj);
                    break;
                default:
                    Debug.LogWarning(obj.tag + " is not a managed tag on Map Data");
                    break;
            }
        }
    //Manage the map locations when they are activated or deactivated
    private void ListManagement(List<GameObject> list, GameObject obj)
    {

        bool toAdd = obj.activeInHierarchy;         //Add if this object is active
        bool isContained = list.Contains(obj);      //Check if the list contains this object
        if (!isContained && toAdd)                  //If the object is not in the list and I need to add
            m_foods.Add(obj);
        else if (isContained && !toAdd)             //If the object is in the list and I need to remove
            m_foods.Remove(obj);
    }

    public List<GameObject> GetLocations(Want.WantType type)
    {
        switch (type)
        {
            case Want.WantType.k_food:        return m_foods;
            case Want.WantType.k_energy:      return m_rests;
            case Want.WantType.k_desire:      return m_shops;
            default:                          return null;
        }        
    }

    #endregion MapManagement
}
