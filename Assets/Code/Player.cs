using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 1;
    public Map map;

    
    public void OnMoveUp(InputValue value)
    {
        transform.position += Vector3.up * speed;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void OnMoveDown(InputValue value)
    {
        transform.position += Vector3.down * speed;
        transform.rotation = Quaternion.Euler(0, 0, 180);
    }

    public void OnMoveLeft(InputValue value)
    {
        transform.position += Vector3.left * speed;
        transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    public void OnMoveRight(InputValue value)
    {
        transform.position += Vector3.right * speed;
        transform.rotation = Quaternion.Euler(0, 0, 270);
    }
}
