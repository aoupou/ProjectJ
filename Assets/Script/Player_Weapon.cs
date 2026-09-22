using UnityEngine;
using UnityEngine.UI;
public class Player_Weapon : MonoBehaviour
{
    [SerializeField] private Transform weaponLayout;
    [SerializeField] private Transform subWeaponLayout;

    [SerializeField] private GameObject MainWeapon;
    [SerializeField] private GameObject SubWeapon;

    void Start()
    {
        CreateWeaponUI();
    }

    private void CreateWeaponUI()
    {
        // 메인 무기 레이아웃 안의 기존 UI 삭제
        foreach (Transform child in weaponLayout)
        {
            Destroy(child.gameObject);
        }

        // 서브 무기 레이아웃 안의 기존 UI 삭제
        foreach (Transform child in subWeaponLayout)
        {
            Destroy(child.gameObject);
        }


        // 메인 무기 UI 생성
        GameObject mainWeapon = Instantiate(
            MainWeapon,
            weaponLayout
        );

        Image main = mainWeapon.GetComponent<Image>();
        main.enabled = true;

        // 서브 무기 UI 생성
        GameObject subWeapon = Instantiate(
            SubWeapon,
            subWeaponLayout
        );

        Image sub = subWeapon.GetComponent<Image>();
        sub.enabled = true;
    }
}
