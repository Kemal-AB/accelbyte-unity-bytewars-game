using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerBarUIController : MonoBehaviour
{
    public RectTransform m_parentPanel;
    public RectTransform m_powerBarPanel;
    Vector2 m_powerBarPanelFullHeight;

    float m_displayTimer = 0.0f;
    public void Init()
    {
        m_powerBarPanelFullHeight = m_parentPanel.sizeDelta;
        
        SetVisible(false);
    }

    void Update()
    {
        if( m_displayTimer > 0.0f )
        {
            m_displayTimer = Mathf.Max( 0.0f, m_displayTimer - Time.deltaTime );

            if( m_displayTimer == 0.0f )
            {
                SetVisible(false);
            }
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

    public void SetPercentageFraction(float percentageFraction, bool showUI = true)
    {
        Vector2 newSize = m_powerBarPanelFullHeight * new Vector2( 1.0f, percentageFraction );

        m_powerBarPanel.sizeDelta = newSize;

        if( showUI )
        {
            m_displayTimer = 0.5f;
            SetVisible(true);
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
        Vector2 screenPoint = WorldToCanvas(worldPosition + new Vector3(1.5f, 0.5f, 0.0f),Camera.main);
        m_parentPanel.anchoredPosition = screenPoint;
    }
}
