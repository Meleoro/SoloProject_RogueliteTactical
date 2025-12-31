using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(AIData))]
public class AIDataEditor : Editor
{
    private SerializedObject so;
    private GUIStyle moduleNameStyle = new GUIStyle();
    private GUIStyle titreStyle = new GUIStyle();

    public SerializedProperty movePatern;
    private int paternSize;


    private void OnEnable()
    {
        so = serializedObject;

        moduleNameStyle.fontSize = 14;
        titreStyle.fontSize = 12;

        moduleNameStyle.fontStyle = FontStyle.Bold;
        titreStyle.fontStyle = FontStyle.Bold;

        moduleNameStyle.normal.textColor = Color.white;
        titreStyle.normal.textColor = Color.white;

        movePatern = so.FindProperty("movePatern");
        paternSize = 15;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        so.Update();

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * paternSize) }))
        {
            for (int y = 0; y < paternSize; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < paternSize; x++)
                    {
                        EditorGUILayout.PropertyField(movePatern.GetArrayElementAtIndex((y * paternSize) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        so.ApplyModifiedProperties();
    }
}
