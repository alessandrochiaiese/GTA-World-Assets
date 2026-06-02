using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for CharacterFootsteps.
    /// </summary>
    [CustomEditor(typeof(CharacterFootsteps))]
    public class CharacterFootstepsInspector : InspectorBase
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var characterFootsteps = target as CharacterFootsteps;
            if (characterFootsteps == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();
            
            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            var feetProperty = PropertyFromName(serializedObject, "m_Feet");
            if (feetProperty != null) {
                EditorGUILayout.PropertyField(feetProperty, true);
            }
            var perFootSoundsProperty = PropertyFromName(serializedObject, "m_PerFootSounds");
            if (perFootSoundsProperty != null) {
                EditorGUILayout.PropertyField(perFootSoundsProperty, true);
            }
            var footstepsProperty = PropertyFromName(serializedObject, "m_Footsteps");
            if (footstepsProperty != null) {
                EditorGUILayout.PropertyField(footstepsProperty, true);
            } else {
                EditorGUILayout.HelpBox("Footstep presets are not available on this legacy runtime component.", MessageType.Info);
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(characterFootsteps, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(characterFootsteps);
            }
        }
    }
}