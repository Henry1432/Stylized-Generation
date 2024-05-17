using UnityEngine;

public class ShowFilterV4_Outline : MonoBehaviour
{
    public Shader gmOutlineShader;
    public Camera gmMainCam;

    [Range(0, 5)]
    [SerializeField] private float mScale = 0.5f;
    [Range(0, 1)]
    [SerializeField] private float mDepthThreshold = 0.4f;
    [SerializeField] private Color mEdgeColor;


    private Material mOutlineMat;

    void OnEnable()
    {
        mOutlineMat = new Material(gmOutlineShader);
        mOutlineMat.hideFlags = HideFlags.HideAndDontSave;
    }

    private void Start()
    {
        gmMainCam.depthTextureMode = DepthTextureMode.Depth;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture main = new RenderTexture(gmMainCam.activeTexture);
        //Debug.Log(main.mipmapCount);
        main.useMipMap = true;
        main.mipMapBias += 0.15f;


        mOutlineMat.SetTexture("_MainTex", main);
        mOutlineMat.SetFloat("_Scale", mScale);
        mOutlineMat.SetFloat("_DepthThreshold", mDepthThreshold);
        mOutlineMat.SetFloat("_EdgeColorR", mEdgeColor.r);
        mOutlineMat.SetFloat("_EdgeColorG", mEdgeColor.g);
        mOutlineMat.SetFloat("_EdgeColorB", mEdgeColor.b);

        RenderTexture outline = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

        Graphics.Blit(source, outline, mOutlineMat);
        Graphics.Blit(outline, destination);
        RenderTexture.ReleaseTemporary(outline);
    }

    void OnDisable()
    {
        mOutlineMat = null;
    }
}
