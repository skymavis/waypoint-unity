using UnityEditor;
using UnityEngine;

namespace SkyMavis.Waypoint.Editor
{
    [CustomPropertyDrawer(typeof(WaypointSettings))]
    public class WaypointSettingsDrawer : PropertyDrawer
    {
        private bool _showFoldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                position.height = lineHeight;
                _showFoldout = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, lineHeight), _showFoldout, "Waypoint Settings", true);

                if (_showFoldout)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        position.y += lineHeight;
                        position.height = 2 * lineHeight;
                        EditorGUI.HelpBox(EditorGUI.IndentedRect(position), "Leave Mavis Hub related properties empty or negative to auto-detect from the command line arguments.", MessageType.Info);
                        position.height = lineHeight;

                        position.y += 2 * lineHeight;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaypointSettings.mavisHubSessionID)));

                        position.y += lineHeight;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaypointSettings.mavisHubPort)));

                        position.y += lineHeight;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaypointSettings.clientID)));

                        position.y += lineHeight;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaypointSettings.endpoint)));

                        position.y += lineHeight;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaypointSettings.deepLinkCallbackURL)));

                        position.y += lineHeight;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaypointSettings.network)));
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!_showFoldout) return EditorGUIUtility.singleLineHeight;

            return 8 * EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(WaypointSettings.network)), true);
        }
    }
}
