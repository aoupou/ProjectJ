using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragIMG : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler

{
    [SerializeField] private RectTransform targetArea;
    [SerializeField] private GoToSTG Stg;
    [SerializeField] private IMGSTG stg;
    [SerializeField] private UI_tojo UI;
    [SerializeField] private DragIMG[] otherImages;
    private RectTransform imageRect;
    private Vector2 originalPosition;
    public enum IMGSTG // 이미지가 몇 스테이지로 가는 이미지인지 인스펙터에서 설정
    {
        defalt = 0,
        stg1, 
        stg2, 
        stg3, 
        stg4,
    };
    private void Awake()
    {
        imageRect = GetComponent<RectTransform>();
        originalPosition = imageRect.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) // 드래그 시작
    {
        
    }

    public void OnDrag(PointerEventData eventData) // 드래그 중
    {
        RectTransform ImageRect = GetComponent<RectTransform>();
        ImageRect.anchoredPosition += eventData.delta;
    }
    public void OnEndDrag(PointerEventData eventData) // 드래그 종료
    {
        RectTransform imageRect = GetComponent<RectTransform>();

        Vector3 imageCenter = imageRect.TransformPoint(imageRect.rect.center);
        Vector2 centerInTarget = targetArea.InverseTransformPoint(imageCenter);

        bool isInside = targetArea.rect.Contains(centerInTarget);
        
        if(isInside)
        {
            OnPlacedTarget();
        }
    }
    void OnPlacedTarget()
    {
        foreach (DragIMG other in otherImages)
        {
            if (other != null && other != this)
            {
                other.imageRect.anchoredPosition = other.originalPosition;
            }
        }
        STGnSellect();
    }
    void STGnSellect()
    {
        switch (stg)
        {
            case IMGSTG.stg1:
                {
                    Stg.STG1Sellect();
                    UI.StageTo(UI_tojo.Stage_sellect.stage1);
                    break;
                }
            case IMGSTG.stg2:
                {
                    Stg.STG2Sellect();
                    UI.StageTo(UI_tojo.Stage_sellect.stage2);
                    break;
                }
            case IMGSTG.stg3:
                {
                    Stg.STG3Sellect();
                    UI.StageTo(UI_tojo.Stage_sellect.stage3);
                    break;
                }
            case IMGSTG.stg4:
                {
                    Stg.STG4Sellect();
                    UI.StageTo(UI_tojo.Stage_sellect.stage4);
                    break;
                }
        }
    }
}
