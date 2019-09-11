Shader "Custom/Blur13" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_IntensityMult ("Intensity Multiplication", Float) = 1.0
		_ShadowColor("_ShadowColor", Color) = (0,0,0,1)
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Extensions.cginc"
	#pragma vertex VERT
	#pragma fragment FRAG

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	struct appdata
	{
		float4 vertex : POSITION;
		half2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		half2 r0 : TEXCOORD1;
		half2 r1 : TEXCOORD2;
		half2 r2 : TEXCOORD3;
		half2 r3 : TEXCOORD4;
		half2 r4 : TEXCOORD5;
		half2 r5 : TEXCOORD6;
	};
	ENDCG

	SubShader 
	{
		Cull Off 
		ZWrite Off 
		ZTest Always

		Pass 
		{
			Name "Horizontal"

			CGPROGRAM
			v2f VERT (appdata IN)
			{
				v2f OUT;

				OUT.pos = UnityObjectToClipPos(IN.vertex);

				float2 off1 = float2(1.411764705882353, 0.0);
				float2 off2 = float2(3.2941176470588234, 0.0);
				float2 off3 = float2(5.176470588235294, 0.0);

				OUT.uv = IN.uv;
				OUT.r0 = IN.uv + off1 / res;
				OUT.r1 = IN.uv - off1 / res;
				OUT.r2 = IN.uv + off2 / res;
				OUT.r3 = IN.uv - off2 / res;
				OUT.r4 = IN.uv + off3 / res;
				OUT.r5 = IN.uv - off3 / res;

				return OUT;
			}

			fixed4 FRAG (v2f i) : COLOR
			{
				fixed4 intensity = tex2D(_MainTex, i.uv) * 0.1964825501511404;
				intensity += tex2D(_MainTex, i.r0) * 0.2969069646728344;
				intensity += tex2D(_MainTex, i.r1) * 0.2969069646728344;
				intensity += tex2D(_MainTex, i.r2) * 0.09447039785044732;
				intensity += tex2D(_MainTex, i.r3) * 0.09447039785044732;
				intensity += tex2D(_MainTex, i.r4) * 0.010381362401148057;
				intensity += tex2D(_MainTex, i.r5) * 0.010381362401148057;

				return intensity;
			}
			ENDCG
		}

		GrabPass 
		{
			"_GrabTex"
		}

		Pass 
		{
			Name "Vertical"

			CGPROGRAM
			sampler2D _GrabTex;
			fixed4 _ShadowColor;
			float _IntensityMult;

			v2f VERT (appdata IN)
			{
				v2f OUT;

				OUT.pos = UnityObjectToClipPos(IN.vertex);

				float2 off1 = float2(1.411764705882353, 0.0);
				float2 off2 = float2(3.2941176470588234, 0.0);
				float2 off3 = float2(5.176470588235294, 0.0);

				OUT.uv = IN.uv;
				OUT.r0 = IN.uv + flip(off1) / res;
				OUT.r1 = IN.uv - flip(off1) / res;
				OUT.r2 = IN.uv + flip(off2) / res;
				OUT.r3 = IN.uv - flip(off2) / res;
				OUT.r4 = IN.uv + flip(off3) / res;
				OUT.r5 = IN.uv - flip(off3) / res;
				return OUT;
			}

			fixed4 FRAG (v2f i) : COLOR
			{
				fixed4 intensity = tex2D(_GrabTex, i.uv) * 0.1964825501511404;
				intensity += tex2D(_GrabTex, i.r0) * 0.2969069646728344;
				intensity += tex2D(_GrabTex, i.r1) * 0.2969069646728344;
				intensity += tex2D(_GrabTex, i.r2) * 0.09447039785044732;
				intensity += tex2D(_GrabTex, i.r3) * 0.09447039785044732;
				intensity += tex2D(_GrabTex, i.r4) * 0.010381362401148057;
				intensity += tex2D(_GrabTex, i.r5) * 0.010381362401148057;
				intensity = clamp(intensity * _IntensityMult, 0.0, 1.0);

				fixed result = inv(intensity);
				return fixed4(_ShadowColor.rgb, result);
			}
			ENDCG
		}
	} 
}
