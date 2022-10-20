using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class GridScroll : MonoBehaviour, IBeginDragHandler, IDragHandler {
    public int count = 5;   //�б�ͬʱ��ʾ��Ԫ�ظ���
    public int poolMaxCount = 50;   //����ص�����С
    public int totalNum = 50;       //ѭ���б��Ԫ������

    public int minIndex;    //ǰ�±�
    public int maxIndex;    //���±�
    public float yspacing = 5.0f;   //����Ԫ��֮�������������
    public float xspacing = 5.0f;   //����������

    public int columnCount = 1; //��������������������Ļ�����Ϊ1

    public bool isInited = false;

    public GameObject content;
    public GameObject cellObject;
    public ScrollRect scrollRect;

    //protected Queue<GameObject> queuePool = new Queue<GameObject> ();

    protected ObjectPool pool;  //��������

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

    protected float currentDragPos;
    protected float beginDragPos;   //�õ�ǰ���λ�öԱ���ק��ʼʱ���λ��������Ҫ��Ҫ����

    public struct CellInfo {
        public GameObject obj;
        public Vector3 pos;
        public int index;   //���index��ʱ��û��Ҫ�ģ������޸Ŀ���Ҫ�õ�
    };
    public CellInfo[] cellObjects;

    private void Awake() {
        Init();
    }

    private void Start() {
        ShowList(count);
    }

    private void Update() {

        //�ڸ���ʱ���������ƶ��ľ���С�ڰ����Ŀ�߶ȣ������б���ԵĽ��ƣ����Ը���׼����һ�£����Ͳ�ִ�и���
        //���������Ż���������ֻ�ʺ�ֻ����ק�������������Ҫ�ټ�һЩ����
        //if (Mathf.Abs(beginDragPos - currentDragPos) > (cellHeight + spacing) / 2)
        UpdateScroll();

        //�������������б��ķǳ��죬����Ԫ�ظ��²����Ļ�����һ֡����ѭ���༸��
        //���û��������䣬��֡�ʽϵ��Ҷ��ķǳ���ʱ����������϶����������ܸо���һ���б�����ӳ�
        while (isInited && (maxIndex - minIndex < count / 2)) {
            UpdateScroll();
        }
    }

    public virtual void Init() {
        content = this.GetComponent<ScrollRect>().content.gameObject;

        pool = this.GetComponent<ObjectPool>();
        pool.objectInPool = cellObject;
        pool.parentObject = content;  //�������Ԫ�صĸ���Ӧ����content

        pool.InitPool();

        cellObjects = new CellInfo[totalNum + 10];

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

        if (!(columnCount > 0)) {
            columnCount = 1;
        }

        //scrollRect.onValueChanged.AddListener(UpdateScroll);
        //Vector2 contentPos = contentRectTransform.position;
        //Debug.Log(" contentPos.y " + contentPos.y);
    }

    //��ʼʱ��ʾ�б�
    public virtual void ShowList(int num) {
        //����content�ߴ�
        contentHeight = (yspacing + cellHeight) * (num / columnCount + (num % columnCount == 0 ? 0 : 1));
        contentWidth = cellWidth * columnCount + xspacing * (columnCount - 1);
        contentRectTransform.sizeDelta = new Vector2(contentWidth, contentHeight);
        this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(planeWidth, contentHeight);

        //���ó�ʼԪ��
        for (int i = 0; i < count; i++) {
            GameObject goTemp;
            float posy;
            Vector3 pos;
            CellInfo cellInfo;

            posy = -(cellHeight * i + yspacing * i);
            pos = new Vector3((i % columnCount) * (xspacing + cellWidth), -(i / columnCount) * (cellHeight + yspacing));

            goTemp = pool.GetObject();
            goTemp.transform.SetParent(content.transform);
            goTemp.transform.localPosition = pos;
            goTemp.name = i.ToString();
            goTemp.GetComponentInChildren<Text>().text = i.ToString();

            cellInfo.obj = goTemp.gameObject;
            cellInfo.index = i;
            cellInfo.pos = goTemp.transform.position;
            cellObjects[i] = cellInfo;

        }

        contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 
            (totalNum / columnCount + (totalNum%columnCount == 0? 0 : 1))  * (yspacing + cellHeight));

        minIndex = 0;
        maxIndex = count - 1;

        isInited = true;
    }

    //ͨ���¼��������µĺ���
    protected virtual void UpdateScroll() {

        CellInfo cInfoUp, cInfoDown;
        GameObject nextGo;
        GameObject go;
        float pos;

        cInfoUp = cellObjects[minIndex];
        cInfoDown = cellObjects[maxIndex];
        go = cInfoUp.obj;
        pos = go.transform.position.y;

        //if (go == null) {
        //    Debug.Log("array outbound!!!");
        //    continue;
        //}

        //�����Խ�磬���һ��Ԫ�ر�����ŷ����б�ͻ����
        if (go != null && IsOutBoundUp(pos) && minIndex != maxIndex) {
            cellObjects[minIndex].obj = null;
            pool.ReturnObject(go);
            minIndex++;

            return;
        }

        //�õ�ǰ�������Ԫ����������һ��Ԫ�ص���Ϣ
        if (go != null && minIndex != 0 && cellObjects[minIndex - 1].obj == null
            && !IsOutBoundUp(pos + yspacing + cellHeight)) {
            nextGo = pool.GetObject();
            cellObjects[minIndex - 1].obj = nextGo;
            nextGo.transform.SetParent(go.transform.parent);
            PlaceCell(minIndex, go, nextGo, 1); //��һ������ȥ����Ԫ�أ��������ȽϷ����������Ż��ռ�
            //nextGo.transform.position = go.transform.position + new Vector3(0, yspacing + cellHeight);
            cellObjects[minIndex - 1].pos = nextGo.transform.position;
            cellObjects[minIndex - 1].index = minIndex - 1;
            nextGo.name = (minIndex - 1).ToString();
            nextGo.GetComponentInChildren<Text>().text = (minIndex - 1).ToString();

            minIndex--;
            return;
        }

        //������Գ�

        go = cInfoDown.obj;
        pos = go.transform.position.y;
        if (go != null && IsOutBoundDown(pos) && minIndex != maxIndex) {
            cellObjects[maxIndex].obj = null;
            pool.ReturnObject(go);
            maxIndex--;
            return;
        }

        if (go != null && maxIndex != totalNum - 1 && cellObjects[maxIndex + 1].obj == null
            && !IsOutBoundDown(pos - yspacing - cellHeight)) {
            nextGo = pool.GetObject();
            cellObjects[maxIndex + 1].obj = nextGo;
            nextGo.transform.SetParent(go.transform.parent);
            PlaceCell(maxIndex, go, nextGo, -1);
            //nextGo.transform.position = go.transform.position - new Vector3(0, yspacing + cellHeight);
            cellObjects[maxIndex + 1].pos = nextGo.transform.position;
            cellObjects[maxIndex + 1].index = maxIndex + 1;
            nextGo.name = (maxIndex + 1).ToString();
            nextGo.GetComponentInChildren<Text>().text = (maxIndex + 1).ToString();
            maxIndex++;
            return;
        }


        //Debug.Log("ChangeVector2 = " + vec.x + ", " + vec.y);
    }

    protected void PlaceCell(int index, GameObject cellGo, GameObject nextGo, int upDown) { //upDown = 1 up   upDown = -1 down
        Vector3 deltaPos = new Vector3();
        if(upDown == 1) {
            if (IsFirstColumn(index)) {
                deltaPos = new Vector3((columnCount - 1) * (xspacing + cellWidth),
                    yspacing + cellHeight);
            } else {
                deltaPos = new Vector3(-(xspacing + cellWidth), 0);
            }
        } else if(upDown == -1) {
            if (IsLastColumn(index)) {
                deltaPos = new Vector3(-((columnCount - 1) * (xspacing + cellWidth)),
                    -(yspacing + cellHeight));
            } else {
                deltaPos = new Vector3(xspacing + cellWidth, 0);
            }
        } else {
            Debug.Log("index=" + index);
        }
        nextGo.transform.position = cellGo.transform.position + deltaPos;
    }

    protected bool IsFirstColumn(int index) {   //������Ԫ��
        if (index % columnCount == 0)
            return true;
        return false;
    }

    protected bool IsLastColumn(int index) {    //������Ԫ��
        if ((index+1) % columnCount == 0)
            return true;
        return false;
    }

    protected bool IsOutBoundUp(float pos) {    //��Խ��
        Vector2 planePos = planeRectTransform.position;
        if (pos - planePos.y > 2 * (cellHeight + yspacing) + planeHeight / 2) {
            //Debug.Log("pos=" + pos + " contentPos.y " + planePos.y);
            return true;
        }
        return false;
    }

    protected bool IsOutBoundDown(float pos) {
        Vector2 planePos = planeRectTransform.position;
        if (planePos.y - pos > (cellHeight + yspacing) + planeHeight / 2) {
            return true;
        }
        return false;
    }

    protected bool IsOutBound(float pos) {
        return IsOutBoundDown(pos) || IsOutBoundUp(pos);
    }

    //public void OnDrag(PointerEventData eventData) {
    //    UpdateScroll();
    //}

    public void OnBeginDrag(PointerEventData eventData) {
        beginDragPos = eventData.position.y;
    }

    public void OnDrag(PointerEventData eventData) {
        currentDragPos = eventData.position.y;
    }

}
