using UnityEngine;
using UnityEngine.InputSystem;

public class BattlePreview : MonoBehaviour
{
    //[SerializeField] private class Battle battle;
    public Battle Battle;
    public Map Map;
    private SpriteRenderer[,] TileFill;

    public Color movePreview ; 
    int moveRange;

    public void Awake()
    {
        TileFill = new SpriteRenderer[Map.Width, Map.Height];
    }
    public void Update()
    {
        
    }


    public void PlayerMove()
    {
        if (Battle.chooseMove == 1)
        {
            Battle.moverange = moveRange;

            

        }


    }
    // 오늘의 목표 ~ 
    //1. 배틀 코드에서 플레이어 입력값 받아오기 [클리어]
    //2. 이동이면 이동에 칸 색깔 변화 / 공격이면 공격에 맞게 칸 색깔 변화(데이터 베이스 최신화 필요) -> 문제점 : 플레이어 위치 받아오는 얘가 없다. ㅁ
    //3. 마우스 회전에 맞게 디렉션 잡아주기 
    //3. 클릭을 했을 때 배틀로 다시 보내주기 
}
