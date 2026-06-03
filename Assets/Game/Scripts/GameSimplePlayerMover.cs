using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GTAWorld.Game
{
    /// <summary>
    /// Input-System friendly fallback mover for the generated demo scene. It keeps the prototype playable while
    /// the full Opsive input/item pipeline is configured. It also drives the Opsive demo animator parameters so
    /// the avatar does not slide around in the idle pose while the real Opsive controller is being tuned.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameSimplePlayerMover : MonoBehaviour
    {
        [SerializeField] private float m_MoveSpeed = 4.5f;
        [SerializeField] private float m_SprintMultiplier = 1.7f;
        [SerializeField] private float m_RotationSpeed = 12f;
        [SerializeField] private Camera m_Camera;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private float m_AnimatorDampTime = 0.08f;

        private CharacterController m_CharacterController;
        private Rigidbody m_Rigidbody;
        private RuntimeAnimatorController m_CachedAnimatorController;
        private bool m_HasHorizontalInput;
        private bool m_HasForwardInput;
        private bool m_HasYaw;

        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Rigidbody = GetComponent<Rigidbody>();
            if (m_Camera == null) {
                m_Camera = Camera.main;
            }
            EnsureAnimator();
            CacheAnimatorParameters();
        }

        private void OnEnable()
        {
            CacheAnimatorParameters();
        }

        private void Update()
        {
            EnsureAnimator();
            var movement = ReadMovement();
            UpdateAnimator(movement);
            if (movement.sqrMagnitude < 0.001f) {
                return;
            }

            var moveDirection = GetCameraRelativeDirection(movement);
            var speed = m_MoveSpeed * (IsSprinting() ? m_SprintMultiplier : 1f);
            var delta = moveDirection * speed * Time.deltaTime;

            if (m_CharacterController != null && m_CharacterController.enabled) {
                m_CharacterController.Move(delta);
            } else if (m_Rigidbody != null && !m_Rigidbody.isKinematic) {
                m_Rigidbody.MovePosition(m_Rigidbody.position + delta);
            } else {
                transform.position += delta;
            }

            var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            var rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSpeed);
            if (m_Rigidbody != null && !m_Rigidbody.isKinematic) {
                m_Rigidbody.MoveRotation(rotation);
            } else {
                transform.rotation = rotation;
            }
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

        private void EnsureAnimator()
        {
            if (m_Animator == null) {
                m_Animator = GetComponentInChildren<Animator>();
                m_CachedAnimatorController = null;
            }
            if (m_Animator == null) {
                return;
            }
            m_Animator.applyRootMotion = false;
            m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (m_CachedAnimatorController != m_Animator.runtimeAnimatorController) {
                CacheAnimatorParameters();
            }
        }

        private void CacheAnimatorParameters()
        {
            if (m_Animator == null) {
                m_Animator = GetComponentInChildren<Animator>();
            }
            m_CachedAnimatorController = m_Animator != null ? m_Animator.runtimeAnimatorController : null;
            m_HasHorizontalInput = HasAnimatorParameter("Horizontal Input", AnimatorControllerParameterType.Float);
            m_HasForwardInput = HasAnimatorParameter("Forward Input", AnimatorControllerParameterType.Float);
            m_HasYaw = HasAnimatorParameter("Yaw", AnimatorControllerParameterType.Float);
        }

        private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
        {
            if (m_Animator == null || m_Animator.runtimeAnimatorController == null) {
                return false;
            }

            var parameters = m_Animator.parameters;
            for (int i = 0; i < parameters.Length; i++) {
                if (parameters[i].name == parameterName && parameters[i].type == parameterType) {
                    return true;
                }
            }
            return false;
        }

        private void UpdateAnimator(Vector2 movement)
        {
            if (m_Animator == null || m_Animator.runtimeAnimatorController == null) {
                EnsureAnimator();
            }
            if (m_Animator == null || m_Animator.runtimeAnimatorController == null) {
                return;
            }

            if (!m_HasHorizontalInput && !m_HasForwardInput && !m_HasYaw) {
                CacheAnimatorParameters();
            }

            var dampTime = m_AnimatorDampTime;
            if (m_HasHorizontalInput) {
                m_Animator.SetFloat("Horizontal Input", movement.x, dampTime, Time.deltaTime);
            }
            if (m_HasForwardInput) {
                m_Animator.SetFloat("Forward Input", movement.y, dampTime, Time.deltaTime);
            }
            if (m_HasYaw) {
                m_Animator.SetFloat("Yaw", movement.x, dampTime, Time.deltaTime);
            }
        }
    }
}
