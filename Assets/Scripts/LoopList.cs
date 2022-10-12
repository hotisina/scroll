using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoopList : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField]
    RectTransform content;
    [SerializeField]
    List<RectTransform> lRects;//item������
    [SerializeField]
    float specing;//item�ľ�����
    [SerializeField]
    float cellWidth;//item�Ŀ��

    Vector2 oldPos;//��¼���λ��
    Vector2 deltaPos;//�������λ��֮��ļ��
    Vector2 anchoredPosition;//��¼content��λ������
    [SerializeField]
    float add;//�ۼ��ƶ�����
    [SerializeField]
    float endPosX;//��¼content��xλ������
    float itemPosY;

    void Start() {
        endPosX = content.anchoredPosition.x;
        itemPosY = lRects[0].anchoredPosition.y;
    }

    private void Update() {
        UpdateScroll();
    }

    void UpdateScroll() {
        //��ֵ�ķ�ʽ����content��λ��
        anchoredPosition.x = Mathf.Lerp(content.anchoredPosition.x, endPosX, 0.1f);
        anchoredPosition.y = content.anchoredPosition.y;
        content.anchoredPosition = anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        oldPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
        deltaPos = eventData.position - oldPos;
        add += deltaPos.x;//�ۼ��ƶ�����
        endPosX += deltaPos.x;

        //����ÿ��item��λ��
        //ͨ����list�Ĳ�����ʵ��ѭ���б�����ݸ���
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