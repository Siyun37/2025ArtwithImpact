using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTilt : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("회전 설정")]
    public float tiltAngle = 8f;   // 최대 회전 각도 (좌우)
    public float tiltSpeed = 4f;   // 흔들리는 속도

    private bool isHover = false;
    private float currentTilt = 0f;
    private RectTransform rectTr;

    void Awake()
    {
        rectTr = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 마우스 올라가 있으면 좌우로 살짝살짝 흔들기 (sin 파)
        float targetTilt = isHover 
            ? Mathf.Sin(Time.time * tiltSpeed) * tiltAngle 
            : 0f; // hover 끝나면 0도로 자연스럽게 돌아감

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * 10f);

        // Y축 기준으로 좌우 회전
        rectTr.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}
