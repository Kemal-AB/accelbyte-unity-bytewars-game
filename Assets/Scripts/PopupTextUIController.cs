using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupTextUIController : MonoBehaviour
{
    public RectTransform m_parentPanel;
    public TMPro.TextMeshProUGUI m_textUI;
    public float m_speed = 2.5f;
    public float m_lifeTime = 2.0f;

    float m_yOffset = 0.0f;
    float m_timeAlive = 0.0f;

    Vector3 m_worldPosition;

    void Update()
    {
        m_timeAlive += Time.deltaTime;

        float timeFraction = Mathf.InverseLerp(m_lifeTime, 0.0f, m_timeAlive);

        float alpha = timeFraction * 0.8f;

        Color textColour = m_textUI.color;
        textColour.a = alpha;

        m_textUI.color = textColour;

        m_worldPosition.y += m_speed * Time.deltaTime;

        SetPosition(m_worldPosition);

        if( m_timeAlive > m_lifeTime )
        {
            Destroy(gameObject);
        }
    }

    void SetVisible(bool visible)
    {
        GetComponent<Canvas>().enabled = visible;
    }

    public void SetColour(Color colour)
    {
        Image[] images = GetComponentsInChildren<Image>();

        foreach( Image image in images )
        {
            var alpha = image.color.a;
            image.color = new Color(colour.r,colour.g,colour.b,alpha);
        }
    }

     public Vector2 WorldToCanvas( Vector3 world_position, Camera camera = null)
     {
         if (camera == null)
         {
             camera = Camera.main;
         }
 
         var viewport_position = camera.WorldToViewportPoint(world_position);
         var canvas_rect = GetComponent<RectTransform>();
 
         return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
                            (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
     }

    public void SetPosition(Vector3 worldPosition)
    {
        m_worldPosition = worldPosition;
        Vector2 screenPoint = WorldToCanvas(worldPosition + new Vector3(1.5f, 0.5f, 0.0f),Camera.main);
        m_parentPanel.anchoredPosition = screenPoint;
    }
}
