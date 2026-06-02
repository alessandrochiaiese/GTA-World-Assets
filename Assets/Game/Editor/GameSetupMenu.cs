using System;
using UnityEditor;
using UnityEngine;
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

        private static readonly string[] OpsiveCharacterComponents = {
            "Opsive.ThirdPersonController.Wrappers.RigidbodyCharacterController",
            "Opsive.ThirdPersonController.Wrappers.AnimatorMonitor",
            "Opsive.ThirdPersonController.Wrappers.CharacterIK"
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
            "Opsive.ThirdPersonController.Input.Wrappers.UnityInput"
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

        [MenuItem("Tools/Game/Create Complete Playable Scene", false, 10)]
        public static void CreateCompletePlayableScene()
        {
            EnsureProjectFolders();

            var game = CreateOrUpdateGameBootstrap();
            var mapAnchor = CreateOrUpdateMapAnchor();
            var avatar = CreateOrUpdatePlayerAvatar(mapAnchor);
            var camera = CreateOrUpdateGameplayCamera(avatar.transform);
            var weaponRoot = CreateWeaponPickupGallery(mapAnchor.transform);
            var rideableRoot = CreateRideableGallery(mapAnchor.transform);
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
            CreateOrUpdateMapAnchor();

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
            integration.EnsurePrototypeVisual();
            integration.SetMale();

            EnsureComponent<GameSimplePlayerMover>(avatar);
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
            var avatar = GameObject.Find("Player_UMA_Opsive");
            if (avatar == null) {
                avatar = new GameObject("Player_UMA_Opsive");
                Undo.RegisterCreatedObjectUndo(avatar, "Create Player UMA/Opsive Avatar");
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
            return controller;
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
            horse.transform.localPosition = new Vector3(-4f, 0f, 8f);
            horse.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
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
            vehicle.transform.localPosition = new Vector3(4f, 0.5f, 8f);
            vehicle.transform.localScale = new Vector3(2f, 1f, 4f);
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
            ground.transform.localScale = new Vector3(12f, 1f, 12f);
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
