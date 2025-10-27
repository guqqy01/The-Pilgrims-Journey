Shader "Hidden/SeparableGaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TexelSize ("Texel Size", Vector) = (1,1,0,0)
        _Direction ("Direction", Vector) = (1,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Direction;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texel = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
                float2 dir = normalize(_Direction.xy);
                fixed4 sum = fixed4(0,0,0,0);

                // 9-tap Gaussian weights (center + 4 pairs)
                float weights[5];
                weights[0] = 0.2270270270; // center
                weights[1] = 0.1945945946;
                weights[2] = 0.1216216216;
                weights[3] = 0.0540540541;
                weights[4] = 0.0162162162;

                sum += tex2D(_MainTex, i.uv) * weights[0];
                for (int t = 1; t < 5; t++)
                {
                    float2 off = dir * texel * t;
                    sum += tex2D(_MainTex, i.uv + off) * weights[t];
                    sum += tex2D(_MainTex, i.uv - off) * weights[t];
                }

                return sum;
            }
            ENDCG
        }
    }
    FallBack Off
}
