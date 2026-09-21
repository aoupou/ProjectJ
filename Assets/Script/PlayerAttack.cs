using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    public GameObject objPrefab;

    private List<GameObject> objs =
        new List<GameObject>();

    public void OnAttack(InputValue value)
    {
        CreateObj();

        GetComponent<Player>().Turn();
    }

    void CreateObj()
    {
        GameObject obj =
            Instantiate(
                objPrefab,
                transform
            );

        obj.name =
            objPrefab.name;

        obj.transform.localPosition =
            new Vector3(
                0,
                GetComponent<Player>().map.TileHeightSize * 0.75f,
                -1
            );

        obj.transform.localRotation =
            Quaternion.Euler(
                0,
                0,
                -90
            );

        objs.Add(obj);

        Player.turn += DeleteObjs;
    }

    void DeleteObjs()
    {
        Player.turn -= DeleteObjs;

        foreach (GameObject obj in objs)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        objs.Clear();
    }
}