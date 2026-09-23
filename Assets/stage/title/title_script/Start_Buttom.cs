using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Start_Buttom : MonoBehaviour
{
    [SerializeField] private LayerMask buttonLayer;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (MposSensor())
            {
                StartGame(); // 샘플 씬으로 이동
            }
        }
    }

    public void StartGame() // 씬을 이동시키는 코드
    {
        SceneManager.LoadScene("stage_choice");
    }

    bool MposSensor()
    {
        Vector2 Mpos = Mouse.current.position.ReadValue();

        Vector2 Wpos =
            mainCamera.ScreenToWorldPoint(Mpos);

        Collider2D hit =
            Physics2D.OverlapPoint(Wpos, buttonLayer);

        return hit != null && (hit.gameObject == gameObject);
    }
}