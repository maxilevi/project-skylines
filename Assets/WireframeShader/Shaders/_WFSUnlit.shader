Shader "WireframeShader/Unlit"{
    Properties{
        // Wire
        _WTex("Wire Texture", 2D) = "white" {}
        [KeywordEnum(UV0, Barycentric)] _WUV("Wire UV Set", Float) = 0.0
        _WColor("Wire Color", Color) = (0,0,1,1)
        _WTransparency("Wire Transparency", Range(0.0,1.0)) = 1.0
        _WOpacity("Wire Opacity", Range(0.0,1.0)) = 1.0
        _WThickness("Wire Thickness", Float) = 0.05
        _WParam("Wire Parameter", Float) = 0
        _WMode("Wire Mode", Float) = 0.0
        [KeywordEnum(Default, Smooth)] _WStyle("Wire Style", Float) = 0.0 
        _WMask("Wire Mask", 2D) = "white" {}
        _AASmooth("AA Smoothing", Float) = 1.5
        _WInvert("Wire Invert", Float) = 0.0
        [Toggle] _Quad("Quad Mesh", Float) = 0.0

        // Glow
        [Toggle] _Glow("Glow Enable", Float) = 0.0
        _GColor("Glow Color", Color) = (0,0,0,1)
        _GDist("Glow Distance", Float) = 0.35
        _GPower("Glow Power", Float) = 0.5

        // Fade
        [Toggle] _Fade("Glow Enable", Float) = 0.0
        _FMode("Fade Mode", Float) = 1
        _FDist("Fade Distance", Float) = 0.1
        _FPow("Fade Power", Float) = 5

        // Settings
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _Fold("__fld", Float) = 1.0
        [HideInInspector] _Limits("__lmt", Float) = 1.0
        [HideInInspector] _TwoSided ("__two", Float) = 1.0

        // Surface
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    CGINCLUDE
        #define WFS_UNLIT
        #define _WLIGHT_UNLIT
    ENDCG

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull [_Cull]
    
        Pass {  
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
                #pragma target 4.0

                #pragma shader_feature _GLOW_ON
                #pragma shader_feature _FADE_ON 
                #pragma shader_feature _QUAD_ON 
                #pragma shader_feature _AA_ON
                #pragma shader_feature _WUV_UV0 _WUV_BARYCENTRIC
                #pragma shader_feature _ _MODE_SCREEN _MODE_WORLD _MODE_DEFAULT
                #pragma shader_feature _WSTYLE_DEFAULT _WSTYLE_SMOOTH

                #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

                #pragma vertex vert
                #pragma geometry geom
                #pragma fragment frag
                #pragma multi_compile_fog

                #include "WFSUnlit.cginc"
            ENDCG
        }
    }

    FallBack "VertexLit"
    CustomEditor "WFShader.WFShaderGUI"
}
