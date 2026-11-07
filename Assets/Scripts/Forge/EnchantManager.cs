using System;
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
