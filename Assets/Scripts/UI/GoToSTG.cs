using UnityEngine;

public class GoToSTG : MonoBehaviour
{
    [SerializeField] private Stg stg;
    [SerializeField] private Stage_change stage_change;
    private Stage_change.Stg? selectedSTG;
    public enum Stg
    {
        stg0 = 1, //defalt
        stg1,
        stg2,
        stg3,
        stg4,
    };
    public void PushGo()
    {
        switch (stg)
        {
            case Stg.stg1:
                {
                    GoToSTG1();
                    break;
                }
            case Stg.stg2:
                {
                    GoToSTG2();
                    break;
                }
            case Stg.stg3:
                {
                    GoToSTG3();
                    break;
                }
            case Stg.stg4:
                {
                    GoToSTG4();
                    break;
                }
        }
    }
    private void GoToSTG1()
    {
        stage_change.ChangeStageTo(Stage_change.Stg.Sample);
    }
    private void GoToSTG2()
    {
        stage_change.ChangeStageTo(Stage_change.Stg.Sample);
    }
    private void GoToSTG3()
    {
        stage_change.ChangeStageTo(Stage_change.Stg.stg3);
    }
    private void GoToSTG4()
    {
        stage_change.ChangeStageTo(Stage_change.Stg.stg4);
    }
    public void STG1Sellect()
    {
        stg = Stg.stg1;
    }
    public void STG2Sellect()
    {
        stg = Stg.stg2;
    }
    public void STG3Sellect()
    {
        stg = Stg.stg3;
    }
    public void STG4Sellect()
    {
        stg = Stg.stg4;
    }
}
