using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class BlendShapeSnapshotPresenter
    {
        private readonly SnapshotRepository m_repository;
        private readonly SnapshotListModel m_listModel;
        private readonly SnapshotDiffService m_diffService;
        private readonly BlendShapeSnapshotService m_snapshotService;
        private readonly SnapshotPreviewRenderer m_previewRenderer;
        private readonly BlendShapeSnapshotView m_view;
        private readonly Action m_repaint;

        private readonly BlendShapeSnapshotViewState m_state = new BlendShapeSnapshotViewState();

        public BlendShapeSnapshotPresenter(
            SnapshotRepository repository,
            SnapshotListModel listModel,
            SnapshotDiffService diffService,
            BlendShapeSnapshotService snapshotService,
            SnapshotPreviewRenderer previewRenderer,
            BlendShapeSnapshotView view,
            Action repaint)
        {
            m_repository = repository;
            m_listModel = listModel;
            m_diffService = diffService;
            m_snapshotService = snapshotService;
            m_previewRenderer = previewRenderer;
            m_view = view;
            m_repaint = repaint;
        }

        public Vector2 MinimumSize => m_view.MinimumSize;

        public void OnEnable()
        {
            m_previewRenderer.OnEnable();
            RefreshAll();
        }

        public void OnDisable()
        {
            m_previewRenderer.OnDisable();
            m_view.Dispose();
        }

        public void OnGUI(Rect windowRect)
        {
            BlendShapeSnapshotViewEvents events = m_view.Draw(windowRect, m_state, m_previewRenderer.Render);
            Handle(events);
        }

        private void Handle(BlendShapeSnapshotViewEvents events)
        {
            if (events.TargetChanged)
                SetTarget(events.TargetRenderer);

            if (events.DescriptionChanged)
                m_state.SnapshotDescription = events.SnapshotDescription ?? string.Empty;

            if (events.DiffBasisChanged)
            {
                m_state.DiffBasis = events.DiffBasis;
                RefreshDiff();
            }

            if (events.SelectionChanged)
                Select(events.SelectedIndex);

            if (events.SaveRequested)
                Save();

            if (events.ApplyRequested)
                Apply();

            if (events.DeleteRequested)
                Delete();

            if (events.RenameRequested)
                Debug.Log("[BlendShapeSnapshot] Rename is not implemented yet.");
        }

        private void SetTarget(SkinnedMeshRenderer renderer)
        {
            if (m_state.TargetRenderer == renderer)
                return;

            m_state.TargetRenderer = renderer;
            m_previewRenderer.CreatePreviewTarget(renderer);
            RefreshList();
            Select(renderer != null ? 0 : -1);
        }

        private void Save()
        {
            if (!m_state.CanSave)
                return;

            m_repository.Save(m_state.TargetRenderer, m_state.SnapshotDescription);
            m_state.SnapshotDescription = string.Empty;
            RefreshList();
            Select(m_state.SnapshotNames.Count > 1 ? 1 : 0);
            m_repaint?.Invoke();
        }

        private void Apply()
        {
            SnapshotSelection selection = CurrentSelection();
            if (!selection.HasSavedSnapshot)
                return;

            if (!m_repository.TryGetSnapshot(m_state.TargetRenderer, selection.DatabaseIndex, out BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot))
                return;

            if (m_snapshotService.TryApplySnapshot(m_state.TargetRenderer, snapshot))
            {
                RefreshDiff();
                m_repaint?.Invoke();
            }
        }

        private void Delete()
        {
            SnapshotSelection selection = CurrentSelection();
            if (!selection.HasSavedSnapshot)
                return;

            string itemName = GetSelectedSnapshotName();
            if (!EditorUtility.DisplayDialog("삭제 확인", $"'{itemName}' 항목을 삭제할까?", "삭제", "취소"))
                return;

            if (!m_repository.DeleteSnapshot(m_state.TargetRenderer, selection.DatabaseIndex))
                return;

            int nextIndex = Mathf.Clamp(selection.ListIndex, 0, m_repository.GetSnapshotCount(m_state.TargetRenderer));
            RefreshList();
            Select(nextIndex);
            m_repaint?.Invoke();
        }

        private void Select(int listIndex)
        {
            if (m_state.TargetRenderer == null)
            {
                m_state.SelectedIndex = -1;
                RefreshDiff();
                return;
            }

            m_state.SelectedIndex = Mathf.Clamp(listIndex, 0, Mathf.Max(0, m_state.SnapshotNames.Count - 1));
            ApplySelectionToPreview();
            RefreshDiff();
            m_repaint?.Invoke();
        }

        private void ApplySelectionToPreview()
        {
            if (m_state.TargetRenderer == null || m_state.TargetRenderer.sharedMesh == null)
                return;

            SnapshotSelection selection = CurrentSelection();
            BlendShapeSnapshotDatabase.BlendShapeSnapshot previewSnapshot;
            if (selection.IsCurrentState)
            {
                previewSnapshot = new BlendShapeSnapshotDatabase.BlendShapeSnapshot(m_state.TargetRenderer, GetSelectedSnapshotName());
            }
            else if (!m_repository.TryGetSnapshot(m_state.TargetRenderer, selection.DatabaseIndex, out previewSnapshot))
            {
                return;
            }

            m_previewRenderer.ApplySnapshot(previewSnapshot);
        }

        private void RefreshAll()
        {
            RefreshList();
            RefreshDiff();
        }

        private void RefreshList()
        {
            List<string> names = m_state.TargetRenderer != null
                ? m_listModel.BuildLatestOrderedNames(m_state.TargetRenderer)
                : new List<string>();

            m_state.SnapshotNames = names;
            m_state.SelectedIndex = names.Count > 0 ? Mathf.Clamp(m_state.SelectedIndex, 0, names.Count - 1) : -1;
            RefreshFlagsAndLabel();
        }

        private void RefreshDiff()
        {
            SnapshotDiffResult diff = m_diffService.BuildDiff(m_state.TargetRenderer, CurrentSelection(), m_state.DiffBasis);
            m_state.DiffEntries = diff.Entries;
            m_state.DiffEmptyMessage = diff.EmptyMessage;
            RefreshFlagsAndLabel();
        }

        private void RefreshFlagsAndLabel()
        {
            bool hasMeshTarget = m_state.TargetRenderer != null && m_state.TargetRenderer.sharedMesh != null;
            SnapshotSelection selection = CurrentSelection();

            m_state.CanSave = hasMeshTarget;
            m_state.CanApply = hasMeshTarget && selection.HasSavedSnapshot;
            m_state.PreviewLabel = GetSelectedSnapshotName();
        }

        private SnapshotSelection CurrentSelection()
        {
            return SnapshotSelection.FromListIndex(m_state.SelectedIndex, m_repository.GetSnapshotCount(m_state.TargetRenderer));
        }

        private string GetSelectedSnapshotName()
        {
            if (m_state.SnapshotNames == null || m_state.SelectedIndex < 0 || m_state.SelectedIndex >= m_state.SnapshotNames.Count)
                return string.Empty;

            return m_state.SnapshotNames[m_state.SelectedIndex];
        }
    }
}
