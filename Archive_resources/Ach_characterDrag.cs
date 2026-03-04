using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Ach_characterDrag : MonoBehaviour
{
    [Header("회전 설정")]
    public float rotationSpeed = 5f;
    public float friction = 5f;
    public float bounceFactor = 0.3f;
    public float minBounceSpeed = 20f;

    [Header("드래그 제한 영역 (UI 패널)")]
    public RectTransform dragAreaPanel; // ✅ 드래그 시작 허용 영역

    private float currentYRotation;
    private float velocity = 0f;
    private int lastDragDirection = 0;

    private bool isDragging = false;
    private bool hasDragged = false;
    private bool hasEverDragged = false;

    // ✅ 이번 "마우스 누름"이 dragArea 안에서 시작됐는지
    private bool pressStartedInArea = false;

    void Start()
    {
        currentYRotation = transform.rotation.eulerAngles.y;
    }

    void Update()
    {
        // ✅ 마우스가 막 눌린 순간: 시작 위치가 dragArea 안인지 기록
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressStartedInArea = IsScreenPointInDragArea(Mouse.current.position.ReadValue());
            // 이번 프레스를 새로 시작
            isDragging = false;
            hasDragged = false;
        }

        // 아직 한 번도 드래그한 적 없고, 마우스도 안 눌렸으면 빠른 리턴
        if (!hasEverDragged && !Mouse.current.leftButton.isPressed) return;

        // ✅ 누르는 동안: 시작 위치가 dragArea 내부였을 때만 드래그 처리
        if (Mouse.current.leftButton.isPressed && pressStartedInArea)
        {
            float dragX = Mouse.current.delta.ReadValue().x;

            if (!isDragging)
                hasDragged = false;

            isDragging = true;
            hasEverDragged = true;

            if (Mathf.Abs(dragX) > 0.01f)
            {
                hasDragged = true;
                lastDragDirection = dragX > 0 ? 1 : -1;
            }

            velocity = dragX * rotationSpeed;
            currentYRotation -= velocity * Time.deltaTime;
        }
        // 버튼을 뗀 프레임: 반동/바운스 처리 (시작 위치가 영역 밖에서 눌렸던 경우는 무시)
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (pressStartedInArea && hasDragged)
            {
                isDragging = false;

                if (Mathf.Abs(velocity) < 5f)
                {
                    int bounceDirection = lastDragDirection != 0 ? lastDragDirection : 1;
                    velocity = -minBounceSpeed * bounceDirection;
                }
                else
                {
                    velocity = velocity * -bounceFactor;
                }
            }

            // 다음 프레스 준비
            pressStartedInArea = false;
        }
        else
        {
            // 마찰 감쇠
            if (Mathf.Abs(velocity) > 0.01f)
            {
                currentYRotation -= velocity * Time.deltaTime;
                velocity = Mathf.Lerp(velocity, 0f, friction * Time.deltaTime);
            }
        }

        transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
    }

    // ✅ 드래그 "시작 시점"의 스크린 포인트가 dragArea 내부인지 확인
    private bool IsScreenPointInDragArea(Vector2 screenPoint)
    {
        if (dragAreaPanel == null) return true; // dragArea 미지정이면 제한 없음
        return RectTransformUtility.RectangleContainsScreenPoint(
            dragAreaPanel,
            screenPoint,
            GetUICameraFor(dragAreaPanel)
        );
    }

    // ✅ 해당 Canvas의 렌더 모드에 맞는 UI 카메라 반환
    private Camera GetUICameraFor(RectTransform rect)
    {
        var canvas = rect.GetComponentInParent<Canvas>();
        if (canvas == null) return null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera) return canvas.worldCamera;
        // Screen Space - Overlay, World Space는 null 사용
        return null;
    }
}
