using UnityEngine;
using UnityEngine.UI;

public class Turn_UI : MonoBehaviour
{
    [SerializeField] private GameObject turnPrefab;
    [SerializeField] private Sprite Turn;
    [SerializeField] private Transform turnParent;
    void Start()
    {
        UpdateTurn();
    }

    // Update is called once per frame
    void UpdateTurn()
    {
        foreach (Transform child in turnParent)
        {
            Destroy(child.gameObject);
        }
        
        GameObject turn = Instantiate(
                turnPrefab,
                turnParent);

        Image image = turn.GetComponent<Image>();
        image.enabled = true;

        image.sprite = Turn;

        


    }

}
