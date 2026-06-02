using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GTAWorld.Game
{
    /// <summary>
    /// Input-System friendly fallback mover for the generated demo scene. It keeps the prototype playable while
    /// the full Opsive input/item pipeline is configured.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameSimplePlayerMover : MonoBehaviour
    {
        [SerializeField] private float m_MoveSpeed = 4.5f;
        [SerializeField] private float m_SprintMultiplier = 1.7f;
        [SerializeField] private float m_RotationSpeed = 12f;
        [SerializeField] private Camera m_Camera;

        private CharacterController m_CharacterController;

        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
            if (m_Camera == null) {
                m_Camera = Camera.main;
            }
        }

        private void Update()
        {
            var movement = ReadMovement();
            if (movement.sqrMagnitude < 0.001f) {
                return;
            }

            var moveDirection = GetCameraRelativeDirection(movement);
            var speed = m_MoveSpeed * (IsSprinting() ? m_SprintMultiplier : 1f);
            var delta = moveDirection * speed * Time.deltaTime;

            if (m_CharacterController != null && m_CharacterController.enabled) {
                m_CharacterController.Move(delta);
            } else {
                transform.position += delta;
            }

            var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSpeed);
        }

        private Vector2 ReadMovement()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) {
                return Vector2.zero;
            }

            var movement = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) {
                movement.y += 1f;
            }
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) {
                movement.y -= 1f;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) {
                movement.x += 1f;
            }
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) {
                movement.x -= 1f;
            }
            return Vector2.ClampMagnitude(movement, 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
#else
            return Vector2.zero;
#endif
        }

        private bool IsSprinting()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#else
            return false;
#endif
        }

        private Vector3 GetCameraRelativeDirection(Vector2 movement)
        {
            var forward = Vector3.forward;
            var right = Vector3.right;
            if (m_Camera != null) {
                forward = m_Camera.transform.forward;
                right = m_Camera.transform.right;
            }

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            return (forward * movement.y + right * movement.x).normalized;
        }
    }
}
