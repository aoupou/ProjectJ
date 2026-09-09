using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public void OnMoveUp(InputValue value)
    {
        transform.position += Vector3.up;
    }

    public void OnMoveDown(InputValue value)
    {
        transform.position += Vector3.down;
    }

    public void OnMoveLeft(InputValue value)
    {
        transform.position += Vector3.left;
    }

    public void OnMoveRight(InputValue value)
    {
        transform.position += Vector3.right;
    }
}
