using UnityEngine;
using UnityEngine.InputSystem;   // Keyboard.current / Mouse.current

namespace KHato
{
    /// <summary>
    /// KHato (2074) – Etapa 2
    /// First-person WASD movement + gravity via CharacterController.
    /// Uses UnityEngine.InputSystem.Keyboard.current and Mouse.current directly;
    /// no dependency on the generated InputSystem_Actions wrapper class.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Walk speed in Unity units/second")]
        [SerializeField] private float walkSpeed = 8f;

        [Header("Gravity")]
        [SerializeField] private float gravity          = -25f;
        [SerializeField] private float groundedGravity  = -2f;    // small constant push while grounded
        [SerializeField] private float groundCheckDistance = 0.15f;
        [SerializeField] private LayerMask groundMask   = ~0;      // all layers

        // ── References ────────────────────────────────────────────────────────
        private CharacterController _cc;

        // ── State ──────────────────────────────────────────────────────────────
        private Vector3 _velocity;
        private bool    _isGrounded;

        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            CheckGrounded();
            ApplyGravity();
            Move();
        }

        // ── Grounded check ────────────────────────────────────────────────────
        private void CheckGrounded()
        {
            float radius   = _cc.radius * 0.9f;
            Vector3 origin = transform.position + _cc.center;
            float dist     = (_cc.height * 0.5f) - _cc.radius + groundCheckDistance;

            _isGrounded  = Physics.SphereCast(origin, radius, Vector3.down, out _,
                                              dist, groundMask, QueryTriggerInteraction.Ignore);
            _isGrounded |= _cc.isGrounded;
        }

        // ── Gravity ───────────────────────────────────────────────────────────
        private void ApplyGravity()
        {
            if (_isGrounded && _velocity.y < 0f)
                _velocity.y = groundedGravity;
            else
                _velocity.y += gravity * Time.deltaTime;
        }

        // ── Horizontal movement ───────────────────────────────────────────────
        private void Move()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float h = 0f, v = 0f;

            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h -= 1f;

            Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 right   = new Vector3(transform.right.x,   0f, transform.right.z).normalized;

            Vector3 move   = (forward * v + right * h) * walkSpeed;
            Vector3 motion = move + _velocity;
            _cc.Move(motion * Time.deltaTime);
        }
    }
}
