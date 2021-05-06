using UnityEngine;

//Helper class in the event that the player or someone(....) decides to delete an object
public class StatusNotifier : MonoBehaviour
{
    private bool m_isActive = true;
    private INotify m_notify;
    private void Update()
    {
        //if there is someone listening and I have been deactivated alert them
        if(m_notify != null && !gameObject.activeInHierarchy)
        {
            m_notify.Notify(gameObject);
            m_isActive = false;
        }
        if(gameObject.activeInHierarchy && !m_isActive)
        {
            m_isActive = true;
        }
    }

    public void SetWhoToNotify(INotify notify) => m_notify = notify;
}