using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

namespace WFShader {
    /// 
    /// Static helper class handling shader keywords and blendmodes
    /// 
    public static class ShaderSetup {
        public static bool isProjector;
        public static bool isUnlit;
        public static bool isDiffuse;

        static Material material;
        static Object[] materials;

        public static void Initialize(MaterialEditor materialEditor) {
            materials = materialEditor.targets;
            material = (Material)materialEditor.target;
            DetectShaderType(material.shader);
        }

        public static void Initialize(Material material, Shader newShader) {
            materials = new[] { material };
            ShaderSetup.material = material;
            DetectShaderType(newShader);
        }

        static void DetectShaderType(Shader shader) {
            isProjector = shader.name.Contains("Projector");
            isUnlit = shader.name.Contains("Unlit");
            isDiffuse = shader.name.Contains("Diffuse") || shader.name.Contains("Vertex");
        }

        public static void MaterialChanged() {
            SetBlendMode();
            SetWireMode();
            SetKeywords(); //Last
        }

        public static void SetKeywords() {
            if (material == null) return;
            if (Mathf.Approximately(material.GetFloat("_AASmooth"), 0f))
                material.DisableKeyword("_AA_ON");
            else material.EnableKeyword("_AA_ON");

            if (isProjector || isUnlit) return;
            bool isTwosided = GetBlendMode() == WFSBlendMode.Cutout 
                && material.GetFloat("_TwoSided") > .5f;
            SetKeyword(material, "WFS_TWOSIDED", isTwosided);
            if (isTwosided) SetKeyword(material, "_ALPHATEST_ON", false);


            var shouldEmissionBeEnabled = !Mathf.Approximately(material.GetFloat("_WEmission"), 0f)
                || !Mathf.Approximately(material.GetFloat("_GEmission"), 0f);

            if (!isDiffuse) {
                #if UNITY_5_5
                    shouldEmissionBeEnabled = shouldEmissionBeEnabled || StandardShaderGUI.ShouldEmissionBeEnabled(material, material.GetColor("_EmissionColor"));
                #else
                    shouldEmissionBeEnabled = shouldEmissionBeEnabled || (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
                #endif
            }else{
                SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
            }

            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
        }

        static void SetKeyword(Material m, string keyword, bool state) {
            if (state) m.EnableKeyword(keyword);
            else m.DisableKeyword(keyword);
        }

#region Wire Mode

        public static void SetWireMode() {
            if (materials == null) return;
            foreach (Material mat in materials) {
                int mode = mat.GetInt("_WMode");
                WireSetup.Setup(mat, mode);
            }
        }

        static class WireSetup {
            static readonly string[] keywordsDX11 = new[] { "_MODE_DEFAULT", "_MODE_SCREEN", "_MODE_WORLD", "_MODE_BARY" };

            public static void Setup(Material mat, int mode) {
                string[] keywords = keywordsDX11;
                var keyword = keywords[mode];
                foreach (var kw in keywords) {
                    if (kw == keyword) mat.EnableKeyword(kw);
                    else mat.DisableKeyword(kw);
                }

            }
        }

#endregion

#region Blend Mode

        public static void SetBlendMode() {
            if (materials == null) return;
            foreach (Material mat in materials) {
                int mode = mat.GetInt("_Mode");
                blendCfgs[mode].Setup(mat);
            }
        }

        public static WFSBlendMode GetBlendMode() {
            var bmode = Prop._Mode._int;
            var type = typeof(WFSBlendMode) ;
            if (!System.Enum.IsDefined(type, bmode)) {
                Prop._Mode._int = 0; // Assumes 0 is always defined
                return 0;
            }
            return (WFSBlendMode)Prop._Mode._int;
        }

        public static void SetZWrite() {
            if (materials == null) return;
            foreach (Material mat in materials) {
                int mode = mat.GetInt("_Mode");
                material.SetInt("_ZWrite", blendCfgs[mode].zWrite);
            }
        }

        static BlendSetup[] blendCfgs = new BlendSetup[8] {
            new BlendSetup {mode=WFSBlendMode.Opaque,              tag="",                 srcBlend=BlendMode.One,             dstBlend=BlendMode.Zero,            zWrite=1, keyword="",                       queue=RenderQueue.Geometry},
            new BlendSetup {mode=WFSBlendMode.Cutout,              tag="TransparentCutout",srcBlend=BlendMode.One,             dstBlend=BlendMode.Zero,            zWrite=1, keyword="_ALPHATEST_ON",          queue=RenderQueue.AlphaTest},
            new BlendSetup {mode=WFSBlendMode.Fade,                tag="Transparent",      srcBlend=BlendMode.SrcAlpha,        dstBlend=BlendMode.OneMinusSrcAlpha,zWrite=0, keyword="_ALPHABLEND_ON",         queue=RenderQueue.Transparent},
            new BlendSetup {mode=WFSBlendMode.Transparent,         tag="Transparent",      srcBlend=BlendMode.One,             dstBlend=BlendMode.OneMinusSrcAlpha,zWrite=0, keyword="_ALPHAPREMULTIPLY_ON",   queue=RenderQueue.Transparent},
            new BlendSetup {mode=WFSBlendMode.Additive,            tag="Transparent",      srcBlend=BlendMode.One,             dstBlend=BlendMode.One,             zWrite=0, keyword="_ALPHAPREMULTIPLY_ON",   queue=RenderQueue.Transparent},
            new BlendSetup {mode=WFSBlendMode.SoftAdditive,        tag="Transparent",      srcBlend=BlendMode.OneMinusDstColor,dstBlend=BlendMode.One,             zWrite=0, keyword="_ALPHAPREMULTIPLY_ON",   queue=RenderQueue.Transparent},
            new BlendSetup {mode=WFSBlendMode.Multiplicative,      tag="Transparent",      srcBlend=BlendMode.DstColor,        dstBlend=BlendMode.OneMinusSrcAlpha,zWrite=0, keyword="_ALPHAPREMULTIPLY_ON",   queue=RenderQueue.Transparent},
            new BlendSetup {mode=WFSBlendMode.MultiplicativeDouble,tag="Transparent",      srcBlend=BlendMode.DstColor,        dstBlend=BlendMode.SrcColor,        zWrite=0, keyword="_ALPHAPREMULTIPLY_ON",   queue=RenderQueue.Transparent},
        };

        struct BlendSetup {
            public WFSBlendMode mode; public string tag; public BlendMode srcBlend; public BlendMode dstBlend; public int zWrite; public RenderQueue queue;

            static readonly string[] keywords = new[] { "_ALPHATEST_ON", "_ALPHABLEND_ON", "_ALPHAPREMULTIPLY_ON" };
            string _keyword;
            public string keyword {
                get { return _keyword; }
                set {
                    if (!keywords.Contains(value) && value.Length > 0) throw new System.Exception("Keyword is not valid.");
                    _keyword = value;
                }
            }

            public void Setup(Material mat) {
                mat.SetOverrideTag("RenderType", tag);
                mat.SetInt("_SrcBlend", (int)srcBlend);
                mat.SetInt("_DstBlend", (int)dstBlend);
                if (mode == WFSBlendMode.Opaque) mat.SetFloat("_WTransparency", 1f);
                mat.renderQueue = (int)queue;
                foreach (var kw in keywords) {
                    if (kw == keyword) {
                        mat.EnableKeyword(kw);
                    } else {
                        mat.DisableKeyword(kw);
                    }
                }
            }
        }

#endregion
    }
}
