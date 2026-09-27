using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public List<GameObject> prefabs;
        public int size;
        public int nextPrefabIndex = 0; // New variable to keep track of next prefab to instantiate
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = InstantiatePrefabFromPool(pool);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    private GameObject InstantiatePrefabFromPool(Pool pool)
    {
        GameObject prefab = pool.prefabs[pool.nextPrefabIndex];
        pool.nextPrefabIndex = (pool.nextPrefabIndex + 1) % pool.prefabs.Count; // Update the index for round-robin allocation
        return Instantiate(prefab);
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null)
            {
                GameObject newObj = InstantiatePrefabFromPool(pool);
                newObj.SetActive(false);
                poolDictionary[tag].Enqueue(newObj);
            }
            else
            {
                Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
                return null;
            }
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public bool ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return false;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);

        return true;
    }
}
