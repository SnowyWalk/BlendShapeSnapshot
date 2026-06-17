using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public partial class BlendShapeSnapshotEditor : EditorWindow, IEditorWindowOrchestrator
    {
        // Modules
        private readonly SnapshotPreviewRenderer m_snapshotPreviewRenderer = new SnapshotPreviewRenderer();
        private readonly SnapshotRepository m_snapshotRepository = new SnapshotRepository();
        private IEditorWindowModule[] m_modules;

        // View
        private SkinnedMeshRenderer m_lastTargetMeshRenderer;
        private float m_contentWidth;

        private const float kWindowMinWidth = 470f;
        private const float kRootHorizontalPadding = 11f;
        private const float kRootTopPadding = 11f;
        private const float kRootBottomPadding = 8f;
        private const float kTargetToPreviewSpacing = 6f;
        private const float kPreviewMinHeight = 120f;
        private const float kPreviewExtraHeightRatio = 0.65f;
        private const float kPreviewAspect = 16f / 9f;
        private const float kPreviewToBodySpacing = 12f;
        private const float kSnapshotBodyMinHeight = 250f;
        private const float kBodyToSaveSpacing = 6f;

        // Model
        private SkinnedMeshRenderer m_targetMeshRenderer;

        // Properties
        private bool IsPreviewing => m_targetMeshRenderer != null;
        private static float LineHeight => EditorGUIUtility.singleLineHeight;
        private static float PreviewLabelHeight => LineHeight;

        [MenuItem("Tools/BlendShape Snapshot Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlendShapeSnapshotEditor>("BlendShape Snapshot Manager");
            window.ApplyMinimumSize();
            window.Show();
        }

        private void OnEnable()
        {
            ApplyMinimumSize();
            UpdateListView();
            if (IsPreviewing)
                m_listView.Select(0);
            AllocateButtonTextures();

            m_modules = new IEditorWindowModule[] { m_snapshotPreviewRenderer, m_snapshotRepository };
            foreach (IEditorWindowModule provider in m_modules)
            {
                provider.Initialize(this);
                provider.OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (IEditorWindowModule provider in m_modules)
            {
                provider.OnDisable();
            }

            ReleaseButtonTextures();

            m_lastTargetMeshRenderer = null;
        }

        private void OnGUI()
        {
            m_contentWidth = Mathf.Max(0f, position.width - kRootHorizontalPadding * 2f);
            ApplyMinimumSize();
            LayoutBudget layoutBudget = CalculateLayoutBudget(m_contentWidth);

            using (new EditorGUILayout.HorizontalScope(new GUIStyle { padding = new RectOffset((int)kRootHorizontalPadding, (int)kRootHorizontalPadding, (int)kRootTopPadding, (int)kRootBottomPadding) }))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawContent(layoutBudget);
                }
            }

            HandleDeleteKey();
            HandleRenameKey();
        }

        private void DrawContent(LayoutBudget layoutBudget)
        {
            DrawSkinnedMeshRendererTarget(); // SMR 넣는 칸

            GUILayout.Space(kTargetToPreviewSpacing);

            DrawSnapShotPreview(layoutBudget.PreviewHeight); // 프리뷰 영역

            GUILayout.Space(kPreviewToBodySpacing);

            DrawSnapShotListAndDiffViewer(layoutBudget.SnapshotBodyHeight); // 스냅샷 섹션 (리스트 & diff뷰어)

            GUILayout.Space(kBodyToSaveSpacing);

            DrawSaveField(); // Save
        }

        private void DrawSkinnedMeshRendererTarget()
        {
            const string label = "대상 Mesh";
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(label)).x + 8f;
            m_targetMeshRenderer = EditorGUILayout.ObjectField(label, m_targetMeshRenderer, typeof(SkinnedMeshRenderer), true, GUILayout.ExpandWidth(true)) as SkinnedMeshRenderer;
            EditorGUIUtility.labelWidth = 0f;

            if (m_lastTargetMeshRenderer != m_targetMeshRenderer)
            {
                m_snapshotPreviewRenderer.CreatePreviewTarget(m_targetMeshRenderer);
                UpdateListView();
                if (IsPreviewing)
                    m_listView.Select(0);
                
                m_lastTargetMeshRenderer = m_targetMeshRenderer;
            }
        }

        void IEditorWindowOrchestrator.Render()
        {
            Repaint();
        }

        private void ApplyMinimumSize()
        {
            float minContentWidth = kWindowMinWidth - kRootHorizontalPadding * 2f;
            minSize = new Vector2(kWindowMinWidth, ComputeMinimumWindowHeight(minContentWidth));
        }

        private static float ComputeMinimumWindowHeight(float contentWidth)
        {
            return kRootTopPadding +
                   LineHeight +
                   kTargetToPreviewSpacing +
                   kPreviewMinHeight +
                   PreviewLabelHeight +
                   kPreviewToBodySpacing +
                   kSnapshotBodyMinHeight +
                   HelpBoxFrameHeight +
                   kBodyToSaveSpacing +
                   GetSaveSectionReservedHeight(contentWidth, false) +
                   kRootBottomPadding;
        }

        private LayoutBudget CalculateLayoutBudget(float contentWidth)
        {
            float contentHeight = Mathf.Max(0f, position.height - kRootTopPadding - kRootBottomPadding);
            float saveSectionHeight = GetSaveSectionReservedHeight(contentWidth, m_targetMeshRenderer != null);
            float minimumContentHeight =
                LineHeight +
                kTargetToPreviewSpacing +
                kPreviewMinHeight +
                PreviewLabelHeight +
                kPreviewToBodySpacing +
                kSnapshotBodyMinHeight +
                HelpBoxFrameHeight +
                kBodyToSaveSpacing +
                saveSectionHeight;

            float extraHeight = Mathf.Max(0f, contentHeight - minimumContentHeight);
            float desiredPreviewHeight = GetDesiredPreviewHeight(contentWidth);
            float previewExtraHeight = Mathf.Min(extraHeight * kPreviewExtraHeightRatio, Mathf.Max(0f, desiredPreviewHeight - kPreviewMinHeight));
            float previewHeight = kPreviewMinHeight + previewExtraHeight;
            float topBlockHeight =
                LineHeight +
                kTargetToPreviewSpacing +
                previewHeight +
                PreviewLabelHeight +
                kPreviewToBodySpacing;

            float snapshotSectionHeight = contentHeight - topBlockHeight - kBodyToSaveSpacing - saveSectionHeight;
            float snapshotBodyHeight = Mathf.Max(kSnapshotBodyMinHeight, snapshotSectionHeight - HelpBoxFrameHeight);

            return new LayoutBudget(previewHeight, snapshotBodyHeight);
        }

        private static float GetDesiredPreviewHeight(float contentWidth)
        {
            float aspectHeight = contentWidth / kPreviewAspect;
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView == null)
                return aspectHeight;

            return Mathf.Min(aspectHeight, sceneView.position.height);
        }

        private struct LayoutBudget
        {
            public readonly float PreviewHeight;
            public readonly float SnapshotBodyHeight;

            public LayoutBudget(float previewHeight, float snapshotBodyHeight)
            {
                PreviewHeight = previewHeight;
                SnapshotBodyHeight = snapshotBodyHeight;
            }
        }
    }
}
