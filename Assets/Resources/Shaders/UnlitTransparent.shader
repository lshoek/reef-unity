Shader "Custom/UnlitTransparent" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "black" {}
		_Alpha ("Alpha", Float) = 1.0
		_AtmosCol ("AtmosphereCol", Color) = (1,1,1,1)
		_AtmosPct ("AtmosPct", Float) = 0
		_Tint ("TintColor", Color) = (1,1,1,1)
		_TintPct ("TintPct", Float) = 0
	}

	SubShader 
	{

		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass 
		{  
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Extensions.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform fixed _Alpha;
			uniform fixed _TintPct;
			uniform fixed _AtmosPct;
			uniform fixed4 _Tint;
			uniform fixed4 _AtmosCol;
			 
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _Alpha;
				fixed4 atmos = avg3(col);

				atmos.rgb *= _AtmosCol;
				col.rgb = lerp(col.rgb, atmos.rgb, _AtmosPct);
				col.rgb = lerp(col, _Tint, _TintPct/4);
				return col;
			}
			ENDCG
		}
	}
}
