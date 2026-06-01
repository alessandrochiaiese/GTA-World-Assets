using UnityEngine;

namespace GTAWorld.Game
{
    /// <summary>
    /// Creates stable hand/back attachment points for weapons and IK targets on UMA/Opsive avatars.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameWeaponMounts : MonoBehaviour
    {
        public enum WeaponMount
        {
            RightHand,
            LeftHand,
            Back,
            Hip
        }

        [SerializeField] private Animator m_Animator;
        [SerializeField] private Transform m_RightHandMount;
        [SerializeField] private Transform m_LeftHandMount;
        [SerializeField] private Transform m_BackMount;
        [SerializeField] private Transform m_HipMount;

        public Animator Animator { get { return m_Animator; } set { m_Animator = value; } }
        public Transform RightHandMount { get { return m_RightHandMount; } }
        public Transform LeftHandMount { get { return m_LeftHandMount; } }
        public Transform BackMount { get { return m_BackMount; } }
        public Transform HipMount { get { return m_HipMount; } }

        private void Reset()
        {
            AutoCreateMounts();
        }

        [ContextMenu("Auto Create Weapon Mounts")]
        public void AutoCreateMounts()
        {
            if (m_Animator == null) {
                m_Animator = GetComponentInChildren<Animator>();
            }

            m_RightHandMount = EnsureMount("RightHandWeaponMount", HumanBodyBones.RightHand, m_RightHandMount, new Vector3(0.03f, 0.02f, 0.08f), Quaternion.Euler(0f, 90f, 0f));
            m_LeftHandMount = EnsureMount("LeftHandWeaponMount", HumanBodyBones.LeftHand, m_LeftHandMount, new Vector3(-0.03f, 0.02f, 0.08f), Quaternion.Euler(0f, -90f, 0f));
            m_BackMount = EnsureMount("BackWeaponMount", HumanBodyBones.Chest, m_BackMount, new Vector3(0f, 0.08f, -0.18f), Quaternion.Euler(0f, 0f, 35f));
            m_HipMount = EnsureMount("HipWeaponMount", HumanBodyBones.Hips, m_HipMount, new Vector3(0.18f, -0.05f, 0.05f), Quaternion.Euler(0f, 0f, 90f));
        }

        public Transform GetMount(WeaponMount mount)
        {
            switch (mount) {
                case WeaponMount.LeftHand:
                    return m_LeftHandMount;
                case WeaponMount.Back:
                    return m_BackMount;
                case WeaponMount.Hip:
                    return m_HipMount;
                default:
                    return m_RightHandMount;
            }
        }

        public void AttachWeapon(GameObject weapon, WeaponMount mount, bool resetLocalTransform = true)
        {
            if (weapon == null) {
                return;
            }

            var targetMount = GetMount(mount);
            if (targetMount == null) {
                AutoCreateMounts();
                targetMount = GetMount(mount);
            }
            if (targetMount == null) {
                Debug.LogWarning("Unable to attach weapon because the requested mount does not exist.", this);
                return;
            }

            weapon.transform.SetParent(targetMount, false);
            if (resetLocalTransform) {
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
                weapon.transform.localScale = Vector3.one;
            }
        }

        private Transform EnsureMount(string mountName, HumanBodyBones bone, Transform current, Vector3 localPosition, Quaternion localRotation)
        {
            if (current != null) {
                return current;
            }

            var parent = transform;
            if (m_Animator != null && m_Animator.isHuman) {
                var boneTransform = m_Animator.GetBoneTransform(bone);
                if (boneTransform != null) {
                    parent = boneTransform;
                }
            }

            var existing = parent.Find(mountName);
            if (existing != null) {
                return existing;
            }

            var mountObject = new GameObject(mountName);
            var mountTransform = mountObject.transform;
            mountTransform.SetParent(parent, false);
            mountTransform.localPosition = localPosition;
            mountTransform.localRotation = localRotation;
            mountTransform.localScale = Vector3.one;
            return mountTransform;
        }
    }
}
