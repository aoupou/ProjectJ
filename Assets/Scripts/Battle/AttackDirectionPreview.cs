using UnityEngine;
using UnityEngine.EventSystems;

public class AttackDirectionPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject attackRangeImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        attackRangeImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        attackRangeImage.SetActive(false);
    }
}