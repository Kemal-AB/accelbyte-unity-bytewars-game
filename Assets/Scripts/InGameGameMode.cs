using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameGameMode : MonoBehaviour
{
    InGameGameState m_gameState;
    public GameObject[] m_objectsToSpawn;
    public Bounds m_bounds;
    public int m_numLevelObjectsToSpawn = 7;
    public int m_numRetriesToPlaceLevelObject = 5;

    // Start is called before the first frame update
    void Start()
    {
        m_gameState = GameObject.FindObjectOfType<InGameGameState>();
        SpawnLevelObjects();   
    }

    // Update is called once per frame
    void Update()
    {
    }

    void SpawnLevelObjects()
    {
        GameObject levelParent = new GameObject("LevelObjects");

        for( int i = 0; i < m_numLevelObjectsToSpawn; i++)
        {
            GameObject randomObject = m_objectsToSpawn[Random.Range(0, m_objectsToSpawn.Length)];

            for (int r = 0; r < m_numRetriesToPlaceLevelObject; r++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(m_bounds.min.x, m_bounds.max.x),
                    Random.Range(m_bounds.min.y, m_bounds.max.y),
                    0.0f
                );

                if (!IsTooCloseToOtherObject(levelParent, randomPosition, randomObject))
                {
                    GameObject.Instantiate(randomObject, randomPosition, Quaternion.identity, levelParent.transform);
                    break;
                }
            }
        }

    }
    bool IsTooCloseToOtherObject(GameObject levelParent, Vector3 testPosition, GameObject testObject)
    {
        float testObjectRadius = testObject.GetComponent<Planet>()?.m_scale ?? 1.0f;

        for (int i = 0; i < levelParent.transform.childCount; i++)
        {
            Transform existingObject = levelParent.transform.GetChild(i);
            float distToExistingObject = Vector3.Distance(testPosition, existingObject.position);

            float existingObjectRadius = existingObject.GetComponent<Planet>()?.m_scale ?? 1.0f;

            float combinedObjectRadius = testObjectRadius + existingObjectRadius;

            if (distToExistingObject < combinedObjectRadius + 1.0f)
            {
                return true;                 
            }
        }

        return false;
    }
}
