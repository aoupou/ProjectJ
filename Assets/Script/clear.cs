using TMPro;
using UnityEngine;

public class clear : MonoBehaviour
{
    [SerializeField] private float current_stage;
    [SerializeField] private TMP_Text clear_text;
    void Start()
    {
        clear_text.text = $"{current_stage} stage clear!!!";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
