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
        [SerializeField] private float m_PositionSmoothing = 18f;
        [SerializeField] private float m_RotationSmoothing = 18f;
        [SerializeField] private bool m_UseTargetRotation;
        [SerializeField] private bool m_SnapWhenFar = true;
        [SerializeField] private float m_SnapDistance = 8f;

        public Transform Target { get { return m_Target; } set { m_Target = value; SnapToTarget(); } }
        public Vector3 Offset { get { return m_Offset; } set { m_Offset = value; SnapToTarget(); } }

        private void OnEnable()
        {
            SnapToTarget();
        }

        [ContextMenu("Snap To Target")]
        public void SnapToTarget()
        {
            if (m_Target == null) {
                return;
            }
            transform.position = m_UseTargetRotation ? m_Target.TransformPoint(m_Offset) : m_Target.position + m_Offset;
            var lookDirection = (m_Target.position + Vector3.up * 1.4f) - transform.position;
            if (lookDirection.sqrMagnitude > 0.0001f) {
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }
        }

        private void LateUpdate()
        {
            if (m_Target == null) {
                return;
            }

            var desiredPosition = m_UseTargetRotation ? m_Target.TransformPoint(m_Offset) : m_Target.position + m_Offset;
            var positionT = 1f - Mathf.Exp(-m_PositionSmoothing * Time.deltaTime);
            if (m_SnapWhenFar && (transform.position - desiredPosition).sqrMagnitude > m_SnapDistance * m_SnapDistance) {
                transform.position = desiredPosition;
            } else {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, positionT);
            }

            var lookDirection = (m_Target.position + Vector3.up * 1.4f) - transform.position;
            if (lookDirection.sqrMagnitude > 0.0001f) {
                var desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                var rotationT = 1f - Mathf.Exp(-m_RotationSmoothing * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationT);
            }
        }
    }
}
