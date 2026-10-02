using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    private Collider2D tri;

    private void Start()
    {
        tri = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Vector2 Mpos = Mouse.current.position.ReadValue();

        Vector2 Wpos =
            Camera.main.ScreenToWorldPoint(Mpos);

        if (tri.OverlapPoint(Wpos))
        {
            Debug.Log("!");
        }
    }
}