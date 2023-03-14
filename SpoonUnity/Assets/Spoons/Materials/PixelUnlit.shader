Shader "Unlit/PixelUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Colors ("Colors", 2D) = "white" {}
        _ScreenSize ("ScreenSize", Vector) = (1024, 1024, 0, 0)
        _Spread ("Spread", Float) = .2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _Colors;
            float4 _MainTex_ST;
            float4 _ScreenSize;
            float _Spread;

            static int bayer4[4*4]= {
                0,8,2,10,
                12,4,14,6,
                3,11,1,9,
                15,7,13,5
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                int n = 4;
                int x = (i.uv.x * _ScreenSize.x) % n;
                int y = (i.uv.y * _ScreenSize.y) % n;
                
                // return y;
                float m = (bayer4[(y)*4 + x] * 1 / pow(n, 2)) -.5;
                //
                // return m;
                fixed4 col = tex2D(_MainTex, i.uv);

                float4 c = col + m*_Spread;

                return c;
                // c = clamp(0, 1, c);
                float grey = 0.21 * c.r + 0.71 * c.g + 0.07 * c.b;
                int colorCount = 6;
                grey = floor(grey * (colorCount - 1) + .5) / (colorCount - 1);

                return tex2D(_Colors, float2(grey , 0));
                
                // return 1;
                return float4(grey, grey, grey, 1);
            }
            ENDCG
        }
    }
}
