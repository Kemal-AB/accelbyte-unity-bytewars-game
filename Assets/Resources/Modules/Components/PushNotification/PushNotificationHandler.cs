using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PushNotificationHandler : MonoBehaviour
{
    private const float NOTIFICATION_EXPIRATION = 10.0f;
    private const int STACK_LIMIT = 5;
    
    [SerializeField] private RectTransform notificationListPanel;
    [SerializeField] private Button dismissNotificationsButton;

    private Queue<GameObject> _pendingNotification = new Queue<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        dismissNotificationsButton.onClick.AddListener(RemoveAllNotifications);
    }

    // Update is called once per frame
    private void Update()
    {
        if (notificationListPanel.childCount < 5 && _pendingNotification.Count > 0)
        {
            GameObject pendingNotifItem = _pendingNotification.Dequeue();
            pendingNotifItem.SetActive(true);
            Destroy(pendingNotifItem, NOTIFICATION_EXPIRATION);
        }
        
        if (notificationListPanel.childCount <= 0 && _pendingNotification.Count <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void AddNotificationItem(GameObject notificationItemPrefab)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        
        // instantiate and set it to destroy based on the expiration time
        GameObject notifItem = Instantiate(notificationItemPrefab, notificationListPanel);
        notifItem.transform.SetAsFirstSibling();
        
        if (notificationListPanel.childCount <= STACK_LIMIT)
        {
            Destroy(notifItem, NOTIFICATION_EXPIRATION);
        }
        else
        {
            notifItem.SetActive(false);
            _pendingNotification.Enqueue(notifItem);
        }
    }
    
    private void RemoveAllNotifications()
    {
        foreach (Transform childTransform in notificationListPanel)
        {
            Destroy(childTransform.gameObject);
        }
        gameObject.SetActive(false);
    }
}
