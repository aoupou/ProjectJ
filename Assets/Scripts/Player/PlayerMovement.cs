using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static Battle;

public class Player : MonoBehaviour
{
    public Battle battle;
    public Map map;

    public float PlayerSize;

    [SerializeField] private AudioClip moveInSound;  // 마법진으로 들어갈 때
    [SerializeField] private AudioClip moveOutSound; // 마법진에서 나올 때
    [SerializeField, Range(0f, 1f)] private float moveVolume = 1f;

    public static event Action movingOut;
    public static event Action moveSellecting;

    private SpriteAnimator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private bool isMoving;

    private void Awake()
    {
        animator = GetComponent<SpriteAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 씬에 AudioSource가 없어도 동작하도록 없으면 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        SetSize();

        int random_x = UnityEngine.Random.Range(0, map.Width);
        int random_y = UnityEngine.Random.Range(0, map.Height);

        float player_x =
            map.MapStartX
            + (map.TileWidthSize / 2)
            + (map.TileWidthSize * random_x);

        float player_y =
            map.MapStartY
            + (map.TileHeightSize / 2)
            + (map.TileHeightSize * random_y);

        transform.position =
            new Vector3(player_x, player_y, 1);

        animator.SetState("Idle", true);
    }

    private void SetSize()
    {
        transform.localScale = new Vector3(
            map.TileWidthSize * PlayerSize,
            map.TileHeightSize * PlayerSize,
            1
        );
    }
    public void MoveUp()
    {
        if (isMoving)
            return;

        StartCoroutine(Move(
            Vector3.up * map.TileHeightSize
        ));
    }
    public void MoveDown()
    {
        if (isMoving)
            return;

        StartCoroutine(Move(
            Vector3.down * map.TileHeightSize
        ));
    }
    public void MoveLeft()
    {
        if (isMoving)
            return;

        spriteRenderer.flipX = true;

        StartCoroutine(Move(
            Vector3.left * map.TileWidthSize
        ));
    }
    public void MoveRight()
    {
        if (isMoving)
            return;

        spriteRenderer.flipX = false;

        StartCoroutine(Move(
            Vector3.right * map.TileWidthSize
        ));
    }

    public void OnMoveUp(InputValue value) // 이거 공격으로도 쓸 수 있을 듯 싶긴한데 나중에 차차 생각해보기
    {
        if (battle.Step != Battle_Step.SelectDirection)
            return;

        if (isMoving)
            return;

        StartCoroutine(Move(
            Vector3.up * map.TileHeightSize
        ));
        
        MoveSellecting();
    }

    public void OnMoveDown(InputValue value)
    {
        if (battle.Step != Battle_Step.SelectDirection)
            return;

        if (isMoving)
            return;

        StartCoroutine(Move(
            Vector3.down * map.TileHeightSize
        ));
        
        MoveSellecting();
    }

    public void OnMoveLeft(InputValue value) 
    {
        if (battle.Step != Battle_Step.SelectDirection)
            return;

        if (isMoving)
            return;

        spriteRenderer.flipX = true;

        StartCoroutine(Move(
            Vector3.left * map.TileWidthSize
        ));
        
        MoveSellecting();
    }

    public void OnMoveRight(InputValue value)
    {
        if (battle.Step != Battle_Step.SelectDirection)
            return;

        if (isMoving)
            return;

        spriteRenderer.flipX = false;

        StartCoroutine(Move(
            Vector3.right * map.TileWidthSize
        ));
        
        MoveSellecting();
    }


    private IEnumerator Move(Vector3 movement)
    {
        isMoving = true;

        Vector3 targetPosition = transform.position + movement;

        // 마법진 속으로 들어감
        PlaySound(moveInSound);
        yield return PlayOnce("WalkingOn");

        // 이동할 칸으로 순간이동
        transform.position = targetPosition;

        // 마법진 속에서 나옴
        PlaySound(moveOutSound);
        yield return PlayOnce("WalkingOff");

        // 이동 완료 → Idle
        animator.SetState("Idle", true);

        isMoving = false;

        // 이동이 끝났으므로 턴
        MovingOut();
    }

    // 효과음 재생 (Inspector에 안 넣었으면 아무것도 안 함)
    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        audioSource.PlayOneShot(clip, moveVolume);
    }

    // 애니메이션을 한 번 재생하고 끝날 때까지 기다림
    private IEnumerator PlayOnce(string state)
    {
        animator.SetState(state, true);

        // 컬렉션에 없는 상태면 기다리지 않음
        if (animator.State != state)
            yield break;

        float elapsedTime = 0f;

        // loop가 켜져 있어도 1회 재생 시간이 지나면 끝냄
        while (!animator.IsFinished && elapsedTime < animator.Duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void MovingOut()
    {
        movingOut?.Invoke();
    }

    public void MoveSellecting()
    {
        moveSellecting?.Invoke();
    }
}