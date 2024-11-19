using UnityEditor;
using UnityEngine;

namespace SkyMavis.Waypoint.Editor
{
    [CustomPropertyDrawer(typeof(WaypointSettings.Network))]
    public class WaypointSettingsNetworkDrawer : PropertyDrawer
    {
        private bool _showFoldout = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight * (_showFoldout ? 4 : 1);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                position.height = lineHeight;
                _showFoldout = EditorGUI.Foldout(position, _showFoldout, "Network", true);

                if (_showFoldout)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        position.y += lineHeight;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaypointSettings.Network.chainID)));

                        position.y += lineHeight;
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(WaypointSettings.Network.rpcURL)));

                        position.y += lineHeight;
                        position = EditorGUI.IndentedRect(position);

                        if (GUI.Button(new Rect(position.x, position.y, position.width / 2, position.height), "Use Ronin Mainnet"))
                        {
                            ApplyNetwork(property, WaypointSettings.Network.Mainnet);
                        }

                        if (GUI.Button(new Rect(position.x + position.width / 2, position.y, position.width / 2, position.height), "Use Ronin Testnet"))
                        {
                            ApplyNetwork(property, WaypointSettings.Network.Testnet);
                        }
                    }
                }
            }
        }

        private static void ApplyNetwork(SerializedProperty property, WaypointSettings.Network network)
        {
            property.FindPropertyRelative(nameof(WaypointSettings.Network.chainID)).intValue = network.chainID;
            property.FindPropertyRelative(nameof(WaypointSettings.Network.rpcURL)).stringValue = network.rpcURL;
        }
    }
}
