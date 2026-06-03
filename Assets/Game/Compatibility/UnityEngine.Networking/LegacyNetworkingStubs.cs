using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Networking
{

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : Attribute
    {
        public int channel;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ClientRpcAttribute : Attribute
    {
        public int channel;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TargetRpcAttribute : Attribute
    {
        public int channel;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SyncVarAttribute : Attribute
    {
        public string hook;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ServerAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ServerCallbackAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ClientAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ClientCallbackAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class NetworkSettingsAttribute : Attribute
    {
        public int channel;
        public float sendInterval;
    }

    /// <summary>
    /// Minimal compatibility shim for legacy assets that still reference Unity's removed UNet assembly.
    /// This is not a multiplayer implementation; it only allows old precompiled plugins to load in modern Unity.
    /// </summary>
    public class NetworkBehaviour : MonoBehaviour
    {
        public bool isLocalPlayer { get; protected set; }
        public bool isServer { get; protected set; }
        public bool isClient { get; protected set; }
        public bool hasAuthority { get; protected set; }
        public NetworkIdentity netIdentity { get { return GetComponent<NetworkIdentity>(); } }
        public NetworkInstanceId netId { get { return netIdentity != null ? netIdentity.netId : default(NetworkInstanceId); } }

        public virtual bool OnSerialize(NetworkWriter writer, bool initialState) { return false; }
        public virtual void OnDeserialize(NetworkReader reader, bool initialState) { }
        public virtual void OnStartServer() { }
        public virtual void OnStartClient() { }
        public virtual void OnStartLocalPlayer() { }
        public virtual void OnStartAuthority() { }
        public virtual void OnStopAuthority() { }
    }

    public struct NetworkInstanceId
    {
        public uint Value;

        public NetworkInstanceId(uint value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public struct NetworkHash128
    {
        public string Value;

        public override string ToString()
        {
            return Value;
        }
    }

    public class NetworkIdentity : MonoBehaviour
    {
        public NetworkInstanceId netId;
        public bool isLocalPlayer;
        public bool isServer;
        public bool isClient;
        public bool hasAuthority;
        public bool localPlayerAuthority;
    }

    public class NetworkConnection
    {
        public int connectionId;
        public readonly List<PlayerController> playerControllers = new List<PlayerController>();
    }

    public class PlayerController
    {
        public short playerControllerId;
        public GameObject gameObject;
        public NetworkIdentity unetView;
    }

    public class NetworkReader
    {
        public virtual Vector3 ReadVector3() { return Vector3.zero; }
        public virtual Quaternion ReadQuaternion() { return Quaternion.identity; }
        public virtual float ReadSingle() { return 0f; }
        public virtual int ReadInt32() { return 0; }
        public virtual bool ReadBoolean() { return false; }
    }

    public class NetworkWriter
    {
        public virtual void Write(Vector3 value) { }
        public virtual void Write(Quaternion value) { }
        public virtual void Write(float value) { }
        public virtual void Write(int value) { }
        public virtual void Write(bool value) { }
    }

    public class MessageBase
    {
        public virtual void Deserialize(NetworkReader reader) { }
        public virtual void Serialize(NetworkWriter writer) { }
    }

    public class NetworkMessage
    {
        public NetworkConnection conn;
        public NetworkReader reader;
        public short msgType;
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
        public static bool active;
        public static bool dontListen;
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

        public static void Destroy(GameObject obj)
        {
            if (obj != null) {
                UnityEngine.Object.Destroy(obj);
            }
        }

        public static bool Listen(int serverPort)
        {
            active = true;
            return true;
        }

        public static void Shutdown()
        {
            active = false;
            connections.Clear();
        }
    }

    public class NetworkClient
    {
        public static bool active;
        public NetworkConnection connection;

        public virtual void Connect(string serverIp, int serverPort)
        {
            active = true;
        }

        public virtual void Disconnect()
        {
            active = false;
        }
    }

    public static class ClientScene
    {
        public static NetworkConnection readyConnection;

        public static bool Ready(NetworkConnection conn)
        {
            readyConnection = conn;
            return true;
        }

        public static bool AddPlayer(short playerControllerId)
        {
            return true;
        }
    }
}
