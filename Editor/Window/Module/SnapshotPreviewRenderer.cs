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
        private SkinnedMeshRenderer m_previewSkinnedMeshRenderer;

        private bool HasPreviewTarget => m_previewRenderUtility != null;

        public void CreatePreviewTarget(SkinnedMeshRenderer targetSkinnedMeshRenderer)
        {
            m_previewRenderUtility?.Cleanup();
            m_previewRenderUtility = null;

            if (targetSkinnedMeshRenderer == null)
                return;

            m_previewRenderUtility = new PreviewRenderUtility();

            GameObject sourceRootGameObject = GetRootGameObject(targetSkinnedMeshRenderer.transform);
            GameObject previewRootGameObject = Object.Instantiate(sourceRootGameObject);
            previewRootGameObject.hideFlags = HideFlags.HideAndDontSave;

            m_previewSkinnedMeshRenderer = FindMatchingRendererInClone(sourceRootGameObject, targetSkinnedMeshRenderer, previewRootGameObject);
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

        public void ApplySnapshot(BlendShapeSnapshotDatabase.BlendShapeSnapshot blendShapeSnapshot)
        {
            if (m_previewSkinnedMeshRenderer == null)
                return;
            
            blendShapeSnapshot.ApplySnapshot(m_previewSkinnedMeshRenderer);
            m_orchestrator.Render();
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
            m_previewSkinnedMeshRenderer = null;
        }

        private void OnSceneViewUpdate(SceneView _)
        {
            if (!HasPreviewTarget)
                return;

            if (IsSceneViewChanged())
                UpdateCamera();
        }

        private bool IsSceneViewChanged()
        {
            Camera sourceCamera = SceneView.lastActiveSceneView?.camera;
            if (sourceCamera == null)
                return false;

            Camera previewCamera = m_previewRenderUtility.camera;

            sourceCamera.transform.GetPositionAndRotation(out Vector3 camPos, out Quaternion camAngle);
            previewCamera.transform.GetPositionAndRotation(out Vector3 previewCamPos, out Quaternion previewCamAngle);
            
            // Check TR
            if (camPos != previewCamPos || camAngle != previewCamAngle)
                return true;
            
            // Check FOV, Near, Far, Ortho
            if (!Mathf.Approximately(previewCamera.fieldOfView, sourceCamera.fieldOfView) ||
                !Mathf.Approximately(previewCamera.nearClipPlane, sourceCamera.nearClipPlane) ||
                !Mathf.Approximately(previewCamera.farClipPlane, sourceCamera.farClipPlane) ||
                previewCamera.orthographic != sourceCamera.orthographic ||
                !Mathf.Approximately(previewCamera.orthographicSize, sourceCamera.orthographicSize))
                return true;
            
            return false;
        }

        private void UpdateCamera()
        {
            Camera sourceCamera = SceneView.lastActiveSceneView?.camera;
            if (sourceCamera == null)
                return;

            Camera previewCamera = m_previewRenderUtility.camera;

            Transform cameraTransform = sourceCamera.transform;
            previewCamera.transform.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
            previewCamera.fieldOfView = sourceCamera.fieldOfView;
            previewCamera.nearClipPlane = sourceCamera.nearClipPlane;
            previewCamera.farClipPlane = sourceCamera.farClipPlane;
            previewCamera.orthographic = sourceCamera.orthographic;
            previewCamera.orthographicSize = sourceCamera.orthographicSize;

            m_orchestrator.Render();
        }

        private void SyncPreviewLightFromScene()
        {
            if (!HasPreviewTarget)
                return;

            if (m_previewRenderUtility.lights.Length > 1)
                m_previewRenderUtility.lights[1].enabled = false;

            Light sourceLight = FindMainSceneLight();
            Light previewLight = m_previewRenderUtility.lights[0];

            if (sourceLight == null)
            {
                previewLight.enabled = false;
                return;
            }

            previewLight.enabled = true;
            previewLight.type = sourceLight.type;
            previewLight.color = sourceLight.color;
            previewLight.intensity = sourceLight.intensity;
            previewLight.transform.SetPositionAndRotation(sourceLight.transform.position, sourceLight.transform.rotation);

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
            while (targetTransform.parent != null && !IsAvatarRoot())
                targetTransform = targetTransform.parent;
            return targetTransform.gameObject;

            bool IsAvatarRoot()
            {
                // TODO: SDK 연동
                return false;
            }
        }

        private SkinnedMeshRenderer FindMatchingRendererInClone(GameObject sourceRoot, SkinnedMeshRenderer sourceRenderer, GameObject cloneRoot)
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