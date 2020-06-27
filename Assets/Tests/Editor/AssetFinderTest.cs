using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class AssetFinderTest : EditorWindow
{
    //[MenuItem("Demo/Asset Finder %g")]
    private static void Init()
    {
        EditorWindow window = GetWindow<AssetFinderTest>("Asset Finder");
        window.minSize = new Vector2(320,320);
    }

    private string m_typeFilter = string.Empty;
    private List<System.Type> m_typeCandidates = new List<System.Type>();
    private System.Type m_typeSelected = null;

    private const int MAX_CANDIDATES = 6;

    private Vector2 m_scrollPos;

    private string m_findFilter = string.Empty;

    private const string EDITORPREFS_TYPESELECTED = "AssetFinder.TypeSelected";

    public void OnEnable()
    {
        if( EditorPrefs.HasKey(EDITORPREFS_TYPESELECTED) )
        {
            string selected = EditorPrefs.GetString(EDITORPREFS_TYPESELECTED);
            m_typeSelected = System.Type.GetType(selected);
        }
    }

    public void OnGUI()
    {
        if ( m_typeSelected == null )
        {
            EditorGUI.BeginChangeCheck();

            m_typeFilter = EditorGUILayout.TextField("Type filter", m_typeFilter);

            if (EditorGUI.EndChangeCheck())
            {
                FindCandidates(m_typeFilter);
            }

            GUILayout.Label("Candidates count = " + m_typeCandidates.Count);


            foreach ( System.Type type in m_typeCandidates )
            {
                if ( GUILayout.Button(type.Name) )
                {
                    m_typeSelected = type;
                    m_typeCandidates.Clear();
                    m_typeFilter = string.Empty;
                    EditorPrefs.SetString( EDITORPREFS_TYPESELECTED, m_typeSelected.AssemblyQualifiedName );
                    break;
                }
            }
        }
        else
        {
            using ( new EditorGUILayout.HorizontalScope() )
            {
                EditorGUILayout.LabelField("SelectedComponent", m_typeSelected.Name);
                if( GUILayout.Button("Clear"))
                {
                    m_typeSelected = null;
                }
            }

            EditorGUI.BeginChangeCheck();

            m_findFilter = EditorGUILayout.DelayedTextField("Filter", m_findFilter);

            if( EditorGUI.EndChangeCheck() || GUILayout.Button("Find all") )
            {
                FindAll( m_findFilter );
            }

            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
            using ( new EditorGUI.DisabledGroupScope(true) )
            {
                foreach (GameObject gameObject in m_selection)
                {
                    EditorGUILayout.ObjectField(gameObject, typeof(GameObject), false);
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void FindCandidates( string _filter = "" )
    {
        m_typeCandidates.Clear();

        if (string.IsNullOrEmpty(_filter))
            return;

        _filter = _filter.ToLower();

        System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach( System.Reflection.Assembly assembly in assemblies )
        {
            System.Type[] types = assembly.GetTypes();
            foreach( System.Type type in types )
            {
                if ( type.IsSubclassOf(typeof(MonoBehaviour))
                    && !type.IsAbstract
                    && type.Name.ToLower().Contains(_filter) )
                {
                    m_typeCandidates.Add(type);

                    if (m_typeCandidates.Count >= MAX_CANDIDATES)
                        return;
                }
            }
        }
    }

    private List<GameObject> m_selection = new List<GameObject>();
    private void FindAll( string _filter = "" )
    {
        m_selection.Clear();

        string[] guids = AssetDatabase.FindAssets( "t:prefab " + _filter );
        foreach( string guid in guids )
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if( !string.IsNullOrEmpty(path) )
            {
                GameObject gameObject = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (gameObject)
                {
                    if (m_typeSelected == null || gameObject.GetComponent(m_typeSelected) != null)
                    {
                        m_selection.Add(gameObject);
                    }
                }
            }
        }

        Selection.objects = m_selection.ToArray();
    }
}
