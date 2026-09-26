using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// HP 회복 아이템을 선택하면 회복될 하트를 기존 하트 오른쪽에 깜빡이며 보여준다 (기획서 10-2-2)
// Player_HP가 매 프레임 자기 하트를 다시 만들기 때문에 Heart_Parent 안에는 못 넣고, 옆에 따로 만든다
public class HpHealPreview : MonoBehaviour
{
    private const float BlinkInterval = 0.3f;

    private Transform heartParent;
    private Coroutine blink;

    // 플레이어 정보 영역(Player_info) 안의 Heart_Parent를 찾아서 연결
    public static void Create(Scene scene)
    {
        GameObject playerInfo = ItemUI.Find(scene, "Player_info");
        Transform heartParent = playerInfo != null ? playerInfo.transform.Find("Heart_Parent") : null;

        if (heartParent == null)
        {
            Debug.LogWarning("[아이템] 'Player_info/Heart_Parent'를 못 찾아서 HP 회복 미리보기를 끔");
            return;
        }

        RectTransform rt = ItemUI.CreateRect(heartParent.parent, "HealPreview");
        rt.localScale = heartParent.localScale;
        rt.sizeDelta = ((RectTransform)heartParent).sizeDelta;
        rt.pivot = new Vector2(0, 0.5f);

        HorizontalLayoutGroup layout = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        HorizontalLayoutGroup original = heartParent.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = original != null ? original.spacing : 0;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        HpHealPreview preview = rt.gameObject.AddComponent<HpHealPreview>();
        preview.heartParent = heartParent;
    }

    private void OnEnable()
    {
        ItemEffects.PreviewStarted += Show;
        ItemEffects.PreviewEnded += Hide;
    }

    private void OnDisable()
    {
        ItemEffects.PreviewStarted -= Show;
        ItemEffects.PreviewEnded -= Hide;
    }

    private void Show(ItemData item)
    {
        if (item.effectType != ItemEffectType.HealHP || heartParent.childCount == 0)
            return;

        Hide();

        // 첫 번째 하트(꽉 찬 하트)를 복사해서 사용, 반 칸은 이미지를 절반만 채움
        GameObject template = heartParent.GetChild(0).gameObject;

        for (int i = 0; i < (item.value + 1) / 2; i++)
        {
            Image heart = Instantiate(template, transform).GetComponent<Image>();
            bool half = i == item.value / 2;

            if (half)
            {
                heart.type = Image.Type.Filled;
                heart.fillMethod = Image.FillMethod.Horizontal;
                heart.fillAmount = 0.5f;
            }
        }

        blink = StartCoroutine(Blink());
    }

    private void Hide()
    {
        if (blink != null)
        {
            StopCoroutine(blink);
            blink = null;
        }

        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    // 기존 하트 줄의 오른쪽 끝에 붙인다
    private void LateUpdate()
    {
        if (transform.childCount == 0)
            return;

        Vector3[] corners = new Vector3[4];
        float right = float.MinValue;
        float centerY = heartParent.position.y;

        foreach (Transform child in heartParent)
        {
            ((RectTransform)child).GetWorldCorners(corners);

            if (corners[2].x > right)
            {
                right = corners[2].x;
                centerY = (corners[0].y + corners[1].y) / 2;
            }
        }

        if (right > float.MinValue)
            transform.position = new Vector3(right, centerY, heartParent.position.z);
    }

    private IEnumerator Blink()
    {
        bool visible = true;

        while (true)
        {
            yield return new WaitForSeconds(BlinkInterval);

            visible = !visible;

            foreach (Transform child in transform)
                child.GetComponent<Image>().enabled = visible;
        }
    }
}
