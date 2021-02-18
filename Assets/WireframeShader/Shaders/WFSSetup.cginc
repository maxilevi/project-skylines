#ifndef WFS_SETUP_INCLUDED
#define WFS_SETUP_INCLUDED


// WFS_TWOSIDED is a Cutout option
#if defined(WFS_TWOSIDED) && !defined(WFS_PASS_META)
	#define _ALPHATEST_ON
#endif


// don't do emission in foward add pass
#if defined(WFS_PASS_FORWARDADD) && defined(_EMISSION)
	#undef _EMISSION
#endif


// WFS vertex output data
#define WFS_OUTPUT(idx)  float3 dist : TEXCOORD##idx;
#define WFS_OUTPUT_WPOS(idx0, idx1)  float3 dist : TEXCOORD##idx0; float3 posWorld : TEXCOORD##idx1;
#define WFS_OUTPUT_SHADOW(idx0, idx1)  float3 dist : TEXCOORD##idx0; float3 posWorld : TEXCOORD##idx1; float4 _pos : SV_POSITION;


// Fragment shader data
#ifdef _EMISSION
	#define WFS_FRAGDATA_EMISSION half3 emission;
#else
	#define WFS_FRAGDATA_EMISSION 
#endif
#if defined(_WLIGHT_UNLIT) || defined(_WLIGHT_OVERLAY)
	#define WFS_FRAGDATA float wire; half3 albedo; WFS_FRAGDATA_EMISSION
#else
	#define WFS_FRAGDATA WFS_FRAGDATA_EMISSION
#endif






#if defined(WFS_PASS_FORWARDBASE) || defined(WFS_PASS_FORWARDADD) || defined(WFS_PASS_DEFERRED)
	#include "UnityStandardConfig.cginc"

	#if UNITY_VERSION < 560 // 5.5.x
		#include "Unity55StandardCore.cginc"
	#elif UNITY_VERSION < 570 // 5.6.x
		#include "Unity56StandardCore.cginc"
	#else
		#include "Unity71StandardCore.cginc"
	#endif

	VertexOutputForwardBase vertBase (VertexInput v) { return vertForwardBase(v); }
	VertexOutputForwardAdd vertAdd (VertexInput v) { return vertForwardAdd(v); }
	half4 fragBase (VertexOutputForwardBase i) : SV_Target { return fragForwardBaseInternal(i); }
	half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { return fragForwardAddInternal(i); }

#elif defined(WFS_PASS_SHADOWCASTER)
	#if UNITY_VERSION < 560 // 5.5.x
		#include "Unity55StandardShadow.cginc"
	#elif UNITY_VERSION < 570 // 5.6.x
		#include "Unity56StandardShadow.cginc"
	#else
		#include "Unity71StandardShadow.cginc"
	#endif
	
#elif defined(WFS_PASS_META)
	#if UNITY_VERSION < 560 // 5.5.x
		#include "Unity55StandardMeta.cginc"
	#elif UNITY_VERSION < 570 // 5.6.x
		#include "Unity56StandardMeta.cginc"
	#else
		#include "Unity71StandardMeta.cginc"
	#endif
#endif




#include "WFSGeom.cginc"




#endif // WFS_SETUP_INCLUDED