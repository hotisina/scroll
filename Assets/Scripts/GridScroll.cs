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
    public int count = 5;   //列表同时显示的元素个数
    public int poolMaxCount = 50;   //对象池的最大大小
    public int totalNum = 50;       //循环列表的元素总数

    public int minIndex;    //前下标
    public int maxIndex;    //后下标
    public float yspacing = 5.0f;   //两个元素之间的纵向间隔距离
    public float xspacing = 5.0f;   //横向间隔距离

    public int columnCount = 1; //列数，如果不是正整数的话会置为1

    public bool isInited = false;

    public GameObject content;
    public GameObject cellObject;
    public ScrollRect scrollRect;

    //protected Queue<GameObject> queuePool = new Queue<GameObject> ();

    protected ObjectPool pool;  //对象池组件

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
    protected float beginDragPos;   //用当前鼠标位置对比拖拽开始时鼠标位置来决定要不要更新

    public struct CellInfo {
        public GameObject obj;
        public Vector3 pos;
        public int index;   //这个index暂时是没必要的，但是修改可能要用到
    };
    public CellInfo[] cellObjects;

    private void Awake() {
        Init();
    }

    private void Start() {
        ShowList(count);
    }

    private void Update() {

        //在更新时，如果鼠标移动的距离小于半个条目高度（考虑列表惯性的近似，可以更精准计算一下），就不执行更新
        //但是这种优化方法现在只适合只有拖拽的情况，滑条需要再加一些处理
        //if (Mathf.Abs(beginDragPos - currentDragPos) > (cellHeight + spacing) / 2)
        UpdateScroll();

        //极端情况，如果列表动的非常快，导致元素更新不及的话就在一帧里面循环多几次
        //如果没有这条语句，在帧率较低且动的非常快时（例如飞速拖动滑条），能感觉到一点列表加载延迟
        while (isInited && (maxIndex - minIndex < count / 2)) {
            UpdateScroll();
        }
    }

    public virtual void Init() {
        content = this.GetComponent<ScrollRect>().content.gameObject;

        pool = this.GetComponent<ObjectPool>();
        pool.objectInPool = cellObject;
        pool.parentObject = content;  //对象池中元素的父亲应该是content

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

    //开始时显示列表
    public virtual void ShowList(int num) {
        //计算content尺寸
        contentHeight = (yspacing + cellHeight) * (num / columnCount + (num % columnCount == 0 ? 0 : 1));
        contentWidth = cellWidth * columnCount + xspacing * (columnCount - 1);
        contentRectTransform.sizeDelta = new Vector2(contentWidth, contentHeight);
        this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(planeWidth, contentHeight);

        //设置初始元素
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

    //通过事件触发更新的函数
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

        //检测上越界，最后一个元素必须活着否则列表就会崩溃
        if (go != null && IsOutBoundUp(pos) && minIndex != maxIndex) {
            cellObjects[minIndex].obj = null;
            pool.ReturnObject(go);
            minIndex++;

            return;
        }

        //用当前最上面的元素生成再上一个元素的信息
        if (go != null && minIndex != 0 && cellObjects[minIndex - 1].obj == null
            && !IsOutBoundUp(pos + yspacing + cellHeight)) {
            nextGo = pool.GetObject();
            cellObjects[minIndex - 1].obj = nextGo;
            nextGo.transform.SetParent(go.transform.parent);
            PlaceCell(minIndex, go, nextGo, 1); //让一个函数去放置元素，参数还比较繁琐，还有优化空间
            //nextGo.transform.position = go.transform.position + new Vector3(0, yspacing + cellHeight);
            cellObjects[minIndex - 1].pos = nextGo.transform.position;
            cellObjects[minIndex - 1].index = minIndex - 1;
            nextGo.name = (minIndex - 1).ToString();
            nextGo.GetComponentInChildren<Text>().text = (minIndex - 1).ToString();

            minIndex--;
            return;
        }

        //和上面对称

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

    protected bool IsFirstColumn(int index) {   //是最左元素
        if (index % columnCount == 0)
            return true;
        return false;
    }

    protected bool IsLastColumn(int index) {    //是最右元素
        if ((index+1) % columnCount == 0)
            return true;
        return false;
    }

    protected bool IsOutBoundUp(float pos) {    //上越界
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
