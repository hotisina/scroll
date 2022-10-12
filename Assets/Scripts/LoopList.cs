using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoopList : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField]
    RectTransform content;
    [SerializeField]
    List<RectTransform> lRects;//item的容器
    [SerializeField]
    float specing;//item的距离间隔
    [SerializeField]
    float cellWidth;//item的宽度

    Vector2 oldPos;//记录鼠标位置
    Vector2 deltaPos;//两个鼠标位置之间的间隔
    Vector2 anchoredPosition;//记录content的位置坐标
    [SerializeField]
    float add;//累加移动距离
    [SerializeField]
    float endPosX;//记录content的x位置坐标
    float itemPosY;

    void Start() {
        endPosX = content.anchoredPosition.x;
        itemPosY = lRects[0].anchoredPosition.y;
    }

    private void Update() {
        UpdateScroll();
    }

    void UpdateScroll() {
        //差值的方式更新content的位置
        anchoredPosition.x = Mathf.Lerp(content.anchoredPosition.x, endPosX, 0.1f);
        anchoredPosition.y = content.anchoredPosition.y;
        content.anchoredPosition = anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        oldPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
        deltaPos = eventData.position - oldPos;
        add += deltaPos.x;//累加移动距离
        endPosX += deltaPos.x;

        //处理每个item的位置
        //通过对list的操作，实现循环列表的数据更新
        while (Mathf.Abs(add) > cellWidth + specing) {
            if (add < 0) {
                add = add + (cellWidth + specing);
                RectTransform rect = lRects[0];
                lRects.Remove(rect);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + (cellWidth + specing) * 8, itemPosY);
                lRects.Add(rect);
            } else {
                add -= (cellWidth + specing);
                RectTransform rect = lRects[lRects.Count - 1];
                lRects.Remove(rect);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - (cellWidth + specing) * 8, itemPosY);
                lRects.Insert(0, rect);
            }
        }
        oldPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        deltaPos = Vector2.zero;
    }
}