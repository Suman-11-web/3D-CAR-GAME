using UnityEngine;
using UnityEngine.UI;

namespace CarGame.Environment
{
    /// <summary>
    /// Generates top-down minimap rendering using an orthographic camera
    /// and synchronizes the player orientation indicator.
    /// </summary>
    public class MinimapSystem : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform targetPlayer;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private RectTransform playerBlipIcon;

        [Header("Camera Settings")]
        [SerializeField] private float cameraHeight = 85f;
        [SerializeField] private float orthographicSize = 45f;
        [SerializeField] private bool rotateCameraWithPlayer = false;

        [Header("Render Texture Output")]
        [SerializeField] private RenderTexture minimapRenderTexture;
        [SerializeField] private RawImage minimapRawImage;

        private void Start()
        {
            if (targetPlayer == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null) targetPlayer = player.transform;
            }

            SetupMinimapCamera();
            ConnectToHUD();
        }

        private void SetupMinimapCamera()
        {
            if (minimapCamera == null)
            {
                GameObject camObj = new GameObject("Minimap_Camera");
                camObj.transform.parent = transform;
                minimapCamera = camObj.AddComponent<Camera>();
            }

            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = orthographicSize;
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color(0.12f, 0.14f, 0.16f);
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            if (minimapRenderTexture == null)
            {
                minimapRenderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                minimapRenderTexture.name = "RT_Minimap";
            }
            minimapCamera.targetTexture = minimapRenderTexture;
        }

        private void ConnectToHUD()
        {
            if (minimapRawImage == null)
            {
                GameObject mapFrame = GameObject.Find("Minimap_Frame");
                if (mapFrame != null)
                {
                    minimapRawImage = mapFrame.GetComponent<RawImage>();
                    if (minimapRawImage == null)
                    {
                        minimapRawImage = mapFrame.AddComponent<RawImage>();
                    }
                    minimapRawImage.texture = minimapRenderTexture;
                }
            }
        }

        private void LateUpdate()
        {
            if (targetPlayer == null || minimapCamera == null) return;

            // Follow player position in X and Z
            Vector3 targetPos = targetPlayer.position;
            minimapCamera.transform.position = new Vector3(targetPos.x, cameraHeight, targetPos.z);

            if (rotateCameraWithPlayer)
            {
                minimapCamera.transform.rotation = Quaternion.Euler(90f, targetPlayer.eulerAngles.y, 0f);
            }
            else
            {
                minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                if (playerBlipIcon != null)
                {
                    playerBlipIcon.localEulerAngles = new Vector3(0f, 0f, -targetPlayer.eulerAngles.y);
                }
            }
        }
    }
}
