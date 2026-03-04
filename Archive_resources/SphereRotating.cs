using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FloatingCharacterRotator : MonoBehaviour
{
    [Header("🌀 떠있는 애니메이션")]
    public float floatSpeed = 1.5f;
    public float floatAmplitude = 0.25f;

    [Header("🌪️ 기본 진동 회전")]
    public float idleRotationAmplitude = 8f;
    public float idleRotationSpeed = 1.2f;

    [Header("👆 사용자 드래그 회전")]
    public float rotationSpeed = 5f;
    public float friction = 5f;
    public float bounceFactor = 0.3f;
    public float minBounceSpeed = 20f;

    [Header("🖱️ 드래그 가능 영역")]
    public RectTransform dragAreaPanel;

    private float currentYRotation;
    private float dragVelocity = 0f;
    private int lastDragDirection = 0;

    private bool isDragging = false;
    private bool hasDragged = false;
    private bool hasEverDragged = false;

    private Vector3 startPos;
    private float idleOffset = 0f; // y 축 회전 진동값

    // ✅ 이번 "마우스 프레스"가 dragArea 안에서 시작됐는지
    private bool pressStartedInArea = false;

    void Start()
    {
        startPos = transform.localPosition;
        currentYRotation = transform.localEulerAngles.y;
    }

    void Update()
    {
        // ✅ 위아래로 둥실둥실
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);

        // ✅ 마우스가 막 눌린 프레임: 시작 위치가 dragArea 내부인지 기록
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressStartedInArea = IsScreenPointInDragArea(Mouse.current.position.ReadValue());
            isDragging = false;
            hasDragged = false;
        }

        // (선택) 남겨두지만 더는 사용하진 않음 — 시작 시점만 본다
        bool canDrag = true;
        if (dragAreaPanel != null)
            canDrag = IsPointerOverDragArea();

        // ✅ 드래그 중: "시작 위치"가 영역 안이었을 때만 회전 처리
        if (Mouse.current.leftButton.isPressed && pressStartedInArea)
        {
            float dragX = Mouse.current.delta.ReadValue().x;

            if (!isDragging)
            {
                hasDragged = false;
                isDragging = true;
                currentYRotation = transform.localEulerAngles.y;
            }

            hasEverDragged = true;

            if (Mathf.Abs(dragX) > 0.01f)
            {
                hasDragged = true;
                lastDragDirection = dragX > 0 ? 1 : -1;
            }

            dragVelocity = dragX * rotationSpeed;
            currentYRotation -= dragVelocity * Time.deltaTime;
        }
        // ✅ 버튼을 뗀 프레임: 시작이 영역 안이었을 때만 바운스 처리
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (pressStartedInArea && hasDragged)
            {
                isDragging = false;

                if (Mathf.Abs(dragVelocity) < 5f)
                {
                    int bounceDirection = lastDragDirection != 0 ? lastDragDirection : 1;
                    dragVelocity = -minBounceSpeed * bounceDirection;
                }
                else
                {
                    dragVelocity = dragVelocity * -bounceFactor;
                }
            }

            // 다음 프레스를 위한 초기화
            pressStartedInArea = false;
        }
        else
        {
            // ✅ 마찰 감쇠
            if (Mathf.Abs(dragVelocity) > 0.01f)
            {
                currentYRotation -= dragVelocity * Time.deltaTime;
                dragVelocity = Mathf.Lerp(dragVelocity, 0f, friction * Time.deltaTime);
            }
        }

        // ✅ 드래그 중이 아닐 때만 idle 회전 진동 추가
        if (!isDragging)
        {
            idleOffset = Mathf.Sin(Time.time * idleRotationSpeed) * idleRotationAmplitude;
        }
        else
        {
            idleOffset = 0f;
        }

        transform.localEulerAngles = new Vector3(0, currentYRotation + idleOffset, 0);
    }

    // ✅ "현재" 포인터가 dragArea 위에 있는지 (참고용)
    private bool IsPointerOverDragArea()
    {
        if (dragAreaPanel == null) return true;

        Vector2 localMousePosition;
        Camera uiCamera = GetUICameraFor(dragAreaPanel);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragAreaPanel,
            Mouse.current.position.ReadValue(),
            uiCamera,
            out localMousePosition
        );

        return dragAreaPanel.rect.Contains(localMousePosition);
    }

    // ✅ "프레스 시작 지점"이 dragArea 내부인지
    private bool IsScreenPointInDragArea(Vector2 screenPoint)
    {
        if (dragAreaPanel == null) return true;
        return RectTransformUtility.RectangleContainsScreenPoint(
            dragAreaPanel,
            screenPoint,
            GetUICameraFor(dragAreaPanel)
        );
    }

    // ✅ Canvas 렌더모드에 맞는 UI 카메라 반환
    private Camera GetUICameraFor(RectTransform rect)
    {
        var canvas = rect.GetComponentInParent<Canvas>();
        if (canvas == null) return null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera) return canvas.worldCamera;
        // Screen Space - Overlay / World Space는 null로 OK
        return null;
    }
}
