using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public ObjectPool poolInstance;

    [Header("Set in Inspetcor")]
    public int initialCount;
    public int maxCount = 100;
    public GameObject objectInPool; //�������д�ŵĶ���

    //public int objectCount;
    public Queue<GameObject> queuePool = new Queue<GameObject>(); //����ض���


    private void Awake() {
        poolInstance = this;
    }
    
    public void InitPool() {
        GameObject goTemp;
        for (int i = 0; i < initialCount; i++) {
            goTemp = Instantiate(objectInPool, poolInstance.transform);
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
            goTemp = Instantiate(objectInPool, poolInstance.transform);
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