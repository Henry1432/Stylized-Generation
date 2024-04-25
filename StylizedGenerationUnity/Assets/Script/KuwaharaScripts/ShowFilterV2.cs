using UnityEngine;

public class ShowFilterV2 : MonoBehaviour
{
    public Shader gmKuwaharaShader;

    [Range(2, 25)]
    [SerializeField] private int mKernelSize = 10;
    [Range(1.0f, 30.0f)]
    [SerializeField] private float mSharpness = 20;
    [Range(0.0f, 50)]
    [SerializeField] private float mHardness = 35;
    [Range(0.01f, 2f)]
    [SerializeField] private float mAlpha = 1.7f;
    [Range(0.26f, 5f)]
    [SerializeField] private float mProcessSoftness = 1.5f;

    [SerializeField] private bool mUseSoften = true;
    [Range(0.01f, 1.0f)]
    [SerializeField] private float mSoftness = 0.25f;

    [Range(1, 10)]
    [SerializeField] private int mPasses = 3;

    private Material mKuwaharaMat;


    void OnEnable()
    {
        mKuwaharaMat = new Material(gmKuwaharaShader);
        mKuwaharaMat.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //I coppied this part more directly from the git https://github.com/GarrettGunnell/Post-Processing/blob/main/Assets/Kuwahara%20Filter/AnisotropicKuwahara.shader
        mKuwaharaMat.SetInt("_KernelSize", mKernelSize);
        mKuwaharaMat.SetInt("_N", 8);
        mKuwaharaMat.SetFloat("_Q", mSharpness);
        mKuwaharaMat.SetFloat("_Hardness", mHardness);
        mKuwaharaMat.SetFloat("_Alpha", mAlpha);
        mKuwaharaMat.SetFloat("_ZeroCrossing", mProcessSoftness);
        mKuwaharaMat.SetFloat("_Softness", mUseSoften ? mSoftness : 2.0f / 2.0f / (mKernelSize / 2.0f));

        var structureTensor = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        Graphics.Blit(source, structureTensor, mKuwaharaMat, 0);
        var eigenvectors1 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        Graphics.Blit(structureTensor, eigenvectors1, mKuwaharaMat, 1);
        var eigenvectors2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        Graphics.Blit(eigenvectors1, eigenvectors2, mKuwaharaMat, 2);
        mKuwaharaMat.SetTexture("_TFM", eigenvectors2);

        RenderTexture[] kuwaharaPasses = new RenderTexture[mPasses];

        for (int i = 0; i < mPasses; ++i)
        {
            kuwaharaPasses[i] = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        }

        Graphics.Blit(source, kuwaharaPasses[0], mKuwaharaMat, 3);

        for (int i = 1; i < mPasses; ++i)
        {
            Graphics.Blit(kuwaharaPasses[i - 1], kuwaharaPasses[i], mKuwaharaMat, 3);
        }

        //Graphics.Blit(structureTensor, destination);
        Graphics.Blit(kuwaharaPasses[mPasses - 1], destination);

        RenderTexture.ReleaseTemporary(structureTensor);
        RenderTexture.ReleaseTemporary(eigenvectors1);
        RenderTexture.ReleaseTemporary(eigenvectors2);
        for (int i = 0; i < mPasses; ++i)
        {
            RenderTexture.ReleaseTemporary(kuwaharaPasses[i]);
        }
    }

    void OnDisable()
    {
        mKuwaharaMat = null;
    }
}
