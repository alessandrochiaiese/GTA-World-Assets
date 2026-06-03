using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GTAWorld.Game
{
    /// <summary>
    /// Lightweight ride interaction for the generated demo scene. It is not a replacement for Opsive's final
    /// ride ability, but it makes the horse/vehicle placeholders usable while the full ability setup is configured.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameRideablePlaceholder : MonoBehaviour
    {
        [SerializeField] private string m_DisplayName = "Rideable";
        [SerializeField] private float m_InteractionDistance = 3f;
        [SerializeField] private Transform m_Seat;
        [SerializeField] private Vector3 m_DismountOffset = new Vector3(1.6f, 0f, 0f);
        [SerializeField] private bool m_ShowPrompt = true;

        private GameObject m_Rider;
        private GameSimplePlayerMover m_RiderMover;
        private Transform m_PreviousParent;

        public string DisplayName { get { return m_DisplayName; } set { m_DisplayName = value; } }
        public Transform Seat { get { return m_Seat; } set { m_Seat = value; } }

        private void Reset()
        {
            EnsureSeat();
        }

        private void Awake()
        {
            EnsureSeat();
        }

        private void Update()
        {
            if (!InteractPressed()) {
                return;
            }

            if (m_Rider != null) {
                Dismount();
                return;
            }

            var avatar = FindNearestAvatar();
            if (avatar != null) {
                Mount(avatar.gameObject);
            }
        }

        private void OnGUI()
        {
            if (!m_ShowPrompt || m_Rider != null) {
                return;
            }

            var avatar = FindNearestAvatar();
            if (avatar == null || Camera.main == null) {
                return;
            }

            var screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
            if (screen.z < 0f) {
                return;
            }

            var rect = new Rect(screen.x - 125f, Screen.height - screen.y - 18f, 250f, 36f);
            GUI.Label(rect, "Press E to ride " + m_DisplayName);
        }

        public void Mount(GameObject rider)
        {
            if (rider == null) {
                return;
            }

            EnsureSeat();
            m_Rider = rider;
            m_PreviousParent = rider.transform.parent;
            m_RiderMover = rider.GetComponent<GameSimplePlayerMover>();
            if (m_RiderMover != null) {
                m_RiderMover.enabled = false;
            }

            rider.transform.SetParent(m_Seat, false);
            rider.transform.localPosition = Vector3.zero;
            rider.transform.localRotation = Quaternion.identity;
        }

        public void Dismount()
        {
            if (m_Rider == null) {
                return;
            }

            var rider = m_Rider;
            rider.transform.SetParent(m_PreviousParent, true);
            rider.transform.position = transform.TransformPoint(m_DismountOffset);
            rider.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

            if (m_RiderMover != null) {
                m_RiderMover.enabled = true;
            }

            m_Rider = null;
            m_RiderMover = null;
            m_PreviousParent = null;
        }

        private void EnsureSeat()
        {
            if (m_Seat != null) {
                return;
            }

            var existing = transform.Find("RideSeat");
            if (existing != null) {
                m_Seat = existing;
                return;
            }

            var seatObject = new GameObject("RideSeat");
            m_Seat = seatObject.transform;
            m_Seat.SetParent(transform, false);
            m_Seat.localPosition = new Vector3(0f, 1.1f, 0f);
            m_Seat.localRotation = Quaternion.identity;
        }

        private GameAvatarIntegration FindNearestAvatar()
        {
            var avatars = GameObject.FindObjectsOfType<GameAvatarIntegration>();
            GameAvatarIntegration nearest = null;
            var nearestDistance = m_InteractionDistance;
            for (int i = 0; i < avatars.Length; i++) {
                if (avatars[i] == null) {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, avatars[i].transform.position);
                if (distance <= nearestDistance) {
                    nearest = avatars[i];
                    nearestDistance = distance;
                }
            }
            return nearest;
        }

        private static bool InteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.E);
#else
            return false;
#endif
        }
    }
}
