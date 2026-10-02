using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_tojo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject ui;
    [SerializeField] private TMP_Text map_size;
    [SerializeField] private TMP_Text boss_name;
    [SerializeField] private Image stageImage;
    [SerializeField] private Stage_sellect stg_info;
    public enum Stage_sellect
    {
        stage1 = 0,
        stage2,
        stage3,
        stage4,
        only_UI,
        UI_out,
    }
    public void UI_tojoda()
    {
        switch(stg_info)
        {
            case Stage_sellect.stage1:
            {
                    OpenStage1();
                    break;
            }
            case Stage_sellect.stage2:
            {
                    OpenStage2();
                    break;
            }
            case Stage_sellect.stage3:
            {
                    OpenStage3();
                    break;
            }
            case Stage_sellect.stage4:
            {
                    break;
            }
            case Stage_sellect.only_UI:
            {
                    Only_UI();
                    break;
            }
            case Stage_sellect.UI_out:
            {
                    UI_out();
                    break;
            }
        }
    }
    private void OpenStage1()
    {
        ui.SetActive(true);
        map_size.text = "Map size 5X5";
        boss_name.text = "boss name";
    }
    private void OpenStage2()
    {
        ui.SetActive(true);
        map_size.text = "Map size 8X8";
        boss_name.text = "good";
    }
    private void OpenStage3()
    {
        ui.SetActive(true);
        map_size.text = "Map size 5X5";
        boss_name.text = "boss name";
    }
    private void Only_UI()
    {
        ui.SetActive(true);
    }
    private void UI_out()
    {
        ui.SetActive(false);
    }
}
