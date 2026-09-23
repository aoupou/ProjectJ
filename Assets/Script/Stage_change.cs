using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Stage_change : MonoBehaviour
{
    [SerializeField] private Stg stg_num; // 타이틀은 0 스테이지 는 각각 1~4 스테체인지 5 상점 6 7,8 각각 클리어 게임오버
    [SerializeField] private float currentHP = 1;  //임시
    [SerializeField] private float EnenmyCurrentHp = 1;  //임시
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
        /* 
       아래 함수들은 턴 스크립트가 완성되면, 그 스크립트랑 연계시킬 예정
    아니면 플레이어나 적 스크립트에 달아놔서 거기 스크립트에서 이 스크립트의 스테이지변수 랑 체인지 스테이지 코드만 빼와서 작동시켜도 될 듯
        작동하는지 궁금하시면 여기 밑에 주석처리 된거 지워서 한번 게임오버, 게임클리어로 이동하는지 봐봐요
       
        */
        /*
        if (IsPlayerDead())  // 만약 죽었다면
        {
            stg_num = Stg.Game_Over; // 스테이지 정보를 게임오버로 넘김
            change_stg(); // 넘겨진 스테이지로 보냄
        }
        */

        
         if (IsCleared())
            {
                stg_num = Stg.stg_CLR; // 스테이지 정보를 스테이지 클리어로 넘김
                change_stg();  // 넘겨진 스테이지로 보냄
            }
        
    }
    public void ChangeStage() 
    { 
        change_stg(); 
    }

    
    bool IsPlayerDead() //플레이어가 죽었는지 확인하는 함수
    {
        if (currentHP == 0)
        {
            return true;
        }    
        else
        {
            return false;
        }
    }
    bool IsCleared() // 스테이지가 클리어 됐는지 확인하는 함수
    {
        if (EnenmyCurrentHp == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
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
