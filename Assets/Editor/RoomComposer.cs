using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EditorWindow for composing rooms in the Scene view.
/// Tabs: Architecture · Props · Lights · Decals · Camera.
/// Menu: Echoes of You ▸ Tools ▸ Room Composer
/// </summary>
public class RoomComposer : EditorWindow
{
    // ──────────────────────────────────────────────
    //  CONSTANTS
    // ──────────────────────────────────────────────

    private const string ArchFolder      = "Assets/Prefabs/Architecture";
    private const string NarrativeFolder = "Assets/Prefabs/Props/Narrative";
    private const string LightingFolder  = "Assets/Prefabs/Lighting";
    private const string DecalFolder     = "Assets/Prefabs/Decals";
    private const string TemplateFolder  = "Assets/Data/RoomTemplates";

    // ──────────────────────────────────────────────
    //  STATE
    // ──────────────────────────────────────────────

    private enum Tab { Architecture, Props, Lights, Decals, Camera }
    private Tab currentTab;
    private Vector2 scroll;

    // Placement mode
    private bool   isPlacing;
    private string placingPrefabPath;
    private float  placementRotation;

    // Camera & Lights tabs
    private CameraProfile selectedCameraProfile;
    private LightingProfile selectedLightingProfile;

    // Lights tab
    private bool flickerCentral;
    private string missingPrefabNotice;

    // Styles (lazy-init)
    private GUIStyle _headerStyle;
    private GUIStyle HeaderStyle
    {
        get
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 13,
                    margin = new RectOffset(4, 4, 6, 4)
                };
            }
            return _headerStyle;
        }
    }

    // ──────────────────────────────────────────────
    //  WINDOW LIFECYCLE
    // ──────────────────────────────────────────────

    [MenuItem("Echoes of You/Tools/Room Composer")]
    public static void ShowWindow() => GetWindow<RoomComposer>("Room Composer");

    private void OnDisable() => CancelPlacement();

    // ──────────────────────────────────────────────
    //  MAIN GUI
    // ──────────────────────────────────────────────

    private void OnGUI()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("ROOM COMPOSER", EditorStyles.boldLabel);
        if (GUILayout.Button("Build Base Prefabs", GUILayout.Width(140)))
        {
            CreateBasePrefabs.CreateAllBasePrefabs();
            missingPrefabNotice = null;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);

        if (!string.IsNullOrEmpty(missingPrefabNotice))
        {
            EditorGUILayout.HelpBox($"Prefab not found: {missingPrefabNotice}", MessageType.Warning);
            if (GUILayout.Button("Build Base Prefabs Now"))
            {
                CreateBasePrefabs.CreateAllBasePrefabs();
                missingPrefabNotice = null;
            }
        }

        currentTab = (Tab)GUILayout.Toolbar((int)currentTab, Enum.GetNames(typeof(Tab)));
        EditorGUILayout.Space(5);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        switch (currentTab)
        {
            case Tab.Architecture: DrawArchitectureTab(); break;
            case Tab.Props:        DrawPropsTab();        break;
            case Tab.Lights:       DrawLightsTab();       break;
            case Tab.Decals:       DrawDecalsTab();       break;
            case Tab.Camera:       DrawCameraTab();       break;
        }
        EditorGUILayout.EndScrollView();

        // Status bar
        if (isPlacing)
        {
            EditorGUILayout.HelpBox(
                $"Colocando: {Path.GetFileNameWithoutExtension(placingPrefabPath)}\n" +
                "Click = colocar | Shift = snap grid (1u) | Ctrl / R = rotar 90° | Esc = cancelar",
                MessageType.Info);
        }
    }

    // ──────────────────────────────────────────────
    //  TAB: ARCHITECTURE
    // ──────────────────────────────────────────────

    private void DrawArchitectureTab()
    {
        GUILayout.Label("ARQUITECTURA BASE", HeaderStyle);

        DrawPlaceButton("Arch_Floor",      "Suelo",             "FloorMat",                ArchFolder);
        DrawPlaceButton("Arch_Wall",       "Pared",             "WallTealMat",             ArchFolder);
        DrawPlaceButton("Arch_WallWindow", "Pared c/ Ventana",  "WallTealMat",             ArchFolder);
        DrawPlaceButton("Arch_Doorway",    "Puerta",            "DoorMat + DoorController",ArchFolder);
        DrawPlaceButton("Arch_Column",     "Columna",           "ArchMat",                 ArchFolder);
        DrawPlaceButton("Arch_Stairs",     "Escalera",          "ArchMat",                 ArchFolder);
        DrawPlaceButton("Arch_Locker",     "Locker",            "ArchMat",                 ArchFolder);
        DrawPlaceButton("Arch_Shelf",      "Estantería",        "WallMustardMat",          ArchFolder);
        DrawPlaceButton("Arch_Desk",       "Pupitre",           "MemoryMat",               ArchFolder);
        DrawPlaceButton("Arch_Chair",      "Silla",             "ArchMat",                 ArchFolder);
        DrawPlaceButton("Arch_Bench",      "Banco",             "ArchMat",                 ArchFolder);
        DrawPlaceButton("Arch_Trashcan",   "Papelera",          "ArchMat",                 ArchFolder);
        DrawPlaceButton("Arch_Fence",      "Valla",             "ArchMat",                 ArchFolder);
        DrawPlaceButton("Arch_Tree",       "Árbol seco",        "ArchMat",                 ArchFolder);

        GUILayout.Space(10);
        if (GUILayout.Button("Exportar como RoomTemplate", GUILayout.Height(28)))
            ExportRoomTemplate();
    }

    // ──────────────────────────────────────────────
    //  TAB: PROPS
    // ──────────────────────────────────────────────

    private static readonly string[] PropNames =
    {
        "Prop_Coat", "Prop_Notebook", "Prop_PhotoFrame", "Prop_StoppedClock",
        "Prop_TeacherNotebook", "Prop_ChalkDrawing", "Prop_Backpack",
        "Prop_DriedFlowers", "Prop_BlankBook", "Prop_LibraryStamp",
        "Prop_JanitorCart", "Prop_ChalkGraffiti", "Prop_SoccerBall",
        "Prop_OverturnedDesk", "Prop_CenterBackpack", "Prop_CoffeeCups",
        "Prop_AttendanceList", "Prop_Stopwatch", "Prop_RecordsBoard"
    };

    private void DrawPropsTab()
    {
        GUILayout.Label("PROPS NARRATIVOS (Auto-luz diegética)", HeaderStyle);

        foreach (string p in PropNames)
        {
            string label = p.Replace("Prop_", "");
            DrawPlaceButton(p, label, "Auto-luz diegética", NarrativeFolder);
        }
    }

    // ──────────────────────────────────────────────
    //  TAB: LIGHTS
    // ──────────────────────────────────────────────

    private void DrawLightsTab()
    {
        GUILayout.Label("LUZ DIEGÉTICA", HeaderStyle);

        selectedLightingProfile = (LightingProfile)EditorGUILayout.ObjectField(
            "Lighting Profile", selectedLightingProfile, typeof(LightingProfile), false);

        GUILayout.Space(5);

        if (DrawPrefabButton("FluorescentLight", "Fluorescente", "Flicker + Audio Sync"))
            StartPlacement($"{LightingFolder}/FluorescentLight.prefab");

        EditorGUILayout.HelpBox(
            "Coloca bajo techo. Auto: LightFlicker + Audio Sync si 'Flicker Central' ON",
            MessageType.Info);

        flickerCentral = EditorGUILayout.Toggle("Flicker Central (parpadea)", flickerCentral);
    }

    // ──────────────────────────────────────────────
    //  TAB: DECALS
    // ──────────────────────────────────────────────

    private void DrawDecalsTab()
    {
        GUILayout.Label("DECALS PROCEDURALES", HeaderStyle);

        if (GUILayout.Button("Moisture Lines (esquinas techo)"))
            PlaceDecal("dec_moisture_lines");

        if (GUILayout.Button("Floor Drag (debajo lockers)"))
            PlaceDecal("dec_floor_drag");

        if (GUILayout.Button("Crack Liminal (grietas Cap III+)"))
            PlaceDecal("dec_crack_liminal");

        if (GUILayout.Button("Lyra Notes (pizarras Lyra)"))
            PlaceDecal("dec_lyra_notes");
    }

    // ──────────────────────────────────────────────
    //  TAB: CAMERA
    // ──────────────────────────────────────────────

    private void DrawCameraTab()
    {
        GUILayout.Label("CÁMARA PERFIL", HeaderStyle);

        selectedCameraProfile = (CameraProfile)EditorGUILayout.ObjectField(
            "Camera Profile", selectedCameraProfile, typeof(CameraProfile), false);

        GUILayout.Space(5);

        if (GUILayout.Button("Add Learning Zone", GUILayout.Height(26)))
        {
            GameObject zone = new GameObject("LearningZone_Trigger");
            var col = zone.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(5f, 3f, 5f);

            var applier = zone.AddComponent<CameraProfileApplier>();
            applier.CurrentProfile = selectedCameraProfile;

            Undo.RegisterCreatedObjectUndo(zone, "Create Learning Zone");
            Selection.activeGameObject = zone;
        }
    }

    // ──────────────────────────────────────────────
    //  BUTTON HELPERS
    // ──────────────────────────────────────────────

    private void DrawPlaceButton(string prefabName, string label, string tooltip, string folder)
    {
        if (DrawPrefabButton(prefabName, label, tooltip))
            StartPlacement($"{folder}/{prefabName}.prefab");
    }

    private bool DrawPrefabButton(string prefabName, string label, string tooltip)
    {
        EditorGUILayout.BeginHorizontal();
        bool clicked = GUILayout.Button(label, GUILayout.Height(22));
        GUILayout.Label(tooltip, EditorStyles.miniLabel, GUILayout.Width(160));
        EditorGUILayout.EndHorizontal();
        return clicked;
    }

    // ──────────────────────────────────────────────
    //  PLACEMENT SYSTEM
    // ──────────────────────────────────────────────

    private void StartPlacement(string prefabPath)
    {
        CancelPlacement();

        // Verify prefab exists
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            missingPrefabNotice = prefabPath;
            Debug.LogWarning($"[RoomComposer] Prefab not found: {prefabPath}. Click 'Build Base Prefabs' to generate.");
            if (EditorUtility.DisplayDialog("Prefab Not Found", $"Prefab not found at {prefabPath}.\nWould you like to build base prefabs now?", "Build Base Prefabs", "Cancel"))
            {
                CreateBasePrefabs.CreateAllBasePrefabs();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    missingPrefabNotice = null;
                }
                else return;
            }
            else return;
        }
        else
        {
            missingPrefabNotice = null;
        }

        isPlacing = true;
        placingPrefabPath = prefabPath;
        placementRotation = 0f;

        SceneView.duringSceneGui += OnSceneGUI;
        Repaint();
    }

    private void CancelPlacement()
    {
        if (!isPlacing) return;
        isPlacing = false;
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.RepaintAll();
        Repaint();
    }

    private void PlaceDecal(string decalName)
    {
        StartPlacement($"{DecalFolder}/{decalName}.prefab");
    }

    // ──────────────────────────────────────────────
    //  SCENE GUI — INTERACTIVE PLACEMENT
    // ──────────────────────────────────────────────

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacing)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            return;
        }

        Event e = Event.current;

        // Prevent default selection behaviour
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // Escape → cancel
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            CancelPlacement();
            e.Use();
            return;
        }

        // Ctrl or R → rotate 90°
        if (e.type == EventType.KeyDown && ((e.modifiers & EventModifiers.Control) != 0 || e.keyCode == KeyCode.R))
        {
            placementRotation = (placementRotation + 90f) % 360f;
            e.Use();
        }

        // Raycast for placement position
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            Vector3 pos = hit.point;

            // Shift → snap to 1-unit grid
            if (e.shift)
            {
                pos.x = Mathf.Round(pos.x);
                pos.z = Mathf.Round(pos.z);
            }

            Quaternion rot = Quaternion.Euler(0f, placementRotation, 0f);

            // Visual feedback
            Handles.color = new Color(0.4f, 0.9f, 0.6f, 0.6f);
            Handles.DrawWireDisc(pos, Vector3.up, 0.5f);
            Handles.DrawLine(pos, pos + rot * Vector3.forward * 0.7f);

            // Label
            Handles.Label(pos + Vector3.up * 0.3f,
                Path.GetFileNameWithoutExtension(placingPrefabPath),
                EditorStyles.whiteBoldLabel);

            // Left-click → place
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(placingPrefabPath);
                if (prefab != null)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    instance.transform.position = pos;
                    instance.transform.rotation = rot;
                    Undo.RegisterCreatedObjectUndo(instance, $"Place {instance.name}");
                    Selection.activeGameObject = instance;
                }
                e.Use();
            }
        }

        sceneView.Repaint();
    }

    // ──────────────────────────────────────────────
    //  EXPORT ROOM TEMPLATE
    // ──────────────────────────────────────────────

    private void ExportRoomTemplate()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(sceneName))
            sceneName = "UntitledRoom";

        RoomTemplate template = CreateInstance<RoomTemplate>();
        template.roomName  = sceneName;
        template.sceneName = sceneName;
        template.guid = Guid.NewGuid().ToString();
        template.createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        template.cameraProfile = selectedCameraProfile;
        template.lightingProfileName = selectedLightingProfile != null ? selectedLightingProfile.name : "DefaultLightingProfile";
        template.echoMode = true;
        template.maxEchoes = 3;

        // Gather Architecture pieces
        var archPieces = FindObjectsByType<ArchitecturePiece>(FindObjectsInactive.Exclude);
        foreach (var arch in archPieces)
        {
            var placement = new RoomTemplate.Placement
            {
                localPosition   = arch.transform.position,
                rotation        = arch.transform.rotation,
                localScale      = arch.transform.localScale,
                materialToken   = arch.materialToken,
                isNarrativeProp = false,
                propName        = ""
            };

            // Try to resolve prefab path
            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(arch.gameObject);
            if (prefabSource != null)
            {
                string path = AssetDatabase.GetAssetPath(prefabSource);
                placement.prefabPath = path;
                placement.prefabGuid = AssetDatabase.AssetPathToGUID(path);
            }
            else
            {
                placement.prefabPath = $"{ArchFolder}/{arch.pieceId}.prefab";
            }

            template.placements.Add(placement);
        }

        // Gather Narrative props
        var narrativeProps = FindObjectsByType<NarrativeProp>(FindObjectsInactive.Exclude);
        foreach (var prop in narrativeProps)
        {
            var placement = new RoomTemplate.Placement
            {
                localPosition   = prop.transform.position,
                rotation        = prop.transform.rotation,
                localScale      = prop.transform.localScale,
                materialToken   = "",
                propName        = prop.propName,
                isNarrativeProp = true
            };

            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(prop.gameObject);
            if (prefabSource != null)
            {
                string path = AssetDatabase.GetAssetPath(prefabSource);
                placement.prefabPath = path;
                placement.prefabGuid = AssetDatabase.AssetPathToGUID(path);
            }
            else
            {
                placement.prefabPath = $"{NarrativeFolder}/{prop.propName}.prefab";
            }

            template.placements.Add(placement);
        }

        // Save asset
        EchoesMaterialLibrary.EnsureFolderExists(TemplateFolder);
        string shortGuid = template.guid.Substring(0, 8);
        string assetPath = $"{TemplateFolder}/{sceneName}_{shortGuid}.asset";

        AssetDatabase.CreateAsset(template, assetPath);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(template);

        Debug.Log($"[RoomComposer] ✓ RoomTemplate exported: {assetPath} " +
                  $"({template.placements.Count} placements)");
    }
}
