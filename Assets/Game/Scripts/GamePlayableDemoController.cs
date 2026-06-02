using UnityEngine;

namespace GTAWorld.Game
{
    /// <summary>
    /// Small runtime demo controller for the one-click scene. It exposes keyboard shortcuts and an on-screen
    /// panel for UMA race/DNA changes and simple weapon mount previews while the final Opsive item setup is tuned.
    /// </summary>
    [DisallowMultipleComponent]
    public class GamePlayableDemoController : MonoBehaviour
    {
        [SerializeField] private GameAvatarIntegration m_Avatar;
        [SerializeField] private GameWeaponMounts m_WeaponMounts;
        [SerializeField] private GameOsmMapAnchor m_MapAnchor;
        [SerializeField] private GameOpsiveRuntimeBridge m_OpsiveRuntimeBridge;
        [SerializeField] private string[] m_DemoDnaNames = { "height", "headSize", "belly", "upperMuscle", "lowerMuscle" };
        [SerializeField] private bool m_ShowHelp = true;
        [SerializeField] private GameObject[] m_WeaponPreviewPrefabs;

        private GameObject m_CurrentWeaponPreview;
        private string m_StatusMessage = "Ready";
        public GameAvatarIntegration Avatar { get { return m_Avatar; } set { m_Avatar = value; } }
        public GameWeaponMounts WeaponMounts { get { return m_WeaponMounts; } set { m_WeaponMounts = value; } }
        public GameOsmMapAnchor MapAnchor { get { return m_MapAnchor; } set { m_MapAnchor = value; } }
        public GameOpsiveRuntimeBridge OpsiveRuntimeBridge { get { return m_OpsiveRuntimeBridge; } set { m_OpsiveRuntimeBridge = value; } }

        public void SetWeaponPreviewPrefabs(GameObject[] weaponPreviewPrefabs)
        {
            m_WeaponPreviewPrefabs = weaponPreviewPrefabs;
        }

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBind();
        }

        private void OnGUI()
        {
            HandleGuiKeyboardShortcuts();
            if (!m_ShowHelp) {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 420f, 210f), "GTA World Demo", GUI.skin.window);
            GUILayout.Label("F1 Male | F2 Female | F3 Random DNA | 1-4 Preview weapons | H Hide");
            GUILayout.Label("WASD moves the demo avatar; UMA must finish building before the mesh appears.");
            GUILayout.Label("Place generated OSM content under World_Map_Setup/OSM_Map_Root.");
            GUILayout.Label("Status: " + m_StatusMessage);
            if (m_OpsiveRuntimeBridge != null) {
                GUILayout.Label("Opsive: " + m_OpsiveRuntimeBridge.StatusMessage);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Male")) {
                SetMale();
            }
            if (GUILayout.Button("Female")) {
                SetFemale();
            }
            if (GUILayout.Button("Random DNA")) {
                RandomizeDemoDna();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pistol")) {
                EquipPreviewWeapon(0);
            }
            if (GUILayout.Button("Rifle")) {
                EquipPreviewWeapon(1);
            }
            if (GUILayout.Button("Melee")) {
                EquipPreviewWeapon(2);
            }
            if (GUILayout.Button("Back")) {
                EquipPreviewWeapon(3);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void HandleGuiKeyboardShortcuts()
        {
            var currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type != EventType.KeyDown) {
                return;
            }

            if (currentEvent.keyCode == KeyCode.F1) {
                SetMale();
                currentEvent.Use();
            } else if (currentEvent.keyCode == KeyCode.F2) {
                SetFemale();
                currentEvent.Use();
            } else if (currentEvent.keyCode == KeyCode.F3) {
                RandomizeDemoDna();
                currentEvent.Use();
            } else if (currentEvent.keyCode == KeyCode.Alpha1) {
                EquipPreviewWeapon(0);
                currentEvent.Use();
            } else if (currentEvent.keyCode == KeyCode.Alpha2) {
                EquipPreviewWeapon(1);
                currentEvent.Use();
            } else if (currentEvent.keyCode == KeyCode.Alpha3) {
                EquipPreviewWeapon(2);
                currentEvent.Use();
            } else if (currentEvent.keyCode == KeyCode.Alpha4) {
                EquipPreviewWeapon(3);
                currentEvent.Use();
            } else if (currentEvent.keyCode == KeyCode.H) {
                m_ShowHelp = !m_ShowHelp;
                currentEvent.Use();
            }
        }

        [ContextMenu("Auto Bind Demo References")]
        public void AutoBind()
        {
            if (m_Avatar == null) {
                m_Avatar = GameObject.FindObjectOfType<GameAvatarIntegration>();
            }
            if (m_WeaponMounts == null && m_Avatar != null) {
                m_WeaponMounts = m_Avatar.GetComponent<GameWeaponMounts>();
            }
            if (m_MapAnchor == null) {
                m_MapAnchor = GameObject.FindObjectOfType<GameOsmMapAnchor>();
            }
            if (m_OpsiveRuntimeBridge == null && m_Avatar != null) {
                m_OpsiveRuntimeBridge = m_Avatar.GetComponent<GameOpsiveRuntimeBridge>();
            }
        }

        public void SetMale()
        {
            AutoBind();
            if (m_Avatar != null) {
                m_Avatar.SetMale();
                m_StatusMessage = "Male UMA race requested";
            } else {
                m_StatusMessage = "Avatar reference missing";
            }
        }

        public void SetFemale()
        {
            AutoBind();
            if (m_Avatar != null) {
                m_Avatar.SetFemale();
                m_StatusMessage = "Female UMA race requested";
            } else {
                m_StatusMessage = "Avatar reference missing";
            }
        }

        public void RandomizeDemoDna()
        {
            AutoBind();
            if (m_Avatar == null || m_DemoDnaNames == null) {
                m_StatusMessage = "Avatar/DNA reference missing";
                return;
            }

            for (int i = 0; i < m_DemoDnaNames.Length; i++) {
                if (!string.IsNullOrEmpty(m_DemoDnaNames[i])) {
                    m_Avatar.SetDna(m_DemoDnaNames[i], Random.Range(0.2f, 0.85f), false);
                }
            }
            m_Avatar.ForceUmaUpdate(true, false, true);
            m_StatusMessage = "Random UMA DNA requested";
        }

        public void EquipPreviewWeapon(int weaponIndex)
        {
            AutoBind();
            if (m_OpsiveRuntimeBridge != null) {
                m_OpsiveRuntimeBridge.EquipSlot(weaponIndex);
            }
            if (m_WeaponMounts == null) {
                m_StatusMessage = "Weapon mounts missing";
                return;
            }

            if (m_WeaponPreviewPrefabs == null || weaponIndex < 0 || weaponIndex >= m_WeaponPreviewPrefabs.Length || m_WeaponPreviewPrefabs[weaponIndex] == null) {
                m_StatusMessage = "No real weapon prefab assigned for this slot yet";
                return;
            }

            if (m_CurrentWeaponPreview != null) {
                Destroy(m_CurrentWeaponPreview);
            }

            m_CurrentWeaponPreview = Instantiate(m_WeaponPreviewPrefabs[weaponIndex]);
            m_CurrentWeaponPreview.name = m_WeaponPreviewPrefabs[weaponIndex].name + "_Preview";
            PreparePreviewWeapon(m_CurrentWeaponPreview);
            var mount = weaponIndex == 3 ? GameWeaponMounts.WeaponMount.Back : GameWeaponMounts.WeaponMount.RightHand;
            m_WeaponMounts.AttachWeapon(m_CurrentWeaponPreview, mount, true);
            NormalizePreviewWeapon(m_CurrentWeaponPreview);
            m_StatusMessage = m_CurrentWeaponPreview.name + " attached to " + mount;
        }

        private static void NormalizePreviewWeapon(GameObject weapon)
        {
            if (weapon == null) {
                return;
            }

            var renderers = weapon.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) {
                return;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var parent = weapon.transform.parent;
            if (parent == null) {
                return;
            }

            var centerOffset = parent.InverseTransformVector(bounds.center - parent.position);
            weapon.transform.localPosition -= centerOffset;
            weapon.transform.localScale = Vector3.one * Mathf.Min(1f, 1.2f / Mathf.Max(bounds.size.magnitude, 0.01f));
        }

        private static void PreparePreviewWeapon(GameObject weapon)
        {
            if (weapon == null) {
                return;
            }

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
    }
}
