using ChromaCube.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChromaCube.Cameras
{
    public class CameraRigController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private float distance = 8.36f;
        [SerializeField] private float height = 4.8f;
        [SerializeField] private float smooth = 8f;
        [SerializeField] private float dragSensitivity = 0.28f;
        [SerializeField] private float pitchSensitivity = 0.12f;
        [SerializeField] private float minPitch = 24f;
        [SerializeField] private float maxPitch = 68f;
        [SerializeField] private float backgroundDistance = 35f;

        private Transform target;
        private GameObject backgroundQuad;
        private Material backgroundMaterial;
        private float yaw = 35f;
        private float pitch = 42f;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }

            EnsureAudioListener();
        }

        public void Configure(Transform followTarget, LevelData level)
        {
            target = followTarget;
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }

            EnsureAudioListener();
            ConfigureBackground(level);
            Snap();
        }

        private void LateUpdate()
        {
            if (target == null || targetCamera == null)
            {
                return;
            }

            ReadOrbitDrag();

            var orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            var offset = orbitRotation * new Vector3(0f, 0f, -distance);
            var lookAt = target.position + Vector3.up * 0.25f;
            var desired = lookAt + offset + Vector3.up * height * 0.18f;

            targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, desired, Time.deltaTime * smooth);
            targetCamera.transform.LookAt(lookAt);
            PositionBackground();
        }

        private void ReadOrbitDrag()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
            {
                return;
            }

            var delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * dragSensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * pitchSensitivity, minPitch, maxPitch);
        }

        private void Snap()
        {
            if (target == null || targetCamera == null)
            {
                return;
            }

            var orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            var lookAt = target.position + Vector3.up * 0.25f;
            targetCamera.transform.position = lookAt + orbitRotation * new Vector3(0f, 0f, -distance) + Vector3.up * height * 0.18f;
            targetCamera.transform.LookAt(lookAt);
            PositionBackground();
        }

        private void EnsureAudioListener()
        {
            if (targetCamera == null || targetCamera.GetComponent<AudioListener>() != null)
            {
                return;
            }

            targetCamera.gameObject.AddComponent<AudioListener>();
        }

        private void ConfigureBackground(LevelData level)
        {
            if (targetCamera == null)
            {
                return;
            }

            if (backgroundQuad == null)
            {
                backgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                backgroundQuad.name = "CameraBackground";
                backgroundQuad.transform.SetParent(targetCamera.transform, false);
                var collider = backgroundQuad.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }

            if (level.backgroundTexture == null)
            {
                backgroundQuad.SetActive(false);
                targetCamera.clearFlags = CameraClearFlags.SolidColor;
                return;
            }

            if (backgroundMaterial == null)
            {
                var shader = Shader.Find("Unlit/Texture");
                backgroundMaterial = new Material(shader);
            }

            backgroundMaterial.mainTexture = level.backgroundTexture;
            backgroundQuad.GetComponent<Renderer>().sharedMaterial = backgroundMaterial;
            backgroundQuad.SetActive(true);
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            PositionBackground();
        }

        private void PositionBackground()
        {
            if (targetCamera == null || backgroundQuad == null || !backgroundQuad.activeSelf)
            {
                return;
            }

            var distanceToPlane = backgroundDistance;
            var frustumHeight = 2f * distanceToPlane * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var frustumWidth = frustumHeight * targetCamera.aspect;
            backgroundQuad.transform.localPosition = new Vector3(0f, 0f, distanceToPlane);
            backgroundQuad.transform.localRotation = Quaternion.identity;
            backgroundQuad.transform.localScale = new Vector3(frustumWidth, frustumHeight, 1f);
        }
    }
}
