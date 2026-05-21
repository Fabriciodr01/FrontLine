
using UnityEngine;
using UnityEditor;

namespace FrontLine.UI
{
    [CustomEditor(typeof(MenuButton))]
    public class MenuButtonEditor : Editor
    {
        private SerializedProperty buttonType;
        private SerializedProperty targetSceneName;
        private SerializedProperty menuSceneName;
        private SerializedProperty enableHoverScale;
        private SerializedProperty hoverScale;
        private SerializedProperty scaleSpeed;

        private void OnEnable()
        {
            buttonType = serializedObject.FindProperty("buttonType");
            targetSceneName = serializedObject.FindProperty("targetSceneName");
            menuSceneName = serializedObject.FindProperty("menuSceneName");
            enableHoverScale = serializedObject.FindProperty("enableHoverScale");
            hoverScale = serializedObject.FindProperty("hoverScale");
            scaleSpeed = serializedObject.FindProperty("scaleSpeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw Button Type dropdown
            EditorGUILayout.PropertyField(buttonType, new GUIContent("Button Type"));

            // Show relevant scene fields based on button type
            MenuButton.ButtonType type = (MenuButton.ButtonType)buttonType.enumValueIndex;

            switch (type)
            {
                case MenuButton.ButtonType.StartGame:
                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(targetSceneName, new GUIContent("Start Scene Name"));
                    break;

                case MenuButton.ButtonType.ReturnToMenu:
                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(menuSceneName, new GUIContent("Menu Scene Name"));
                    break;

                case MenuButton.ButtonType.QuitGame:
                    // No scene fields needed
                    break;
            }

            // Draw hover scale section
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(enableHoverScale, new GUIContent("Enable Hover Scale"));

            if (enableHoverScale.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(hoverScale, new GUIContent("Hover Scale"));
                EditorGUILayout.PropertyField(scaleSpeed, new GUIContent("Scale Speed"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
