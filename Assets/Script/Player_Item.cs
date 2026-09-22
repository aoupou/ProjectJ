using UnityEngine;
using UnityEngine.UI;

public class Player_Item : MonoBehaviour
{
    [SerializeField] private int Item_Count = 3;

    [SerializeField] private GameObject Item_Prefab;

    [SerializeField] private Sprite Empty_Item;
    [SerializeField] private Transform ItemParent;

    void Start()
    {
        UpdateItem();

    }

    // Update is called once per frame
    private void UpdateItem()
    {
        foreach (Transform child in ItemParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < Item_Count; i++)
        {
            GameObject Item = Instantiate(
                Item_Prefab,
                ItemParent
            );

            Image image = Item.GetComponent<Image>();
            
            image.sprite = Empty_Item;

            image.enabled = true;
        }
    }
}
