using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GTAWorld.Game
{
    /// <summary>
    /// Playable weapon fallback for the generated demo. It gives the UMA avatar one reliable weapon loop
    /// (equip, hold in hand, aim/fire/reload feedback) while the full Opsive ItemSet/IK pipeline is tuned.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameFallbackWeaponController : MonoBehaviour
    {
        [SerializeField] private GameWeaponMounts m_WeaponMounts;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private Camera m_Camera;
        [SerializeField] private GameObject[] m_WeaponPrefabs;
        [SerializeField] private int m_DefaultWeaponIndex;
        [SerializeField] private float m_FireRate = 0.18f;
        [SerializeField] private int m_MagazineSize = 12;
        [SerializeField] private float m_ReloadTime = 0.9f;
        [SerializeField] private float m_Range = 80f;
        [SerializeField] private Vector3 m_RightHandPositionOffset = new Vector3(0.02f, 0.01f, 0.03f);
        [SerializeField] private Vector3 m_RightHandEulerOffset = new Vector3(0f, 90f, 0f);
        [SerializeField] private bool m_EquipDefaultOnStart = true;
        [SerializeField] private bool m_HandleInput = true;

        private GameObject m_CurrentWeapon;
        private Transform m_Muzzle;
        private int m_CurrentWeaponIndex = -1;
        private int m_AmmoInMagazine;
        private float m_NextFireTime;
        private bool m_Reloading;
        private string m_StatusMessage = "Weapon ready";
        private bool m_HasState;
        private bool m_HasIntData;
        private bool m_HasFloatData;

        public string StatusMessage { get { return m_StatusMessage; } }
        public int CurrentWeaponIndex { get { return m_CurrentWeaponIndex; } }
        public int AmmoInMagazine { get { return m_AmmoInMagazine; } }
        public int MagazineSize { get { return m_MagazineSize; } }

        public void SetWeaponPrefabs(GameObject[] weaponPrefabs)
        {
            m_WeaponPrefabs = weaponPrefabs;
        }

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBind();
            m_AmmoInMagazine = m_MagazineSize;
        }

        private void Start()
        {
            if (m_EquipDefaultOnStart) {
                EquipWeapon(m_DefaultWeaponIndex);
            }
        }

        private void Update()
        {
            AutoBind();
            if (!m_HandleInput) {
                return;
            }

            var slot = ReadEquipSlot();
            if (slot >= 0) {
                EquipWeapon(slot);
            }
            if (FirePressed()) {
                Fire();
            }
            if (ReloadPressed()) {
                StartReload();
            }
        }

        [ContextMenu("Auto Bind Weapon References")]
        public void AutoBind()
        {
            if (m_WeaponMounts == null) {
                m_WeaponMounts = GetComponent<GameWeaponMounts>();
            }
            if (m_Animator == null) {
                m_Animator = GetComponentInChildren<Animator>();
            }
            if (m_Camera == null) {
                m_Camera = Camera.main;
            }
            CacheAnimatorParameters();
        }

        public bool EquipWeapon(int weaponIndex)
        {
            if (m_WeaponPrefabs == null || weaponIndex < 0 || weaponIndex >= m_WeaponPrefabs.Length || m_WeaponPrefabs[weaponIndex] == null) {
                SetStatus("No playable weapon prefab in slot " + (weaponIndex + 1));
                return false;
            }
            if (m_WeaponMounts == null) {
                SetStatus("Weapon mounts missing");
                return false;
            }

            if (m_CurrentWeapon != null) {
                Destroy(m_CurrentWeapon);
            }

            m_CurrentWeaponIndex = weaponIndex;
            m_CurrentWeapon = CreateVisualWeaponClone(m_WeaponPrefabs[weaponIndex]);
            m_CurrentWeapon.name = m_WeaponPrefabs[weaponIndex].name + "_Playable";
            PrepareHeldWeapon(m_CurrentWeapon);
            var mount = weaponIndex == 3 ? GameWeaponMounts.WeaponMount.Back : GameWeaponMounts.WeaponMount.RightHand;
            m_WeaponMounts.AttachWeapon(m_CurrentWeapon, mount, true);
            ApplyHeldWeaponPose(m_CurrentWeapon, mount);
            m_Muzzle = EnsureMuzzle(m_CurrentWeapon.transform);
            m_AmmoInMagazine = m_MagazineSize;
            UpdateAnimatorForEquip(weaponIndex);
            SetStatus("Equipped " + m_CurrentWeapon.name + " (" + m_AmmoInMagazine + "/" + m_MagazineSize + ")");
            return true;
        }

        public void Fire()
        {
            if (m_Reloading || Time.time < m_NextFireTime) {
                return;
            }
            if (m_CurrentWeapon == null && !EquipWeapon(m_DefaultWeaponIndex)) {
                return;
            }
            if (m_AmmoInMagazine <= 0) {
                StartReload();
                return;
            }

            m_NextFireTime = Time.time + m_FireRate;
            m_AmmoInMagazine--;
            var origin = m_Camera != null ? m_Camera.transform.position : m_Muzzle.position;
            var direction = m_Camera != null ? m_Camera.transform.forward : transform.forward;
            var endPoint = origin + direction * m_Range;
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, m_Range, ~0, QueryTriggerInteraction.Ignore)) {
                endPoint = hit.point;
            }

            StartCoroutine(ShowTracer(m_Muzzle != null ? m_Muzzle.position : origin, endPoint));
            UpdateAnimatorForFire();
            SetStatus("Fired " + (m_CurrentWeaponIndex + 1) + " (" + m_AmmoInMagazine + "/" + m_MagazineSize + ")");
        }

        public void StartReload()
        {
            if (m_Reloading || m_AmmoInMagazine == m_MagazineSize) {
                return;
            }
            StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            m_Reloading = true;
            SetStatus("Reloading...");
            UpdateAnimatorForReload();
            yield return new WaitForSeconds(m_ReloadTime);
            m_AmmoInMagazine = m_MagazineSize;
            m_Reloading = false;
            SetStatus("Reloaded (" + m_AmmoInMagazine + "/" + m_MagazineSize + ")");
        }

        private IEnumerator ShowTracer(Vector3 start, Vector3 end)
        {
            var tracerObject = new GameObject("Weapon_Tracer");
            var line = tracerObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = 0.025f;
            line.endWidth = 0.005f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.yellow;
            line.endColor = new Color(1f, 0.4f, 0f, 0f);
            yield return new WaitForSeconds(0.06f);
            Destroy(tracerObject);
        }

        private static GameObject CreateVisualWeaponClone(GameObject source)
        {
            if (source == null) {
                return GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            var clone = new GameObject(source.name + "_VisualOnly");
            CopyVisualChildren(source.transform, clone.transform);
            if (clone.GetComponentsInChildren<Renderer>(true).Length == 0) {
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fallback.name = "Fallback_Weapon_Mesh";
                fallback.transform.SetParent(clone.transform, false);
                fallback.transform.localScale = new Vector3(0.12f, 0.08f, 0.75f);
            }
            return clone;
        }

        private static void CopyVisualChildren(Transform source, Transform target)
        {
            CopyVisualComponents(source, target.gameObject);
            for (int i = 0; i < source.childCount; ++i) {
                var sourceChild = source.GetChild(i);
                var targetChild = new GameObject(sourceChild.name);
                targetChild.transform.SetParent(target, false);
                targetChild.transform.localPosition = sourceChild.localPosition;
                targetChild.transform.localRotation = sourceChild.localRotation;
                targetChild.transform.localScale = sourceChild.localScale;
                CopyVisualChildren(sourceChild, targetChild.transform);
            }
        }

        private static void CopyVisualComponents(Transform source, GameObject target)
        {
            var meshFilter = source.GetComponent<MeshFilter>();
            var meshRenderer = source.GetComponent<MeshRenderer>();
            if (meshFilter != null && meshRenderer != null) {
                var targetFilter = target.AddComponent<MeshFilter>();
                targetFilter.sharedMesh = meshFilter.sharedMesh;
                var targetRenderer = target.AddComponent<MeshRenderer>();
                targetRenderer.sharedMaterials = meshRenderer.sharedMaterials;
            }

            var skinnedRenderer = source.GetComponent<SkinnedMeshRenderer>();
            if (skinnedRenderer != null) {
                var targetRenderer = target.AddComponent<SkinnedMeshRenderer>();
                targetRenderer.sharedMesh = skinnedRenderer.sharedMesh;
                targetRenderer.sharedMaterials = skinnedRenderer.sharedMaterials;
                targetRenderer.localBounds = skinnedRenderer.localBounds;
            }
        }

        private void PrepareHeldWeapon(GameObject weapon)
        {
            var behaviours = weapon.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++) {
                if (behaviours[i] != null) {
                    behaviours[i].enabled = false;
                }
            }
            var colliders = weapon.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) {
                colliders[i].enabled = false;
            }
            var rigidbodies = weapon.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < rigidbodies.Length; i++) {
                rigidbodies[i].isKinematic = true;
                rigidbodies[i].useGravity = false;
            }
        }

        private void ApplyHeldWeaponPose(GameObject weapon, GameWeaponMounts.WeaponMount mount)
        {
            if (weapon == null) {
                return;
            }
            if (mount == GameWeaponMounts.WeaponMount.RightHand) {
                weapon.transform.localPosition = m_RightHandPositionOffset;
                weapon.transform.localRotation = Quaternion.Euler(m_RightHandEulerOffset);
            } else {
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
            }
            weapon.transform.localScale = Vector3.one;
        }

        private static Transform EnsureMuzzle(Transform weapon)
        {
            var existing = weapon.Find("Game_Muzzle");
            if (existing != null) {
                return existing;
            }
            var muzzle = new GameObject("Game_Muzzle").transform;
            muzzle.SetParent(weapon, false);
            muzzle.localPosition = new Vector3(0f, 0f, 0.45f);
            muzzle.localRotation = Quaternion.identity;
            return muzzle;
        }

        private void CacheAnimatorParameters()
        {
            if (m_Animator == null || m_Animator.runtimeAnimatorController == null) {
                return;
            }
            var parameters = m_Animator.parameters;
            m_HasState = HasAnimatorParameter(parameters, "State", AnimatorControllerParameterType.Int);
            m_HasIntData = HasAnimatorParameter(parameters, "Int Data", AnimatorControllerParameterType.Int);
            m_HasFloatData = HasAnimatorParameter(parameters, "Float Data", AnimatorControllerParameterType.Float);
        }

        private static bool HasAnimatorParameter(AnimatorControllerParameter[] parameters, string parameterName, AnimatorControllerParameterType parameterType)
        {
            for (int i = 0; i < parameters.Length; i++) {
                if (parameters[i].name == parameterName && parameters[i].type == parameterType) {
                    return true;
                }
            }
            return false;
        }

        private void UpdateAnimatorForEquip(int slot)
        {
            if (m_Animator == null) {
                return;
            }
            if (m_HasIntData) {
                m_Animator.SetInteger("Int Data", slot + 1);
            }
            if (m_HasFloatData) {
                m_Animator.SetFloat("Float Data", 1f);
            }
        }

        private void UpdateAnimatorForFire()
        {
            if (m_Animator == null) {
                return;
            }
            if (m_HasState) {
                m_Animator.SetInteger("State", 2);
            }
            if (m_HasFloatData) {
                m_Animator.SetFloat("Float Data", 1f);
            }
        }

        private void UpdateAnimatorForReload()
        {
            if (m_Animator == null) {
                return;
            }
            if (m_HasState) {
                m_Animator.SetInteger("State", 3);
            }
        }

        private void SetStatus(string status)
        {
            m_StatusMessage = status;
        }

        private static int ReadEquipSlot()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return -1;
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) return 0;
            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) return 1;
            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) return 2;
            if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame) return 3;
            return -1;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 0;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 2;
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 3;
            return -1;
#else
            return -1;
#endif
        }

        private static bool FirePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        private static bool ReloadPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.rKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.R);
#else
            return false;
#endif
        }
    }
}
