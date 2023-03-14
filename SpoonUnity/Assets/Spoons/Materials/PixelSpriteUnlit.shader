Shader "Unlit/PixelSpriteUnlit"
{
    Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 screenpos : TEXCOORD1;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				OUT.screenpos = ComputeScreenPos(OUT.vertex);
				#ifdef PIXELSNAP_ON
												OUT.vertex = UnityPixelSnap (OUT.vertex);

				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{

				float2 uv = IN.texcoord;
				int scale = 2;
				uv *= 2;
				

				uv = frac(uv);
				float2 gv = IN.texcoord - uv;
				// uv = gv * .5;
				
				fixed4 c = SampleSpriteTexture (uv) * IN.color;
				// c.rgb *= c.a;
				// float4 c = 1;

				// IN.screenpos.xy / _ScreenParams.xy;
				// float aspect = _ScreenParams.x / _ScreenParams.y;
				// float2 uv = IN.screenpos.xy;
				// uv.x *= aspect;
				// float2 sv = uv;
				// int scale = 12;
				// sv *= scale;
				// sv = frac(sv);
				// float2 sg = (uv * scale) - sv;

				// map back the screen coordinate to the texture

				// c.rg = sg / (scale + 1);
				// c.b = 0;
				if (IN.texcoord.x > .8 || IN.texcoord.x < .2)
				{
					return float4(1,0,0,1);
				}
				//
				// c.a = 1;
				// c.x = IN.vertex.x * .0006;
				return c;
			}
		ENDCG
		}
	}
}
