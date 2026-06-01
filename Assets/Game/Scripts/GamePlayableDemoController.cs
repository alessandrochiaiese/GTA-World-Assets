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
        [SerializeField] private string[] m_DemoDnaNames = { "height", "headSize", "belly", "upperMuscle", "lowerMuscle" };
        [SerializeField] private bool m_ShowHelp = true;

        private GameObject m_CurrentWeaponPreview;
        public GameAvatarIntegration Avatar { get { return m_Avatar; } set { m_Avatar = value; } }
        public GameWeaponMounts WeaponMounts { get { return m_WeaponMounts; } set { m_WeaponMounts = value; } }
        public GameOsmMapAnchor MapAnchor { get { return m_MapAnchor; } set { m_MapAnchor = value; } }

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBind();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) {
                SetMale();
            }
            if (Input.GetKeyDown(KeyCode.F2)) {
                SetFemale();
            }
            if (Input.GetKeyDown(KeyCode.F3)) {
                RandomizeDemoDna();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                EquipPreviewWeapon(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                EquipPreviewWeapon(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                EquipPreviewWeapon(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                EquipPreviewWeapon(3);
            }
            if (Input.GetKeyDown(KeyCode.H)) {
                m_ShowHelp = !m_ShowHelp;
            }
        }

        private void OnGUI()
        {
            if (!m_ShowHelp) {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 420f, 210f), "GTA World Demo", GUI.skin.window);
            GUILayout.Label("F1 Male | F2 Female | F3 Random DNA | 1-4 Preview weapons | H Hide");
            GUILayout.Label("Move/play with the Opsive controller after tuning input and item types.");
            GUILayout.Label("Place generated OSM content under World_Map_Setup/OSM_Map_Root.");

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
        }

        public void SetMale()
        {
            AutoBind();
            if (m_Avatar != null) {
                m_Avatar.SetMale();
            }
        }

        public void SetFemale()
        {
            AutoBind();
            if (m_Avatar != null) {
                m_Avatar.SetFemale();
            }
        }

        public void RandomizeDemoDna()
        {
            AutoBind();
            if (m_Avatar == null || m_DemoDnaNames == null) {
                return;
            }

            for (int i = 0; i < m_DemoDnaNames.Length; i++) {
                if (!string.IsNullOrEmpty(m_DemoDnaNames[i])) {
                    m_Avatar.SetDna(m_DemoDnaNames[i], Random.Range(0.2f, 0.85f), false);
                }
            }
            m_Avatar.ForceUmaUpdate(true, false, true);
        }

        public void EquipPreviewWeapon(int weaponIndex)
        {
            AutoBind();
            if (m_WeaponMounts == null) {
                return;
            }

            if (m_CurrentWeaponPreview != null) {
                Destroy(m_CurrentWeaponPreview);
            }

            m_CurrentWeaponPreview = CreatePreviewWeapon(weaponIndex);
            var mount = weaponIndex == 3 ? GameWeaponMounts.WeaponMount.Back : GameWeaponMounts.WeaponMount.RightHand;
            m_WeaponMounts.AttachWeapon(m_CurrentWeaponPreview, mount, true);
        }

        private GameObject CreatePreviewWeapon(int weaponIndex)
        {
            GameObject weapon;
            switch (weaponIndex) {
                case 1:
                    weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    weapon.name = "Preview_Assault_Rifle";
                    weapon.transform.localScale = new Vector3(0.12f, 0.12f, 0.9f);
                    break;
                case 2:
                    weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    weapon.name = "Preview_Melee_Weapon";
                    weapon.transform.localScale = new Vector3(0.06f, 0.06f, 0.8f);
                    break;
                case 3:
                    weapon = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    weapon.name = "Preview_Back_Weapon";
                    weapon.transform.localScale = new Vector3(0.12f, 0.45f, 0.12f);
                    break;
                default:
                    weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    weapon.name = "Preview_Pistol";
                    weapon.transform.localScale = new Vector3(0.12f, 0.08f, 0.28f);
                    break;
            }
            weapon.transform.localRotation = Quaternion.identity;
            return weapon;
        }
    }
}
