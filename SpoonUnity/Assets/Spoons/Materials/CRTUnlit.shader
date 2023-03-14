
Shader "Unlit/CRTUnlit_Old"
{
    
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Curvature ("Curvature", Range(0, 10)) = .2
        _ScreenSize ("ScreenSize", Vector) = (1024, 1024, 30, 0)
        _VigSize ("VigSize", Float) = 300
        _ColorScans ("ColorScans", Vector) = (0, 0, 0, 0)
        
        _TargetScreenSize("TargetScreenSize", Vector) = (2048, 1800, 0, 0)
        
        _VertJerkOpt("Vert Jerk Opt", Float) = 1
        _VertMovementOpt("Vert Movement Opt", Float) = 1
        _BottomStaticOpt("Vert Static Opt", Float) = 1
        _ScanlinesOpt("Scanlines Opt", Float) = 1
        _RgbOffsetOpt("RGBOffsetOpt", Float) = 1
        _HorzFuzzOpt("HorzFuzzOpt", Float) = 1
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
            float _Curvature;
            float4 _ScreenSize;
            float4 _ColorScans;
            float4 _TargetScreenSize;
            float _VigSize;

            float _VertJerkOpt;
            float _VertMovementOpt;
            float _BottomStaticOpt;
            float _ScanlinesOpt;
            float _RgbOffsetOpt;
            float _HorzFuzzOpt;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            
            float2 mod(float2 n)
            {
                return (n.x - n.y * floor(n.x / n.y));
            }
            float mod(float x, float y)
            {
                return (x - y * floor(x / y));
            }
            
            float3 mod289(float3 x) {
              return x - floor(x * (1.0 / 289.0)) * 289.0;
            }

            float2 mod289(float2 x) {
              return x - floor(x * (1.0 / 289.0)) * 289.0;
            }

            float3 permute(float3 x) {
              return mod289(((x*34.0)+1.0)*x);
            }

            float snoise(float2 v)
              {
              const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                                  0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                                 -0.577350269189626,  // -1.0 + 2.0 * C.x
                                  0.024390243902439); // 1.0 / 41.0
            // First corner
              float2 i  = floor(v + dot(v, C.yy) );
              float2 x0 = v -   i + dot(i, C.xx);

            // Other corners
              float2 i1;
              //i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
              //i1.y = 1.0 - i1.x;
              i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
              // x0 = x0 - 0.0 + 0.0 * C.xx ;
              // x1 = x0 - i1 + 1.0 * C.xx ;
              // x2 = x0 - 1.0 + 2.0 * C.xx ;
              float4 x12 = x0.xyxy + C.xxzz;
              x12.xy -= i1;

            // Permutations
              i = mod289(i); // Avoid truncation effects in permutation
              float3 p = permute( permute( i.y + float3(0.0, i1.y, 1.0 ))
		            + i.x + float3(0.0, i1.x, 1.0 ));

              float3 m = max(0.5 - float3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
              m = m*m ;
              m = m*m ;

            // Gradients: 41 points uniformly over a line, mapped onto a diamond.
            // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

              float3 x = 2.0 * frac(p * C.www) - 1.0;
              float3 h = abs(x) - 0.5;
              float3 ox = floor(x + 0.5);
              float3 a0 = x - ox;

            // Normalise gradients implicitly by scaling m
            // Approximation of: m *= inversesqrt( a0*a0 + h*h );
              m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

            // Compute final noise value at P
              float3 g;
              g.x  = a0.x  * x0.x  + h.x  * x0.y;
              g.yz = a0.yz * x12.xz + h.yz * x12.yw;
              return 130.0 * dot(m, g);
            }

            float staticV(float2 uv, float time) {
                float iTime = time;
                float staticHeight = snoise(float2(9.0,iTime*1.2+3.0))*0.3+5.0;
                float staticAmount = snoise(float2(1.0,iTime*1.2-6.0))*0.1+0.3;
                float staticStrength = snoise(float2(-9.75,iTime*0.6-3.0))*2.0+2.0;
	            return (1.0-step(snoise(float2(5.0*pow(iTime,2.0)+pow(uv.x*7.0,1.2),pow((mod(iTime,100.0)+100.0)*uv.y*0.3+3.0,staticHeight))),staticAmount))*staticStrength;
            }
            fixed4 distort(float2 uv, float iTime)
            {
//https://www.shadertoy.com/view/ldXGW4
                float jerkOffset = (1.0-step(snoise(float2(iTime*1.3,5.0)),0.8))*0.05;

	            float fuzzOffset = snoise(float2(iTime*15.0,uv.y*80.0))*0.003;
	            float largeFuzzOffset = snoise(float2(iTime*1.0,uv.y*25.0))*0.004;
                
                float vertMovementOn = (1.0-step(snoise(float2(iTime*0.2,8.0)),0.4))*_VertMovementOpt;
                float vertJerk = (1.0-step(snoise(float2(iTime*1.5,5.0)),0.6))*_VertJerkOpt;
                float vertJerk2 = (1.0-step(snoise(float2(iTime*5.5,5.0)),0.2))*_VertJerkOpt;
                float yOffset = abs(sin(iTime)*4.0)*vertMovementOn+vertJerk*vertJerk2*0.3;
                float y = mod(uv.y+yOffset,1.0);
                
	            
	            float xOffset = (fuzzOffset + largeFuzzOffset) * _HorzFuzzOpt;
                
                float staticVal = 0.0;
               
                for (float y = -1.0; y <= 1.0; y += 1.0) {
                    float maxDist = 5.0/200.0;
                    float dist = y/200.0;
    	            staticVal += staticV(float2(uv.x,uv.y+dist), iTime)*(maxDist-abs(dist))*1.5;
                }
                    
                staticVal *= _BottomStaticOpt;
	            
	            float red 	=   tex2D(	_MainTex, 	float2(uv.x + xOffset -0.01*_RgbOffsetOpt,y)).r+staticVal;
	            float green = 	tex2D(	_MainTex, 	float2(uv.x + xOffset,	  y)).g+staticVal;
	            float blue 	=	tex2D(	_MainTex, 	float2(uv.x + xOffset +0.01*_RgbOffsetOpt,y)).b+staticVal;
	            
	            float3 color = float3(red,green,blue);
	            float scanline = sin(uv.y*800.0)*0.04*_ScanlinesOpt;
	            color -= scanline;
	            
	            return fixed4(color,1.0);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                if (i.uv.x < 0 || i.uv.y < 0 || i.uv.x > 1 || i.uv.y > 1)
                {
                    return float4(0,0,0,1);
                }

                float2 tuv = i.uv;

                float initialAspect = _ScreenSize.x / _ScreenSize.y;
                float finalAspect = _TargetScreenSize.x / _TargetScreenSize.y;
                
                float m = 0;
                float n = 0;

                if (initialAspect > finalAspect)
                {
                    // m = initialAspect / finalAspect;
                    n = (initialAspect / finalAspect) - 1;

                } else
                {
                    m = (finalAspect * initialAspect) - 1;
                }
                
                tuv.y *= 1 + 2 * m;
                tuv.x *= 1 + 2 * n;

                tuv.y -= m;
                tuv.x -= n;
                if (tuv.x < 0 || tuv.y < 0 || tuv.x > 1 || tuv.y > 1) return float4(0, 0, 0, 1);

                // return float4(tuv, 0, 0);
                
                float2 uv = tuv * 2 -1;
            
                
                float2 offset = uv / _Curvature;
                
                
                float2 curvedSpace = uv + uv * offset * offset;
                float2 mappedUv = curvedSpace * .5 + .5;

                
                float2 absUv = abs(mappedUv*2 - 1);
                float2 invertAbsUv = 1 - absUv;
                float2 v = float2(_VigSize / _ScreenSize.x, _VigSize / _ScreenSize.y);
                float2 vig = smoothstep(0, v, invertAbsUv);
                float vigMask = vig.x * vig.y;
                
                // fixed4 col = tex2D(_MainTex, mappedUv);

                fixed4 col = distort(mappedUv, _Time.x);
                // return mappedUv;
                float t = _Time.z * _ColorScans.w;
                float s = (sin(_ScreenSize.y * tuv * _ColorScans.z + t) + 1) * _ColorScans.x + 1;
                float c = (cos(_ScreenSize.y * tuv * _ColorScans.z + t) + 1) * _ColorScans.y + 1;

                // return col;
                col.g *= s;
                col.rb *= c;
                col = col * vigMask;

                // col.rg = mappedUv;
                // col.b = 0;
                return col;
                // return vig;
            }
            ENDCG
        }
    }
}
