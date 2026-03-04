using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineCamera[] cameras = new CinemachineCamera[4]; // 0: StaticStart, 1: Follow, 2: StaticEnd, 3: CloseUp
    public Transform player;

    public float triggerZ_StartFollow = 330f;   // Follow 시작 기준 z
    public float triggerZ_EndFollow = 481.2f;     // Follow 종료 기준 z (StaticEnd 전환)

    private int[] priorities = new int[] { 40, 30, 20, 10 }; // Priority: StaticStart > Follow > StaticEnd > CloseUp

    private bool hasStartedFollow = false;
    private bool hasEndedFollow = false;

    private Transform followCamTransform;
    private Quaternion followCamInitialRot;

    public static CameraController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 초기 Priority 설정
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = priorities[i];
        }

        // Follow 카메라 위치/회전 저장
        followCamTransform = cameras[1].transform;
        followCamInitialRot = followCamTransform.rotation;
    }

    void Update()
    {
        // 1. Follow 시작 조건: player가 triggerZ_StartFollow에 도달
        if (!hasStartedFollow && player.position.z >= triggerZ_StartFollow)
        {
            cameras[0].Priority = priorities[3]; // StaticStart 낮춤
            cameras[1].Priority = priorities[0]; // Follow 올림
            cameras[2].Priority = priorities[2]; // StaticEnd 기본값 유지
            cameras[3].Priority = priorities[3]; // CloseUp 가장 낮게 유지

            hasStartedFollow = true;
        }

        // 2. Follow 종료 조건: player가 triggerZ_EndFollow에 도달 → StaticEnd 카메라로 전환
        if (hasStartedFollow && !hasEndedFollow && player.position.z >= triggerZ_EndFollow)
        {
            cameras[1].Priority = priorities[2]; // Follow 낮춤
            cameras[2].Priority = priorities[0]; // StaticEnd 올림
            hasEndedFollow = true;
        }
    }

    void LateUpdate()
    {
        // Follow 카메라가 현재 활성화 상태일 때 → z축만 따라감
        if (hasStartedFollow && !hasEndedFollow && cameras[1].Priority == priorities[0])
        {
            Vector3 pos = followCamTransform.position;
            pos.z = player.position.z;
            followCamTransform.position = pos;
            followCamTransform.rotation = followCamInitialRot;
        }
    }

    // 3. 클로즈업 카메라 전환은 애니메이션 이벤트에서 호출할 함수로 분리
    public void SwitchToCloseUpCamera()
    {
        cameras[2].Priority = priorities[2]; // StaticEnd 낮춤
        cameras[3].Priority = priorities[0]; // CloseUp 올림
    }
}
