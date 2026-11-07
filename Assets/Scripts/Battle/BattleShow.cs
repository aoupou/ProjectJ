using UnityEngine;
using System.Collections;

public class BattleShow : MonoBehaviour
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] Battle battle;

    private IEnumerator moveRoutineOn()
    {
        spriteAnimator.SetState("WalkingOn", true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        spriteAnimator.SetState("Idle", true);
        battle.Finish_Anim();
    }
    private IEnumerator moveRoutineOff()
    {
        spriteAnimator.SetState("WalkingOff", true);
        yield return new WaitForSeconds(spriteAnimator.Duration);

        spriteAnimator.SetState("Idle", true);
        battle.Finish_Anim();
    }
    public void MoveAniOn()
    {
        StartCoroutine(moveRoutineOn());
    }
    public void MoveAniOff()
    {
        StartCoroutine(moveRoutineOff());
    }
    void Update()
    {
        
    }
}
