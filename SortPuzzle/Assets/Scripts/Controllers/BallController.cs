using UnityEngine;
using UnityEngine.UI;
using static Ball;

public class BallController : MonoBehaviour
{
    public BallColor color;
    public void SetSprite(Sprite newSprite)
    {
        Image imageComponent = GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = newSprite;
            imageComponent.color = Color.white;
        }
    }
}