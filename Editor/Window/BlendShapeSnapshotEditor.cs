using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class BlendShapeSnapshotEditor : EditorWindow
    {
        private BlendShapeSnapshotPresenter m_presenter;

        [MenuItem("Tools/BlendShape Snapshot Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlendShapeSnapshotEditor>("BlendShape Snapshot Manager");
            window.ApplyMinimumSize();
            window.Show();
        }

        private void OnEnable()
        {
            Compose();
            ApplyMinimumSize();
            m_presenter.OnEnable();
        }

        private void OnDisable()
        {
            m_presenter?.OnDisable();
        }

        private void OnGUI()
        {
            ApplyMinimumSize();
            m_presenter.OnGUI(position);
        }

        private void Compose()
        {
            m_presenter = BlendShapeSnapshotComposition.Create(Repaint);
        }

        private void ApplyMinimumSize()
        {
            if (m_presenter != null)
                minSize = m_presenter.MinimumSize;
        }
    }
}
