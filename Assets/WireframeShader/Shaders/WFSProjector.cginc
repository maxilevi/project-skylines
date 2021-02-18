// -------------------------------------------------------------------
//  Projector vertex and fragment code
// -------------------------------------------------------------------

#ifndef WFS_PROJECTOR_INCLUDED
#define WFS_PROJECTOR_INCLUDED

#include "UnityCG.cginc"
#include "WFSCG.cginc"

struct appdata{
	float4 vertex : POSITION;
	float2 uv0 : TEXCOORD0;
};

struct v2f {
	float4 pos : SV_POSITION;
	float4 projUV : TEXCOORD0;
	float2 tex : TEXCOORD1;
	float3 dist : TEXCOORD2;
	float3 posWorld : TEXCOORD3;
	UNITY_FOG_COORDS(4)
};

float4x4 unity_Projector;
float4 _WTex_ST;

v2f vert(appdata v){
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.projUV = mul(unity_Projector, v.vertex);
	o.tex = TRANSFORM_TEX(v.uv0, _WTex);
	o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;

	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

half4 frag(v2f i) : SV_Target {
	half alpha, metallic, smoothness; 
	float wire;
	half3 emission, albedo;
	WFSFragBase(i.dist, float4(i.tex,0,0), albedo, alpha, emission, metallic, smoothness, wire);

	half mask = tex2Dproj(_WMask, UNITY_PROJ_COORD(i.projUV)).g;
	half4 col = half4(albedo, alpha);
	#ifdef _ALPHABLEND_ON
		col.a *= mask;
	#else
		col *= mask;
	#endif


	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}

#include "WFSGeom.cginc"

#endif //WFS_PROJECTOR_INCLUDED