using UnityEngine;
using Unity.Cinemachine;

public class cameraFollowTrigger : MonoBehaviour
{
    public CinemachineCamera[] cameras = new CinemachineCamera[3];
    // static - follow - closeup

    public Transform player;         // 캐릭터 Transform
    public float triggerZ = 15f;     // 따라가기 시작할 z 위치

    private int[] priorities = new int[] { 30, 20, 10 };

    private bool switched = false;
    private bool hasStartedFollowing = false;

    void Start()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = priorities[i]; // static > follow > closeup
        }
    }

    void Update()
    {
        if (!switched && player.position.z >= triggerZ)
        {
            // 1번 카메라(따라가기)를 최우선으로 설정
            cameras[0].Priority = priorities[2]; // Static → 낮춤
            cameras[1].Priority = priorities[0]; // Follow → 최상
            cameras[2].Priority = priorities[1]; // CloseUp → 중간

            switched = true;
        }
    }
}
