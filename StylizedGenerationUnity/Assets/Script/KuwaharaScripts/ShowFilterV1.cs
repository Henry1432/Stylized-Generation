using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowFilterV1 : MonoBehaviour
{
    public Shader kuwaharaShader;

    [Range(1, 20)]
    public int kernelSize = 1;

    [Range(1, 5)]
    public int passes = 1;

    private Material kuwaharaMat;


    void OnEnable()
    {
        kuwaharaMat = new Material(kuwaharaShader);
        kuwaharaMat.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        kuwaharaMat.SetInt("_KernelSize", kernelSize);

        RenderTexture[] kuwaharaPasses = new RenderTexture[passes];

        for (int i = 0; i < passes; ++i)
        {
            kuwaharaPasses[i] = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        }

        Graphics.Blit(source, kuwaharaPasses[0], kuwaharaMat);
        for (int i = 1; i < passes; ++i)
        {
            Graphics.Blit(kuwaharaPasses[i - 1], kuwaharaPasses[i], kuwaharaMat);
        }
        Graphics.Blit(kuwaharaPasses[passes - 1], destination);

        for (int i = 0; i < passes; ++i)
        {
            RenderTexture.ReleaseTemporary(kuwaharaPasses[i]);
        }
    }

    void OnDisable()
    {
        kuwaharaMat = null;
    }
}
