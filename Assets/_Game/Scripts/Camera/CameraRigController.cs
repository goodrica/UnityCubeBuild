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

        private Transform target;
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
        }

        private void EnsureAudioListener()
        {
            if (targetCamera == null || targetCamera.GetComponent<AudioListener>() != null)
            {
                return;
            }

            targetCamera.gameObject.AddComponent<AudioListener>();
        }
    }
}
