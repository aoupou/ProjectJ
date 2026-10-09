using UnityEngine;

public class Scroll_UI : MonoBehaviour
{
    [SerializeField] private RectTransform InfoGroup;
    [SerializeField] private float Defalt_Y_Pos = 0f;
    [SerializeField] private float Down_Y_Pos = -682f;
    [SerializeField] private float Up_Y_Pos = 682f;
    
    public void ScrollUpUI()
    {
        Vector2 pos = InfoGroup.anchoredPosition;
        pos.y= Up_Y_Pos;
        InfoGroup.anchoredPosition = pos;
    }
    public void ScrollDownUI()
    {
        Vector2 pos = InfoGroup.anchoredPosition;
        pos.y = Down_Y_Pos;
        InfoGroup.anchoredPosition = pos;
    }
    public void Scroll2DefaltUI()
    {
        Vector2 pos = InfoGroup.anchoredPosition;
        pos.y = Defalt_Y_Pos;
        InfoGroup.anchoredPosition = pos;
    }

}
