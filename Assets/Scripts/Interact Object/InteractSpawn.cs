using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractSpawn : MonoBehaviour
{

    List<Transform> transforms = new List<Transform>();
    [SerializeField] private GameObject[] InteractablePrefabs;

    private List<GameObject> InteractableObjects = new List<GameObject>();
    
    void Start()
    {
        transforms = GetComponentsInChildren<Transform>().ToList();
    }

    public void CreateObject()
    {
        int rand = Random.Range(0, InteractablePrefabs.Length);
        GameObject obj = Instantiate(InteractablePrefabs[rand]);
        InteractableObjects.Add(obj);



    }
}
