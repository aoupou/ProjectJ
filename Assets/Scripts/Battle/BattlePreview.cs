using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BattlePreview : MonoBehaviour
{
    //[SerializeField] private class Battle battle;
    public Battle battle;
    public Map map;
    private SpriteRenderer[,] tileFill;
    private TMP_Text[,] tileText;


    public Color movePreviewColor ; 


    public void Awake()
    {
        tileFill = new SpriteRenderer[map.Width, map.Height];
        tileText = new TMP_Text[map.Width, map.Height];
    }

    public void AddTile(int x, int y, SpriteRenderer fill,TMP_Text Tiledamage)
    {
        tileFill[x, y] =  fill;
        tileText[x, y] = Tiledamage;
    }

    public void Update()
    {
        
    }

    public void Paint(Vector2Int playerMoveRange)
    {
        if (playerMoveRange.x < 0 || playerMoveRange.x >= map.Width || playerMoveRange.y < 0 || playerMoveRange.y >= map.Height) { return; }

        tileFill[playerMoveRange.x,playerMoveRange.y].color = movePreviewColor;
    }
    public void PlayerMove()
    {
        int moveRange = battle.Moverange();
        Debug.Log(moveRange);

        Vector2Int playerPoisiton = battle.OnPlayer();

        Vector2Int playerMoveRangeTile = new Vector2Int (playerPoisiton.x + moveRange, playerPoisiton.y);

        Paint(playerMoveRangeTile);
    }
    // 오늘의 목표 ~ 
    //1. 배틀 코드에서 플레이어 입력값 받아오기 
    //2. 이동이면 이동에 칸 색깔 변화 / 공격이면 공격에 맞게 칸 색깔 변화(데이터 베이스 최신화 필요) -> 문제점 : 플레이어 위치 받아오는 얘가 없다. ㅁ
    //3. 마우스 회전에 맞게 디렉션 잡아주기 
    //3. 클릭을 했을 때 배틀로 다시 보내주기 
}
