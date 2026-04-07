using UnityEngine;
using UnityEngine.EventSystems;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float maxScale = 1.2f;
    public float minScale = 0.9f;
    public float scaleDuration = 0.3f;
    
    private Vector3 originalScale;
    private float scaleTimer;
    private bool isHovering;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        scaleTimer += Time.deltaTime;
        float cycle = Mathf.PingPong(scaleTimer / scaleDuration, 1f);
        float targetScale = Mathf.Lerp(minScale, maxScale, cycle);
        transform.localScale = originalScale * targetScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        scaleTimer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}
