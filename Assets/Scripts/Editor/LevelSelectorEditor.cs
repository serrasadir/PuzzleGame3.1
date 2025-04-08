using UnityEngine;
using UnityEditor;
using TMPro;
public class LevelSelectorEditor : EditorWindow
{
    int selectedLevel;

    [MenuItem("Editor/Level Selector")]
    private static void ShowWindow()
    {
        var window = GetWindow<LevelSelectorEditor>();
        window.titleContent = new GUIContent("Level Selector");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Set the Current Level", EditorStyles.boldLabel);
       
        selectedLevel = EditorGUILayout.IntField("Current Level", selectedLevel);
        selectedLevel = Mathf.Clamp(selectedLevel, 1, 10);

        if (GUILayout.Button("Set Level"))
        {


            PlayerPrefs.SetInt("PlayerLevel", selectedLevel);
            PlayerPrefs.Save();

            Debug.Log("Level set to " + selectedLevel);
        }

        if (GUILayout.Button("Reset Level to 1"))
        {
            PlayerPrefs.DeleteAll();

            PlayerPrefs.SetInt("PlayerLevel", 1);
            PlayerPrefs.Save();
            Debug.Log("Current level reset to 1");
        }
    }
}
