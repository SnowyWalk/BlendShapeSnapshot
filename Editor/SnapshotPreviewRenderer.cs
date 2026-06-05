using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public class SnapshotPreviewRenderer : IEditorWindowModule
    {
        private IEditorWindowOrchestrator m_orchestrator;

        private PreviewRenderUtility m_previewRenderUtility;

        private bool HasPreviewTarget => m_previewRenderUtility != null;

        public void CreatePreviewTarget(SkinnedMeshRenderer targetSkinnedMeshRenderer, BlendShapeSnapshotDatabase.BlendShapeSnapshot blendShapeSnapshot = null)
        {
            m_previewRenderUtility?.Cleanup();
            m_previewRenderUtility = null;

            if (targetSkinnedMeshRenderer == null)
                return;

            m_previewRenderUtility = new PreviewRenderUtility();

            GameObject sourceRootGameObject = GetRootGameObject(targetSkinnedMeshRenderer.transform);
            GameObject previewRootGameObject = Object.Instantiate(sourceRootGameObject);
            previewRootGameObject.hideFlags = HideFlags.HideAndDontSave;

            targetSkinnedMeshRenderer = FindMatchingRendererInClone(sourceRootGameObject, targetSkinnedMeshRenderer, previewRootGameObject);
            if (blendShapeSnapshot != null)
                blendShapeSnapshot.ApplySnapshot(targetSkinnedMeshRenderer);
            m_previewRenderUtility.AddSingleGO(previewRootGameObject);

            UpdateCamera();
            SyncPreviewLightFromScene();
        }

        public void Render(Rect rt)
        {
            if (!HasPreviewTarget)
                return;

            try
            {
                m_previewRenderUtility.BeginPreview(rt, GUIStyle.none);

                // 안티앨리어싱 적용
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

        void IEditorWindowModule.Initialize(IEditorWindowOrchestrator orchestrator)
        {
            m_orchestrator = orchestrator;
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
            if (HasPreviewTarget)
                UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (SceneView.lastActiveSceneView?.camera == null)
                return;

            Camera cam = SceneView.lastActiveSceneView.camera;
            Transform cameraTransform = cam.transform;
            Camera previewCamera = m_previewRenderUtility.camera;

            bool isDirty =
                previewCamera.transform.position != cameraTransform.position ||
                previewCamera.transform.rotation != cameraTransform.rotation ||
                !Mathf.Approximately(previewCamera.fieldOfView, cam.fieldOfView) ||
                !Mathf.Approximately(previewCamera.nearClipPlane, cam.nearClipPlane) ||
                !Mathf.Approximately(previewCamera.farClipPlane, cam.farClipPlane) ||
                previewCamera.orthographic != cam.orthographic ||
                !Mathf.Approximately(previewCamera.orthographicSize, cam.orthographicSize);

            if (!isDirty)
                return;

            previewCamera.transform.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
            previewCamera.fieldOfView = cam.fieldOfView;
            previewCamera.nearClipPlane = cam.nearClipPlane;
            previewCamera.farClipPlane = cam.farClipPlane;
            previewCamera.orthographic = cam.orthographic;
            previewCamera.orthographicSize = cam.orthographicSize;

            m_orchestrator.Render();
        }

        private void SyncPreviewLightFromScene()
        {
            if (!HasPreviewTarget)
                return;

            Light sourceLight = FindMainSceneLight();
            Light previewLight = m_previewRenderUtility.lights[0];

            if (sourceLight == null)
            {
                previewLight.enabled = false;
                if (m_previewRenderUtility.lights.Length > 1)
                    m_previewRenderUtility.lights[1].enabled = false;
                return;
            }

            previewLight.enabled = true;
            previewLight.type = sourceLight.type;
            previewLight.color = sourceLight.color;
            previewLight.intensity = sourceLight.intensity;
            previewLight.transform.SetPositionAndRotation(
                sourceLight.transform.position,
                sourceLight.transform.rotation);

            if (m_previewRenderUtility.lights.Length > 1)
                m_previewRenderUtility.lights[1].enabled = false;

            Unsupported.SetRenderSettingsUseFogNoDirty(false);
            m_previewRenderUtility.ambientColor = RenderSettings.ambientLight;
        }

        private static Light FindMainSceneLight()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            foreach (Light light in lights)
            {
                if (!light.enabled || !light.gameObject.activeInHierarchy)
                    continue;

                if (light.type == LightType.Directional)
                    return light;
            }

            return null;
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