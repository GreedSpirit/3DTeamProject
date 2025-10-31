
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


    void Start()
    {
        _transforms = GetComponentsInChildren<Transform>().ToList();
    }

    //랜덤 = Bullet,Healkit 중 랜덤으로 선택되서 나오는 오브젝트의 총량
    //BulletBoxAmount = 생성될 불릿박스의 총량, 
    //HealkitAmount = 생성될 힐킷의 총량, 
    //ex CreateInteractObjects(1,2,3) = 둘중하나 1, 불릿 2, 힐킷3 총 6개

    public void CreateInteractObjects(int RandAmount, int BulletBoxAmount, int HealkitAmount)
    {
        CreateRandomInteractObjects(RandAmount, InteractType.Rand);
        CreateRandomInteractObjects(BulletBoxAmount, InteractType.BulletBox);
        CreateRandomInteractObjects(HealkitAmount, InteractType.Healkit);
    }




    private void CreateRandomInteractObjects(int num, InteractType type)//해당 타입 랜덤위치에 입력한 갯수만큼 생성
    {
        for (int i = 0; i < num; i++)
        {
            CreateRandomInteractObject(type);
        }
    }
    private void CreateRandomInteractObject(InteractType type)// 해당타입 랜덤위치에 생성
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
                inter.transform.position = _transforms[positionRand].position; 
                return;
            }
        }
        GameObject obj = Instantiate(_InteractablePrefabs[Index], _transforms[positionRand]);
        _InteractableObjects.Add(obj);
    }
    public void DisActiveAll()
    {
        foreach (GameObject inter in _InteractableObjects)
        {
            inter.SetActive(false);
        }
    }
}
