Shader "Custom/CRTEffect" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 10)) = 1
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.5
    }
 
    SubShader {
        Tags { "RenderType"="Opaque" }
 
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
            float _DistortionStrength;
            float _ScanlineIntensity;
            float _VignetteIntensity;
 
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target {
                float4 color = tex2D(_MainTex, i.uv);
 
                // Add distortion effect
                float2 uvDistorted = i.uv + _DistortionStrength * sin(i.uv.y * 40 + _Time.y * 10);
                color = tex2D(_MainTex, uvDistorted);
 
                // Add scanlines effect
                float scanlineValue = sin(i.uv.y * 400) * _ScanlineIntensity + 1 - _ScanlineIntensity / 2;
                color.rgb *= scanlineValue;
 
                // Add vignette effect
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                color.rgb *= 1.0 - smoothstep(0.8, 1.2, dist * (1 + _VignetteIntensity * 2));
 
                return color;
            }
            ENDCG
        }
    }
}