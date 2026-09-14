using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public Map map;
    public GameObject objPrefab;
    List<GameObject> objs = new List<GameObject>();

    int randomx = 0;
    int randomy = 0;

    public float PlayerSize;
    public static event Action turn;
    void SetSize()
    {
        transform.localScale = new Vector3(map.TileWidthSize * PlayerSize, map.TileHeightSize * PlayerSize, 1);
    }

    private void Start()
    {
       SetSize();
        randomx = UnityEngine.Random.Range(0, map.Width);
        randomy = UnityEngine.Random.Range(0, map.Height);

        float playerx = (map.MapSize / 2 * -1) + (map.TileWidthSize / 2) + (map.TileWidthSize * randomx);
        float playery = (map.MapSize / 2 * -1) + (map.TileHeightSize / 2) + (map.TileHeightSize * randomy);

        transform.localPosition = new Vector3(playerx,playery ,1 );
    }

    public void OnMoveUp(InputValue value)
    {
        transform.position += Vector3.up * map.TileHeightSize;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        Turn();
    }

    public void OnMoveDown(InputValue value)
    {
        transform.position += Vector3.down * map.TileHeightSize;
        transform.rotation = Quaternion.Euler(0, 0, 180);
        Turn();
    }

    public void OnMoveLeft(InputValue value)
    {
        transform.position += Vector3.left * map.TileWidthSize;
        transform.rotation = Quaternion.Euler(0, 0, 90);
        Turn();
    }

    public void OnMoveRight(InputValue value)
    {
        transform.position += Vector3.right * map.TileWidthSize;
        transform.rotation = Quaternion.Euler(0, 0, 270);
        Turn();
    }
    public void OnAttack(InputValue value)
    {
        Turn();

        CreateObj();
    }
    void CreateObj()
    {
        GameObject obj = Instantiate(
            objPrefab,
            transform
        );

        obj.name = objPrefab.name;

        obj.transform.localPosition = new Vector3(0, map.TileHeightSize * 0.75f, -1);
        obj.transform.localRotation = Quaternion.Euler(0,0,-90);

        objs.Add(obj);

        turn += DeleteObjs;
    }

    void DeleteObjs()
    {
        turn -= DeleteObjs;

        foreach (GameObject obj in objs)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        objs.Clear();
    }
    private void Turn()
    {
        turn?.Invoke();
    }
}
