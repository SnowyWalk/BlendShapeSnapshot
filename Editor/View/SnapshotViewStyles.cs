using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class SnapshotViewStyles
    {
        private Texture2D m_applyBtnNormalTex;
        private Texture2D m_applyBtnHoverTex;

        public static GUIStyle NoWrapHintStyle => new GUIStyle(EditorStyles.centeredGreyMiniLabel) { wordWrap = false, clipping = TextClipping.Clip };

        public GUIStyle EnabledApplyButton
        {
            get
            {
                EnsureTextures();
                GUIStyle style = BoldButton(SnapshotViewLayout.ApplyButtonHeight);
                style.normal.background = m_applyBtnNormalTex;
                style.hover.background = m_applyBtnHoverTex;
                style.active.background = m_applyBtnNormalTex;
                style.normal.textColor = Color.white;
                style.hover.textColor = Color.white;
                style.active.textColor = Color.white;
                return style;
            }
        }

        public static GUIStyle DisabledApplyButton => BoldButton(SnapshotViewLayout.ApplyButtonHeight);
        public static GUIStyle SaveButton => BoldButton(SnapshotViewLayout.SaveButtonHeight);

        public static GUIStyle SaveHeader => new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };

        public void Dispose()
        {
            if (m_applyBtnNormalTex != null)
                Object.DestroyImmediate(m_applyBtnNormalTex);

            if (m_applyBtnHoverTex != null)
                Object.DestroyImmediate(m_applyBtnHoverTex);

            m_applyBtnNormalTex = null;
            m_applyBtnHoverTex = null;
        }

        private void EnsureTextures()
        {
            if (m_applyBtnNormalTex == null)
                m_applyBtnNormalTex = GUIUtils.MakeTex(1, 1, new Color(0.2f, 0.45f, 0.75f, 1f));

            if (m_applyBtnHoverTex == null)
                m_applyBtnHoverTex = GUIUtils.MakeTex(1, 1, new Color(0.25f, 0.52f, 0.85f, 1f));
        }

        private static GUIStyle BoldButton(float height)
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fixedHeight = height,
            };
        }
    }
}
