Shader "Custom/KinectDepthColorMixer" {
Properties 
{
	_ColorTex ("Texture", 2D) = "white" {}
	_DepthTex ("Texture", 2D) = "white" {}
	_AmbientColor ("AmbientColor", Color) = (0,0,1,1)
	_DownsampleSize ("DownsampleSize", Float) = 8
	_DepthMin ("DepthMin", Float) = 0
	_DepthMax ("DepthMax", Float) = 0
	_DepthMinRamp ("DepthMinRamp", Float) = 0
	_DepthMaxRamp ("DepthMaxRamp", Float) = 0
	_Alpha ("Alpha", Float) = 0
	_AmbientColorRate ("AmbientColorRate", Float) = 1.0
}
SubShader {
Pass {


CGPROGRAM
#pragma target 5.0
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "Extensions.cginc"

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;       
};

struct v2f
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
};

sampler2D _ColorTex;
sampler2D _DepthTex;
float4 _ColorTex_ST;
float4 _DepthTex_ST;

fixed4 _AmbientColor;
float _DownsampleSize;
float _DepthMin;
float _DepthMax;
float _DepthMinRamp;
float _DepthMaxRamp;
float _Alpha;
float _AmbientColorRate;

StructuredBuffer<float2> DepthCoords;

v2f vert (appdata v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _DepthTex);
	o.uv.y = inv(o.uv.y);
	return o;
}

float map(float value, float min1, float max1, float min2, float max2)
{
	float pct = (value - min1) / (max1 - min1); // convert value to pct
	return pct * (max2 - min2) + min2; // map to max
}

fixed4 frag (v2f i, in uint id : SV_InstanceID) : COLOR
{
	float time = _Time.y;
	float2 uv = i.uv.xy;
	float2 uv_ex = i.uv.xy;
	fixed4 col;

	float x = uv.x * 25.0 + time;
	float y = uv.y * 25.0 + time;
	uv.y += cos(x+y) * 0.0090 * cos(y);
	uv.x += sin(x-y) * 0.0090 * sin(y);
	uv_ex.y += cos(x+y) * 0.025 * cos(y);
	uv_ex.x += sin(x-y) * 0.025 * sin(y);

	int col_w = (int)(uv.x * ((float)512/_DownsampleSize));
	int col_h = (int)(uv.y * ((float)424/_DownsampleSize));
	int c_idx = (int)(col_h * ((float)512/_DownsampleSize) + col_w);

	fixed depth = tex2D(_DepthTex, i.uv).r;
	fixed4 texcol = tex2D(_ColorTex, DepthCoords[c_idx]);
	//fixed gray = clamp(avg3(texcol),0,1.0);

	float map_min = smoothstep(0, 1.0, clamp(map(depth, _DepthMin, _DepthMin+_DepthMinRamp, 0, 1.0), 0, 1.0));
	float map_max = smoothstep(0, 1.0, clamp(map(depth, _DepthMax-_DepthMaxRamp, _DepthMax, 1.0, 0), 0, 1.0));
	float pct = max(map_min, map_max);

	float lb_min = smoothstep(0, 1.0, clamp(map(uv_ex.x, 0, 0.2, 0, 1.0), 0, 1.0));
	float lb_max = smoothstep(0, 1.0, clamp(map(uv_ex.x, 0.8, 1.0, 1.0, 0), 0, 1.0));

	col.rgb = lerp(texcol.rgb, texcol.rgb * _AmbientColor.rgb, _AmbientColorRate);
	col.a = inv(smoothstep(0, 1.0, pct)) * min(lb_min,lb_max) * _Alpha;

	return col;
}
ENDCG


}
}
}
