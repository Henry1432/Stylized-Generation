Shader "Hidden/FilterV4"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        CGINCLUDE
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
        

        #define PI 3.14159265358979323f
        
        sampler2D _MainTex, _TFM;
        float4 _MainTex_TexelSize;
        int _KernelSize, _N, _Size;
        float _Hardness, _Q, _Alpha, _ZeroCrossing, _Softness;

        float _Scale;
        float _DepthThreshold;
        float _EdgeColorR; 
        float _EdgeColorG;
        float _EdgeColorB;

        sampler2D _CameraDepthTexture;
        
        float blurFunc(float sigma, float pos) {
            return (1.0f / sqrt(2.0f * PI * sigma * sigma)) * exp(-(pos * pos) / (2.0f * sigma * sigma));
        }

        int grabOutline(v2f i)
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
                
                int edgeDepth = (sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 50) > _DepthThreshold ? 1 : 0;

                return edgeDepth;
        }

        ENDCG

        //saving avarage pixel information
        Pass {

            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            float4 frag(v2f i) : SV_Target {
                float2 pixelPos = _MainTex_TexelSize.xy;
                
                //I coppied this part more directly from the git https://github.com/GarrettGunnell/Post-Processing/blob/main/Assets/Kuwahara%20Filter/AnisotropicKuwahara.shader
                float3 avaragePixelx = (
                    1.0f * tex2D(_MainTex, i.uv + float2(-pixelPos.x, -pixelPos.y)).rgb +
                    2.0f * tex2D(_MainTex, i.uv + float2(-pixelPos.x,  0.0)).rgb +
                    1.0f * tex2D(_MainTex, i.uv + float2(-pixelPos.x,  pixelPos.y)).rgb +
                    -1.0f * tex2D(_MainTex, i.uv + float2(pixelPos.x, -pixelPos.y)).rgb +
                    -2.0f * tex2D(_MainTex, i.uv + float2(pixelPos.x,  0.0)).rgb +
                    -1.0f * tex2D(_MainTex, i.uv + float2(pixelPos.x,  pixelPos.y)).rgb
                ) / 4.0f;

                float3 avaragePixely = (
                    1.0f * tex2D(_MainTex, i.uv + float2(-pixelPos.x, -pixelPos.y)).rgb +
                    2.0f * tex2D(_MainTex, i.uv + float2( 0.0, -pixelPos.y)).rgb +
                    1.0f * tex2D(_MainTex, i.uv + float2( pixelPos.x, -pixelPos.y)).rgb +
                    -1.0f * tex2D(_MainTex, i.uv + float2(-pixelPos.x, pixelPos.y)).rgb +
                    -2.0f * tex2D(_MainTex, i.uv + float2( 0.0, pixelPos.y)).rgb +
                    -1.0f * tex2D(_MainTex, i.uv + float2( pixelPos.x, pixelPos.y)).rgb
                ) / 4.0f;

                
                return float4(dot(avaragePixelx, avaragePixelx), dot(avaragePixely, avaragePixely), dot(avaragePixelx, avaragePixely), 1.0f);
            }
            ENDCG
        }

        //gaussian blur little
        Pass {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            float4 frag(v2f i) : SV_Target {
                int r = 5;

                float4 col = 0;
                float gaussTotal = 0.0f;

                for (int x = -r; x <= r; ++x) {
                    float4 blurPos = tex2D(_MainTex, i.uv + float2(x, 0) * _MainTex_TexelSize.xy);
                    float gauss = blurFunc(2.0f, x);

                    col += blurPos * gauss;
                    gaussTotal += gauss;
                }

                return col / gaussTotal;
            }
            ENDCG
        }

        //gaussian blur big
        Pass {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            float4 frag(v2f i) : SV_Target {
                int r = 5;

                float4 col = 0;
                float gaussTotal = 0.0f;

                for (int y = -r; y <= r; ++y) {
                    float4 blurPos = tex2D(_MainTex, i.uv + float2(0, y) * _MainTex_TexelSize.xy);
                    float gauss = blurFunc(2.0f, y);

                    col += blurPos * gauss;
                    gaussTotal += gauss;
                }

                float3 g = col.rgb / gaussTotal;
                //same as above little to this point

                //I coppied this part more directly from the git https://github.com/GarrettGunnell/Post-Processing/blob/main/Assets/Kuwahara%20Filter/AnisotropicKuwahara.shader
                float lambda1 = 0.5f * (g.y + g.x + sqrt(g.y * g.y - 2.0f * g.x * g.y + g.x * g.x + 4.0f * g.z * g.z));
                float lambda2 = 0.5f * (g.y + g.x - sqrt(g.y * g.y - 2.0f * g.x * g.y + g.x * g.x + 4.0f * g.z * g.z));

                float2 v = float2(lambda1 - g.x, -g.z);
                float2 t = length(v) > 0.0 ? normalize(v) : float2(0.0f, 1.0f);
                float phi = -atan2(t.y, t.x);

                float A = (lambda1 + lambda2 > 0.0f) ? (lambda1 - lambda2) / (lambda1 + lambda2) : 0.0f;
                
                return float4(t, phi, A);
            }
            ENDCG
        }

        //apply real Kuwahara
        Pass{
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            float4 frag(v2f i) : SV_Target
            {
                float alpha = _Alpha;
                //scaled vector texture information
                float4 t = tex2D(_TFM, i.uv);

                int kernelRadius = _KernelSize / 2;
                //scaling kernel radius based on pixel info and alpha
                float a = float((kernelRadius)) * clamp((alpha + t.w) / alpha, 0.1f, 2.0f);
                float b = float((kernelRadius)) * clamp(alpha / (alpha + t.w), 0.1f, 2.0f);
                
                //checking if scaled pixel information is valid using matrix math, SR is multiplied with pixel position in big loop, and if its magnitude is greator then 0.25 it is a valid point, else its not and nothing needs to be done
                float cos_phi = cos(t.z);
                float sin_phi = sin(t.z);
                
                float2x2 R = {cos_phi, -sin_phi,
                              sin_phi, cos_phi};

                float2x2 S = {0.5f / a, 0.0f,
                              0.0f, 0.5f / b};

                float2x2 SR = mul(S, R);

                int max_x = int(sqrt(a * a * cos_phi * cos_phi + b * b * sin_phi * sin_phi));
                int max_y = int(sqrt(a * a * sin_phi * sin_phi + b * b * cos_phi * cos_phi));

                float softness = _Softness;

                float zeroCross = _ZeroCrossing;
                float sinZeroCross = sin(zeroCross);
                float eta = (softness + cos(zeroCross)) / (sinZeroCross * sinZeroCross);
                int k;
                float4 m[8];
                float3 s[8];

                for (k = 0; k < _N; ++k) {
                    m[k] = 0.0f;
                    s[k] = 0.0f;
                }


                [loop]
                for (int y = -max_y; y <= max_y; ++y) {
                    [loop]
                    for (int x = -max_x; x <= max_x; ++x) {
                        //get scaled pixel pos informaion
                        float2 v = mul(SR, float2(x, y));
                        if (dot(v, v) <= 0.25f) {
                            //get pix color;
                            float3 c = tex2D(_MainTex, i.uv + float2(x, y) * _MainTex_TexelSize.xy).rgb;
                            c = saturate(c);
                            float sum = 0;
                            float w[8];
                            float z, vxx, vyy;
                            
                            /* Calculate Polynomial Weights */ //poly weights math directly coppied from git
                            vxx = softness - eta * v.x * v.x;
                            vyy = softness - eta * v.y * v.y;
                            //get either 0 or pixel pos for weight, whichever is heighter
                            z = max(0, v.y + vxx); 
                            //modify weight and total weight
                            w[0] = z * z;
                            sum += w[0];
                            //do this for all pixel position combinations
                            z = max(0, -v.x + vyy);
                            w[2] = z * z;
                            sum += w[2];
                            z = max(0, -v.y + vxx); 
                            w[4] = z * z;
                            sum += w[4];
                            z = max(0, v.x + vyy); 
                            w[6] = z * z;
                            sum += w[6];
                            v = sqrt(2.0f) / 2.0f * float2(v.x - v.y, v.x + v.y);
                            vxx = softness - eta * v.x * v.x;
                            vyy = softness - eta * v.y * v.y;
                            z = max(0, v.y + vxx); 
                            w[1] = z * z;
                            sum += w[1];
                            z = max(0, -v.x + vyy); 
                            w[3] = z * z;
                            sum += w[3];
                            z = max(0, -v.y + vxx); 
                            w[5] = z * z;
                            sum += w[5];
                            z = max(0, v.x + vyy); 
                            w[7] = z * z;
                            sum += w[7];
                            
                            float g = exp(-3.125f * dot(v,v)) / sum;
                            
                            for (int k = 0; k < 8; ++k) {
                                float wk = w[k] * g;
                                //get total weighted color information
                                m[k] += float4(c * wk, wk);
                                s[k] += c * c * wk;
                            }
                        }
                    }
                }

                //return the weighted color Info
                float4 output = float4(0,0,0,0);
                for (int k = 0; k < _N; ++k) {
                    m[k].rgb /= m[k].w;
                    s[k] = abs(s[k] / m[k].w - m[k].rgb * m[k].rgb);

                    float sigma2 = s[k].r + s[k].g + s[k].b;
                    float w = 1.0f / (1.0f + pow(_Hardness * 1000.0f * sigma2, 0.5f * _Q));

                    output += float4(m[k].rgb * w, w);
                }
                
                float edgeDepth = grabOutline(i);

                if(edgeDepth == 1)
                {
                    return float4(_EdgeColorR * edgeDepth,_EdgeColorG * edgeDepth, _EdgeColorB * edgeDepth, edgeDepth);
                }
                else
                {
                    return saturate(output / output.w);
                }
                //return float4(1,1,1,1);
            }
                ENDCG
        }
    }
}