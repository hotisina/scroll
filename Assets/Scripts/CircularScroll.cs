using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class CircularScroll : MonoBehaviour
{
    public int count = 5;
    public int initialCount = 5;
    public int poolMaxCount = 50;
    public int totalNum = 50;

    public int minIndex;
    public int maxIndex;
    public float spacing = 0.5f;

    public GameObject content;
    public GameObject cellObject;
    public ScrollRect scrollRect;
    
    protected Queue<GameObject> queuePool = new Queue<GameObject> ();

    //protected RectTransform contentRect;

    protected float planeWidth;
    protected float planeHeight;
    protected RectTransform planeRectTransform;

    protected float contentWidth;
    protected float contentHeight;
    protected RectTransform contentRectTransform;

    protected float cellWidth;
    protected float cellHeight;
    protected RectTransform cellRectTransform;

    public struct CellInfo {
        public GameObject obj;
        public Vector3 pos;
        public int index;
    };
    public CellInfo[] cellObjects;

    private void Awake() {
        Init ();
    }

    private void Start() {
        ShowList(count);
    }

    private void Update() {
        
    }

    public virtual void Init() {
        content = this.GetComponent<ScrollRect>().content.gameObject;
        
        //if(cellObject == null) {
        //    cellObject = content.transform.GetChild(0).gameObject;
        //}

        InitPool(cellObject);

        cellObjects = new CellInfo[totalNum];

        foreach (GameObject go in queuePool) {
            go.transform.SetParent(content.transform);
        }

        cellRectTransform = cellObject.GetComponent<RectTransform>();
        cellWidth = cellRectTransform.rect.width;
        cellHeight = cellRectTransform.rect.height;

        planeRectTransform = this.GetComponent<RectTransform>();
        planeWidth = planeRectTransform.rect.width;
        planeHeight = planeRectTransform.rect.height;

        contentRectTransform = content.GetComponent<RectTransform>();
        contentWidth = contentRectTransform.rect.width;
        contentHeight = contentRectTransform.rect.height;

        scrollRect = this.GetComponent<ScrollRect>();

        scrollRect.onValueChanged.AddListener(UpdateScroll);
        //Vector2 contentPos = contentRectTransform.position;
        //Debug.Log(" contentPos.y " + contentPos.y);
    }

    protected virtual void UpdateScroll(Vector2 vec) {
        CellInfo cInfo;
        GameObject nextGo;
        GameObject go;
        float pos;
        for (int i = minIndex; i < maxIndex + 1; i++) {
            cInfo = cellObjects[i];
            go = cInfo.obj;
            pos = go.transform.position.y;

            if (go != null && IsOutBound(pos)) {
                cellObjects[i].obj = null;
                ReturnObject(go);
                if (i == minIndex) {                                                      
                    minIndex++;
                    continue;
                }
                if (i == maxIndex) {
                    maxIndex--;
                    continue;
                }
            }

            if (go != null && i != 0 && cellObjects[i-1].obj == null && !IsOutBound(pos + spacing + cellHeight)) {
                nextGo = GetObject();
                cellObjects[i - 1].obj = nextGo;
                nextGo.transform.SetParent(go.transform.parent);
                nextGo.transform.position = go.transform.position + new Vector3(0, spacing + cellHeight);
                cellObjects[i - 1].pos = nextGo.transform.position;
                cellObjects[i - 1].index = i - 1;
                nextGo.name = (i - 1).ToString();
                nextGo.GetComponentInChildren<Text>().text = (i - 1).ToString();
                minIndex--;
                return;
            }

            if (go != null && i != totalNum - 1 && cellObjects[i + 1].obj == null && !IsOutBound(pos - spacing - cellHeight)) {
                nextGo = GetObject();
                cellObjects[i + 1].obj = nextGo;
                nextGo.transform.SetParent(go.transform.parent);
                nextGo.transform.position = go.transform.position - new Vector3(0, spacing + cellHeight);
                cellObjects[i + 1].pos = nextGo.transform.position;
                cellObjects[i + 1].index = i + 1;
                nextGo.name = (i + 1).ToString();
                nextGo.GetComponentInChildren<Text>().text = (i + 1).ToString();
                maxIndex++;
                return;
            }

            /* else if(go == null){
                go = GetObject();
                go.transform.position = cellObjects[i].pos;
                cellObjects[i].index = i > count/2 ? cellObjects[0].index + i : cellObjects[count].index - count + i;
                cellObjects[i].obj = go;
                cellObjects[i].pos = go.transform.position;
                go.name = cellObjects[i].index.ToString();
                go.GetComponentInChildren<Text>().text = cellObjects[i].index.ToString();
                go.transform.parent = content.transform;
            }*/

        }

        //Debug.Log("ChangeVector2 = " + vec.x + ", " + vec.y);
    }

    protected bool IsOutBound(float pos) {
        Vector2 planePos = planeRectTransform.position;
        if (pos - planePos.y > 2 * cellHeight + planeHeight/2 || planePos.y - pos > cellHeight + planeHeight / 2) {
            //Debug.Log("pos=" + pos + " contentPos.y " + planePos.y);
            return true;
        }   
        return false;
    }

    public virtual void ShowList(int num) {
        minIndex = -1;
        maxIndex = -1;

        //º∆À„content≥ﬂ¥Á
        contentHeight = (spacing + cellHeight) * num;
        contentRectTransform.sizeDelta = new Vector2(contentWidth, contentHeight);
        this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(planeWidth, contentHeight);

        for(int i = 0; i < count; i++) {
            GameObject goTemp;
            float posy;
            Vector3 pos;
            CellInfo cellInfo;

            posy =  -(cellHeight * i + spacing * i);
            pos = new Vector3(0, posy, 0);

            goTemp = GetObject();
            goTemp.transform.SetParent(content.transform);
            goTemp.transform.localPosition = pos;
            goTemp.name = i.ToString();
            goTemp.GetComponentInChildren<Text>().text = i.ToString();

            cellInfo.obj = goTemp.gameObject;
            cellInfo.index = i;
            cellInfo.pos = goTemp.transform.position;
            cellObjects[i] = cellInfo;

        }

        minIndex = 0;
        maxIndex = count - 1;
        //cellObject.SetActive(false);

        this.transform.GetChild(1).GetComponent<Scrollbar>().size = num / totalNum;
    }

    public void InitPool(GameObject go) {
        GameObject goTemp;
        for (int i = 0; i < initialCount - 1; i++) {
            goTemp = Instantiate(go, this.transform);
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
            goTemp = Instantiate(cellObject, this.transform);
            goTemp.SetActive(true);
        }
        return goTemp;
    }

    public void ReturnObject(GameObject go) {
        if (queuePool.Count < poolMaxCount) {
            queuePool.Enqueue(go);
            go.SetActive(false);
        } else {
            Destroy(go);
        }
    }
}
