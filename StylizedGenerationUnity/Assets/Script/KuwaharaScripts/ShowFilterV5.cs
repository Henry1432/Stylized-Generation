using UnityEngine;

public class ShowFilterV5 : MonoBehaviour
{
    public Shader gmHenryShader;
    public Camera gmMainCam;

    [Range(2, 25)]
    [SerializeField] private int mKernelSize = 8;
    [Range(1.0f, 50.0f)]
    [SerializeField] private float mSharpness = 25;
    [Range(0.0f, 1)]
    [SerializeField] private float mHardness = 0.72f;
    [Range(0.01f, 2f)]
    [SerializeField] private float mAlpha = 1.5f;
    [Range(0.26f, 5f)]
    [SerializeField] private float mProcessSoftness = 0.8f;

    [SerializeField] private bool mUseSoften = true;
    [Range(0.01f, 1.0f)]
    [SerializeField] private float mSoftness = 0.5f;

    [Range(1, 10)]
    [SerializeField] private int mPasses = 2;

    [Header("Outline")]

    [Range(0, 5)]
    [SerializeField] private float mScale = 0.8f;
    [Range(0, 1)]
    [SerializeField] private float mDepthThreshold = 0.3f;
    [SerializeField] private Color mEdgeColor;

    private Material mHenryMat;

    void OnEnable()
    {
        mHenryMat = new Material(gmHenryShader);
        mHenryMat.hideFlags = HideFlags.HideAndDontSave;
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture main = new RenderTexture(gmMainCam.activeTexture);
        //Debug.Log(main.mipmapCount);
        main.useMipMap = true;
        main.mipMapBias += 0.15f;

        mHenryMat.SetTexture("_MainTex", main);

        Screen.SetResolution(1440, 810, false);
        //I coppied this part more directly from the git https://github.com/GarrettGunnell/Post-Processing/blob/main/Assets/Kuwahara%20Filter/AnisotropicKuwahara.shader
        mHenryMat.SetInt("_KernelSize", mKernelSize);
        mHenryMat.SetInt("_N", 8);
        mHenryMat.SetFloat("_Q", mSharpness);
        mHenryMat.SetFloat("_Hardness", mHardness);
        mHenryMat.SetFloat("_Alpha", mAlpha);
        mHenryMat.SetFloat("_ZeroCrossing", mProcessSoftness);
        mHenryMat.SetFloat("_Softness", mUseSoften ? mSoftness : 2.0f / 2.0f / (mKernelSize / 2.0f));

        //actually implement in shader, figure out how to overlay the outline on the existing frame, should be able to just run it at end, only question is how to not have the screen be black in the void
        mHenryMat.SetFloat("_Scale", mScale);
        mHenryMat.SetFloat("_DepthThreshold", mDepthThreshold);
        mHenryMat.SetFloat("_EdgeColorR", mEdgeColor.r);
        mHenryMat.SetFloat("_EdgeColorG", mEdgeColor.g);
        mHenryMat.SetFloat("_EdgeColorB", mEdgeColor.b);

        var structureTensor = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        Graphics.Blit(source, structureTensor, mHenryMat, 0);
        var eigenvectors1 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        Graphics.Blit(structureTensor, eigenvectors1, mHenryMat, 1);
        var eigenvectors2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        Graphics.Blit(eigenvectors1, eigenvectors2, mHenryMat, 2);
        mHenryMat.SetTexture("_TFM", eigenvectors1);

        RenderTexture[] kuwaharaPasses = new RenderTexture[mPasses];

        for (int i = 0; i < mPasses; ++i)
        {
            kuwaharaPasses[i] = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        }

        Graphics.Blit(source, kuwaharaPasses[0], mHenryMat, 3);

        for (int i = 1; i < mPasses; ++i)
        {
            Graphics.Blit(kuwaharaPasses[i - 1], kuwaharaPasses[i], mHenryMat, 3);
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
        mHenryMat = null;
    }
}
