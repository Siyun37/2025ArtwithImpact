using UnityEngine;
using System.Collections;

public class PaperUnfold : MonoBehaviour
{
    public Material paperMaterial;

    [Header("Shader Property ID")]
    private string rollValueRef = "Vector1_98d33b1d219b486e97f4a6d459a007a3";

    [Header("Roll Settings")]
    public float unfoldTarget = 1.0f;
    public float defaultValue = 0.14f;
    public float duration = 1.5f;
    public float waitBeforeFold = 10f;  // 다 펴진 후 기다릴 시간

    private bool isAnimating = false;

    void Start()
    {
        if (paperMaterial != null)
        {
            paperMaterial.SetFloat(rollValueRef, defaultValue);
        }
    }

    public void StartUnfold()
    {
        if (paperMaterial != null && !isAnimating)
        {
            this.TryStartCoroutine(UnfoldAndFold());
        }
    }

    private IEnumerator UnfoldAndFold()
    {
        isAnimating = true;

        // 펼치기
        yield return this.TryStartCoroutine(AnimateUnfold(defaultValue, unfoldTarget));

        // 다 펼치고 대기
        yield return new WaitForSeconds(waitBeforeFold);

        // 다시 접기
        yield return this.TryStartCoroutine(AnimateUnfold(unfoldTarget, defaultValue));

        isAnimating = false;
    }

    private IEnumerator AnimateUnfold(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float value = Mathf.Lerp(from, to, elapsed / duration);
            paperMaterial.SetFloat(rollValueRef, value);
            elapsed += Time.deltaTime;
            yield return null;
        }

        paperMaterial.SetFloat(rollValueRef, to); // 정확한 종료 값 적용
    }

    // 필요하면 외부에서 리셋 호출도 가능
    public void ResetRoll()
    {
        if (paperMaterial != null)
        {
            paperMaterial.SetFloat(rollValueRef, defaultValue);
        }
    }
}
