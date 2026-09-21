using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
    public Map map;

    public float PlayerSize;

    [SerializeField] private float moveDuration = 1f;

    public static event Action turn;

    private SpriteAnimator animator;
    private SpriteRenderer spriteRenderer;

    private bool isMoving;

    private void Awake()
    {
        animator = GetComponent<SpriteAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetSize();

        int randomx = UnityEngine.Random.Range(0, map.Width);
        int randomy = UnityEngine.Random.Range(0, map.Height);

        float playerx =
            (map.MapSize / 2 * -1)
            + (map.TileWidthSize / 2)
            + (map.TileWidthSize * randomx);

        float playery =
            (map.MapSize / 2 * -1)
            + (map.TileHeightSize / 2)
            + (map.TileHeightSize * randomy);

        transform.localPosition =
            new Vector3(playerx, playery, 1);

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

    public void OnMoveUp(InputValue value)
    {
        if (isMoving)
            return;

        StartCoroutine(Move(
            Vector3.up * map.TileHeightSize
        ));
    }

    public void OnMoveDown(InputValue value)
    {
        if (isMoving)
            return;

        StartCoroutine(Move(
            Vector3.down * map.TileHeightSize
        ));
    }

    public void OnMoveLeft(InputValue value)
    {
        if (isMoving)
            return;

        spriteRenderer.flipX = true;

        StartCoroutine(Move(
            Vector3.left * map.TileWidthSize
        ));
    }

    public void OnMoveRight(InputValue value)
    {
        if (isMoving)
            return;

        spriteRenderer.flipX = false;

        StartCoroutine(Move(
            Vector3.right * map.TileWidthSize
        ));
    }

    private IEnumerator Move(Vector3 movement)
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + movement;

        // 이동 시작 → Walking
        animator.SetState("Walking", true);

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                elapsedTime / moveDuration;

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            yield return null;
        }

        // 정확히 목표 위치에 맞추기
        transform.position = targetPosition;

        // 이동 완료 → Idle
        animator.SetState("Idle", true);

        isMoving = false;

        // 이동이 끝났으므로 턴
        Turn();
    }

    public void Turn()
    {
        turn?.Invoke();
    }
}