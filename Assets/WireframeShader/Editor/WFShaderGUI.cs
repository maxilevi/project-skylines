using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WFShader {
    internal class WFShaderGUI : StandardShaderGUI {
        Material material;
        WFSBlendMode bmode;
        bool firstTimeApply = true;
        MaterialProperty[] props;
        MaterialEditor materialEditor;

        static WFShaderGUI() {
            Prop.propFinder = (name, props) => { return FindProperty(name, props, false); };
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props) {
            material = materialEditor.target as Material;
            this.props = props;
            this.materialEditor = materialEditor;

            // Init
            Prop.Initialize(props);
            Layout.Initialize(materialEditor);
            ShaderSetup.Initialize(materialEditor);

            if (ShaderSetup.isProjector) ShaderPropertiesGUI(material);
            else base.OnGUI(materialEditor, props); 

            if (firstTimeApply) {
                ShaderSetup.MaterialChanged();
                firstTimeApply = false;
            }
        }

        public override void ShaderPropertiesGUI(Material material) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-7);
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            DrawGUI();
            if (EditorGUI.EndChangeCheck()) {
                ShaderSetup.MaterialChanged();
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(1);
            EditorGUILayout.EndHorizontal();
        }

        void DrawGUI() {

            DrawBanner();

            DrawBlendMode();

            //EditorGUILayout.Space();

            if (Layout.BeginFold("Wireframe")) DrawWireframeGUI();
            Layout.EndFold();

            if (!ShaderSetup.isProjector) {
                if (Layout.BeginFold("Surface")) DrawSurfaceGUI();
                Layout.EndFold();
            }

            if (Layout.BeginFold("Glow")) DrawGlowGUI();
            Layout.EndFold();

            if (!ShaderSetup.isProjector) {
                if (Layout.BeginFold("Fade")) DrawFadeGUI();
                Layout.EndFold();
            }

            if (Layout.BeginFold("Preferences")) DrawPrefGUI();
            Layout.EndFold();
        }

        static Texture2D bannerTex = null;
        static GUIStyle rateTxt = null;
        static GUIStyle title = null;
        static GUIStyle linkStyle = null;

        void DrawBanner() {
            if (bannerTex == null) bannerTex = Resources.Load<Texture2D>("banner");

            if (rateTxt == null) {
                rateTxt = new GUIStyle();
                //rateTxt.alignment = TextAnchor.LowerCenter;
                rateTxt.alignment = TextAnchor.LowerRight;
                rateTxt.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
                rateTxt.fontSize = 9;
                rateTxt.padding = new RectOffset(0, 1, 0, 1);
            }

            if (title == null) {
                title = new GUIStyle(rateTxt);
                //title.alignment = TextAnchor.UpperCenter;
                title.normal.textColor = new Color(1f, 1f, 1f);
                title.alignment = TextAnchor.MiddleCenter;
                title.fontSize = 19;
                title.padding = new RectOffset(0, 0, 0, 8);
            }

            if(linkStyle == null) linkStyle = new GUIStyle();

            // Banner
            if (bannerTex != null) {
                GUILayout.Space(3);
                var rect = GUILayoutUtility.GetRect(0, int.MaxValue, 30, 30);
                EditorGUI.DrawPreviewTexture(rect, bannerTex, null, ScaleMode.ScaleAndCrop);
                //EditorGUI.LabelField(rect, "Rate \u2605\u2605\u2605\u2605\u2605", rateTxt);
                EditorGUI.LabelField(rect, "Rate | Review", rateTxt);
                rateTxt.alignment = TextAnchor.LowerCenter;
                EditorGUI.LabelField(rect, "DirectX 11", rateTxt);
                rateTxt.alignment = TextAnchor.LowerRight;
                EditorGUI.LabelField(rect, "Wireframe Shader", title);

                if (GUI.Button(rect, "", linkStyle)) {
                    Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/account/downloads/search=Wireframe%20Shader%20DX11");
                }
                GUILayout.Space(3);
            }

        }

        void DrawBlendMode() {
            var renderModeDrawer = ShaderSetup.isProjector ? Prop._ModeProj : null;

            if (Prop._Mode.Draw(0, renderModeDrawer)) {
                bmode = ShaderSetup.GetBlendMode();
                ShaderSetup.SetZWrite(); //only set zwrite when changing the render mode, because there is a zwrite option in the GUI
            }
            //BlendModePopup();
            if (!ShaderSetup.isProjector) bmode = ShaderSetup.GetBlendMode();

            if (bmode == WFSBlendMode.Cutout) {
                Prop._Cutoff.Draw(indentation: 2);

                if (ShaderSetup.isUnlit) {
                    EditorGUI.BeginChangeCheck();
                    Prop._TwoSided.Draw(indentation: 2);
                    if (EditorGUI.EndChangeCheck()) {
                        var isTwosided = Prop._TwoSided._bool;
                        var mode = isTwosided ? CullMode.Off : CullMode.Back;
                        Prop._Cull._float = (float)mode;
                    }
                } else {
                    Prop._TwoSided.Draw(indentation: 2);
                }
            }

            if (bmode == WFSBlendMode.Cutout && Prop._Cull._int != (int)CullMode.Back  && Prop._TwoSided._bool && !ShaderSetup.isUnlit)
                Prop._Cull._float = (int)CullMode.Back;
        }

        void DrawWireframeGUI() {
            GUILayout.Space(-3);
            GUILayout.Label("Lighting", EditorStyles.boldLabel);

            if (Prop._WTex.Draw() && !ShaderSetup.isProjector) {
                Prop._WOpacity._float = Prop._WColor._color.a;
            }

            if (Prop._WTex._texture != null) {
                Prop._WUV.Draw(indentation: 2);
            }

            if (Prop._WOpacity.Draw()) {
                var c = Prop._WColor._color;
                Prop._WColor._color = new Color(c.r, c.g, c.b, Prop._WOpacity._float);
            }

            if (ShaderSetup.isProjector || bmode != WFSBlendMode.Opaque)
                Prop._WTransparency.Draw();

            if (Prop._WLight.active) {
                if (!ShaderSetup.isProjector && Prop._WLight._int != (int)LightMode.Unlit) {
                    Prop._WEmission.Draw();
                    Prop._WMetal.Draw();
                    Prop._WGloss.Draw();
                }
                Prop._WLight.Draw();
            }

            GUILayout.Space(10);
            GUILayout.Label("Shape", EditorStyles.boldLabel);
            Prop._WThickness.Draw();
            Prop._WStyle.Draw();
            var wireStyle = (WireStyle)Prop._WStyle._int;
            if (wireStyle != WireStyle.Default) {
                Prop._WParam.Draw(indentation: 2);
            }
            Prop._WInvert.Draw();
            Prop._Quad.Draw();
            Prop._WMask.Draw();

            GUILayout.Space(10);
            GUILayout.Label("Rendering", EditorStyles.boldLabel);

            var wireModeDrawer = wireStyle == WireStyle.Smooth ? Prop._WModeSmooth : null;
            Prop._WMode.Draw(0, wireModeDrawer);

            Prop._AASmooth.Draw();
        }

        static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
        static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
        void DrawSurfaceGUI() {
            if (ShaderSetup.isUnlit || ShaderSetup.isDiffuse) {
                var albedoMap = FindProperty("_MainTex", props);
                var albedoColor = FindProperty("_Color", props);
                var bumpMap = FindProperty("_BumpMap", props, false);
                var bumpScale = FindProperty("_BumpScale", props, false);

                materialEditor.TexturePropertySingleLine(albedoText, albedoMap, albedoColor);
                if(bumpMap != null) {
                    materialEditor.TexturePropertySingleLine(normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
                }
                materialEditor.TextureScaleOffsetProperty(albedoMap);
            } else {
                base.ShaderPropertiesGUI(material);
            }
        }

        void DrawGlowGUI() {
            Prop._Glow.Draw();
            EditorGUI.BeginDisabledGroup(!Prop._Glow._bool);

            Prop._GColor.Draw();
            Prop._GEmission.Draw();
            Prop._GDist.Draw();
            Prop._GPower.Draw();

            EditorGUI.EndDisabledGroup();
        }

        void DrawFadeGUI() {
            Prop._Fade.Draw();
            EditorGUI.BeginDisabledGroup(!Prop._Fade._bool);

            Prop._FMode.Draw();
            Prop._FDist.Draw();
            Prop._FPow.Draw();

            EditorGUI.EndDisabledGroup();
        }

        void DrawPrefGUI() {
            Prop._Limits.Draw();
            Prop._Cull.Draw();
            Prop._ZWrite.Draw();
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
            ShaderSetup.Initialize(material, newShader);
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            ShaderSetup.MaterialChanged();
        }


    }
}
