using System;
using UnityEditor;
using UnityEngine;

namespace WFShader {

    /// 
    /// Function to draw property fields. Returns true if the property changed.
    /// 
    public delegate bool DrawField(Property prop);

    /// 
    /// Layout helper static class
    /// 
    public static class Layout {
        static MaterialEditor materialEditor;

        public static void Initialize(MaterialEditor materialEditor) {
            Layout.materialEditor = materialEditor;
            Foldout.Initialize(materialEditor.target as Material);
        }

        #region Foldout

        static GUIStyle _foldoutStyle;
        static GUIStyle foldoutStyle {
            get {
                if (_foldoutStyle == null) {
                    _foldoutStyle = new GUIStyle(EditorStyles.foldout);
                    _foldoutStyle.font = EditorStyles.boldFont;
                }
                return _foldoutStyle;
            }
        }

        public static bool BeginFold(string foldName) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(3);
            EditorGUI.indentLevel++;

            Foldout fold = Foldout.Get(foldName);
            bool foldState = EditorGUI.Foldout(EditorGUILayout.GetControlRect(),
                fold.state, fold.title, true, foldoutStyle);
            fold.state = foldState;

            EditorGUI.indentLevel--;
            if (foldState) GUILayout.Space(5);
            //EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(1);
            EditorGUILayout.BeginVertical();

            return foldState;
        }

        public static void EndFold() {
            EditorGUILayout.EndVertical();
            GUILayout.Space(1);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);
            //EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            GUILayout.Space(0);
        }

        #endregion

        #region Draw Fields

        public static DrawField defaultUI = (p) => {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(p.matProp, p.label);
            return EditorGUI.EndChangeCheck();
        };

        public static DrawField EnumField(Type type) {
            return (p) => {
                BeginChangeCheck(p);
                Enum enumVal = (Enum)Enum.ToObject(type, p._int);
                if(!Enum.IsDefined(type, enumVal)) {
                    p._int = 0; // Assumes 0 is always defined
                    enumVal = (Enum)Enum.ToObject(type, p._int);
                    if (!Enum.IsDefined(type, enumVal)) {
                        p._int = 2; // Assumes 2 is always defined
                        enumVal = (Enum)Enum.ToObject(type, p._int);
                    }
                }
                var newValue = EditorGUILayout.EnumPopup(p.label, enumVal);
                if (EndChangeCheck(p)) {
                    p._int = Convert.ToInt32(newValue);
                }
                return changed;
            };
        }

        static bool SliderBase(Property p, float min, float max, float scale = 1f) {
            BeginChangeCheck(p);
            float newValue;
            if (Prop._Limits._bool) {
                newValue = EditorGUILayout.Slider(p.label, p._float / scale, min, max) * scale;
            } else {
                newValue = EditorGUILayout.FloatField(p.label, p._float / scale) * scale;
            }
            if (EndChangeCheck(p)) p._float = newValue;
            return changed;
        }

        public static DrawField Slider(float min, float max, float scale=1f) {
            return (p) => {return SliderBase(p, min, max, scale);};
        }

        public static DrawField Slider(float scale = 1f) {
            return (p) => {return SliderBase(p, 0, 1, scale);};
        }

        public static DrawField slider = (p) => {return SliderBase(p, 0, 1, 1);};

        public static DrawField textureField = (p) => {
            EditorGUI.BeginChangeCheck();
            materialEditor.TexturePropertySingleLine(p.label, p.matProp);
            return EditorGUI.EndChangeCheck();
        };

        public static DrawField TextureExtraField(Property extra) {
            return (p) => {
                var ofs = EditorGUIUtility.labelWidth;
                materialEditor.SetDefaultGUIWidths();
                EditorGUIUtility.labelWidth = 0;
                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertySingleLine(p.label, p.matProp, extra.matProp);
                EditorGUIUtility.labelWidth = ofs;
                return EditorGUI.EndChangeCheck();
            };
        }

        public static DrawField colorField = (p) => {
            EditorGUI.BeginChangeCheck();
            var ofs = EditorGUIUtility.labelWidth;
            materialEditor.SetDefaultGUIWidths();
            materialEditor.ShaderProperty(p.matProp, p.label);
            EditorGUIUtility.labelWidth = ofs;
            return EditorGUI.EndChangeCheck();
        };

        public static DrawField toggle = (p) => {
            BeginChangeCheck(p);
            var newValue = EditorGUILayout.Toggle(p.label, p._bool);
            if (EndChangeCheck(p)) p._bool = newValue;
            return changed;
        };

        public static DrawField Toggle(float scale = 1f) {
            return (p) => {
                BeginChangeCheck(p);
                var newValue = EditorGUILayout.Toggle(p.label, p._bool);
                if (EndChangeCheck(p)) p._float = newValue ? scale : 0f;
                return changed;
            };
        }

        #endregion

        #region Change Check
        static bool changed;

        public static void BeginChangeCheck(Property p) {
            if (p.matProp.hasMixedValue)
                EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
        }

        public static bool EndChangeCheck(Property p) {
            EditorGUI.showMixedValue = false;
            changed = EditorGUI.EndChangeCheck();
            return changed;
        }

        #endregion

    }
}
