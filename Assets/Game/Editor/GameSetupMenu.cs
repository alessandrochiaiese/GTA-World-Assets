using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UMA;
using GTAWorld.Game;

namespace GTAWorld.Game.Editor
{
    /// <summary>
    /// Editor shortcuts for assembling the project's UMA + Opsive + OSM workflow.
    /// </summary>
    public static class GameSetupMenu
    {
        private const string GameRoot = "Assets/Game";
        private const string PrefabRoot = GameRoot + "/Prefabs";
        private const string AvatarPrefabPath = PrefabRoot + "/UMA_Opsive_Avatar_Template.prefab";
        private const string PlayerAvatarName = "Player_UMA_Opsive";
        private const string DefaultUmaCharacterPrefabPath = "Assets/UMA/Getting Started/UMADynamicCharacterAvatar.prefab";
        private const string OpsiveShooterAnimatorPath = "Assets/Third Person Controller/Demos/Third Person Shooter/Animator/Shooter.controller";
        private const string OpsiveShooterScenePath = "Assets/Third Person Controller/Demos/Third Person Shooter/Third Person Shooter.unity";
        private const string GameScenesRoot = GameRoot + "/Scenes";
        private const string GeneratedOpsiveScenePath = GameScenesRoot + "/GTA_Opsive_UMA_Demo.unity";

        private static readonly string[] OpsiveCharacterComponents = {
            "Opsive.ThirdPersonController.Wrappers.RigidbodyCharacterController",
            "Opsive.ThirdPersonController.Wrappers.CharacterIK"
        };

        private static readonly string[] OpsivePlayerCandidateComponents = {
            "Opsive.ThirdPersonController.RigidbodyCharacterController",
            "Opsive.ThirdPersonController.Wrappers.RigidbodyCharacterController"
        };

        private static readonly string[] OpsiveInspectorMismatchComponents = {
            "Opsive.ThirdPersonController.ItemHandler",
            "Opsive.ThirdPersonController.Wrappers.ItemHandler",
            "Opsive.ThirdPersonController.CharacterHealth",
            "Opsive.ThirdPersonController.Wrappers.CharacterHealth",
            "Opsive.ThirdPersonController.CharacterFootsteps",
            "Opsive.ThirdPersonController.Wrappers.CharacterFootsteps",
            "Opsive.ThirdPersonController.Inventory",
            "Opsive.ThirdPersonController.Wrappers.Inventory",
            "Opsive.ThirdPersonController.Input.UnityInput",
            "Opsive.ThirdPersonController.Input.Wrappers.UnityInput",
            "Opsive.ThirdPersonController.AnimatorMonitor",
            "Opsive.ThirdPersonController.Wrappers.AnimatorMonitor"
        };

        private static readonly string[] OpsiveAvatarAbilities = {
            "Opsive.ThirdPersonController.Wrappers.Abilities.Jump",
            "Opsive.ThirdPersonController.Wrappers.Abilities.Fall",
            "Opsive.ThirdPersonController.Wrappers.Abilities.PickupItem",
            "Opsive.ThirdPersonController.Wrappers.Abilities.Interact",
            "Opsive.ThirdPersonController.Wrappers.Abilities.Ride"
        };

        private static readonly string[] OpsiveGameComponents = {
            "Opsive.ThirdPersonController.Wrappers.Scheduler",
            "Opsive.ThirdPersonController.Wrappers.ObjectPool",
            "Opsive.ThirdPersonController.Wrappers.EventHandler",
            "Opsive.ThirdPersonController.Wrappers.DecalManager",
            "Opsive.ThirdPersonController.Wrappers.LayerManager",
            "Opsive.ThirdPersonController.Wrappers.ObjectManager"
        };

        private static readonly string[] OpsiveCameraComponents = {
            "Opsive.ThirdPersonController.Wrappers.CameraHandler",
            "Opsive.ThirdPersonController.Wrappers.CameraMonitor",
            "Opsive.ThirdPersonController.Wrappers.CameraController"
        };

        private static readonly string[] WeaponPickupPrefabPaths = {
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Pistol/Pistol Pickup.prefab",
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Assault Rifle/Assault Rifle Pickup.prefab",
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Shotgun/Shotgun Pickup.prefab",
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Sniper Rifle/Sniper Rifle Pickup.prefab",
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Rocket Launcher/Rocket Launcher Pickup.prefab",
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Grenade/Grenade Pickup.prefab",
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Knife/Knife Pickup.prefab",
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Shield/Shield Pickup.prefab",
            "Assets/Third Person Controller/Demos/Clean Scene/Prefabs/Katana Pickup.prefab",
            "Assets/Third Person Controller/Demos/Clean Scene/Prefabs/Bow Pickup.prefab"
        };

        private static readonly string[] WeaponPreviewPrefabPaths = {
            "Assets/UMA/Examples/MountingObjects Example/Assets/Pistol.prefab",
            "Assets/Third Person Controller/Demos/Third Person Shooter/Prefabs/Items/Assault Rifle/Assault Rifle Pickup.prefab",
            "Assets/UMA/Examples/MountingObjects Example/Assets/Sword.prefab",
            "Assets/Third Person Controller/Demos/Clean Scene/Prefabs/Bow Pickup.prefab"
        };


        private static readonly string[] OpsiveFullSystemComponents = {
            "Opsive.ThirdPersonController.Wrappers.ControllerHandler",
            "Opsive.ThirdPersonController.Wrappers.Inventory",
            "Opsive.ThirdPersonController.Wrappers.InventoryHandler",
            "Opsive.ThirdPersonController.Wrappers.ItemHandler"
        };

        private static readonly string[] OpsiveLoadoutItemNames = {
            "GTA_Pistol",
            "GTA_Rifle",
            "GTA_Melee",
            "GTA_Back"
        };

        [MenuItem("Tools/Game/Create Project Folders", false, 0)]
        public static void CreateProjectFolders()
        {
            EnsureProjectFolders();
            AssetDatabase.Refresh();
            Debug.Log("Game folders are ready under Assets/Game.");
        }

        [MenuItem("Tools/Game/Create/UMA + Opsive Avatar Template", false, 20)]
        public static void CreateAvatarTemplate()
        {
            EnsureProjectFolders();

            var avatar = new GameObject("UMA_Opsive_Avatar_Template");
            Undo.RegisterCreatedObjectUndo(avatar, "Create UMA/Opsive Avatar Template");
            PrepareAvatarObject(avatar, true);
            Selection.activeGameObject = avatar;

            var prefab = PrefabUtility.SaveAsPrefabAsset(avatar, AvatarPrefabPath);
            if (prefab != null) {
                EditorGUIUtility.PingObject(prefab);
                Debug.Log("Created avatar template prefab: " + AvatarPrefabPath, prefab);
            } else {
                Debug.LogWarning("Avatar scene object was created, but Unity could not save the prefab at " + AvatarPrefabPath, avatar);
            }
        }

        [MenuItem("Tools/Game/Prepare Selected UMA Avatar For Opsive", false, 21)]
        public static void PrepareSelectedAvatar()
        {
            var selected = Selection.activeGameObject;
            if (selected == null) {
                EditorUtility.DisplayDialog("Prepare UMA Avatar", "Select the UMA avatar GameObject in the Hierarchy first.", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(selected, "Prepare UMA Avatar For Opsive");
            PrepareAvatarObject(selected, true);
            EditorUtility.SetDirty(selected);
            Debug.Log("Prepared selected avatar for UMA + Opsive integration: " + selected.name, selected);
        }

        [MenuItem("Tools/Game/Build Full Opsive Character + Item Loadout", false, 22)]
        public static void BuildFullOpsiveCharacterAndLoadout()
        {
            var selected = Selection.activeGameObject;
            if (selected == null) {
                EditorUtility.DisplayDialog("Build Full Opsive", "Select the UMA/Opsive avatar GameObject first.", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(selected, "Build Full Opsive Character + Item Loadout");
            ConfigureFullOpsiveSystem(selected);
            EditorUtility.SetDirty(selected);
            Debug.Log("Full Opsive character components, item placements and generated loadout were applied to: " + selected.name, selected);
        }

        [MenuItem("Tools/Game/ONE CLICK - Create Complete GTA Demo", false, -10)]
        public static void CreateCompleteGtaDemo()
        {
            CreateOpsiveShooterUmaDemoScene();
        }

        [MenuItem("Tools/Game/Create/Opsive Shooter Scene + UMA Player", false, 9)]
        public static void CreateOpsiveShooterUmaDemoScene()
        {
            EnsureProjectFolders();
            EnsureFolder(GameRoot, "Scenes");
            ConfigureKeyboardInputForOpsive();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(OpsiveShooterScenePath) == null) {
                Debug.LogWarning("Opsive Third Person Shooter scene not found. Falling back to the generated sandbox scene.");
                CreateCompletePlayableScene();
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                return;
            }

            if (File.Exists(GeneratedOpsiveScenePath)) {
                AssetDatabase.DeleteAsset(GeneratedOpsiveScenePath);
            }
            if (!AssetDatabase.CopyAsset(OpsiveShooterScenePath, GeneratedOpsiveScenePath)) {
                Debug.LogError("Unable to copy the Opsive shooter scene. Falling back to the generated sandbox scene.");
                CreateCompletePlayableScene();
                return;
            }
            AssetDatabase.ImportAsset(GeneratedOpsiveScenePath);

            var scene = EditorSceneManager.OpenScene(GeneratedOpsiveScenePath, OpenSceneMode.Single);
            CreateOrUpdateUmaRuntime();
            var opsivePlayer = FindOpsivePlayerCandidate();
            if (opsivePlayer == null) {
                Debug.LogWarning("Copied the Opsive shooter scene but could not find its character controller. A UMA/Opsive player will be generated instead.");
                var anchor = CreateOrUpdateMapAnchor();
                var generatedPlayer = CreateOrUpdatePlayerAvatar(anchor);
                CreateOrUpdateGameplayCamera(generatedPlayer != null ? generatedPlayer.transform : null);
            } else {
                ConvertOpsiveScenePlayerToUma(opsivePlayer);
                var anchor = CreateOrUpdateMapAnchor();
                if (anchor != null) {
                    anchor.PlayerSpawnPoint = opsivePlayer.transform;
                    CreateOsmPlaceholderCity(anchor.MapRoot);
                }
                CreateOrUpdateGameplayCamera(opsivePlayer.transform);
            }

            var game = CreateOrUpdateGameBootstrap();
            var bootstrap = EnsureComponent<GameSceneBootstrap>(game);
            var playerObject = GameObject.Find(PlayerAvatarName);
            bootstrap.PlayerAvatar = playerObject != null ? playerObject.GetComponent<GameAvatarIntegration>() : null;
            bootstrap.MapAnchor = GameObject.FindObjectOfType<GameOsmMapAnchor>();
            bootstrap.MainCamera = Camera.main;
            bootstrap.DemoController = CreateOrUpdateDemoController(bootstrap);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Copied the Opsive Third Person Shooter demo scene and replaced/prepared the player with UMA integration. Use this scene first for the real Opsive movement/weapon pipeline.");
        }

        private static void ConfigureKeyboardInputForOpsive()
        {
            InvokeStaticEditorMethod("Opsive.ThirdPersonController.Editor.Input.UnityInputInspector", "UpdateInputManager");
            EnableBothUnityInputBackends();
        }

        private static void EnableBothUnityInputBackends()
        {
            var property = typeof(PlayerSettings).GetProperty("activeInputHandling", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanWrite) {
                Debug.LogWarning("Could not change Unity Active Input Handling automatically. If Opsive still logs UnityEngine.Input errors, set Project Settings > Player > Active Input Handling to Both.");
                return;
            }

            var value = property.GetValue(null, null);
            if (value != null && value.ToString() == "Both") {
                return;
            }

            object bothValue = null;
            if (property.PropertyType.IsEnum) {
                bothValue = Enum.Parse(property.PropertyType, "Both");
            } else if (property.PropertyType == typeof(int)) {
                bothValue = 2;
            }
            if (bothValue == null) {
                Debug.LogWarning("Could not determine the Unity Active Input Handling enum value. If Opsive still logs UnityEngine.Input errors, set Project Settings > Player > Active Input Handling to Both.");
                return;
            }

            property.SetValue(null, bothValue, null);
            Debug.Log("Unity Active Input Handling set to Both for legacy Opsive keyboard/mouse input.");
        }

        private static void InvokeStaticEditorMethod(string typeName, string methodName)
        {
            var type = FindType(typeName);
            var method = type != null ? type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : null;
            if (method == null) {
                Debug.LogWarning("Could not find " + typeName + "." + methodName + "; Opsive InputManager axes may need to be updated manually from Tools > Third Person Controller > Start Window.");
                return;
            }
            method.Invoke(null, null);
            Debug.Log("Opsive InputManager keyboard/mouse axes updated.");
        }

        private static GameObject FindOpsivePlayerCandidate()
        {
            var objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            for (int i = 0; i < objects.Length; ++i) {
                if (HasAnyComponent(objects[i], OpsivePlayerCandidateComponents)) {
                    return objects[i];
                }
            }
            for (int i = 0; i < objects.Length; ++i) {
                var objectName = objects[i].name.ToLowerInvariant();
                if (objectName.Contains("nolan") || objectName.Contains("player") || objectName.Contains("character")) {
                    return objects[i];
                }
            }
            return null;
        }

        private static bool HasAnyComponent(GameObject target, string[] typeNames)
        {
            if (target == null || typeNames == null) {
                return false;
            }
            for (int i = 0; i < typeNames.Length; ++i) {
                var type = FindType(typeNames[i]);
                if (type != null && target.GetComponent(type) != null) {
                    return true;
                }
            }
            return false;
        }

        private static void ConvertOpsiveScenePlayerToUma(GameObject opsivePlayer)
        {
            if (opsivePlayer == null) {
                return;
            }

            opsivePlayer.name = PlayerAvatarName;

            var umaVisual = opsivePlayer.transform.Find("UMA_Dynamic_Avatar_Visual");
            GameObject umaObject = umaVisual != null ? umaVisual.gameObject : null;
            if (umaObject == null) {
                umaObject = CreateUmaCharacterFromSettings();
                if (umaObject != null) {
                    Undo.RegisterCreatedObjectUndo(umaObject, "Add UMA Visual To Opsive Scene Player");
                    umaObject.name = "UMA_Dynamic_Avatar_Visual";
                    umaObject.transform.SetParent(opsivePlayer.transform, false);
                }
            }

            if (umaObject != null) {
                umaObject.transform.localPosition = Vector3.zero;
                umaObject.transform.localRotation = Quaternion.identity;
                umaObject.transform.localScale = Vector3.one;
                ConfigureUmaAvatar(umaObject);
            }

            HideOriginalCharacterRenderers(opsivePlayer.transform, umaObject != null ? umaObject.transform : null);

            var animator = umaObject != null ? umaObject.GetComponentInChildren<Animator>() : opsivePlayer.GetComponentInChildren<Animator>();
            var mounts = EnsureComponent<GameWeaponMounts>(opsivePlayer);
            mounts.Animator = animator;
            mounts.AutoCreateMounts();

            var integration = EnsureComponent<GameAvatarIntegration>(opsivePlayer);
            integration.Animator = animator;
            integration.WeaponMounts = mounts;
            integration.AutoBind();
            integration.SetMale();

            EnsureComponent<GameProceduralLocomotionAnimator>(opsivePlayer);
            EnsureComponent<GameFallbackWeaponController>(opsivePlayer).SetWeaponPrefabs(LoadWeaponPreviewPrefabs());
            EnsureComponent<GameOpsiveRuntimeBridge>(opsivePlayer).SetDefaultItemTypes(BuildGeneratedOpsiveLoadout(opsivePlayer));
        }

        private static void HideOriginalCharacterRenderers(Transform root, Transform exceptRoot)
        {
            if (root == null) {
                return;
            }
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; ++i) {
                if (renderers[i] == null) {
                    continue;
                }
                if (exceptRoot != null && renderers[i].transform.IsChildOf(exceptRoot)) {
                    continue;
                }
                renderers[i].enabled = false;
            }
        }

        [MenuItem("Tools/Game/Create Complete Playable Scene", false, 10)]
        public static void CreateCompletePlayableScene()
        {
            EnsureProjectFolders();
            CleanupGeneratedSceneObjects();

            var game = CreateOrUpdateGameBootstrap();
            var mapAnchor = CreateOrUpdateMapAnchor();
            CreateOrUpdateUmaRuntime();
            var avatar = CreateOrUpdatePlayerAvatar(mapAnchor);
            var camera = CreateOrUpdateGameplayCamera(avatar.transform);
            var weaponRoot = CreateWeaponPickupGallery(mapAnchor.transform);
            var rideableRoot = CreateRideableGallery(mapAnchor.transform);
            CreateOsmPlaceholderCity(mapAnchor.MapRoot);
            CreateLightingAndGround(mapAnchor.transform);

            var bootstrap = EnsureComponent<GameSceneBootstrap>(game);
            bootstrap.PlayerAvatar = avatar.GetComponent<GameAvatarIntegration>();
            bootstrap.MapAnchor = mapAnchor;
            bootstrap.MainCamera = camera;
            bootstrap.WeaponPickupRoot = weaponRoot;
            bootstrap.RideableRoot = rideableRoot;
            bootstrap.DemoController = CreateOrUpdateDemoController(bootstrap);

            Selection.activeGameObject = avatar;
            EditorGUIUtility.PingObject(avatar);
            Debug.Log("Complete playable UMA + Opsive + OSM scene scaffold created. Connect your generated OSM map under World_Map_Setup/OSM_Map_Root, then tune the Opsive character/items as needed.", avatar);
        }

        [MenuItem("Tools/Game/Create/Scene Gameplay Bootstrap", false, 40)]
        public static void CreateSceneGameplayBootstrap()
        {
            EnsureProjectFolders();

            var game = CreateOrUpdateGameBootstrap();
            var mapAnchor = CreateOrUpdateMapAnchor();
            CreateOrUpdateUmaRuntime();
            CreateOsmPlaceholderCity(mapAnchor.MapRoot);

            Selection.activeGameObject = game;
            Debug.Log("Created/updated scene bootstrap objects for Opsive managers and OSM map anchoring.", game);
        }

        [MenuItem("Tools/Game/Create Weapon Mounts On Selected Avatar", false, 60)]
        public static void CreateWeaponMountsOnSelectedAvatar()
        {
            var selected = Selection.activeGameObject;
            if (selected == null) {
                EditorUtility.DisplayDialog("Create Weapon Mounts", "Select the avatar GameObject in the Hierarchy first.", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(selected, "Create Weapon Mounts");
            var mounts = EnsureComponent<GameWeaponMounts>(selected);
            mounts.AutoCreateMounts();
            EditorUtility.SetDirty(selected);
            Debug.Log("Weapon/IK mounts are ready on: " + selected.name, selected);
        }

        [MenuItem("Tools/Game/Open Integration README", false, 100)]
        public static void OpenIntegrationReadme()
        {
            EnsureProjectFolders();
            var readme = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GameRoot + "/README.md");
            if (readme != null) {
                Selection.activeObject = readme;
                EditorGUIUtility.PingObject(readme);
            }
        }

        private static void PrepareAvatarObject(GameObject avatar, bool addUmaComponent)
        {
            CreateOrUpdateUmaRuntime();
            EnsureComponent<Animator>(avatar);

            var rigidbody = EnsureComponent<Rigidbody>(avatar);
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            var capsule = EnsureComponent<CapsuleCollider>(avatar);
            if (capsule.height <= 0f) {
                capsule.height = 1.8f;
                capsule.center = new Vector3(0f, 0.9f, 0f);
                capsule.radius = 0.3f;
            }

            if (addUmaComponent) {
                AddComponentByName(avatar, "UMA.CharacterSystem.DynamicCharacterAvatar", false);
            }
            ConfigureUmaAvatar(avatar);

            RemoveComponentsByName(avatar, OpsiveInspectorMismatchComponents);
            AddComponentsByName(avatar, OpsiveCharacterComponents);
            AddComponentsByName(avatar, OpsiveAvatarAbilities);

            var mounts = EnsureComponent<GameWeaponMounts>(avatar);
            mounts.Animator = avatar.GetComponentInChildren<Animator>();
            mounts.AutoCreateMounts();

            var integration = EnsureComponent<GameAvatarIntegration>(avatar);
            integration.Animator = avatar.GetComponentInChildren<Animator>();
            integration.WeaponMounts = mounts;
            integration.AutoBind();
            integration.RemovePrototypeVisual();
            integration.SetMale();

            EnsureComponent<GameSimplePlayerMover>(avatar);
            EnsureComponent<GameProceduralLocomotionAnimator>(avatar);
            ConfigureFullOpsiveSystem(avatar);
        }

        private static void CleanupGeneratedSceneObjects()
        {
            DestroySceneObjectsNamed(PlayerAvatarName);
            DestroySceneObjectsNamed("UMA_Opsive_Avatar_Template");
            DestroySceneObjectsNamed("Demo_Runtime_Controller");
        }

        private static void DestroySceneObjectsNamed(string objectName)
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = objects.Length - 1; i >= 0; i--) {
                var obj = objects[i];
                if (obj == null || EditorUtility.IsPersistent(obj)) {
                    continue;
                }
                if (obj.name != objectName && !obj.name.StartsWith(objectName + " (")) {
                    continue;
                }
                Undo.DestroyObjectImmediate(obj);
            }
        }

        private static GameObject CreateUmaCharacterFromSettings()
        {
            var settings = UMASettings.GetOrCreateSettings();
            var characterPrefab = settings != null ? settings.characterPrefab : null;
            if (characterPrefab == null) {
                characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultUmaCharacterPrefabPath);
            }
            if (characterPrefab == null) {
                Debug.LogWarning("No UMA character prefab is configured. Set UMA Settings > Character Prefab or import/fetch the UMA Getting Started prefab so the one-click scene can create a visible UMA mesh.");
                return null;
            }

            var avatar = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
            if (avatar != null) {
                Undo.RegisterCreatedObjectUndo(avatar, "Create UMA Character From Settings");
            }
            return avatar;
        }

        private static void CreateOrUpdateUmaRuntime()
        {
            var context = GameObject.FindObjectOfType<UMAGlobalContext>();
            if (context == null) {
                var contextObject = new GameObject("UMA_Runtime_Context");
                Undo.RegisterCreatedObjectUndo(contextObject, "Create UMA Runtime Context");
                context = contextObject.AddComponent<UMAGlobalContext>();
            }

            var generator = GameObject.FindObjectOfType<UMAGeneratorBase>();
            if (generator == null) {
                var settings = UMASettings.GetOrCreateSettings();
                GameObject generatorObject = null;
                if (settings != null && settings.generatorPrefab != null) {
                    generatorObject = PrefabUtility.InstantiatePrefab(settings.generatorPrefab) as GameObject;
                    if (generatorObject != null) {
                        Undo.RegisterCreatedObjectUndo(generatorObject, "Create UMA Generator");
                    }
                }
                if (generatorObject == null) {
                    generatorObject = new GameObject("UMAGenerator");
                    Undo.RegisterCreatedObjectUndo(generatorObject, "Create UMA Generator");
                    generatorObject.AddComponent<UMAGenerator>();
                }
                generatorObject.name = "UMAGenerator";
            }

            UMAContextBase.Instance = context;
            context.ValidateDictionaries();
        }

        private static void ConfigureUmaAvatar(GameObject avatar)
        {
            var umaAvatar = avatar.GetComponent("DynamicCharacterAvatar") as Component;
            if (umaAvatar == null) {
                return;
            }

            var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(OpsiveShooterAnimatorPath);
            var avatarBase = umaAvatar as UMAAvatarBase;
            if (avatarBase != null) {
                avatarBase.context = UMAContextBase.Instance;
                avatarBase.umaGenerator = GameObject.FindObjectOfType<UMAGeneratorBase>();
                if (animatorController != null) {
                    avatarBase.animationController = animatorController;
                }
            }

            var animator = avatar.GetComponentInChildren<Animator>();
            if (animator != null) {
                if (animatorController != null) {
                    animator.runtimeAnimatorController = animatorController;
                }
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            SetBooleanProperty(umaAvatar, "BuildCharacterEnabled", true);
            InvokeUmaMethod(umaAvatar, "ChangeRace", new object[] { "HumanMale", true });
            InvokeUmaMethod(umaAvatar, "BuildCharacter", new object[] { true, true, true });
        }

        private static GameObject CreateOrUpdateGameBootstrap()
        {
            var game = GameObject.Find("Game");
            if (game == null) {
                game = new GameObject("Game");
                Undo.RegisterCreatedObjectUndo(game, "Create Game Bootstrap");
            }
            AddComponentsByName(game, OpsiveGameComponents);
            EnsureComponent<GameSceneBootstrap>(game);
            return game;
        }

        private static GameOsmMapAnchor CreateOrUpdateMapAnchor()
        {
            var mapAnchor = GameObject.FindObjectOfType<GameOsmMapAnchor>();
            if (mapAnchor == null) {
                var mapObject = new GameObject("World_Map_Setup");
                Undo.RegisterCreatedObjectUndo(mapObject, "Create OSM Map Setup");
                mapAnchor = mapObject.AddComponent<GameOsmMapAnchor>();
            }
            mapAnchor.Reset();
            return mapAnchor;
        }

        private static GameObject CreateOrUpdatePlayerAvatar(GameOsmMapAnchor mapAnchor)
        {
            var avatar = GameObject.Find(PlayerAvatarName);
            if (avatar == null) {
                avatar = CreateUmaCharacterFromSettings();
                if (avatar == null) {
                    avatar = new GameObject(PlayerAvatarName);
                    Undo.RegisterCreatedObjectUndo(avatar, "Create Player UMA/Opsive Avatar");
                }
                avatar.name = PlayerAvatarName;
            }

            PrepareAvatarObject(avatar, true);
            if (mapAnchor != null && mapAnchor.PlayerSpawnPoint != null) {
                avatar.transform.position = mapAnchor.PlayerSpawnPoint.position;
                avatar.transform.rotation = mapAnchor.PlayerSpawnPoint.rotation;
            } else {
                avatar.transform.position = new Vector3(0f, 1f, 0f);
            }
            return avatar;
        }

        private static Camera CreateOrUpdateGameplayCamera(Transform target)
        {
            var camera = Camera.main;
            if (camera == null) {
                var cameraObject = new GameObject("Main Camera");
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Gameplay Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = target != null ? target.position + new Vector3(0f, 2.2f, -4.5f) : new Vector3(0f, 2.2f, -4.5f);
            camera.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
            AddComponentsByName(camera.gameObject, OpsiveCameraComponents);

            var follow = EnsureComponent<GameThirdPersonCameraFollow>(camera.gameObject);
            follow.Target = target;
            return camera;
        }

        private static GamePlayableDemoController CreateOrUpdateDemoController(GameSceneBootstrap bootstrap)
        {
            var controllerObject = GameObject.Find("Demo_Runtime_Controller");
            if (controllerObject == null) {
                controllerObject = new GameObject("Demo_Runtime_Controller");
                Undo.RegisterCreatedObjectUndo(controllerObject, "Create Demo Runtime Controller");
            }

            var controller = EnsureComponent<GamePlayableDemoController>(controllerObject);
            controller.Avatar = bootstrap.PlayerAvatar;
            controller.MapAnchor = bootstrap.MapAnchor;
            controller.WeaponMounts = bootstrap.PlayerAvatar != null ? bootstrap.PlayerAvatar.GetComponent<GameWeaponMounts>() : null;
            controller.OpsiveRuntimeBridge = bootstrap.PlayerAvatar != null ? bootstrap.PlayerAvatar.GetComponent<GameOpsiveRuntimeBridge>() : null;
            controller.FallbackWeaponController = bootstrap.PlayerAvatar != null ? bootstrap.PlayerAvatar.GetComponent<GameFallbackWeaponController>() : null;
            controller.SetWeaponPreviewPrefabs(LoadWeaponPreviewPrefabs());
            return controller;
        }

        private static GameObject[] LoadWeaponPreviewPrefabs()
        {
            var prefabs = new GameObject[WeaponPreviewPrefabPaths.Length];
            for (int i = 0; i < WeaponPreviewPrefabPaths.Length; i++) {
                prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(WeaponPreviewPrefabPaths[i]);
            }
            return prefabs;
        }

        private static Transform CreateWeaponPickupGallery(Transform parent)
        {
            var root = FindOrCreateChild(parent, "Weapon_Pickups");
            for (int i = 0; i < WeaponPickupPrefabPaths.Length; i++) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WeaponPickupPrefabPaths[i]);
                if (prefab == null) {
                    continue;
                }

                var existing = root.Find(prefab.name);
                if (existing != null) {
                    continue;
                }

                var pickup = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (pickup == null) {
                    continue;
                }
                Undo.RegisterCreatedObjectUndo(pickup, "Create Weapon Pickup");
                pickup.name = prefab.name;
                pickup.transform.SetParent(root, false);
                pickup.transform.localPosition = new Vector3((i % 5) * 2.25f - 4.5f, 0.15f, 4f + (i / 5) * 2.25f);
            }
            return root;
        }

        private static void ConfigureFullOpsiveSystem(GameObject avatar)
        {
            if (avatar == null) {
                return;
            }

            AddComponentsByName(avatar, OpsiveFullSystemComponents);
            EnsureOpsiveItemPlacements(avatar);
            var itemTypes = BuildGeneratedOpsiveLoadout(avatar);
            var runtimeBridge = EnsureComponent<GameOpsiveRuntimeBridge>(avatar);
            runtimeBridge.SetDefaultItemTypes(itemTypes);
            var fallbackWeapons = EnsureComponent<GameFallbackWeaponController>(avatar);
            fallbackWeapons.SetWeaponPrefabs(LoadWeaponPreviewPrefabs());
        }

        private static void EnsureOpsiveItemPlacements(GameObject avatar)
        {
            var mounts = avatar.GetComponent<GameWeaponMounts>();
            if (mounts == null) {
                mounts = EnsureComponent<GameWeaponMounts>(avatar);
            }
            mounts.Animator = avatar.GetComponentInChildren<Animator>();
            mounts.AutoCreateMounts();

            AddItemPlacement(mounts.RightHandMount);
            AddItemPlacement(mounts.LeftHandMount);
            AddItemPlacement(mounts.BackMount);
            AddItemPlacement(mounts.HipMount);
        }

        private static void AddItemPlacement(Transform mount)
        {
            if (mount == null) {
                return;
            }
            AddComponentByName(mount.gameObject, "Opsive.ThirdPersonController.Wrappers.ItemPlacement", false);
        }

        private static UnityEngine.Object[] BuildGeneratedOpsiveLoadout(GameObject avatar)
        {
            var itemBuilderType = FindType("Opsive.ThirdPersonController.ItemBuilder");
            var itemTypeType = FindType("Opsive.ThirdPersonController.ItemType");
            var primaryItemTypeType = FindType("Opsive.ThirdPersonController.Wrappers.PrimaryItemType") ?? FindType("Opsive.ThirdPersonController.PrimaryItemType");
            if (itemBuilderType == null || itemTypeType == null || primaryItemTypeType == null) {
                Debug.LogWarning("Opsive item types are not available yet. The avatar still has mounts and fallback weapons, but the real Opsive loadout could not be generated.", avatar);
                return new UnityEngine.Object[0];
            }

            var loadoutRoot = FindOrCreateChild(avatar.transform, "Game_Opsive_Loadout");
            var itemTypes = new UnityEngine.Object[WeaponPreviewPrefabPaths.Length];
            for (int i = 0; i < WeaponPreviewPrefabPaths.Length; i++) {
                itemTypes[i] = GetOrCreateOpsiveItemType(primaryItemTypeType, OpsiveLoadoutItemNames[Mathf.Min(i, OpsiveLoadoutItemNames.Length - 1)]);
                if (itemTypes[i] == null) {
                    continue;
                }
                var itemParent = GetGeneratedItemParent(avatar, loadoutRoot, i);
                BuildGeneratedOpsiveItem(itemBuilderType, itemTypeType, itemParent, WeaponPreviewPrefabPaths[i], itemTypes[i], OpsiveLoadoutItemNames[Mathf.Min(i, OpsiveLoadoutItemNames.Length - 1)], i == 2 ? "Melee" : "Shootable");
            }

            ApplyDefaultLoadout(avatar, itemTypes);
            return itemTypes;
        }

        private static Transform GetGeneratedItemParent(GameObject avatar, Transform fallbackRoot, int itemIndex)
        {
            var mounts = avatar != null ? avatar.GetComponent<GameWeaponMounts>() : null;
            if (mounts == null) {
                return fallbackRoot;
            }
            if (itemIndex == 3 && mounts.BackMount != null) {
                return mounts.BackMount;
            }
            if (mounts.RightHandMount != null) {
                return mounts.RightHandMount;
            }
            return fallbackRoot;
        }

        private static UnityEngine.Object GetOrCreateOpsiveItemType(Type itemType, string itemName)
        {
            EnsureFolder(GameRoot, "OpsiveGenerated");
            EnsureFolder(GameRoot + "/OpsiveGenerated", "ItemTypes");
            var assetPath = GameRoot + "/OpsiveGenerated/ItemTypes/" + itemName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (existing != null) {
                AssignStableItemTypeId(existing, itemName);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var instance = ScriptableObject.CreateInstance(itemType);
            if (instance == null) {
                return null;
            }
            instance.name = itemName;
            AssignStableItemTypeId(instance, itemName);
            AssetDatabase.CreateAsset(instance, assetPath);
            return instance;
        }

        private static void AssignStableItemTypeId(UnityEngine.Object itemType, string itemName)
        {
            if (itemType == null) {
                return;
            }

            var stableId = StablePositiveHash("GTAWorld_" + itemName);
            var idProperty = itemType.GetType().GetProperty("ID", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (idProperty != null && idProperty.CanWrite && idProperty.PropertyType == typeof(int)) {
                idProperty.SetValue(itemType, stableId, null);
                return;
            }
            var idField = itemType.GetType().GetField("m_ID", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (idField != null && idField.FieldType == typeof(int)) {
                idField.SetValue(itemType, stableId);
            }
        }

        private static int StablePositiveHash(string value)
        {
            unchecked {
                var hash = 23;
                if (!string.IsNullOrEmpty(value)) {
                    for (var i = 0; i < value.Length; ++i) {
                        hash = (hash * 31) + value[i];
                    }
                }
                if (hash == int.MinValue) {
                    return 1;
                }
                hash = Mathf.Abs(hash);
                return hash == 0 ? 1 : hash;
            }
        }

        private static void BuildGeneratedOpsiveItem(Type itemBuilderType, Type itemTypeType, Transform loadoutRoot, string prefabPath, UnityEngine.Object itemType, string itemName, string builderItemType)
        {
            if (loadoutRoot == null || itemType == null || !itemTypeType.IsInstanceOfType(itemType)) {
                return;
            }

            var existing = loadoutRoot.Find(itemName);
            if (existing != null) {
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject itemObject = null;
            if (prefab != null) {
                itemObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            if (itemObject == null) {
                itemObject = GameObject.CreatePrimitive(builderItemType == "Melee" ? PrimitiveType.Cube : PrimitiveType.Cylinder);
            }
            if (itemObject == null) {
                return;
            }

            Undo.RegisterCreatedObjectUndo(itemObject, "Create Opsive Loadout Item");
            itemObject.name = itemName;
            itemObject.transform.SetParent(loadoutRoot, false);
            itemObject.transform.localPosition = Vector3.zero;
            itemObject.transform.localRotation = Quaternion.identity;
            StripRuntimePickupComponents(itemObject);

            var itemTypesEnum = itemBuilderType.GetNestedType("ItemTypes");
            var handAssignmentEnum = itemBuilderType.GetNestedType("HandAssignment");
            var method = itemBuilderType.GetMethod("BuildItem", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (itemTypesEnum == null || handAssignmentEnum == null || method == null) {
                return;
            }

            try {
                var typeValue = Enum.Parse(itemTypesEnum, builderItemType);
                var handValue = Enum.Parse(handAssignmentEnum, "Right");
                method.Invoke(null, new object[] { itemObject, itemType, itemName, typeValue, handValue });
            } catch (Exception exception) {
                Debug.LogWarning("Unable to build Opsive item '" + itemName + "'. " + exception.GetBaseException().Message, itemObject);
            }
        }

        private static void StripRuntimePickupComponents(GameObject itemObject)
        {
            var behaviours = itemObject.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = behaviours.Length - 1; i >= 0; i--) {
                if (behaviours[i] != null) {
                    Undo.DestroyObjectImmediate(behaviours[i]);
                }
            }
            var colliders = itemObject.GetComponentsInChildren<Collider>(true);
            for (int i = colliders.Length - 1; i >= 0; i--) {
                if (colliders[i] != null) {
                    Undo.DestroyObjectImmediate(colliders[i]);
                }
            }
            var rigidbodies = itemObject.GetComponentsInChildren<Rigidbody>(true);
            for (int i = rigidbodies.Length - 1; i >= 0; i--) {
                if (rigidbodies[i] != null) {
                    Undo.DestroyObjectImmediate(rigidbodies[i]);
                }
            }
        }

        private static void ApplyDefaultLoadout(GameObject avatar, UnityEngine.Object[] itemTypes)
        {
            var inventory = avatar.GetComponent("Inventory") as Component;
            if (inventory == null || itemTypes == null) {
                return;
            }

            var inventoryType = inventory.GetType();
            var baseInventoryType = FindType("Opsive.ThirdPersonController.Inventory") ?? inventoryType.BaseType;
            var itemAmountType = baseInventoryType != null ? baseInventoryType.GetNestedType("ItemAmount") : null;
            var defaultLoadoutProperty = inventoryType.GetProperty("DefaultLoadout") ?? (baseInventoryType != null ? baseInventoryType.GetProperty("DefaultLoadout") : null);
            if (itemAmountType == null || defaultLoadoutProperty == null || !defaultLoadoutProperty.CanWrite) {
                return;
            }

            var validItemTypes = new System.Collections.Generic.List<UnityEngine.Object>();
            for (int i = 0; i < itemTypes.Length; i++) {
                if (itemTypes[i] != null) {
                    validItemTypes.Add(itemTypes[i]);
                }
            }
            var loadout = Array.CreateInstance(itemAmountType, validItemTypes.Count);
            for (int i = 0; i < validItemTypes.Count; i++) {
                var itemAmount = Activator.CreateInstance(itemAmountType, validItemTypes[i], 1);
                loadout.SetValue(itemAmount, i);
            }
            defaultLoadoutProperty.SetValue(inventory, loadout, null);
        }

        private static void CreateOsmPlaceholderCity(Transform mapRoot)
        {
            if (mapRoot == null || mapRoot.Find("OSM_Visible_Test_City") != null) {
                return;
            }

            var cityRoot = new GameObject("OSM_Visible_Test_City").transform;
            Undo.RegisterCreatedObjectUndo(cityRoot.gameObject, "Create OSM Placeholder City");
            cityRoot.SetParent(mapRoot, false);

            var terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Undo.RegisterCreatedObjectUndo(terrain, "Create OSM Terrain");
            terrain.name = "OSM_Green_Terrain_Blockout";
            terrain.transform.SetParent(cityRoot, false);
            terrain.transform.localPosition = Vector3.zero;
            terrain.transform.localScale = new Vector3(18f, 1f, 18f);
            SetRendererColor(terrain, new Color(0.25f, 0.48f, 0.22f));

            const float blockSpacing = 28f;
            const float roadLength = 170f;
            const float roadWidth = 3.8f;
            for (int i = -3; i <= 3; i++) {
                CreateRoad(cityRoot, "Road_NS_" + i, new Vector3(i * blockSpacing, 0.04f, 0f), new Vector3(roadWidth, 0.04f, roadLength));
                CreateRoad(cityRoot, "Road_EW_" + i, new Vector3(0f, 0.045f, i * blockSpacing), new Vector3(roadLength, 0.04f, roadWidth));
            }

            var buildingIndex = 0;
            for (int x = -3; x <= 2; x++) {
                for (int z = -3; z <= 2; z++) {
                    var height = 5f + ((x + 3 + z + 3) % 5) * 2.5f;
                    var offset = new Vector3(x * blockSpacing + 10f, 0f, z * blockSpacing + 10f);
                    CreateBuilding(cityRoot, "OSM_Building_" + buildingIndex, offset, height);
                    buildingIndex++;
                }
            }
        }

        private static void CreateRoad(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(road, "Create OSM Road");
            road.name = name;
            road.transform.SetParent(parent, false);
            road.transform.localPosition = localPosition;
            road.transform.localScale = localScale;
            SetRendererColor(road, new Color(0.23f, 0.23f, 0.23f));
        }

        private static void CreateBuilding(Transform parent, string name, Vector3 localPosition, float height)
        {
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(building, "Create OSM Building");
            building.name = name;
            building.transform.SetParent(parent, false);
            building.transform.localPosition = new Vector3(localPosition.x, height * 0.5f, localPosition.z);
            building.transform.localScale = new Vector3(11f, height, 11f);
            SetRendererColor(building, new Color(0.88f, 0.88f, 0.84f));
        }

        private static void SetRendererColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) {
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null) {
                shader = Shader.Find("Sprites/Default");
            }
            if (shader == null) {
                shader = Shader.Find("Standard");
            }
            if (shader == null) {
                if (renderer.sharedMaterial != null) {
                    renderer.sharedMaterial.color = color;
                }
                return;
            }

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Color")) {
                material.SetColor("_Color", color);
            }
            material.color = color;
            renderer.sharedMaterial = material;
        }

        private static Transform CreateRideableGallery(Transform parent)
        {
            var root = FindOrCreateChild(parent, "Rideables_And_Vehicles");
            CreateHorsePlaceholder(root);
            CreateVehiclePlaceholder(root);
            return root;
        }

        private static void CreateHorsePlaceholder(Transform root)
        {
            if (root.Find("Horse_Blitz_Rideable") != null) {
                return;
            }

            var horseAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Third Person Controller/Demos/Clean Scene/Models/Blitz.fbx");
            GameObject horse;
            if (horseAsset != null) {
                horse = PrefabUtility.InstantiatePrefab(horseAsset) as GameObject;
            } else {
                horse = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            }
            if (horse == null) {
                return;
            }

            Undo.RegisterCreatedObjectUndo(horse, "Create Rideable Horse");
            horse.name = "Horse_Blitz_Rideable";
            horse.transform.SetParent(root, false);
            horse.transform.localPosition = new Vector3(-18f, 0f, 34f);
            horse.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var rideable = EnsureComponent<GameRideablePlaceholder>(horse);
            rideable.DisplayName = "Horse";
            AddComponentByName(horse, "Opsive.ThirdPersonController.Wrappers.RideableObject", false);
        }

        private static void CreateVehiclePlaceholder(Transform root)
        {
            if (root.Find("Vehicle_Car_Placeholder") != null) {
                return;
            }

            var vehicle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(vehicle, "Create Vehicle Placeholder");
            vehicle.name = "Vehicle_Car_Placeholder";
            vehicle.transform.SetParent(root, false);
            vehicle.transform.localPosition = new Vector3(18f, 0.5f, 34f);
            vehicle.transform.localScale = new Vector3(4f, 1.4f, 7f);
            SetRendererColor(vehicle, new Color(0.12f, 0.18f, 0.28f));
            var rideable = EnsureComponent<GameRideablePlaceholder>(vehicle);
            rideable.DisplayName = "Vehicle";
            AddComponentByName(vehicle, "Opsive.ThirdPersonController.Wrappers.RideableObject", false);
        }

        private static void CreateLightingAndGround(Transform parent)
        {
            if (GameObject.FindObjectOfType<Light>() == null) {
                var lightObject = new GameObject("Directional Light");
                Undo.RegisterCreatedObjectUndo(lightObject, "Create Directional Light");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            if (parent == null || parent.Find("Temporary_Ground_For_OSM_Testing") != null) {
                return;
            }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Undo.RegisterCreatedObjectUndo(ground, "Create Temporary Ground");
            ground.name = "Temporary_Ground_For_OSM_Testing";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            SetRendererColor(ground, new Color(0.2f, 0.42f, 0.18f));
        }

        private static Transform FindOrCreateChild(Transform parent, string name)
        {
            if (parent == null) {
                var rootObject = GameObject.Find(name);
                if (rootObject == null) {
                    rootObject = new GameObject(name);
                    Undo.RegisterCreatedObjectUndo(rootObject, "Create " + name);
                }
                return rootObject.transform;
            }

            var child = parent.Find(name);
            if (child != null) {
                return child;
            }

            var childObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(childObject, "Create " + name);
            childObject.transform.SetParent(parent, false);
            return childObject.transform;
        }

        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets", "Game");
            EnsureFolder(GameRoot, "Scripts");
            EnsureFolder(GameRoot, "Editor");
            EnsureFolder(GameRoot, "Prefabs");
            EnsureFolder(GameRoot, "Scenes");
            EnsureFolder(GameRoot, "Art");
            EnsureFolder(GameRoot, "OSM");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component == null) {
                component = Undo.AddComponent<T>(target);
            }
            return component;
        }

        private static void RemoveComponentsByName(GameObject target, string[] typeNames)
        {
            for (int i = 0; i < typeNames.Length; i++) {
                var type = FindType(typeNames[i]);
                if (type == null || !typeof(Component).IsAssignableFrom(type)) {
                    continue;
                }

                var components = target.GetComponentsInChildren(type, true);
                for (int j = 0; j < components.Length; j++) {
                    if (components[j] != null) {
                        Undo.DestroyObjectImmediate(components[j]);
                    }
                }
            }
        }

        private static void AddComponentsByName(GameObject target, string[] typeNames)
        {
            for (int i = 0; i < typeNames.Length; i++) {
                AddComponentByName(target, typeNames[i], true);
            }
        }

        private static Component AddComponentByName(GameObject target, string typeName, bool warnIfMissing)
        {
            var type = FindType(typeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type)) {
                if (warnIfMissing) {
                    Debug.LogWarning("Optional component not found: " + typeName + ". Import/update the related package if this feature is required.", target);
                }
                return null;
            }

            var existing = target.GetComponent(type);
            if (existing != null) {
                return existing;
            }

            return Undo.AddComponent(target, type);
        }

        private static void SetBooleanProperty(Component component, string propertyName, bool value)
        {
            if (component == null) {
                return;
            }

            var property = component.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (property != null && property.PropertyType == typeof(bool) && property.CanWrite) {
                property.SetValue(component, value, null);
            }
        }

        private static bool InvokeUmaMethod(Component component, string methodName, object[] arguments)
        {
            if (component == null) {
                return false;
            }

            var methods = component.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].Name != methodName) {
                    continue;
                }
                var parameters = methods[i].GetParameters();
                if (parameters.Length != arguments.Length) {
                    continue;
                }
                methods[i].Invoke(component, arguments);
                return true;
            }
            return false;
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
