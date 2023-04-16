Shader "Unlit/PaintBrushUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brush ("Brush", Vector) = (0, 0, 0, 0)
        _BrushOld ("Brush Old", Vector) = (0,0,0,0)
        _Canvas("Canvas", Vector) = (0, 0, 0, 0)
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
            float4 _MainTex_ST;
            float4 _Brush;
            float4 _BrushOld;
            float4 _Canvas;

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
                // sample the texture
                             

                fixed4 col = tex2D(_MainTex, i.uv);

                   float aspect = _Canvas.x / _Canvas.y;


                float brushSize = .03;
                float2 brushDiff = _Brush.xy - _BrushOld.xy;

                float brushDist = length(brushDiff);
                int steps = ceil(brushDist / (brushSize * .2));
                i.uv.x *= aspect;
                for (int b = 0 ; b < steps; b ++)
                {
                    float r = (float)(b+1) / steps;
                    float2 brushUv = _Brush.xy - brushDiff * r;
                    brushUv.x *= aspect;

                    float d = length(i.uv - brushUv);
                    float m = .8 * smoothstep(brushSize, 0, d);
                    col.rgb += m;
                }
                
                // need to interpolate from old position to current position 

                
                col.a = 1;
                
                return col;
            }
            ENDCG
        }
    }
}
