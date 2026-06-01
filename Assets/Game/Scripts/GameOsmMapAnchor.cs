using UnityEngine;

namespace GTAWorld.Game
{
    /// <summary>
    /// Scene marker for real-world/OpenStreetMap content. Use this to keep generated map roots, spawn points,
    /// and gameplay systems organized while OSM import/generation scripts are connected.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameOsmMapAnchor : MonoBehaviour
    {
        [SerializeField] private Transform m_MapRoot;
        [SerializeField] private Transform m_PlayerSpawnPoint;
        [SerializeField] private double m_CenterLatitude;
        [SerializeField] private double m_CenterLongitude;
        [SerializeField] private float m_UnityMetersPerRealMeter = 1f;

        public Transform MapRoot { get { return m_MapRoot; } set { m_MapRoot = value; } }
        public Transform PlayerSpawnPoint { get { return m_PlayerSpawnPoint; } set { m_PlayerSpawnPoint = value; } }
        public double CenterLatitude { get { return m_CenterLatitude; } set { m_CenterLatitude = value; } }
        public double CenterLongitude { get { return m_CenterLongitude; } set { m_CenterLongitude = value; } }
        public float UnityMetersPerRealMeter { get { return m_UnityMetersPerRealMeter; } set { m_UnityMetersPerRealMeter = Mathf.Max(0.001f, value); } }

        public void Reset()
        {
            if (m_MapRoot == null) {
                var mapRoot = transform.Find("OSM_Map_Root");
                if (mapRoot == null) {
                    mapRoot = new GameObject("OSM_Map_Root").transform;
                    mapRoot.SetParent(transform, false);
                }
                m_MapRoot = mapRoot;
            }

            if (m_PlayerSpawnPoint == null) {
                var spawn = transform.Find("PlayerSpawnPoint");
                if (spawn == null) {
                    spawn = new GameObject("PlayerSpawnPoint").transform;
                    spawn.SetParent(transform, false);
                    spawn.localPosition = new Vector3(0f, 1f, 0f);
                }
                m_PlayerSpawnPoint = spawn;
            }
        }
    }
}
