using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GrassToolWindow : EditorWindow
{
    [SerializeField] private UnityEngine.Mesh grassMesh = null;
    [SerializeField] private float grassDensity = 0.0f;

    private static Bounds bounds;

    [MenuItem("Tools/Grass/Grass Window")]
    public static void ShowGrassWindow()
    {
        GrassToolWindow wnd = GetWindow<GrassToolWindow>();
        wnd.titleContent = new GUIContent("Grass Window");
    }

    public void OnGUI()
    {
        this.grassMesh = (Mesh)EditorGUILayout.ObjectField("Grass Mesh : ", this.grassMesh, typeof(Mesh), true);
        this.grassDensity = EditorGUILayout.FloatField("Grass Density : ", this.grassDensity);

        // // Each editor window contains a root VisualElement object
        // VisualElement root = rootVisualElement;
        // GridLayout grid = new GridLayout();

        // Label label = new Label("Grass blade mesh : ");
        // root.Add(label);

        // ObjectField grassMesh = new ObjectField();
        // grassMesh.name = "grassmesh";
        // root.Add(grassMesh);



        // Button button = new Button();
        // button.name = "initGrass";
        // button.text = "Initialize grass";
        // root.Add(button);
        //button.clicked += GrassComputer.InitGrass();

        // button.onClick.AddListener(GrassComputer.InitGrass());

        // // VisualElements objects can contain other VisualElement following a tree hierarchy
        // Label label = new Label("Hello World!");
        // root.Add(label);

        // // Create button
        // Button button = new Button();
        // button.name = "button";
        // button.text = "Button";
        // root.Add(button);

        // // Create toggle
        // Toggle toggle = new Toggle();
        // toggle.name = "toggle";
        // toggle.label = "Toggle";
        // root.Add(toggle);
    }
}