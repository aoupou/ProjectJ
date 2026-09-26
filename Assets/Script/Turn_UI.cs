using TMPro;
using UnityEngine;

public class Turn_UI : MonoBehaviour
{
    public TextMeshProUGUI turnText;

    public void SetTurn(int turn)
    {
        turnText.text = $"{turn}";
    }
}