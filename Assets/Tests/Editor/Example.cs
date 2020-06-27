using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class Example : EditorWindow
{
    //[MenuItem("Demo/AssetUpdater %g")]
    public static void Init()
    {
        EditorWindow window = GetWindow<Example>("Example");
        window.minSize = new Vector2(480, 480);
    }

    private string m_typeFilter = string.Empty;
    private List<System.Type> m_typesCandidates = new List<System.Type>();
    private System.Type m_typeSelected = null;

    private const string EDITOR_PREFS_TYPESELECTED = "AssetUpdater.TypeSelected";

    private Vector2 m_scrollPos;

    private string m_targetFilter = string.Empty;

    public void OnEnable()
    {
        if (EditorPrefs.HasKey(EDITOR_PREFS_TYPESELECTED))
        {
            string typeName = EditorPrefs.GetString(EDITOR_PREFS_TYPESELECTED);
            m_typeSelected = System.Type.GetType(typeName);
            Debug.Log("Reload previous type : " + typeName + " = " + m_typeSelected.Name );
        }
    }

    public void OnGUI()
    {
        EditorGUILayout.Space();

        #region Type Filter
        if (m_typeSelected == null)
        {
            EditorGUI.BeginChangeCheck();
            m_typeFilter = EditorGUILayout.TextField("Type filter", m_typeFilter);

            if (EditorGUI.EndChangeCheck())
                GetTypeCandidates(m_typeFilter);

            GUILayout.Label("Candidates count : " + m_typesCandidates.Count);

            foreach (System.Type type in m_typesCandidates)
            {
                if (GUILayout.Button(type.Name))
                {
                    m_typeSelected = type;
                    m_typeFilter = string.Empty;
                    m_typesCandidates.Clear();

                    EditorPrefs.SetString(EDITOR_PREFS_TYPESELECTED, m_typeSelected.AssemblyQualifiedName);

                    break;
                }
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Selected component", m_typeSelected.Name);
                if( GUILayout.Button("Clear") )
                {
                    m_typeSelected = null;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            m_targetFilter = EditorGUILayout.DelayedTextField("Filter", m_targetFilter);

            if( EditorGUI.EndChangeCheck() || GUILayout.Button("Select all") )
            {
                FindAll( m_targetFilter );
            }

            EditorGUILayout.Space();

            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
            EditorGUI.BeginDisabledGroup(true);
            foreach ( GameObject target in m_selection )
            {
                EditorGUILayout.ObjectField(target, typeof(GameObject), false);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
        }
        #endregion Type Filter
    }

    public void GetTypeCandidates(string _filter = "" )
    {
        m_typesCandidates.Clear();

        if (_filter == "")
            return;

        _filter = _filter.ToLower();

        System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            System.Type[] candidates = assembly.GetTypes();
            foreach( System.Type candidate in candidates )
            {
                if( candidate.IsSubclassOf(typeof(MonoBehaviour))
                    && !candidate.IsAbstract
                    && candidate.Name.ToLower().Contains(_filter) )
                {
                    m_typesCandidates.Add(candidate);
                }
            }
        }
    }

    private List<GameObject> m_selection = new List<GameObject>();

    private void FindAll( string _filter )
    {
        m_selection.Clear();

        string[] guids = AssetDatabase.FindAssets( "t:prefab " + _filter );
        foreach( string guid in guids )
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if( !string.IsNullOrEmpty(path) )
            {
                GameObject gameObject = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if( gameObject )
                {
                    if( m_typeSelected == null || gameObject.GetComponent(m_typeSelected) )
                    {
                        m_selection.Add(gameObject);
                    }
                }
            }
        }

        Selection.objects = m_selection.ToArray();
    }
}
