Shader "Custom/UnlitTransparent" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "black" {}
		_Alpha ("Alpha", Float) = 1.0
		_Tint ("TintColor", Color) = (0,0,0,0)
		_TintPct ("TintPct", Float) = 0
	}

	SubShader 
	{

		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass 
		{  
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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
			uniform fixed4 _Tint;
			uniform fixed _TintPct;
			 
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
				col.rgb = lerp(col, _Tint, _TintPct/4);
				return col;
			}
			ENDCG
		}
	}
}
