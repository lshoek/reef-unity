/**
	author: lesley van hoek
	handy shader extensions
**/

#define res half2(_MainTex_TexelSize.z, _MainTex_TexelSize.w)

#define xt _MainTex_TexelSize.x
#define yt _MainTex_TexelSize.y
#define wt _MainTex_TexelSize.z
#define ht _MainTex_TexelSize.w

#define xt_gt _GrabTexture_TexelSize.x
#define yt_gt _GrabTexture_TexelSize.y

#define inv(i) (1.0 - i)
#define invmult(i) (1.0 / (i))
#define flip(i) float2(i.y, i.x)

#define avg3(i) (i.r + i.g + i.b / 3.0)
#define avg4(i) (i.r + i.g + i.b + i.a / 4.0)
#define fxd3(i) fixed3(i, i, i)
#define fxd4(i) fixed4(i, i, i, i)

#define col3d(i, a) fixed4(i.r, i.g, i.b, a)
#define col1d(i, a) fixed4(i, i, i, a) 

#define PI 3.14159265359
#define TWO_PI 6.28318530718