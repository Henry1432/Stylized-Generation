Shader "Hidden/KuwaharaFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _KernelSize;
            
            float luminance(float3 color) {
                return dot(color, float3(0.299f, 0.587f, 0.114f));
            }

            float4 GrabColor(float2 uv, int x1, int x2, int y1, int y2, float n)
            {
                float light = 0.0f, lightpow = 0.0f;
                float3 col_total = 0.0f;

                [loop]
                for(int x = x1; x <= x2; ++x)
                {
                    [loop]
                    for (int y = y1; y <= y2; ++y)
                    {
                        //got the color grab from Kuwahara Git here: https://github.com/GarrettGunnell/Post-Processing/tree/main/Assets/Kuwahara%20Filter
                        float3 col = tex2D(_MainTex, uv + float2(x, y) * _MainTex_TexelSize.xy).rgb;
                        float l = luminance(col);
                        light += l;
                        lightpow += l * l;
                        col_total += saturate(col);
                    }
                }                

                float ava = light/n;
                float std = abs(lightpow / n - ava * ava);

                return float4(col_total / n, std);
            }

            float4 frag(v2f i) : SV_Target
            {
                float windowSize = 2.0f * _KernelSize + 1;
                int quadrantSize = int(ceil(windowSize / 2.0f));
                int numSamples = quadrantSize * quadrantSize;

                float4 q1 = GrabColor(i.uv, -_KernelSize, 0, -_KernelSize, 0, numSamples);
                float4 q2 = GrabColor(i.uv, 0, _KernelSize, -_KernelSize, 0, numSamples);
                float4 q3 = GrabColor(i.uv, 0, _KernelSize, 0, _KernelSize, numSamples);
                float4 q4 = GrabColor(i.uv, -_KernelSize, 0, 0, _KernelSize, numSamples);
                
                float minstd = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minstd;

                if (dot(q, 1) > 1)
                    return saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                else
                    return saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));
            }
            ENDCG
        }
    }
}
