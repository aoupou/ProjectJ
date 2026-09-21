using UnityEngine;
using UnityEngine.InputSystem;

public class exit : MonoBehaviour
{
    public LayerMask buttonLayer; // 버튼 레이어 취급

    private Camera mainCamera; // 카메라 변수 선언
    private void Start()
    {
        mainCamera = Camera.main; // 메인 카메라 컴포넌트 미리 참조
    }
    
    void Exit() // 실행하면 프로그램을 종료시키는 코드
    {
        Application.Quit(); // 종료
    }

    bool MposSensor()
    {
        /*
        Vector2 Mpos = Mouse.current.position.ReadValue(); // 현재 마우스의 좌표값을 가져옴

        Vector2 Wpos =
            mainCamera.ScreenToWorldPoint(Mpos); // 먼저 참조한 메인 카메라의 좌표값을 토대로 현재 마우스 좌표값을 월드 좌표값으로 변환
        */

        Vector2 Wpos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()); // 위 코드를 한줄로 줄인 것. 더 줄일 수 있긴 하지만 가독성의 문제 때문에 패스
        Collider2D hit = Physics2D.OverlapPoint(Wpos, buttonLayer);
        return hit != null && (hit.gameObject == gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (MposSensor())
            {
                Exit();
            }
        }
    }
}
