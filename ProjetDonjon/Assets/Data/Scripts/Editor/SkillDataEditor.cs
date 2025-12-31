using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    private SerializedObject so;
    private GUIStyle moduleNameStyle = new GUIStyle();
    private GUIStyle titreStyle = new GUIStyle();
    private SkillData currentScript;

    public SerializedProperty skillName;
    public SerializedProperty skillDescription;
    public SerializedProperty skillType;
    public SerializedProperty skillEffects;
    public SerializedProperty skillPatern;
    public SerializedProperty skillAOEPatern;
    public SerializedProperty skillAOEPaternLeft;
    public SerializedProperty skillAOEPaternRight;
    public SerializedProperty skillAOEPaternUp;
    public SerializedProperty skillAOEPaternDown;

    private int paternSize;


    private void OnEnable()
    {
        so = serializedObject;
        currentScript = target as SkillData;

        moduleNameStyle.fontSize = 14;
        titreStyle.fontSize = 12;

        moduleNameStyle.fontStyle = FontStyle.Bold;
        titreStyle.fontStyle = FontStyle.Bold;

        moduleNameStyle.normal.textColor = Color.white;
        titreStyle.normal.textColor = Color.white;

        skillName = so.FindProperty("skillName");
        skillDescription = so.FindProperty("skillDescription");
        skillType = so.FindProperty("skillType");
        skillEffects = so.FindProperty("skillEffects");
        skillPatern = so.FindProperty("skillPatern");
        skillAOEPatern = so.FindProperty("skillAOEPatern");
        skillAOEPaternLeft = so.FindProperty("skillAOEPaternLeft");
        skillAOEPaternRight = so.FindProperty("skillAOEPaternRight");
        skillAOEPaternUp = so.FindProperty("skillAOEPaternUp");
        skillAOEPaternDown = so.FindProperty("skillAOEPaternDown");

        paternSize = 15;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        so.Update();

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Launch Skill Patern");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * paternSize) }))
        {
            for (int y = 0; y < paternSize; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < paternSize; x++)
                    {
                        EditorGUILayout.PropertyField(skillPatern.GetArrayElementAtIndex((y * paternSize) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("AOE Patern");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * 9) }))
        {
            for (int y = 0; y < 9; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < 9; x++)
                    {
                        EditorGUILayout.PropertyField(skillAOEPatern.GetArrayElementAtIndex((y * 9) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("AOE Patern Left");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * 9) }))
        {
            for (int y = 0; y < 9; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < 9; x++)
                    {
                        EditorGUILayout.PropertyField(skillAOEPaternLeft.GetArrayElementAtIndex((y * 9) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("AOE Patern Right");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * 9) }))
        {
            for (int y = 0; y < 9; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < 9; x++)
                    {
                        EditorGUILayout.PropertyField(skillAOEPaternRight.GetArrayElementAtIndex((y * 9) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("AOE Patern Up");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * 9) }))
        {
            for (int y = 0; y < 9; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < 9; x++)
                    {
                        EditorGUILayout.PropertyField(skillAOEPaternUp.GetArrayElementAtIndex((y * 9) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("AOE Patern Down");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * 9) }))
        {
            for (int y = 0; y < 9; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < 9; x++)
                    {
                        EditorGUILayout.PropertyField(skillAOEPaternDown.GetArrayElementAtIndex((y * 9) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        so.ApplyModifiedProperties();
    }
}
