using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scrollbar_fixed : MonoBehaviour, IDragHandler
{
    public ScrollRect scrollRect;             // Scroll View 연결
    public RectTransform handle;              // 고정 핸들
    public RectTransform handleArea;          // Sliding 영역

    public float handleHeight = 150f;          // 고정 핸들 높이
    private float usableHeight;

    void Start()
    {
        // 핸들 높이 고정
        handle.sizeDelta = new Vector2(handle.sizeDelta.x, handleHeight);

        // 위쪽 기준으로 얼마나 움직일 수 있는지
        usableHeight = handleArea.rect.height - handleHeight;

        // 시작 위치를 꼭 위로
        scrollRect.verticalNormalizedPosition = 1f;
    }

    void Update()
    {
        float normPos = 1f - scrollRect.verticalNormalizedPosition; // scrollRect는 1 = 위쪽, 0 = 아래쪽

        float y = -normPos * usableHeight; // pivot이 위쪽이므로 y는 음수로 내려감
        handle.anchoredPosition = new Vector2(handle.anchoredPosition.x, y);
    }

    public void OnDrag(PointerEventData eventData)
    {
        float newY = Mathf.Clamp(handle.anchoredPosition.y + eventData.delta.y, -usableHeight, 0f);
        handle.anchoredPosition = new Vector2(handle.anchoredPosition.x, newY);

        float normPos = Mathf.InverseLerp(0f, -usableHeight, newY); // 반대 방향
        scrollRect.verticalNormalizedPosition = 1f - normPos;
    }
}
