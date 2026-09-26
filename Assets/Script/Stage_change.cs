using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Stage_change : MonoBehaviour
{
    [SerializeField] private Stg stg_num; // 타이틀은 0 스테이지 는 각각 1~4 스테체인지 5 상점 6 7,8 각각 클리어 게임오버
    public enum Stg  // 스테이지 정보

    {
        title = 0,
        stg1,
        stg2,
        stg3,
        stg4,
        stg_changer,
        shop,
        stg_CLR,
        Game_Over,
        Nooooo = 14,
        Quit = 15,
        Sample = 16,
    };

    private void Update()
    {

    }
    public void ChangeStage() 
    { 
        change_stg(); 
    }
    public void GameClear()
    {
        stg_num = Stg.stg_CLR;
        change_stg();
    }
    public void GameOver()
    {
        stg_num = Stg.Game_Over;
        change_stg();
    }




    // Update is called once per frame
    void change_stg()
    {
        // SceneManager.LoadScene("stage_choice");
        switch (stg_num)
        {
            case Stg.title:
                {
                    SceneManager.LoadScene("Title");
                    break;
                }
            case Stg.stg1:
                {
                    SceneManager.LoadScene("stg_1");
                    break;
                }
            case Stg.stg2:
                {
                    SceneManager.LoadScene("stg_2");
                    break;
                }
            case Stg.stg3:
                {
                    SceneManager.LoadScene("stg_3");
                    break;
                }
            case Stg.stg4:
                {
                    SceneManager.LoadScene("stg_4");
                    break;
                }
            case Stg.stg_changer:
                {
                    SceneManager.LoadScene("stage_choice");
                    break;
                }
            case Stg.shop:
                {
                    SceneManager.LoadScene("shop");
                    break;
                }
            case Stg.stg_CLR:
                {
                    SceneManager.LoadScene("stg_CLR");
                    break;
                }
            case Stg.Game_Over:
                {
                    SceneManager.LoadScene("Game_Over");
                    break;
                }
            case Stg.Quit:
                {
                    Application.Quit(); 
                    break;
                }
            case Stg.Sample:
                {
                    SceneManager.LoadScene("SampleScene");
                    break;
                }
                default:
                {
                    break;
                }
        }
    }
    
}
