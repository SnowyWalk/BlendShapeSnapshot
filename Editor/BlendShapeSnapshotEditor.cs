using UnityEditor;
using UnityEngine;

public class BlendShapeSnapshotEditor : EditorWindow
{
    private PreviewRenderUtility previewUtility;
    private GameObject previewObject;
    private Material previewMaterial;

    private Vector3 copiedCameraPosition = new Vector3(0f, 1f, -5f);
    private Quaternion copiedCameraRotation = Quaternion.identity;
    private float copiedFieldOfView = 60f;
    private float copiedNearClipPlane = 0.03f;
    private float copiedFarClipPlane = 1000f;
    private bool hasCopiedSceneCamera;

    [MenuItem("Tools/Blend Shape Snapshot")]
    public static void ShowWindow()
    {
        BlendShapeSnapshotEditor window = GetWindow<BlendShapeSnapshotEditor>("Blend Shape Snapshot");
        window.minSize = new Vector2(420f, 320f);
        window.Show();
    }

    private void OnEnable()
    {
        CreatePreview();
    }

    private void OnDisable()
    {
        CleanupPreview();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Blend Shape Snapshot", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Scene Camera"))
        {
            CopySceneCamera();
        }

        if (GUILayout.Button("Reset Preview Object"))
        {
            RecreatePreviewObject();
        }
        EditorGUILayout.EndHorizontal();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.Vector3Field("Copied Position", copiedCameraPosition);
            EditorGUILayout.Vector3Field("Copied Euler", copiedCameraRotation.eulerAngles);
        }

        if (!hasCopiedSceneCamera)
        {
            EditorGUILayout.HelpBox("Move the Scene view camera, then click Copy Scene Camera.", MessageType.Info);
        }

        Rect previewRect = GUILayoutUtility.GetRect(1f, 10000f, 220f, 10000f);
        DrawPreview(previewRect);
    }

    private void CreatePreview()
    {
        CleanupPreview();

        previewUtility = new PreviewRenderUtility();
        previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
        previewUtility.camera.backgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);
        previewUtility.lights[0].intensity = 1.2f;
        previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
        previewUtility.lights[1].intensity = 0.6f;

        RecreatePreviewObject();
    }

    private void CleanupPreview()
    {
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
            previewObject = null;
        }

        if (previewMaterial != null)
        {
            DestroyImmediate(previewMaterial);
            previewMaterial = null;
        }

        if (previewUtility != null)
        {
            previewUtility.Cleanup();
            previewUtility = null;
        }
    }

    private void RecreatePreviewObject()
    {
        if (previewUtility == null)
        {
            return;
        }

        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
        }

        previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewObject.name = "Temporary Preview Object";
        previewObject.hideFlags = HideFlags.HideAndDontSave;
        ResetPreviewObjectTransform();
        previewObject.transform.localScale = Vector3.one;

        Renderer renderer = previewObject.GetComponent<Renderer>();
        if (previewMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                shader = Shader.Find("Hidden/InternalErrorShader");
            }

            previewMaterial = new Material(shader);
            previewMaterial.hideFlags = HideFlags.HideAndDontSave;
            previewMaterial.color = new Color(0.2f, 0.65f, 1f, 1f);
        }

        renderer.sharedMaterial = previewMaterial;
        previewUtility.AddSingleGO(previewObject);
        Repaint();
    }

    private void CopySceneCamera()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null || sceneView.camera == null)
        {
            ShowNotification(new GUIContent("No active Scene view camera."));
            return;
        }

        Camera sceneCamera = sceneView.camera;
        copiedCameraPosition = sceneCamera.transform.position;
        copiedCameraRotation = sceneCamera.transform.rotation;
        copiedFieldOfView = sceneCamera.fieldOfView;
        copiedNearClipPlane = sceneCamera.nearClipPlane;
        copiedFarClipPlane = sceneCamera.farClipPlane;
        hasCopiedSceneCamera = true;

        if (previewObject != null)
        {
            ResetPreviewObjectTransform();
        }

        Repaint();
    }

    private void ResetPreviewObjectTransform()
    {
        if (previewObject == null)
        {
            return;
        }

        previewObject.transform.position = hasCopiedSceneCamera
            ? copiedCameraPosition + copiedCameraRotation * Vector3.forward * 3f
            : Vector3.zero;
        previewObject.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
    }

    private void DrawPreview(Rect rect)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        if (previewUtility == null)
        {
            CreatePreview();
        }

        if (previewObject == null)
        {
            RecreatePreviewObject();
        }

        Camera previewCamera = previewUtility.camera;
        previewCamera.transform.SetPositionAndRotation(copiedCameraPosition, copiedCameraRotation);
        previewCamera.fieldOfView = copiedFieldOfView;
        previewCamera.nearClipPlane = copiedNearClipPlane;
        previewCamera.farClipPlane = copiedFarClipPlane;

        previewUtility.BeginPreview(rect, GUIStyle.none);
        previewCamera.Render();
        Texture texture = previewUtility.EndPreview();
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false);
    }
}
