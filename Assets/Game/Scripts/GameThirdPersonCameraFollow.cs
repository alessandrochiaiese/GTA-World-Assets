using UnityEngine;

namespace GTAWorld.Game
{
    /// <summary>
    /// Lightweight fallback camera follow used by the one-click scene setup until the Opsive camera is fully tuned.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameThirdPersonCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform m_Target;
        [SerializeField] private Vector3 m_Offset = new Vector3(0f, 2.2f, -4.5f);
        [SerializeField] private float m_PositionSmoothing = 10f;
        [SerializeField] private float m_RotationSmoothing = 12f;

        public Transform Target { get { return m_Target; } set { m_Target = value; } }
        public Vector3 Offset { get { return m_Offset; } set { m_Offset = value; } }

        private void LateUpdate()
        {
            if (m_Target == null) {
                return;
            }

            var desiredPosition = m_Target.TransformPoint(m_Offset);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * m_PositionSmoothing);

            var lookDirection = (m_Target.position + Vector3.up * 1.4f) - transform.position;
            if (lookDirection.sqrMagnitude > 0.0001f) {
                var desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * m_RotationSmoothing);
            }
        }
    }
}
