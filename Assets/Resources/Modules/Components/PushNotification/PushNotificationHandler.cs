using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public void AddNotificationItem(GameObject notificationItemPrefab)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        
        if (notificationListPanel.childCount < STACK_LIMIT)
        {
            // instantiate and set it to destroy based on the expiration time
            GameObject notifItem = Instantiate(notificationItemPrefab, notificationListPanel);
            notifItem.transform.SetAsFirstSibling();
            RemoveNotificationItem(notifItem);
        }
    }

    private void RemoveNotificationItem(GameObject notifGameObject)
    {
        Destroy(notifGameObject, NOTIFICATION_EXPIRATION);
    }
    
    private void RemoveAllNotifications()
    {
        foreach (Transform childTransform in notificationListPanel)
        {
            Destroy(childTransform);
        }
        gameObject.SetActive(false);
    }
}
