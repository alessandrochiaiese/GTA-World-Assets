using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GTAWorld.Game
{
    /// <summary>
    /// Project-level bridge for a UMA DynamicCharacterAvatar that will also be driven by Opsive's Third Person Controller.
    /// The class intentionally uses reflection for UMA calls so this folder remains a safe integration layer while assets are upgraded.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameAvatarIntegration : MonoBehaviour
    {
        [Serializable]
        public class DnaValue
        {
            public string Name;
            [Range(0f, 1f)] public float Value = 0.5f;
            public bool RebuildImmediately;
        }

        [Serializable]
        public class WardrobeEntry
        {
            public string Slot;
            public string Recipe;
            public bool ClearSlotFirst = true;
        }

        [Header("UMA")]
        [Tooltip("DynamicCharacterAvatar component on this GameObject or a child. Auto-filled if empty.")]
        [SerializeField] private Component m_UmaAvatar;
        [SerializeField] private string m_MaleRace = "HumanMale";
        [SerializeField] private string m_FemaleRace = "HumanFemale";
        [SerializeField] private List<DnaValue> m_StartupDna = new List<DnaValue>();
        [SerializeField] private List<WardrobeEntry> m_StartupWardrobe = new List<WardrobeEntry>();

        [Header("Opsive")]
        [Tooltip("Animator used by the Third Person Controller. Auto-filled if empty.")]
        [SerializeField] private Animator m_Animator;
        [Tooltip("Weapon/IK mount helper used by the project menus.")]
        [SerializeField] private GameWeaponMounts m_WeaponMounts;

        [Header("Startup")]
        [SerializeField] private bool m_AutoBindOnAwake = true;
        [SerializeField] private bool m_ApplyStartupCustomizationOnStart;

        public Component UmaAvatar { get { return m_UmaAvatar; } set { m_UmaAvatar = value; } }
        public Animator Animator { get { return m_Animator; } set { m_Animator = value; } }
        public GameWeaponMounts WeaponMounts { get { return m_WeaponMounts; } set { m_WeaponMounts = value; } }

        private void Awake()
        {
            if (m_AutoBindOnAwake) {
                AutoBind();
            }
        }

        private void Start()
        {
            if (m_ApplyStartupCustomizationOnStart) {
                ApplyStartupCustomization();
            }
        }

        private void Reset()
        {
            AutoBind();
        }

        [ContextMenu("Auto Bind UMA/Opsive References")]
        public void AutoBind()
        {
            if (m_Animator == null) {
                m_Animator = GetComponentInChildren<Animator>();
            }
            if (m_WeaponMounts == null) {
                m_WeaponMounts = GetComponent<GameWeaponMounts>();
            }
            if (m_UmaAvatar == null) {
                m_UmaAvatar = FindComponentByTypeName(gameObject, "UMA.CharacterSystem.DynamicCharacterAvatar");
            }
        }

        [ContextMenu("Apply Startup Customization")]
        public void ApplyStartupCustomization()
        {
            AutoBind();
            ApplyWardrobe(m_StartupWardrobe);
            ApplyDna(m_StartupDna);
            ForceUmaUpdate(true, true, true);
        }

        public void SetMale()
        {
            ChangeRace(m_MaleRace);
        }

        public void SetFemale()
        {
            ChangeRace(m_FemaleRace);
        }

        public bool ChangeRace(string raceName, bool force = true)
        {
            if (string.IsNullOrEmpty(raceName) || m_UmaAvatar == null) {
                return false;
            }

            // Prefer the exact UMA overload: ChangeRace(string, ChangeRaceOptions, bool) is not reflection-friendly because of the enum,
            // so use the available ChangeRace(string, bool) overload when present.
            if (InvokeUma("ChangeRace", new object[] { raceName, force })) {
                return true;
            }
            return InvokeUma("ChangeRace", new object[] { raceName });
        }

        public void ApplyWardrobe(IEnumerable<WardrobeEntry> entries)
        {
            if (entries == null || m_UmaAvatar == null) {
                return;
            }

            foreach (var entry in entries) {
                if (entry == null || string.IsNullOrEmpty(entry.Slot)) {
                    continue;
                }
                if (entry.ClearSlotFirst) {
                    ClearWardrobeSlot(entry.Slot);
                }
                if (!string.IsNullOrEmpty(entry.Recipe)) {
                    SetWardrobeSlot(entry.Slot, entry.Recipe);
                }
            }
        }

        public bool SetWardrobeSlot(string slot, string recipe)
        {
            if (string.IsNullOrEmpty(slot) || string.IsNullOrEmpty(recipe)) {
                return false;
            }
            return InvokeUma("SetSlot", new object[] { slot, recipe });
        }

        public bool ClearWardrobeSlot(string slot)
        {
            if (string.IsNullOrEmpty(slot)) {
                return false;
            }
            return InvokeUma("ClearSlot", new object[] { slot });
        }

        public void ApplyDna(IEnumerable<DnaValue> values)
        {
            if (values == null || m_UmaAvatar == null) {
                return;
            }

            foreach (var dna in values) {
                if (dna == null || string.IsNullOrEmpty(dna.Name)) {
                    continue;
                }
                SetDna(dna.Name, dna.Value, dna.RebuildImmediately);
            }
        }

        public bool SetDna(string dnaName, float value, bool rebuild = false)
        {
            if (string.IsNullOrEmpty(dnaName)) {
                return false;
            }
            return InvokeUma("SetDNA", new object[] { dnaName, Mathf.Clamp01(value), rebuild });
        }

        public bool ForceUmaUpdate(bool dnaDirty = true, bool textureDirty = true, bool meshDirty = true)
        {
            return InvokeUma("ForceUpdate", new object[] { dnaDirty, textureDirty, meshDirty });
        }

        private bool InvokeUma(string methodName, object[] arguments)
        {
            if (m_UmaAvatar == null) {
                return false;
            }

            var type = m_UmaAvatar.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++) {
                var method = methods[i];
                if (method.Name != methodName) {
                    continue;
                }
                var parameters = method.GetParameters();
                if (parameters.Length != arguments.Length) {
                    continue;
                }
                if (!ArgumentsMatch(parameters, arguments)) {
                    continue;
                }
                method.Invoke(m_UmaAvatar, arguments);
                return true;
            }

            Debug.LogWarning("UMA method not found: " + methodName + " on " + type.FullName, this);
            return false;
        }

        private static bool ArgumentsMatch(ParameterInfo[] parameters, object[] arguments)
        {
            for (int i = 0; i < parameters.Length; i++) {
                if (arguments[i] == null) {
                    continue;
                }
                var parameterType = parameters[i].ParameterType;
                if (!parameterType.IsInstanceOfType(arguments[i])) {
                    return false;
                }
            }
            return true;
        }

        private static Component FindComponentByTypeName(GameObject root, string typeName)
        {
            var type = FindType(typeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type)) {
                return null;
            }
            return root.GetComponentInChildren(type, true) as Component;
        }

        private static Type FindType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) {
                return type;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++) {
                type = assemblies[i].GetType(typeName);
                if (type != null) {
                    return type;
                }
            }
            return null;
        }
    }
}
