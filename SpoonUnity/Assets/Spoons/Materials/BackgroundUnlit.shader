Shader "Unlit/BackgroundUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TopSkyTex ("TopSkyTex", 2D) = "white" {}
        _LowSkyTex ("LowSkyTex", 2D) = "white" {}
        _X ("X", Float) = 0
        
        _GameTime ("Game Time", Range(0, 1)) = .5
        
        _HouseLine ("HouseLine", Float) = 0
        _Sizes ("Sizes", Vector) = (1,1,1,1)
        _XSpeed ("XSpeed", Float) = 0
        _BackLayerOffset ("BackLayerOffset", Float) = 0
        
        _Sky("Sky color", Color) = (.25, .5, .5, 1)
        _SkyLow("SkyLow color", Color) = (.25, .5, .5, 1)
        _HouseLayer1("HouseLayer1", Color) = (.25, .5, .5, 1)
        _HouseLayer2("HouseLayer2", Color) = (.25, .5, .5, 1)
        _HouseLayer3("HouseLayer3", Color) = (.25, .5, .5, 1)
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
            sampler2D _TopSkyTex;
            sampler2D _LowSkyTex;
            float4 _MainTex_ST;
            float4 _Sky;
            float4 _SkyLow;
            float4 _Sizes;
            float _X;
            float _XSpeed;
            float _HouseLine;
            float _BackLayerOffset;


            float _GameTime;
            
            float3 _HouseLayer1;
            float3 _HouseLayer2;
            float3 _HouseLayer3;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // float random(float x) {
            //    return frac(sin(dot(x,float2(12.988,78.233)))*43758.54523);
            // }
            float random (float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
            }
            
            float houseLayer(float2 uv, float groundLevel, float2 offset)
            {
               
                float buildingHeight = .3;
                float buildingTop = groundLevel + buildingHeight;
                // float g = uv.y < groundLevel;
                float b = uv.y < buildingTop && uv.y > groundLevel;
                // return b;
                float buildingY = (uv.y - groundLevel) / buildingHeight;
                float buildingScale = 10;
                float2 buildingUv = float2(frac(uv.x * buildingScale), buildingY);
                float2 buildingGv = float2(floor(uv.x * buildingScale), 0);

                buildingUv = buildingUv * 2 - 1;
                float r = random(buildingGv);
                // return r * .2;
                buildingUv.x += (r * 2 - 1) * .2;
                
                // buildingUv.x *= scale.x;
                buildingUv.y += offset.y;
                float roof = smoothstep(.6, .55, buildingUv.y + abs(buildingUv.x));
                
                roof *= buildingUv.y > -.2;
                float building = abs(buildingUv.x ) < (.5+offset.x) && buildingUv.y < 0;
                float house = max(building,roof);

                return house * b;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                
                fixed4 backgroundColor = tex2D(_MainTex, i.uv);
                // return col2;
                
                float4 col = 1;
                i.uv.x *= (_ScreenParams.x / _ScreenParams.y) * .43;
                _X *= _XSpeed;
                i.uv.x += _X;

                float groundLevel = _HouseLine;
                float house = houseLayer(float2(i.uv.x - _X*.98, i.uv.y ), groundLevel, 0);
                float house2 = houseLayer(float2(i.uv.x - _X*.99 + 5.05, i.uv.y), groundLevel, float2(.1, .3));
                float house3 = houseLayer(float2(i.uv.x - _X + 12.03, i.uv.y), groundLevel, float2(.2, .35));
                
                col.rgb = 0;

                float skyY = (i.uv.y - groundLevel) / (1 - groundLevel);

                float2 skyUv = float2(_GameTime, 0);
                float4 skyLow = tex2D(_LowSkyTex, skyUv);
                float4 skyHigh = tex2D(_TopSkyTex, skyUv);
                
                col.rgb = lerp(skyLow, skyHigh, skyY).rgb;
                // col.rgb = 1;
                col.rgb = (1-house3) * col.rgb +  _HouseLayer3 * house3;
                col.rgb = (1-house2) * col.rgb +  _HouseLayer2 * house2;
                col.rgb = (1-house) * col.rgb +  _HouseLayer1 * house;

                // col.rgb = skyY;
                col.rgb = (1-backgroundColor.a) * col.rgb + backgroundColor.a * backgroundColor.rgb;


                
                return col;
            }
            ENDCG
        }
    }
}
