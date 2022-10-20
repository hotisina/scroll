using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Set in Inspetcor")]
    public int initialCount;
    public int maxCount = 100;
    public GameObject objectInPool; //决定池中存放的对象
    public GameObject parentObject; //决定实例化时的父对象

    //public int objectCount;
    public Queue<GameObject> queuePool = new Queue<GameObject>(); //对象池队列

    public void InitPool() {
        GameObject goTemp;
        for (int i = 0; i < initialCount; i++) {
            goTemp = Instantiate(objectInPool, parentObject.transform);
            queuePool.Enqueue(goTemp);
            goTemp.SetActive(false);
        }
    }

    public GameObject GetObject() {
        GameObject goTemp;
        if (queuePool.Count > 0) {
            goTemp = queuePool.Dequeue();
            goTemp.SetActive(true);
        } else {
            goTemp = Instantiate(objectInPool, parentObject.transform);
        }
        return goTemp;
    }

    public void ReturnObject(GameObject go) {
        if (queuePool.Count < maxCount) {
            queuePool.Enqueue(go);
            go.SetActive(false);
        } else {
            Destroy(go);
        }
    }

    public int pooledObjectCount{
        get {
            return queuePool.Count;
        }
    }
}
