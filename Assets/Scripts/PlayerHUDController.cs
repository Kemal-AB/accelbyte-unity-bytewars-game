using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [System.Serializable]
    public class HUDElement
    {
        public GameObject m_object;
        public TextMeshProUGUI m_text;
    }

    [System.Serializable]
    public class Column
    {
        public HUDElement m_title;
        public HUDElement m_value;
    }

    public Column[] m_columns;

    enum ColumnEnum
    {
        Lives,
        Score,
        Kills
    }

    void Start()
    {
        foreach(Column column in m_columns)
        {
            column.m_title.m_text = column.m_title.m_object.GetComponent<TextMeshProUGUI>();
            column.m_value.m_text = column.m_value.m_object.GetComponent<TextMeshProUGUI>();
        }
    }

    void SetEnabled(bool enabled)
    {
        GetComponent<Canvas>().enabled = enabled;
    }

    void SetColour(Color colour)
    {
        foreach(Column column in m_columns)
        {
            column.m_title.m_text.color = colour;
            column.m_value.m_text.color = colour;
        }
    }

    void SetLivesValue(int livesValue)
    {
        m_columns[(int)ColumnEnum.Lives].m_value.m_text.text = livesValue.ToString();
    }

    void SetScoreValue(int scoreValue)
    {
        m_columns[(int)ColumnEnum.Score].m_value.m_text.text = scoreValue.ToString();
    }

    void SetKillsValue(int killsValue)
    {
        m_columns[(int)ColumnEnum.Kills].m_value.m_text.text = killsValue.ToString();
    }
}
