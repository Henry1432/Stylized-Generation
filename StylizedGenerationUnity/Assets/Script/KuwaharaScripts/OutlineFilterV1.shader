Shader "Hidden/OutlineFilterV1"
{
    Properties
    {

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
                float2 depth : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.depth = ComputeScreenPos(o.vertex).z;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Scale;
            float _DepthThreshold;
            float _EdgeColorR;
            float _EdgeColorG;
            float _EdgeColorB;

            sampler2D _CameraDepthTexture;

            float4 frag(v2f i) : SV_Target
            {
                float2 uvTR = float2(i.uv.x + (_MainTex_TexelSize.x * _Scale), i.uv.y + (_MainTex_TexelSize.y * _Scale));
                float2 uvBR = float2(i.uv.x + (_MainTex_TexelSize.x * _Scale), i.uv.y - (_MainTex_TexelSize.y * _Scale));
                float2 uvTL = float2(i.uv.x - (_MainTex_TexelSize.x * _Scale), i.uv.y + (_MainTex_TexelSize.y * _Scale));
                float2 uvBL = float2(i.uv.x - (_MainTex_TexelSize.x * _Scale), i.uv.y - (_MainTex_TexelSize.y * _Scale));
                
                float depth0 = tex2D(_CameraDepthTexture, uvBL).r;
                float depth1 = tex2D(_CameraDepthTexture, uvTR).r;
                float depth2 = tex2D(_CameraDepthTexture, uvBR).r;
                float depth3 = tex2D(_CameraDepthTexture, uvTL).r;
                
                float depthFiniteDifference0 = depth1 - depth0;
                float depthFiniteDifference1 = depth3 - depth2;
                
                float edgeDepth = (sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 50) > _DepthThreshold ? 1 : 0;
                //float4 returnColor = float4(_EdgeColorR, _EdgeColorG, _EdgeColorB, 1 * edgeDepth);
                float4 returnColor = float4(_EdgeColorR * edgeDepth,_EdgeColorG * edgeDepth, _EdgeColorB * edgeDepth, edgeDepth);

                return returnColor;
            }

            ENDCG
        }
    }
}