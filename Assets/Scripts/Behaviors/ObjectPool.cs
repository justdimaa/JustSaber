using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int count;

    private IList<GameObject> _pooledObjects;

    void Start()
    {
        _pooledObjects = new List<GameObject>(count);

        for (int i = 0; i < count; i++)
        {
            InstantiatePoolObject();
        }
    }

    private GameObject InstantiatePoolObject()
    {
        var gameObject = GameObject.Instantiate(prefab);
        gameObject.transform.SetParent(transform);
        gameObject.SetActive(false);
        _pooledObjects.Add(gameObject);
        return gameObject;
    }

    public GameObject GetPoolObject()
    {
        var poolObject = _pooledObjects.FirstOrDefault(o => !o.activeInHierarchy);
        return poolObject ?? InstantiatePoolObject();
    }
}
