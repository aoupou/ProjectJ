using UnityEngine;
using UnityEngine.UI;

public class Player_HP : MonoBehaviour
{
    [SerializeField] private int currentHP = 10;

    [SerializeField] private GameObject heartPrefab;

    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;

    [SerializeField] private Transform heartParent;

    void Update() // 테스트만 업데이트 나중에는 스타트로 바꿀 것
    {
        UpdateHearts();
    }

    void UpdateHearts()
    {
        // 기존 하트 삭제
        foreach (Transform child in heartParent)
        {
            Destroy(child.gameObject);
        }

        // 꽉찬 하트 개수
        int fullHeartCount = currentHP / 2;

        // 나머지 존재여부 확인
        int remainder = currentHP % 2;

        // 꽉찬 하트 생성
        for (int i = 0; i < fullHeartCount; i++)
        {
            GameObject heart = Instantiate(
                heartPrefab,
                heartParent
            );

            Image image = heart.GetComponent<Image>();

            image.sprite = fullHeart;
        }

        // 나머지가 존재하면 반하트
        if (remainder == 1)
        {
            GameObject heart = Instantiate(
                heartPrefab,
                heartParent
            );

            Image image = heart.GetComponent<Image>();

            image.sprite = halfHeart;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP < 0)
        {
            currentHP = 0;
        }

        UpdateHearts();
    }
}