using System.Collections;
using TMPro;
using UnityEngine;

public class EnchantManager : MonoBehaviour
{
    [Header("강화 설정")]
    [SerializeField] private Upgrade upgrade;

    [Header("대장간 UI")]
    [SerializeField] private GameObject Plus;
    [SerializeField] private GameObject ChoiceEnchant;
    [SerializeField] private GameObject SellectedItem;
    [SerializeField] private TMP_Text EnchantName;
    [SerializeField] private TMP_Text beadsText;

    [Header("강화 결과 UI")]
    [SerializeField] private GameObject Success;
    [SerializeField] private GameObject Fail;

    // null이면 아직 강화할 항목을 선택하지 않은 상태야.
    private UpgradeType? selectedUpgrade;
    private Coroutine enchantResultRoutine;

    private void OnEnable()
    {
        UpdateBeadsText(GameSession.Beads);
        GameSession.BeadsChanged += UpdateBeadsText;
    }

    private void OnDisable()
    {
        GameSession.BeadsChanged -= UpdateBeadsText;
    }

    private void UpdateBeadsText(int balance)
    {
        if (beadsText != null)
            beadsText.text = $"비즈: {balance}";
    }

    public void PushPlus()
    {
        Plus.SetActive(false);
        ChoiceEnchant.SetActive(true);
    }

    public void PushSword() => SelectUpgrade(UpgradeType.Sword, "검");
    public void PushSpear() => SelectUpgrade(UpgradeType.Spear, "창");
    public void PushGun() => SelectUpgrade(UpgradeType.Gun, "총");
    public void PushMagic() => SelectUpgrade(UpgradeType.Magic, "마법");
    public void PushHealth() => SelectUpgrade(UpgradeType.Health, "체력");
    public void PushMoveRange() => SelectUpgrade(UpgradeType.MoveRange, "이동력");

    private void SelectUpgrade(UpgradeType type, string displayName)
    {
        selectedUpgrade = type;
        EnchantName.text = displayName;
        SellectedItem.SetActive(true);
    }

    public void PushEnchant()
    {
        // 항목을 선택하기 전이나 결과창이 떠 있는 동안에는 시도를 막음
        if (!selectedUpgrade.HasValue || enchantResultRoutine != null)
            return;

        if (upgrade == null)
        {
            Debug.LogError("EnchantManager에 Upgrade 컴포넌트를 연결해 주세요.", this);
            return;
        }

        // 선택한 항목으로 강화 1회를 요청
        UpgradeResult result = upgrade.TryUpgrade(selectedUpgrade.Value);

        switch (result)
        {
            case UpgradeResult.Success:
            case UpgradeResult.Failed:
                enchantResultRoutine = StartCoroutine(
                    ShowEnchantResult(result == UpgradeResult.Success));
                break;

            case UpgradeResult.NotEnoughBeads:
                Debug.Log("비즈가 부족해요.", this);
                break;

            case UpgradeResult.MaxLevel:
                Debug.Log("이미 최대 강화 단계예요.", this);
                break;

            case UpgradeResult.InvalidRule:
                Debug.LogWarning("강화 비용과 성공률 설정을 확인해 주세요.", this);
                break;

            case UpgradeResult.SaveFailed:
                Debug.LogError("강화 결과를 저장하지 못했어요.", this);
                break;
        }
    }

    private IEnumerator ShowEnchantResult(bool succeeded)
    {
        // 성공 또는 실패 창을 3초 동안 표시
        Success.SetActive(succeeded);
        Fail.SetActive(!succeeded);

        yield return new WaitForSecondsRealtime(3f);

        Success.SetActive(false);
        Fail.SetActive(false);
        enchantResultRoutine = null;
    }
}