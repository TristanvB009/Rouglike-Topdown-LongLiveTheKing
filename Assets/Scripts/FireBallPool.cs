using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallPool : MonoBehaviour
{
    [SerializeField] private GameObject[] FireBallPrefabs; // Array of fireball prefabs

    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    public static FireBallPool Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        GrowPool();
    }

    private void GrowPool()
    {
        for (int i = 0; i < 200; i++)
        {
            var instanceToAdd = Instantiate(GetRandomFireBallPrefab());
            instanceToAdd.transform.SetParent(transform);
            AddToPool(instanceToAdd);
        }
    }

    private GameObject GetRandomFireBallPrefab()
    {
        int randomIndex = Random.Range(0, FireBallPrefabs.Length);
        return FireBallPrefabs[randomIndex];
    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObjects.Enqueue(instance);
    }

    public GameObject GetFromPool()
    {
        if (availableObjects.Count == 0)
        {
            GrowPool();
        }

        var instance = availableObjects.Dequeue();
        instance.SetActive(true);
        return instance;
    }
}