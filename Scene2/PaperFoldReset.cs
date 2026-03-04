using UnityEngine;

public class PaperFoldReset : MonoBehaviour
{

    public Material paperMaterial;

    // Shader Graph Reference 이름 (정확히 입력)
    private string rollValueRef = "Vector1_98d33b1d219b486e97f4a6d459a007a3";

    public float defaultValue = 0.14f;    // 초기 기본값


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (paperMaterial != null)
        {
            paperMaterial.SetFloat(rollValueRef, defaultValue);
        }
    }
}
