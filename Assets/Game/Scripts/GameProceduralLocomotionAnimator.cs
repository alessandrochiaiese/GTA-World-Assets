using UnityEngine;

namespace GTAWorld.Game
{
    /// <summary>
    /// Last-resort visual locomotion layer for the generated demo. If the legacy Opsive animator/controller does
    /// not transition out of idle on the local Unity version, this script still makes the UMA body visibly walk by
    /// adding a small procedural swing to humanoid bones in LateUpdate.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameProceduralLocomotionAnimator : MonoBehaviour
    {
        [SerializeField] private Animator m_Animator;
        [SerializeField] private float m_MinimumMoveSpeed = 0.05f;
        [SerializeField] private float m_WalkCycleSpeed = 7.5f;
        [SerializeField] private float m_ArmSwing = 24f;
        [SerializeField] private float m_LegSwing = 18f;
        [SerializeField] private float m_BodyBob = 0.035f;
        [SerializeField] private bool m_EnableWhenAnimatorHasController = true;

        private Transform m_LeftUpperArm;
        private Transform m_RightUpperArm;
        private Transform m_LeftUpperLeg;
        private Transform m_RightUpperLeg;
        private Transform m_Spine;
        private Quaternion m_LeftUpperArmBase;
        private Quaternion m_RightUpperArmBase;
        private Quaternion m_LeftUpperLegBase;
        private Quaternion m_RightUpperLegBase;
        private Quaternion m_SpineBase;
        private Vector3 m_LastPosition;
        private float m_Cycle;

        private void Awake()
        {
            RebindBones();
            m_LastPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (m_Animator == null || !m_Animator.isHuman) {
                RebindBones();
            }
            if (m_Animator == null || !m_Animator.isHuman) {
                return;
            }
            if (!m_EnableWhenAnimatorHasController && m_Animator.runtimeAnimatorController != null) {
                return;
            }

            var velocity = (transform.position - m_LastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            m_LastPosition = transform.position;
            velocity.y = 0f;
            var movement = Mathf.Clamp01(velocity.magnitude / 4.5f);
            if (movement < m_MinimumMoveSpeed) {
                ResetProceduralPose();
                return;
            }

            m_Cycle += Time.deltaTime * m_WalkCycleSpeed * Mathf.Lerp(0.65f, 1.35f, movement);
            var swing = Mathf.Sin(m_Cycle);
            var oppositeSwing = -swing;
            ApplyLocalX(m_LeftUpperArm, m_LeftUpperArmBase, oppositeSwing * m_ArmSwing * movement);
            ApplyLocalX(m_RightUpperArm, m_RightUpperArmBase, swing * m_ArmSwing * movement);
            ApplyLocalX(m_LeftUpperLeg, m_LeftUpperLegBase, swing * m_LegSwing * movement);
            ApplyLocalX(m_RightUpperLeg, m_RightUpperLegBase, oppositeSwing * m_LegSwing * movement);
            ApplyLocalZ(m_Spine, m_SpineBase, Mathf.Sin(m_Cycle * 2f) * m_BodyBob * 100f * movement);
        }

        [ContextMenu("Rebind Procedural Bones")]
        public void RebindBones()
        {
            if (m_Animator == null) {
                m_Animator = GetComponentInChildren<Animator>();
            }
            if (m_Animator == null || !m_Animator.isHuman) {
                return;
            }

            m_LeftUpperArm = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            m_RightUpperArm = m_Animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            m_LeftUpperLeg = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            m_RightUpperLeg = m_Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            m_Spine = m_Animator.GetBoneTransform(HumanBodyBones.Spine);

            m_LeftUpperArmBase = GetLocalRotation(m_LeftUpperArm);
            m_RightUpperArmBase = GetLocalRotation(m_RightUpperArm);
            m_LeftUpperLegBase = GetLocalRotation(m_LeftUpperLeg);
            m_RightUpperLegBase = GetLocalRotation(m_RightUpperLeg);
            m_SpineBase = GetLocalRotation(m_Spine);
        }

        private void ResetProceduralPose()
        {
            SetLocalRotation(m_LeftUpperArm, m_LeftUpperArmBase);
            SetLocalRotation(m_RightUpperArm, m_RightUpperArmBase);
            SetLocalRotation(m_LeftUpperLeg, m_LeftUpperLegBase);
            SetLocalRotation(m_RightUpperLeg, m_RightUpperLegBase);
            SetLocalRotation(m_Spine, m_SpineBase);
        }

        private static void SetLocalRotation(Transform bone, Quaternion rotation)
        {
            if (bone != null) {
                bone.localRotation = rotation;
            }
        }

        private static Quaternion GetLocalRotation(Transform bone)
        {
            return bone != null ? bone.localRotation : Quaternion.identity;
        }

        private static void ApplyLocalX(Transform bone, Quaternion baseRotation, float angle)
        {
            if (bone != null) {
                bone.localRotation = baseRotation * Quaternion.Euler(angle, 0f, 0f);
            }
        }

        private static void ApplyLocalZ(Transform bone, Quaternion baseRotation, float angle)
        {
            if (bone != null) {
                bone.localRotation = baseRotation * Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}
