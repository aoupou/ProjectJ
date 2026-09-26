using UnityEngine;
using UnityEngine.UI;

// 버튼을 누르거나 마우스를 올렸을 때 버튼 그림이 어두워지는 효과를 그대로 따라한다
// (예시 글자를 가린 덮개가 버튼과 따로 놀지 않게)
public class ItemTintFollow : MonoBehaviour
{
    private Graphic source;
    private Graphic self;

    public static void Add(Graphic self, Graphic source)
    {
        if (self == null || source == null)
            return;

        ItemTintFollow follow = self.gameObject.AddComponent<ItemTintFollow>();
        follow.self = self;
        follow.source = source;
    }

    private void LateUpdate()
    {
        if (source != null)
            self.canvasRenderer.SetColor(source.canvasRenderer.GetColor());
    }
}
