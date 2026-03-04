using UnityEngine;

public class Paper_forBottle : MonoBehaviour
{
    public PaperUnfold paperUnfold;                  // 펼치기 스크립트
    public FloatingObject paperFloating;             // Floating 제어 스크립트
    private Transform bottleTransform;

    void Start()
    {
        bottleTransform = transform;
    }

    // 애니메이션 이벤트용 중계 함수: 펼치기
    public void TriggerPaperUnfold()
    {
        if (paperUnfold != null)
        {
            paperUnfold.StartUnfold();
        }
        else
        {
            Debug.LogWarning("Please connect PaperUnfold script in inspector");
        }
    }

    // 애니메이션 이벤트용 중계 함수: 부유 시작
    public void TriggerPaperFloating()
    {
        if (paperFloating != null && bottleTransform != null)
        {
            paperFloating.Detach(true);
        }
        else
        {
            Debug.LogWarning("Please connect FloatingObject script in inspector");
        }
    }
}
