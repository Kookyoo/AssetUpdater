using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.CodeDom;

public class AssetFinder : EditorWindow
{
    [MenuItem("Demo/Asset Finder %g")]
    private static void Init()
    {
        GetWindow<AssetFinder>();
    }

    private string m_typeFilter = string.Empty;
    private Vector2 m_scrollPos;

    private const string EDITORPREFS_TYPESELECTED = "AssetFinder.TypeSelected";

    private string m_finalFilter = string.Empty;

    public void OnEnable()
    {
        if( EditorPrefs.HasKey(EDITORPREFS_TYPESELECTED) )
        {
            string typeString = EditorPrefs.GetString(EDITORPREFS_TYPESELECTED);
            m_typeSelected = System.Type.GetType(typeString);
        }
    }

    public void OnGUI()
    {
        if (m_typeSelected == null)
        {
            using (EditorGUI.ChangeCheckScope scope = new EditorGUI.ChangeCheckScope())
            {
                m_typeFilter = EditorGUILayout.TextField("Component filter", m_typeFilter);

                if (scope.changed)
                    ParseAssemblies(m_typeFilter);
            }

            foreach (System.Type type in m_typesCandidates)
            {
                if (GUILayout.Button(type.Name))
                {
                    m_typeSelected = type;
                    EditorPrefs.SetString(EDITORPREFS_TYPESELECTED, m_typeSelected.AssemblyQualifiedName);
                }
            }
        }
        else
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Component", m_typeSelected.Name);

                if( GUILayout.Button("Clear"))
                {
                    m_typeSelected = null;
                }
            }

            m_finalFilter = EditorGUILayout.TextField("Filter", m_finalFilter);

            if (GUILayout.Button("Find All"))
                FindAssets( m_typeSelected, m_finalFilter );

            using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(m_scrollPos) )
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    foreach (GameObject gameObject in m_selection)
                    {
                        EditorGUILayout.ObjectField(gameObject, typeof(GameObject), false);
                    }
                    m_scrollPos = scope.scrollPosition;
                }
            }
        }
    }

    private List<System.Type> m_typesCandidates = new List<System.Type>();
    private System.Type m_typeSelected = null;

    private void ParseAssemblies( string _filter )
    {
        m_typesCandidates.Clear();
        _filter = _filter.ToLower();

        System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach( System.Reflection.Assembly assembly in assemblies )
        {
            System.Type[] types = assembly.GetTypes();
            foreach ( System.Type type in types )
            {
                if (type.IsSubclassOf(typeof(MonoBehaviour))
                    && !type.IsAbstract
                    && type.FullName.ToLower().Contains(_filter) )
                {
                    m_typesCandidates.Add(type);
                }
            }
        }
    }

    private List<GameObject> m_selection = new List<GameObject>();
    private void FindAssets( System.Type _type, string _filter = "" )
    {
        m_selection.Clear();

        string[] guids = AssetDatabase.FindAssets("t:prefab " + _filter);
        foreach( string guid in guids )
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if( !string.IsNullOrEmpty(path))
            {
                GameObject gameObject = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject) ) as GameObject;
                if( gameObject != null )
                {
                    if( m_typeSelected == null || gameObject.GetComponent(m_typeSelected) != null )
                    {
                        m_selection.Add(gameObject);
                    }
                }
            }
        }

        Selection.objects = m_selection.ToArray();
    }
}
