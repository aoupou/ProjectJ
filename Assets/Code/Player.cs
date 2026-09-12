using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Map map;

    int randomx = 0;
    int randomy = 0;

    public float PlayerSize;
    void SetSize()
    {
        transform.localScale = new Vector3(map.TileWidthSize * PlayerSize, map.TileHeightSize * PlayerSize, 1);
    }

    private void Start()
    {
       SetSize();
        randomx = Random.Range(0, map.Width);
        randomy = Random.Range(0, map.Height);

        float playerx = (map.MapSize / 2 * -1) + (map.TileWidthSize / 2) + (map.TileWidthSize * randomx);
        float playery = (map.MapSize / 2 * -1) + (map.TileHeightSize / 2) + (map.TileHeightSize * randomy);

        transform.localPosition = new Vector3(playerx,playery ,1 );
    }

    public void OnMoveUp(InputValue value)
    {
        transform.position += Vector3.up * map.TileHeightSize;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void OnMoveDown(InputValue value)
    {
        transform.position += Vector3.down * map.TileHeightSize;
        transform.rotation = Quaternion.Euler(0, 0, 180);
    }

    public void OnMoveLeft(InputValue value)
    {
        transform.position += Vector3.left * map.TileWidthSize;
        transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    public void OnMoveRight(InputValue value)
    {
        transform.position += Vector3.right * map.TileWidthSize;
        transform.rotation = Quaternion.Euler(0, 0, 270);
    }
}
