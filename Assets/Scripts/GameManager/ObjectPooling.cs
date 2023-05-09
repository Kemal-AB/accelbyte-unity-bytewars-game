using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling
{
    private const int InitialCapacity = 4;
    private Dictionary<string, List<GameEntityAbs>> _pooledItemsDict = new Dictionary<string, List<GameEntityAbs>>();
    private Dictionary<string, List<FxEntity>> _pooledFxs = new Dictionary<string, List<FxEntity>>();
    private Transform _container;
    public GameEntityAbs Get(GameEntityAbs referencePrefab)
    {
        List<GameEntityAbs> pooledItems;
        if (_pooledItemsDict.TryGetValue(referencePrefab.name, out pooledItems))
        {
            foreach (var pooledItem in pooledItems)
            {
                if (pooledItem && !pooledItem.gameObject.activeSelf)
                {
                    pooledItem.gameObject.SetActive(true);
                    return pooledItem;
                }
            }
            pooledItems.RemoveAll((p) => !p);
            var newItem = Object.Instantiate(referencePrefab, _container);
            newItem.SetId(pooledItems.Count);
            _pooledItemsDict[referencePrefab.name].Add(newItem);
            return newItem;
        }
        else
        {
            GameEntityAbs[] newItems = new GameEntityAbs[InitialCapacity];
            for (int i = 0; i < InitialCapacity; i++)
            {
                var n = Object.Instantiate(referencePrefab, _container);
                n.gameObject.SetActive(false);
                n.SetId(i);
                newItems[i] = n;
            }
            _pooledItemsDict.Add(referencePrefab.name, new List<GameEntityAbs>(newItems));
            var re = _pooledItemsDict[referencePrefab.name][InitialCapacity - 1];
            re.gameObject.SetActive(true);
            return re;
        }
    }

    public void ResetAll()
    {
        foreach (var kvp in _pooledItemsDict)
        {
            var list = kvp.Value;
            foreach (var item in list)
            {
                item.Reset();
            }
        }
    }

    public void DestroyAll()
    {
        foreach (var kvp in _pooledItemsDict)
        {
            var list = kvp.Value;
            foreach (var item in list)
            {
                GameObject.Destroy(item.gameObject);
            }
        }
    }
    
    public FxEntity Get(FxEntity referencePrefab)
    {
        List<FxEntity> pooledItems;
        if (_pooledFxs.TryGetValue(referencePrefab.name, out pooledItems))
        {
            foreach (var pooledItem in pooledItems)
            {
                if (!pooledItem.gameObject.activeSelf)
                {
                    pooledItem.gameObject.SetActive(true);
                    return pooledItem;
                }
            }

            var newItem = Object.Instantiate(referencePrefab, _container);
            _pooledFxs[referencePrefab.name].Add(newItem);
            return newItem;
        }
        else
        {
            FxEntity[] newItems = new FxEntity[InitialCapacity];
            for (int i = 0; i < InitialCapacity; i++)
            {
                var n = Object.Instantiate(referencePrefab, _container);
                n.gameObject.SetActive(false);
                newItems[i] = n;
            }
            _pooledFxs.Add(referencePrefab.name, new List<FxEntity>(newItems));
            var re = _pooledFxs[referencePrefab.name][InitialCapacity - 1];
            re.gameObject.SetActive(true);
            return re;
        }
    }

    public ObjectPooling(Transform container, GameEntityAbs[] gamePrefabs, FxEntity[] fxPrefabs)
    {
        _container = container;
        for (int i = 0; i < gamePrefabs.Length; i++)
        {
           GameEntityAbs[] newItems = new GameEntityAbs[InitialCapacity];
            for (int j = 0; j < InitialCapacity; j++)
            {
                var ge = Object.Instantiate(gamePrefabs[i], _container);
                ge.SetId(j);
                ge.gameObject.SetActive(false);
                newItems[j] = ge;
            }
            _pooledItemsDict.Add(gamePrefabs[i].name, new List<GameEntityAbs>(newItems));
        }
        
        for (int k = 0; k < fxPrefabs.Length; k++)
        {
            FxEntity[] newItems = new FxEntity[InitialCapacity];
            for (int l = 0; l < InitialCapacity; l++)
            {
                var ge = Object.Instantiate(fxPrefabs[k], _container);
                ge.gameObject.SetActive(false);
                newItems[l] = ge;
            }
            _pooledFxs.Add(fxPrefabs[k].name, new List<FxEntity>(newItems));
        }
    }

    public void ClearAll()
    {
        foreach (var kvp in _pooledItemsDict)
        {
            var list = kvp.Value;
            list.RemoveAll((entity) => !entity);
        }

        foreach (var kv in _pooledFxs)
        {
            var list = kv.Value;
            list.RemoveAll((f) => !f);
        }
    }
}
