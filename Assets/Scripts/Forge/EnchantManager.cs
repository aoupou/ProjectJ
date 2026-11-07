using System.Collections;
using TMPro;
using UnityEngine;

public class EnchantManager : MonoBehaviour
{
    [SerializeField] private float A;
    [SerializeField] private GameObject Plus;
    [SerializeField] private GameObject ChoiceEnchant;
    [SerializeField] private GameObject SellectedItem;
    [SerializeField] private TMP_Text EnchantName;
    [SerializeField] private GameObject Success;
    [SerializeField] private GameObject Fail;
    private Coroutine enchantResultRoutine;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void PushPlus()
    {
        ClosePlusUI();
        ShowChoiceEnchantUI();
    }
    public void PushSword()
    {
        ShowSellectedItemUI();
        EnchantName.text = "검";
    }
    public void PushSpear()
    {
        ShowSellectedItemUI();
        EnchantName.text = "창";
    }
    public void PushMagic()
    {
        ShowSellectedItemUI();
        EnchantName.text = "마법";
    }
    public void PushGun()
    {
        ShowSellectedItemUI();
        EnchantName.text = "총";
    }
    public void PushMoveRange()
    {
        ShowSellectedItemUI();
        EnchantName.text = "이동력";
    }
    public void PushHealth()
    {
        ShowSellectedItemUI();
        EnchantName.text = "체력";
    }


    public void PushEnchant()
    {
        if (enchantResultRoutine != null)
            StopCoroutine(enchantResultRoutine);

        enchantResultRoutine = StartCoroutine(ShowEnchantResult());
    }

    private IEnumerator ShowEnchantResult()
    {
        int enchantChance = UnityEngine.Random.Range(0, 10);
        bool isSuccess = enchantChance > 4;

        Success.SetActive(isSuccess);
        Fail.SetActive(!isSuccess);

        yield return new WaitForSeconds(3f);

        Success.SetActive(false);
        Fail.SetActive(false);
        enchantResultRoutine = null;
    }
    private void ShowPlusUI()
    {
        Plus.SetActive(true);
    }
    private void ClosePlusUI()
    {
        Plus.SetActive(false);
    }
    private void ShowChoiceEnchantUI()
    {
        ChoiceEnchant.SetActive(true);
    }
    private void closeChoiceEnchantUI()
    {
        ChoiceEnchant.SetActive(false);
    }
    private void ShowSellectedItemUI()
    {
        SellectedItem.SetActive(true);
    }
    private void closeSellectedItemUI()
    {
        SellectedItem.SetActive(false);
    }
}
