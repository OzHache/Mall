using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shop : MallZone
{
    public enum Interest { k_fishing, k_boutque, k_chocolate, k_clothing, k_electronic, k_pets, k_jewlery, k_officeSupplies }
    //generated on load
    private Interest m_interest;
    private TextMesh m_text;

    public Interest GetInterest() { return m_interest; }
    private void Awake()
    {
        Array interestTypes = Enum.GetValues(typeof(Interest));
        //choose a random interest
        m_interest = (Interest)interestTypes.GetValue(Random.Range(0, interestTypes.Length));
    }
    private void Start()
    {
        m_text = GetComponentInChildren<TextMesh>();
        switch (m_interest)
        {
            case Interest.k_fishing:            
                m_text.text = "Trouts";
                name += " Trouts";
                break;
            case Interest.k_boutque:            
                m_text.text = "ChillConvo";
                name += " ChillConvo";
                break;
            case Interest.k_chocolate:          
                m_text.text = "SeasCandy";
                name += " SeasCandy";
                break;
            case Interest.k_clothing:           
                m_text.text = "NewNavy";
                name += " NewNavy";
                break;
            case Interest.k_electronic:         
                m_text.text = "PagerHouse";
                name += " PagerHouse";
                break;
            case Interest.k_pets:               
                m_text.text = "PetsInc";
                name += " PetsInc";
                break;
            case Interest.k_jewlery:            
                m_text.text = "Jays";
                name += " Jays";
                break;
            case Interest.k_officeSupplies:     
                m_text.text = "PaperClips";
                name += " PaperClips";
                break;
            default:
                break;
        } 
        
        List<Seat> seats = new List<Seat>();
        
        //set up the seats
        for (var i = 0; i < transform.childCount; i++)
        {
            var seat = transform.GetChild(i).GetComponent<Seat>();
            if (seat != null)
                seats.Add(seat);
        }
        //fill the seats
        m_seats = new Seat[seats.Count]; 
        for (var i = 0; i < seats.Count; i++)
        {
            m_seats[i] = seats[i];
        }
        m_reservations = new AgentBehaviour[seats.Count];
        StartCoroutine(ServiceCustomers());
        StartCoroutine(Shopping());
    }

    private IEnumerator Shopping()
    {
        int seatIndex = 0;
        //get the trigger volume
        List<Collider> colliders = new List<Collider>(GetComponents<Collider>());
        var shopSpace = (BoxCollider)colliders.Find(item => item.isTrigger && item is BoxCollider);
        if(shopSpace == null)
        {
            Debug.LogError("no collider Found");
            Destroy(gameObject);
            StopAllCoroutines();
            base.StopAllCoroutines();
            yield break;
        }
        //determine the center of the box collider
        Vector3 center = shopSpace.center;
        center.y = 0.0f;
        Vector3 size = shopSpace.size;
        //adjust for scale
        size.x *= transform.localScale.x;
        size.y = 0.0f;
        size.z *= transform.localScale.x;

        //for moving the shop seats around to simulate movement. Did not finish
        /*while (false)
        {
            
            //move the seats around sporadicaly
            yield return new WaitForSeconds(1.0f);
            Vector3 newPosition = Vector3.zero;

            newPosition.x = Random.Range(center.x - size.x, center.x + size.x);
            newPosition.z = Random.Range(center.z - size.z, center.z + size.z);

            m_seats[seatIndex].transform.position = newPosition;

            //change seats
            seatIndex += 1;
            if (seatIndex >= m_seats.Length)
                seatIndex = 0;

        }*/
    }
}