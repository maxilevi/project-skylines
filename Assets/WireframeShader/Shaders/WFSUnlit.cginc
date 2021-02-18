// -------------------------------------------------------------------
//  Unlit pass.
// -------------------------------------------------------------------

#ifndef WFS_UNLIT_INCLUDED
#define WFS_UNLIT_INCLUDED


// WFS_TWOSIDED is a Cutout option
#if defined(WFS_TWOSIDED) && !defined(WFS_PASS_META)
	#define _ALPHATEST_ON
#endif


#include "UnityCG.cginc"
#include "WFSCG.cginc"

struct appdata_t {
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
	float4 pos : SV_POSITION;
	half2 texcoord : TEXCOORD0;
	float3 dist : TEXCOORD1;
	float3 posWorld : TEXCOORD2;
	UNITY_FOG_COORDS(3)
	UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert (appdata_t v){
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_OUTPUT(v2f, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
	o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;

	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}
			
half4 frag (v2f i) : SV_Target{
	fixed4 col = tex2D(_MainTex, i.texcoord)*_Color;

	fixed4 surfCol = col;
	half mask;
	float fade, wire;
	float3 thickness;
	WFSWire(i.dist, i.texcoord, mask, thickness, wire);

	#ifdef _FADE_ON
		WFSFade(length(i.posWorld - _WorldSpaceCameraPos), wire, fade);
	#endif

	#ifdef _GLOW_ON
		half3 emission = half3(0,0,0);
		WFSGlow(i.dist, thickness, mask, fade, col, emission);
	#endif

	half4 wireCol = WFSWireColor(i.dist, thickness, i.texcoord);
					
	// BLEND
	wireCol.rgb = lerp(surfCol.rgb, wireCol.rgb, wireCol.a); // alpha blend wire to surface
	half transparency = lerp(surfCol.a*_WTransparency, _WTransparency, wireCol.a);

	// WIRELERP
	col.rgb = lerp(wireCol.rgb, col.rgb, wire);
	col.a = lerp(transparency, col.a, wire);

	#if defined(_ALPHATEST_ON)
		clip(col.a - _Cutoff);
	#endif 

	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}

#include "WFSGeom.cginc"

#endif // WFS_UNLIT_INCLUDED