using UnityEngine;
using System;

namespace WFShader {
    /// 
    /// This handles the foldouts in the material GUI. 
    /// The states are stored in the first few bits of the float "_Fold".
    /// 
    public struct Foldout {

        // Modify these titles to change the labels of the foldouts
        static readonly string[] titles = new string[5]{
            " - Wireframe - ",
            " - Surface - ",
            " - Glow - ",
            " - Fade - ",
            " - Preferences - " };

        static readonly string[] names = new[] { "Wireframe", "Surface", "Glow", "Fade", "Preferences" };

        static int foldState;
        static Material target; 

        public static void Initialize(Material material) {
            foldState = Mathf.RoundToInt(material.GetFloat("_Fold"));
            target = material;
        }

        public static Foldout Get(string foldName) {
            int bitPosition = Array.IndexOf(names, foldName);
            return new Foldout { bitPosition=bitPosition };
        }


        #region Non-statics

        public int bitPosition;

        public string title {
            get { return titles[bitPosition]; }
        }

        public bool state {
            get { return foldState.GetBit(bitPosition); }
            set {
                foldState = foldState.SetBit(bitPosition, value);
                target.SetFloat("_Fold", foldState);
            }
        }

        #endregion

    }
}
