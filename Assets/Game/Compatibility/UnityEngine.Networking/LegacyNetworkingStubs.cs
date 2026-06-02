using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Networking
{
    /// <summary>
    /// Minimal compatibility shim for legacy assets that still reference Unity's removed UNet assembly.
    /// This is not a multiplayer implementation; it only allows old precompiled plugins to load in modern Unity.
    /// </summary>
    public class NetworkBehaviour : MonoBehaviour
    {
    }

    public class NetworkIdentity : MonoBehaviour
    {
    }

    public class NetworkConnection
    {
        public int connectionId;
    }

    public class NetworkManager : MonoBehaviour
    {
        public GameObject playerPrefab;

        public virtual void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
        }
    }

    public class NetworkAnimator : MonoBehaviour
    {
        public Animator animator;
    }

    public class NetworkTransform : MonoBehaviour
    {
        public enum TransformSyncMode
        {
            SyncNone,
            SyncTransform,
            SyncRigidbody2D,
            SyncRigidbody3D,
            SyncCharacterController
        }

        public TransformSyncMode transformSyncMode = TransformSyncMode.SyncTransform;
    }

    public static class NetworkServer
    {
        public static readonly List<NetworkConnection> connections = new List<NetworkConnection>();

        public static void AddPlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId)
        {
            if (conn != null && !connections.Contains(conn)) {
                connections.Add(conn);
            }
        }

        public static void Spawn(GameObject obj)
        {
        }
    }
}
