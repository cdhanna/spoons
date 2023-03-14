Shader "Unlit/BackgroundUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _X ("X", Float) = 0
        _XSpeed ("XSpeed", Float) = 0
        _BackLayerOffset ("BackLayerOffset", Float) = 0
        _Sidewalk("Sidwalk color", Color) = (.25, .5, .5, 1)
        _Grass("Grass color", Color) = (.25, .5, .5, 1)
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
            float4 _MainTex_ST;
            float4 _Sidewalk;
            float4 _Sky;
            float4 _SkyLow;
            float4 _Grass;
            float _X;
            float _XSpeed;
            float _BackLayerOffset;
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

            float random(float2 x) {
               return frac(sin(dot(x,float2(12.9898,78.233)))*43758.5453123);
            }
            
            float houseLayer(float2 uv, float groundLevel, float2 offset)
            {
                float buildingHeight = .3;
                float buildingTop = groundLevel + buildingHeight;
                // float g = uv.y < groundLevel;
                float b = uv.y < buildingTop && uv.y > groundLevel;
                float buildingY = (uv.y - groundLevel) / buildingHeight;
                float buildingScale = 10;
                float2 buildingUv = float2(frac(uv.x * buildingScale), buildingY) *2 - 1;
                float2 buildingGv = (float2(uv.x * buildingScale, buildingY) * 2 - 1) - buildingUv;
                float r = random(buildingGv);
                buildingUv.x += r * .3;
                // buildingUv.x *= scale.x;
                buildingUv.y += offset.y;
                float roof = smoothstep(.6, .5, buildingUv.y + abs(buildingUv.x));
                roof *= buildingUv.y > -.2;
                float building = abs(buildingUv.x ) < (.5+offset.x) && buildingUv.y < 0;
                float house = max(building,roof);

                return house * b;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                float4 col = 1;
                _X *= _XSpeed;
                i.uv.x += _X;

                float groundLevel = .2;
                // float buildingHeight = .3;
                // float buildingTop = groundLevel + buildingHeight;
                float g = i.uv.y < groundLevel;
                // float b = i.uv.y < buildingTop && i.uv.y > groundLevel;
                float house = houseLayer(float2(i.uv.x - _X*.98, i.uv.y ), groundLevel, 0);
                float house2 = houseLayer(float2(i.uv.x - _X*.99 + 5.05, i.uv.y), groundLevel, float2(.1, .3));
                float house3 = houseLayer(float2(i.uv.x - _X + 12.03, i.uv.y), groundLevel, float2(.2, .35));
                // float buildingY = (i.uv.y - groundLevel) / buildingHeight;
                // float buildingScale = 10;
                // float2 buildingUv = float2(frac(i.uv.x * buildingScale), buildingY) *2 - 1;
                //
                // float roof = smoothstep(.6, .5, buildingUv.y + abs(buildingUv.x));
                // roof *= buildingUv.y > -.2;
                // float building = abs(buildingUv.x ) < .5 && buildingUv.y < 0;
                // float house = building + roof;

                
                float s = 1 - g;
                float sideWalkGradient = 1 - i.uv.y / groundLevel;

                float sideWalkScale = 5;
                // i.uv.x += sideWalkGradient * .1;

                float dist = i.uv.x - .5 - _X;
                // if (sideWalkGradient < .9)
                {
                    i.uv.x += dist /(clamp(0, .6,sideWalkGradient)*1);
                }
                
                float2 sideWalkUv = g * float2(frac(i.uv.x * sideWalkScale), 1 - sideWalkGradient);
                float2 sideWalkGv = float2(i.uv.x * sideWalkScale, 1 - sideWalkGradient) - sideWalkUv;
                
                float3 sidewalk = _Sidewalk;

                if (sideWalkUv.y < .6 && sideWalkUv.y > .2)
                sidewalk -= (sideWalkUv.x > .97) * .3;

                if (sideWalkUv.y > .6)
                {
                    sidewalk = _Grass;
                }
                if (sideWalkUv.y < .2)
                {
                    sidewalk = 1 - (sideWalkUv.y / .2) ;
                    sidewalk *= float3(.5, .5, .6);
                }
                sidewalk *= .2 + (1 - sideWalkUv.y);
                
                col.rgb = g * sidewalk;

                float skyY = (i.uv.y - groundLevel) / (1 - groundLevel);
                col.rgb += s * lerp(_SkyLow, _Sky, skyY);
                col.rgb = (1-house3) * col.rgb +  _HouseLayer3 * house3;
                col.rgb = (1-house2) * col.rgb +  _HouseLayer2 * house2;
                col.rgb = (1-house) * col.rgb +  _HouseLayer1 * house;
                // col.rgb = s * skyY;
                
                return col;
            }
            ENDCG
        }
    }
}
