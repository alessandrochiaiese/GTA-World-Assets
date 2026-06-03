using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GTAWorld.Game
{
    /// <summary>
    /// Runtime glue for the generated demo's legacy Opsive setup. The class intentionally uses reflection so the
    /// project can compile while the exact Third Person Controller version is still being stabilized.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameOpsiveRuntimeBridge : MonoBehaviour
    {
        [SerializeField] private Component m_Inventory;
        [SerializeField] private Component m_InventoryHandler;
        [SerializeField] private Component m_ItemHandler;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private UnityEngine.Object[] m_DefaultItemTypes;
        [SerializeField] private bool m_LoadDefaultLoadoutOnStart;
        [SerializeField] private bool m_HandleDemoInput = true;
        [SerializeField] private bool m_LogBridgeEvents;

        private string m_StatusMessage = "Opsive bridge ready";
        private bool m_HasState;
        private bool m_HasIntData;
        private bool m_HasFloatData;

        public UnityEngine.Object[] DefaultItemTypes { get { return m_DefaultItemTypes; } }
        public string StatusMessage { get { return m_StatusMessage; } }

        public void SetDefaultItemTypes(UnityEngine.Object[] itemTypes)
        {
            m_DefaultItemTypes = itemTypes;
        }

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBind();
        }

        private void Start()
        {
            if (m_LoadDefaultLoadoutOnStart) {
                StartCoroutine(LoadDefaultLoadoutDelayed());
            }
        }

        private void Update()
        {
            if (!m_HandleDemoInput) {
                return;
            }

            var slot = ReadEquipSlot();
            if (slot >= 0) {
                EquipSlot(slot);
            }
            if (UsePressed()) {
                UseCurrentItem();
            }
            if (ReloadPressed()) {
                ReloadCurrentItem();
            }
        }

        [ContextMenu("Auto Bind Opsive Runtime References")]
        public void AutoBind()
        {
            if (m_Inventory == null) {
                m_Inventory = FindComponentByTypeNames("Opsive.ThirdPersonController.Inventory", "Opsive.ThirdPersonController.Wrappers.Inventory");
            }
            if (m_InventoryHandler == null) {
                m_InventoryHandler = FindComponentByTypeNames("Opsive.ThirdPersonController.InventoryHandler", "Opsive.ThirdPersonController.Wrappers.InventoryHandler");
            }
            if (m_ItemHandler == null) {
                m_ItemHandler = FindComponentByTypeNames("Opsive.ThirdPersonController.ItemHandler", "Opsive.ThirdPersonController.Wrappers.ItemHandler");
            }
            if (m_Animator == null) {
                m_Animator = GetComponentInChildren<Animator>();
            }
            CacheAnimatorParameters();
        }

        private IEnumerator LoadDefaultLoadoutDelayed()
        {
            LoadDefaultLoadout();
            yield return null;
            LoadDefaultLoadout();
        }

        [ContextMenu("Load Default Loadout")]
        public void LoadDefaultLoadout()
        {
            AutoBind();
            var loaded = InvokeAny(m_Inventory, new[] { "LoadDefaultLoadout" })
                      || InvokeAny(m_InventoryHandler, new[] { "LoadDefaultLoadout" });
            SetStatus(loaded ? "Opsive default loadout loaded" : "Opsive default loadout method not found");
        }

        public void EquipSlot(int slot)
        {
            AutoBind();
            if (m_DefaultItemTypes == null || slot < 0 || slot >= m_DefaultItemTypes.Length || m_DefaultItemTypes[slot] == null) {
                return;
            }

            var itemType = m_DefaultItemTypes[slot];
            var equipped = InvokeAny(m_InventoryHandler, new[] { "EquipItem", "EquipItemType", "Equip", "TryEquipItem", "TryEquipItemType" }, itemType)
                        || InvokeAny(m_ItemHandler, new[] { "EquipItem", "EquipItemType", "Equip", "TryEquipItem", "TryEquipItemType" }, itemType)
                        || InvokeAny(m_Inventory, new[] { "EquipItem", "EquipItemType", "Equip", "TryEquipItem", "TryEquipItemType" }, itemType)
                        || InvokeAny(m_InventoryHandler, new[] { "EquipItem", "EquipItemSet", "EquipItemSetIndex", "TryEquipItemSet" }, slot)
                        || InvokeAny(m_ItemHandler, new[] { "EquipItem", "EquipItemSet", "EquipItemSetIndex", "TryEquipItemSet" }, slot);

            SetStatus(equipped ? "Opsive equipped slot " + (slot + 1) : "Opsive equip method not found for slot " + (slot + 1));
            UpdateAnimatorForItem(slot, equipped ? 1f : 0.5f);
        }

        public void UseCurrentItem()
        {
            AutoBind();
            var used = InvokeAny(m_ItemHandler, new[] { "UseItem", "TryUseItem", "Use", "StartUse", "StartItemUse" })
                    || InvokeAny(m_InventoryHandler, new[] { "UseItem", "TryUseItem", "Use", "StartUse", "StartItemUse" })
                    || InvokeAny(m_ItemHandler, new[] { "UseItem", "TryUseItem", "Use", "StartUse", "StartItemUse" }, true)
                    || InvokeAny(m_InventoryHandler, new[] { "UseItem", "TryUseItem", "Use", "StartUse", "StartItemUse" }, true);
            SetStatus(used ? "Opsive use/fire invoked" : "Opsive use/fire method not found");
            UpdateAnimatorForUse();
        }

        public void ReloadCurrentItem()
        {
            AutoBind();
            var reloaded = InvokeAny(m_ItemHandler, new[] { "Reload", "ReloadItem", "TryReload", "StartReload" })
                        || InvokeAny(m_InventoryHandler, new[] { "Reload", "ReloadItem", "TryReload", "StartReload" })
                        || InvokeAny(m_ItemHandler, new[] { "Reload", "ReloadItem", "TryReload", "StartReload" }, true)
                        || InvokeAny(m_InventoryHandler, new[] { "Reload", "ReloadItem", "TryReload", "StartReload" }, true);
            SetStatus(reloaded ? "Opsive reload invoked" : "Opsive reload method not found");
        }

        private Component FindComponentByTypeNames(params string[] typeNames)
        {
            var components = GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                if (components[i] == null) {
                    continue;
                }
                var type = components[i].GetType();
                for (int j = 0; j < typeNames.Length; j++) {
                    if (type.FullName == typeNames[j] || (type.BaseType != null && type.BaseType.FullName == typeNames[j])) {
                        return components[i];
                    }
                }
            }
            return null;
        }

        private bool InvokeAny(Component target, string[] methodNames, params object[] arguments)
        {
            if (target == null || methodNames == null) {
                return false;
            }

            var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < methodNames.Length; i++) {
                for (int j = 0; j < methods.Length; j++) {
                    if (methods[j].Name != methodNames[i]) {
                        continue;
                    }
                    object[] invokeArguments;
                    if (!TryCreateInvokeArguments(methods[j].GetParameters(), arguments, out invokeArguments)) {
                        continue;
                    }
                    try {
                        methods[j].Invoke(target, invokeArguments);
                        return true;
                    } catch (Exception exception) {
                        SetStatus("Opsive bridge could not invoke " + methods[j].Name + " on " + target.GetType().Name + ": " + exception.GetBaseException().Message);
                        return false;
                    }
                }
            }
            return false;
        }

        private static bool TryCreateInvokeArguments(ParameterInfo[] parameters, object[] arguments, out object[] invokeArguments)
        {
            invokeArguments = null;
            if (arguments.Length > parameters.Length) {
                return false;
            }

            invokeArguments = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                if (i < arguments.Length) {
                    if (arguments[i] != null && !parameters[i].ParameterType.IsInstanceOfType(arguments[i])) {
                        return false;
                    }
                    invokeArguments[i] = arguments[i];
                    continue;
                }

                if (parameters[i].IsOptional) {
                    invokeArguments[i] = parameters[i].DefaultValue;
                } else if (parameters[i].ParameterType == typeof(bool)) {
                    invokeArguments[i] = true;
                } else if (parameters[i].ParameterType == typeof(int)) {
                    invokeArguments[i] = 0;
                } else if (parameters[i].ParameterType.IsValueType) {
                    invokeArguments[i] = Activator.CreateInstance(parameters[i].ParameterType);
                } else {
                    return false;
                }
            }
            return true;
        }

        private void SetStatus(string status)
        {
            m_StatusMessage = status;
            if (m_LogBridgeEvents) {
                Debug.Log(status, this);
            }
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

        private void UpdateAnimatorForItem(int slot, float confidence)
        {
            if (m_Animator == null) {
                return;
            }
            if (m_HasIntData) {
                m_Animator.SetInteger("Int Data", slot + 1);
            }
            if (m_HasFloatData) {
                m_Animator.SetFloat("Float Data", confidence);
            }
        }

        private void UpdateAnimatorForUse()
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

        private static int ReadEquipSlot()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) {
                return -1;
            }
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

        private static bool UsePressed()
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
