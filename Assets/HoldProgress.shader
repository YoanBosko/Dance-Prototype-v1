// ClipShader.shader
Shader "Custom/HoldProgress" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0,1)) = 1
        _FillOrigin ("Fill Origin", Float) = 0
    }
    
    SubShader {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Progress;
            float _FillOrigin;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Clip berdasarkan progress
                if ((_FillOrigin == 0 && i.uv.y > _Progress) || 
                    (_FillOrigin == 1 && i.uv.y < (1 - _Progress))) {
                    discard;
                }
                return col;
            }
            ENDCG
        }
    }
}