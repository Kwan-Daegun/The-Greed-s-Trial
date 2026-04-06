using UnityEngine;
 
[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;
 
    public void Move(float delta)
{
    // Adding a tiny bit of "snapping" or using a more stable movement
    float moveAmount = delta * parallaxFactor;
    transform.Translate(Vector3.left * moveAmount);
}
 
}