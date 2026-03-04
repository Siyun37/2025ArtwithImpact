using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraController_S2_2 : MonoBehaviour
{
    public CinemachineCamera[] cameras = new CinemachineCamera[2];
    // 0 : static in front of the tree , 1 : closeup
    private int[] priorities = new int[] { 20, 10 };

    public float delayBeforeSwitch = 1f; // 딜레이 시간

    void Start()
    {
        this.TryStartCoroutine(SwitchCameraWithDelay());
    }

    IEnumerator SwitchCameraWithDelay()
    {
        // 시작 시에는 기본 상태 유지 (Priority 변경 없음)
        yield return new WaitForSeconds(delayBeforeSwitch);

        // 딜레이 후 Priority 변경
        cameras[0].Priority = priorities[1];  // 낮은 우선순위
        cameras[1].Priority = priorities[0];  // 높은 우선순위
    }
}
