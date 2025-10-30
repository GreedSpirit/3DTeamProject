
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractSpawn : MonoBehaviour
{
    public enum InteractType
    {
        BulletBox,
        Healkit,    
        Rand,
    }
    List<Transform> _transforms = new List<Transform>();
    [SerializeField] private GameObject[] _InteractablePrefabs;

    private List<GameObject> _InteractableObjects = new List<GameObject>();

    [SerializeField] private int _RandAmount = 3;
    [SerializeField] private int _healKitAmount = 3;
    [SerializeField] private int _BulletBoxAmount = 3;

    void Start()
    {
        _transforms = GetComponentsInChildren<Transform>().ToList();
        CreateRandomInteractObjects(_RandAmount, InteractType.Rand);
        CreateRandomInteractObjects(_BulletBoxAmount, InteractType.BulletBox);
        CreateRandomInteractObjects(_healKitAmount, InteractType.Healkit);
    }

    public void CreateRandomInteractObjects(int num, InteractType type)
    {
        for (int i = 0; i < num; i++)
        {
            CreateRandomInteractObject(type);
        }
    }
    private void CreateRandomInteractObject(InteractType type)
    {
        int Index;
        if (type == InteractType.Rand)
        {
            Index = Random.Range(0, _InteractablePrefabs.Length);
        }
        else
        {
            Index = (int)type;
        }
        int positionRand = Random.Range(0, _transforms.Count);
        foreach (GameObject inter in _InteractableObjects)
        {
            if (!inter.activeSelf)
            {
                inter.SetActive(true);
                inter.transform.position = _transforms[positionRand].position; return;
            }
        }
        GameObject obj = Instantiate(_InteractablePrefabs[Index], _transforms[positionRand]);
        _InteractableObjects.Add(obj);
    }
}
