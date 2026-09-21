using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteAnimationCollection collection;

    private SpriteRenderer spriteRenderer;
    private SpriteAnimationData current;
    private float time;

    public string State { get; private set; }
    public bool IsFinished { get; private set; }

    /// <summary>현재 애니메이션 1회 재생 시간(초)</summary>
    public float Duration => current != null && current.sprites != null
        ? current.sprites.Length / current.framesPerSecond : 0f;

    private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

    private void OnEnable()
    {
        if (collection != null) SetState(collection.defaultState, true);
    }

    /// <summary>상태 이름에 맞는 애니메이션을 재생. 같은 상태면 이어서 재생(restart=true면 처음부터).</summary>
    public void SetState(string state, bool restart = false)
    {
        if (!restart && state == State && current != null) return;

        var next = collection != null ? collection.Get(state) : null;
        if (next == null)
        {
            Debug.LogWarning($"[SpriteAnimator] '{state}' 상태의 애니메이션이 없습니다.", this);
            return;
        }
        State = state;
        current = next;
        time = 0f;
        IsFinished = false;
        Apply();
    }

    /// <summary>컬렉션 상태와 무관하게 애니메이션 에셋을 직접 처음부터 재생(콤보 단계용).</summary>
    public void Play(SpriteAnimationData anim, string stateName = null)
    {
        if (anim == null) return;
        State = stateName;
        current = anim;
        time = 0f;
        IsFinished = false;
        Apply();
    }

    /// <summary>컬렉션의 콤보 단계 목록 (없으면 null)</summary>
    public System.Collections.Generic.List<SpriteAnimationCollection.ComboStep> GetCombo(string name)
        => collection != null ? collection.GetCombo(name) : null;

    private void Update()
    {
        if (current == null || IsFinished) return;
        time += Time.deltaTime;
        Apply();
    }

    private void Apply()
    {
        Sprite[] frames = current.sprites;
        if (frames == null || frames.Length == 0) return;

        int frame = (int)(time * current.framesPerSecond);
        if (current.loop) frame %= frames.Length;
        else if (frame >= frames.Length) { frame = frames.Length - 1; IsFinished = true; }

        spriteRenderer.sprite = frames[frame];
    }
}
