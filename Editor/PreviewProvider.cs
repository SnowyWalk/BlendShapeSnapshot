using System;
using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public class PreviewProvider : IEditorWindowProvider
    {
        private PreviewRenderUtility m_previewRenderUtility;

        public void Init(GameObject targetGameObject, BlendShapeSnapshotInfo blendShapeSnapshotInfo)
        {
            m_previewRenderUtility.Cleanup();
            m_previewRenderUtility.AddSingleGO(GetRootGameObject(targetGameObject.transform));
            
            UpdateCamera();
        }

        void IEditorWindowProvider.OnEnable()
        {
            m_previewRenderUtility = new PreviewRenderUtility();
            SceneView.duringSceneGui += OnSceneViewUpdate;
        }

        void IEditorWindowProvider.OnDisable()
        {
            m_previewRenderUtility.Cleanup();
            
            SceneView.duringSceneGui -= OnSceneViewUpdate;
            m_previewRenderUtility = null;
        }

        private void OnSceneViewUpdate(SceneView _)
        {
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            Camera cam = SceneView.currentDrawingSceneView.camera;
            Transform cameraTransform = cam.transform;
            
            m_previewRenderUtility.camera.transform.position = cameraTransform.position;
            m_previewRenderUtility.camera.transform.rotation = cameraTransform.rotation;
            m_previewRenderUtility.camera.fieldOfView = cam.fieldOfView;
            m_previewRenderUtility.camera.nearClipPlane = cam.nearClipPlane;
            m_previewRenderUtility.camera.farClipPlane = cam.farClipPlane;
            m_previewRenderUtility.camera.orthographic = cam.orthographic;
            m_previewRenderUtility.camera.orthographicSize = cam.orthographicSize;
            m_previewRenderUtility.camera.aspect = cam.aspect;
            // m_previewRenderUtility.camera.projectionMatrix = cam.projectionMatrix;
            // m_previewRenderUtility.camera.cullingMask = cam.cullingMask;
            // m_previewRenderUtility.camera.backgroundColor = cam.backgroundColor;
            // m_previewRenderUtility.camera.clearFlags = cam.clearFlags;
            // m_previewRenderUtility.camera.depth = cam.depth;
            // m_previewRenderUtility.camera.rect = cam.rect;
            // m_previewRenderUtility.camera.targetTexture = cam.targetTexture;
            // m_previewRenderUtility.camera.enabled = cam.enabled;
        }
        
        private GameObject GetRootGameObject(Transform targetTransform)
        {
            while (targetTransform.parent != null)
                targetTransform = targetTransform.parent;
            return targetTransform.gameObject;
        }
    }

    
}
