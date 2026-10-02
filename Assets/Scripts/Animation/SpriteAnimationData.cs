using UnityEngine;

[CreateAssetMenu(
    fileName = "SpriteAnimationData",
    menuName = "Animation/Sprite Animation Data"
)]
public class SpriteAnimationData : ScriptableObject
{
    public Sprite[] sprites;

    public float framesPerSecond = 12f;

    public bool loop = true;
}