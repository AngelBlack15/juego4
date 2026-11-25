using UnityEngine;
using UnityEngine.InputSystem;   // Mouse.current

namespace KHato
{
    /// <summary>
    /// KHato (2074) – Etapa 2
    /// Mouse-look for first-person view.
    ///   Horizontal (Yaw)   → rotates the Player root around Y.
    ///   Vertical  (Pitch)  → tilts CameraHolder around X, clamped.
    /// Uses UnityEngine.InputSystem.Mouse.current directly;
    /// no dependency on the generated InputSystem_Actions wrapper class.
    /// </summary>
    public class MouseLook : MonoBehaviour
    {
        [Header("Sensitivity")]
        [SerializeField] private float mouseSensitivity = 0.15f;

        [Header("Vertical clamp")]
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch =  80f;

        [Header("References")]
        [Tooltip("Child CameraHolder whose X-axis is tilted for vertical look")]
        [SerializeField] private Transform cameraHolder;

        // ── State ──────────────────────────────────────────────────────────────
        private float _pitch;

        // ─────────────────────────────────────────────────────────────────────

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;

            if (cameraHolder != null)
                _pitch = cameraHolder.localEulerAngles.x;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 delta = mouse.delta.ReadValue();

            // Yaw: rotate Player body around world Y
            transform.Rotate(Vector3.up, delta.x * mouseSensitivity, Space.World);

            // Pitch: tilt CameraHolder (invert Y for natural feel)
            _pitch -= delta.y * mouseSensitivity;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);

            if (cameraHolder != null)
                cameraHolder.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }
    }
}
