using UnityEngine;

namespace GTAWorld.Game
{
    /// <summary>
    /// Stores the key objects created by the one-click scene setup so other project systems can find them.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameAvatarIntegration m_PlayerAvatar;
        [SerializeField] private GameOsmMapAnchor m_MapAnchor;
        [SerializeField] private Camera m_MainCamera;
        [SerializeField] private Transform m_WeaponPickupRoot;
        [SerializeField] private Transform m_RideableRoot;
        [SerializeField] private GamePlayableDemoController m_DemoController;

        public GameAvatarIntegration PlayerAvatar { get { return m_PlayerAvatar; } set { m_PlayerAvatar = value; } }
        public GameOsmMapAnchor MapAnchor { get { return m_MapAnchor; } set { m_MapAnchor = value; } }
        public Camera MainCamera { get { return m_MainCamera; } set { m_MainCamera = value; } }
        public Transform WeaponPickupRoot { get { return m_WeaponPickupRoot; } set { m_WeaponPickupRoot = value; } }
        public Transform RideableRoot { get { return m_RideableRoot; } set { m_RideableRoot = value; } }
        public GamePlayableDemoController DemoController { get { return m_DemoController; } set { m_DemoController = value; } }
    }
}
