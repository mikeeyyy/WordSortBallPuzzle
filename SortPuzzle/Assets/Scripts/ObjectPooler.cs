using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int size;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Pool> poolInfoDictionary;

    private void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolInfoDictionary = new Dictionary<string, Pool>();

        foreach (Pool pool in pools)
        {
            poolInfoDictionary.Add(pool.tag, pool);
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform); 
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Transform parent)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return null;
        }

        GameObject objectToSpawn;

        if (poolDictionary[tag].Count == 0)
        {
            Pool pool = poolInfoDictionary[tag];
            objectToSpawn = Instantiate(pool.prefab, transform);
        }
        else
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }

        objectToSpawn.transform.SetParent(parent);
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.localScale = Vector3.zero; 
        objectToSpawn.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Destroy(objectToReturn); 
            return;
        }
        objectToReturn.SetActive(false);
        objectToReturn.transform.SetParent(transform); 
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}