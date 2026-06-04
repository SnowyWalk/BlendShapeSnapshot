using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public class SnapshotPreviewRenderer : IEditorWindowModule
    {
        private PreviewRenderUtility m_previewRenderUtility;

        public void Init(SkinnedMeshRenderer targetSkinnedMeshRenderer, BlendShapeSnapshotDatabase blendShapeSnapshotDatabase = null)
        {
            m_previewRenderUtility?.Cleanup();
            m_previewRenderUtility = new PreviewRenderUtility();

            GameObject sourceRootGameObject  = GetRootGameObject(targetSkinnedMeshRenderer.transform);
            GameObject previewRootGameObject = UnityEngine.Object.Instantiate(sourceRootGameObject);
            previewRootGameObject.hideFlags = HideFlags.HideAndDontSave;
            
            targetSkinnedMeshRenderer = FindMatchingRendererInClone(sourceRootGameObject, targetSkinnedMeshRenderer, previewRootGameObject);
            if (blendShapeSnapshotDatabase != null)
                blendShapeSnapshotDatabase.Apply(targetSkinnedMeshRenderer);
            m_previewRenderUtility.AddSingleGO(previewRootGameObject);

            UpdateCamera();
        }

        public void Render(Rect rt)
        {
            if (m_previewRenderUtility == null)
                return;

            try
            {
                m_previewRenderUtility.BeginPreview(rt, GUIStyle.none);
                RenderTexture previewCamRenderTexture = m_previewRenderUtility.camera.targetTexture;
                if (previewCamRenderTexture?.antiAliasing == 0)
                {
                    previewCamRenderTexture.Release();
                    previewCamRenderTexture.antiAliasing = Mathf.Max(1, QualitySettings.antiAliasing);
                    previewCamRenderTexture.Create();
                }

                m_previewRenderUtility.Render();
            }
            finally
            {
                m_previewRenderUtility.EndAndDrawPreview(rt);
            }
        }

        void IEditorWindowModule.OnEnable()
        {
            SceneView.duringSceneGui += OnSceneViewUpdate;
        }

        void IEditorWindowModule.OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewUpdate;

            m_previewRenderUtility?.Cleanup();
            m_previewRenderUtility = null;
        }

        private void OnSceneViewUpdate(SceneView _)
        {
            if (m_previewRenderUtility != null)
                UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (SceneView.lastActiveSceneView?.camera == null)
                return;

            Camera cam = SceneView.lastActiveSceneView.camera;
            Transform cameraTransform = cam.transform;
            Camera previewCamera = m_previewRenderUtility.camera;
            
            bool changed =
                previewCamera.transform.position != cameraTransform.position ||
                previewCamera.transform.rotation != cameraTransform.rotation ||
                !Mathf.Approximately(previewCamera.fieldOfView, cam.fieldOfView) ||
                !Mathf.Approximately(previewCamera.nearClipPlane, cam.nearClipPlane) ||
                !Mathf.Approximately(previewCamera.farClipPlane, cam.farClipPlane) ||
                previewCamera.orthographic != cam.orthographic ||
                !Mathf.Approximately(previewCamera.orthographicSize, cam.orthographicSize);
            

            previewCamera.transform.position = cameraTransform.position;
            previewCamera.transform.rotation = cameraTransform.rotation;
            previewCamera.fieldOfView = cam.fieldOfView;
            previewCamera.nearClipPlane = cam.nearClipPlane;
            previewCamera.farClipPlane = cam.farClipPlane;
            previewCamera.orthographic = cam.orthographic;
            previewCamera.orthographicSize = cam.orthographicSize;
        }

        private GameObject GetRootGameObject(Transform targetTransform)
        {
            while (targetTransform.parent != null)
                targetTransform = targetTransform.parent;
            return targetTransform.gameObject;
        }

        private SkinnedMeshRenderer FindMatchingRendererInClone(
            GameObject sourceRoot,
            SkinnedMeshRenderer sourceRenderer,
            GameObject cloneRoot)
        {
            var path = GetSiblingIndexPath(sourceRoot.transform, sourceRenderer.transform);

            Transform current = cloneRoot.transform;
            foreach (int siblingIndex in path)
            {
                if (siblingIndex < 0 || siblingIndex >= current.childCount)
                    return null;

                current = current.GetChild(siblingIndex);
            }

            return current.GetComponent<SkinnedMeshRenderer>();

            List<int> GetSiblingIndexPath(Transform root, Transform target)
            {
                var path = new List<int>();

                Transform current = target;
                while (current != null && current != root)
                {
                    path.Add(current.GetSiblingIndex());
                    current = current.parent;
                }

                path.Reverse();
                return path;
            }
        }
    }


}