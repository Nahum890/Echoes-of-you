// ============================================================
// TextureOptimizerTool.cs
// Echoes of You — URP Texture Optimization Editor Window
// Senior Technical Artist Tool — Unity 2022+ / URP
// ============================================================
// USAGE: Window → Echoes → Texture Optimizer
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EchoesEditor
{
    // -------------------------------------------------------
    // Data Models
    // -------------------------------------------------------
    [Serializable]
    public class TextureAuditEntry
    {
        public string assetPath;
        public string fileName;
        public TextureCategory category;
        public TextureTargetSettings targetSettings;
        public int currentMaxSize;
        public TextureImporterCompression currentCompression;
        public bool currentMipmaps;
        public FilterMode currentFilterMode;
        public int currentAniso;
        public bool currentStreaming;
        public TextureImporterType currentType;
        public long fileSizeBytes;
        public bool needsChange;
        public bool selected;
    }

    public class TextureDuplicateGroup
    {
        public string fileName;
        public List<string> paths = new List<string>();
        public long singleSizeBytes;
    }

    // -------------------------------------------------------
    // Main Editor Window
    // -------------------------------------------------------
    public class TextureOptimizerTool : EditorWindow
    {
        // ---- State ----
        private int _activeTab = 0;
        private readonly string[] _tabNames = { "📋 Audit", "⚙️ Optimize", "🧹 Cleanup", "📊 Report" };

        // Audit Tab
        private List<TextureAuditEntry> _auditEntries = new List<TextureAuditEntry>();
        private Vector2 _auditScroll;
        private bool _auditDone = false;
        private string _auditFilter = "";
        private TextureCategory _auditCategoryFilter = (TextureCategory)(-1);
        private bool _showOnlyNeedsChange = false;

        // Cleanup Tab
        private List<TextureDuplicateGroup> _duplicates = new List<TextureDuplicateGroup>();
        private List<string> _unusedTextures = new List<string>();
        private List<string> _unusedMaterials = new List<string>();
        private Vector2 _cleanupScroll;
        private bool _cleanupDone = false;

        // Optimize Tab
        private bool _dryRun = true;
        private string _optimizeLog = "";
        private Vector2 _optimizeScroll;

        // Report Tab
        private string _lastReportPath = "";
        private Vector2 _reportScroll;

        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _tagGreen;
        private GUIStyle _tagYellow;
        private GUIStyle _tagRed;
        private bool _stylesInitialized = false;

        // -------------------------------------------------------
        [MenuItem("Window/Echoes/Texture Optimizer")]
        public static void ShowWindow()
        {
            var window = GetWindow<TextureOptimizerTool>("Texture Optimizer");
            window.minSize = new Vector2(900, 600);
        }

        // -------------------------------------------------------
        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(4, 4, 8, 4)
            };

            _sectionStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 6, 6),
                margin = new RectOffset(4, 4, 4, 4)
            };

            _tagGreen = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.3f, 0.9f, 0.4f) },
                fontStyle = FontStyle.Bold
            };

            _tagYellow = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(1f, 0.85f, 0.1f) },
                fontStyle = FontStyle.Bold
            };

            _tagRed = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(1f, 0.35f, 0.35f) },
                fontStyle = FontStyle.Bold
            };

            _stylesInitialized = true;
        }

        // -------------------------------------------------------
        private void OnGUI()
        {
            InitStyles();

            // Header
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("🎮  Echoes of You — Texture Optimizer (URP)", _headerStyle);
            EditorGUILayout.Space(2);

            // Tabs
            _activeTab = GUILayout.Toolbar(_activeTab, _tabNames, GUILayout.Height(28));
            EditorGUILayout.Space(6);

            switch (_activeTab)
            {
                case 0: DrawAuditTab(); break;
                case 1: DrawOptimizeTab(); break;
                case 2: DrawCleanupTab(); break;
                case 3: DrawReportTab(); break;
            }
        }

        // =====================================================
        // TAB 0 — AUDIT
        // =====================================================
        private void DrawAuditTab()
        {
            EditorGUILayout.LabelField("Auditoría de Texturas", _headerStyle);
            EditorGUILayout.HelpBox(
                "Escanea todas las texturas del proyecto, las categoriza automáticamente y muestra las " +
                "configuraciones actuales vs. las configuraciones objetivo URP optimizadas.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍  Escanear Proyecto", GUILayout.Height(32)))
                RunAudit();

            GUI.enabled = _auditDone && _auditEntries.Count > 0;
            if (GUILayout.Button("📤  Seleccionar Todo", GUILayout.Width(140), GUILayout.Height(32)))
                _auditEntries.ForEach(e => e.selected = e.needsChange);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (!_auditDone)
            {
                EditorGUILayout.HelpBox("Presiona 'Escanear Proyecto' para comenzar.", MessageType.None);
                return;
            }

            // Stats bar
            int needChange = _auditEntries.Count(e => e.needsChange);
            long totalBytes = _auditEntries.Sum(e => e.fileSizeBytes);
            EditorGUILayout.BeginVertical(_sectionStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"📁 Total texturas: {_auditEntries.Count}", GUILayout.Width(200));
            EditorGUILayout.LabelField($"⚠️  Necesitan cambio: {needChange}", GUILayout.Width(200));
            EditorGUILayout.LabelField($"💾 Tamaño total: {FormatBytes(totalBytes)}", GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // Filters
            EditorGUILayout.BeginHorizontal();
            _auditFilter = EditorGUILayout.TextField("🔎 Filtrar:", _auditFilter, GUILayout.Width(300));
            _auditCategoryFilter = (TextureCategory)EditorGUILayout.EnumPopup("Categoría:", _auditCategoryFilter, GUILayout.Width(280));
            _showOnlyNeedsChange = EditorGUILayout.ToggleLeft("Solo cambios pendientes", _showOnlyNeedsChange);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Column headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.Label("Textura", GUILayout.MinWidth(180));
            GUILayout.Label("Categoría", GUILayout.Width(100));
            GUILayout.Label("Tamaño disco", GUILayout.Width(90));
            GUILayout.Label("MaxSize actual", GUILayout.Width(90));
            GUILayout.Label("MaxSize objetivo", GUILayout.Width(100));
            GUILayout.Label("MipMaps", GUILayout.Width(70));
            GUILayout.Label("Streaming", GUILayout.Width(70));
            GUILayout.Label("Estado", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            _auditScroll = EditorGUILayout.BeginScrollView(_auditScroll);

            var filtered = _auditEntries.Where(e =>
                (string.IsNullOrEmpty(_auditFilter) || e.fileName.ToLower().Contains(_auditFilter.ToLower())) &&
                ((int)_auditCategoryFilter == -1 || e.category == _auditCategoryFilter) &&
                (!_showOnlyNeedsChange || e.needsChange)
            ).ToList();

            foreach (var entry in filtered)
            {
                EditorGUILayout.BeginHorizontal();

                entry.selected = EditorGUILayout.Toggle(entry.selected, GUILayout.Width(20));

                if (GUILayout.Button(entry.fileName, EditorStyles.linkLabel, GUILayout.MinWidth(180)))
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture>(entry.assetPath);

                GUILayout.Label(GetCategoryLabel(entry.category), GUILayout.Width(100));
                GUILayout.Label(FormatBytes(entry.fileSizeBytes), GUILayout.Width(90));
                GUILayout.Label(entry.currentMaxSize.ToString(), GUILayout.Width(90));

                var style = entry.needsChange ? _tagYellow : _tagGreen;
                GUILayout.Label(entry.targetSettings.maxSize.ToString(), style, GUILayout.Width(100));
                GUILayout.Label(entry.targetSettings.mipmapEnabled ? "✅" : "❌", GUILayout.Width(70));
                GUILayout.Label(entry.targetSettings.streamingMipmaps ? "✅" : "❌", GUILayout.Width(70));

                if (entry.needsChange)
                    GUILayout.Label("⚠️ Cambiar", _tagYellow, GUILayout.Width(80));
                else
                    GUILayout.Label("✅ OK", _tagGreen, GUILayout.Width(80));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void RunAudit()
        {
            _auditEntries.Clear();
            var guids = AssetDatabase.FindAssets("t:Texture", new[] { "Assets" });
            int total = guids.Length;
            int done = 0;

            foreach (var guid in guids)
            {
                done++;
                if (done % 50 == 0)
                    EditorUtility.DisplayProgressBar("Auditando texturas...", $"{done}/{total}", (float)done / total);

                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                var category = TextureCategoryRules.Classify(path);
                var target = TextureCategoryRules.GetSettings(category);

                string absPath = Path.Combine(Application.dataPath, "..", path).Replace('/', Path.DirectorySeparatorChar);
                long fileSize = File.Exists(absPath) ? new FileInfo(absPath).Length : 0;

                bool needsChange =
                    importer.maxTextureSize > target.maxSize ||
                    importer.mipmapEnabled != target.mipmapEnabled ||
                    importer.streamingMipmaps != target.streamingMipmaps ||
                    (target.textureType == TextureImporterType.NormalMap && importer.textureType != TextureImporterType.NormalMap);

                _auditEntries.Add(new TextureAuditEntry
                {
                    assetPath = path,
                    fileName = Path.GetFileNameWithoutExtension(path),
                    category = category,
                    targetSettings = target,
                    currentMaxSize = importer.maxTextureSize,
                    currentCompression = importer.textureCompression,
                    currentMipmaps = importer.mipmapEnabled,
                    currentFilterMode = importer.filterMode,
                    currentAniso = importer.anisoLevel,
                    currentStreaming = importer.streamingMipmaps,
                    currentType = importer.textureType,
                    fileSizeBytes = fileSize,
                    needsChange = needsChange,
                    selected = needsChange
                });
            }

            EditorUtility.ClearProgressBar();
            _auditDone = true;
            Debug.Log($"[TextureOptimizer] Auditoría completa: {_auditEntries.Count} texturas, {_auditEntries.Count(e => e.needsChange)} necesitan cambio.");
        }

        // =====================================================
        // TAB 1 — OPTIMIZE
        // =====================================================
        private void DrawOptimizeTab()
        {
            EditorGUILayout.LabelField("Optimización de Import Settings", _headerStyle);

            if (!_auditDone || _auditEntries.Count == 0)
            {
                EditorGUILayout.HelpBox("Primero ejecuta la Auditoría en el tab '📋 Audit'.", MessageType.Warning);
                return;
            }

            int selected = _auditEntries.Count(e => e.selected);
            EditorGUILayout.HelpBox(
                $"Se aplicarán los cambios a {selected} textura(s) seleccionadas.\n" +
                "Modo Dry Run: solo muestra los cambios sin aplicarlos.\n" +
                "Modo Apply: modifica los .meta files y reimporta.",
                MessageType.Info);

            EditorGUILayout.Space(4);

            // Category preview table
            EditorGUILayout.BeginVertical(_sectionStyle);
            EditorGUILayout.LabelField("Configuración por categoría:", EditorStyles.boldLabel);
            DrawCategoryRow("🏛️ Environment", "512", "BC7", "✅", "✅", "Bilinear", "4");
            DrawCategoryRow("🪑 Props pequeños", "256", "DXT", "✅", "✅", "Bilinear", "2");
            DrawCategoryRow("👤 Personaje", "512", "BC7", "✅", "❌", "Trilinear", "8");
            DrawCategoryRow("🎇 Partículas/VFX", "256", "DXT5", "❌", "❌", "Bilinear", "1");
            DrawCategoryRow("🖥️ UI", "1024", "Sin comp.", "❌", "❌", "Bilinear", "1");
            DrawCategoryRow("🗺️ Normal Maps", "512", "BC5", "✅", "✅", "Trilinear", "4");
            DrawCategoryRow("🌅 HDRI/Skybox", "1024", "BC6H", "✅", "❌", "Trilinear", "1");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            _dryRun = EditorGUILayout.Toggle("Dry Run (solo preview)", _dryRun);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            using (new EditorGUI.DisabledScope(selected == 0))
            {
                string btnLabel = _dryRun
                    ? $"🔍  Dry Run — Previsualizar cambios ({selected} texturas)"
                    : $"⚡  Aplicar optimización ({selected} texturas)";

                GUI.backgroundColor = _dryRun ? new Color(0.3f, 0.6f, 1f) : new Color(0.2f, 0.9f, 0.3f);
                if (GUILayout.Button(btnLabel, GUILayout.Height(36)))
                {
                    if (!_dryRun && !EditorUtility.DisplayDialog(
                        "⚡ Confirmar Optimización",
                        $"Se modificarán los import settings de {selected} texturas y se reimportarán.\n\nEsto puede tomar varios minutos. ¿Continuar?",
                        "✅ Aplicar", "Cancelar"))
                    {
                        return;
                    }
                    RunOptimization(_dryRun);
                }
                GUI.backgroundColor = Color.white;
            }

            if (!string.IsNullOrEmpty(_optimizeLog))
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Log de optimización:", EditorStyles.boldLabel);
                _optimizeScroll = EditorGUILayout.BeginScrollView(_optimizeScroll, GUILayout.Height(300));
                EditorGUILayout.TextArea(_optimizeLog, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawCategoryRow(string cat, string maxSize, string comp, string mip, string stream, string filter, string aniso)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(cat, GUILayout.Width(170));
            GUILayout.Label($"Max: {maxSize}", GUILayout.Width(80));
            GUILayout.Label($"Comp: {comp}", GUILayout.Width(90));
            GUILayout.Label($"Mip: {mip}", GUILayout.Width(50));
            GUILayout.Label($"Stream: {stream}", GUILayout.Width(70));
            GUILayout.Label($"Filter: {filter}", GUILayout.Width(90));
            GUILayout.Label($"Aniso: {aniso}", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
        }

        private void RunOptimization(bool dryRun)
        {
            var toProcess = _auditEntries.Where(e => e.selected).ToList();
            var log = new StringBuilder();
            int changed = 0;
            int skipped = 0;

            log.AppendLine($"=== Texture Optimizer — {(dryRun ? "DRY RUN" : "APPLY")} ===");
            log.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            log.AppendLine($"Texturas a procesar: {toProcess.Count}");
            log.AppendLine();

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < toProcess.Count; i++)
                {
                    var entry = toProcess[i];
                    EditorUtility.DisplayProgressBar("Optimizando...", entry.fileName, (float)i / toProcess.Count);

                    var importer = AssetImporter.GetAtPath(entry.assetPath) as TextureImporter;
                    if (importer == null) { skipped++; continue; }

                    var s = entry.targetSettings;
                    log.AppendLine($"[{entry.category}] {entry.fileName}");
                    log.AppendLine($"  MaxSize: {entry.currentMaxSize} → {s.maxSize}");
                    log.AppendLine($"  MipMaps: {entry.currentMipmaps} → {s.mipmapEnabled}");
                    log.AppendLine($"  Streaming: {entry.currentStreaming} → {s.streamingMipmaps}");
                    log.AppendLine($"  FilterMode: {entry.currentFilterMode} → {s.filterMode}");
                    log.AppendLine($"  Aniso: {entry.currentAniso} → {s.anisotropicLevel}");
                    log.AppendLine($"  Compression: {entry.currentCompression} → {s.compression}");

                    if (!dryRun)
                    {
                        importer.maxTextureSize = s.maxSize;
                        importer.mipmapEnabled = s.mipmapEnabled;
                        importer.streamingMipmaps = s.streamingMipmaps;
                        importer.filterMode = s.filterMode;
                        importer.anisoLevel = s.anisotropicLevel;
                        importer.textureCompression = s.compression;

                        if (s.textureType != TextureImporterType.Default)
                            importer.textureType = s.textureType;

                        importer.sRGBTexture = s.srgb;
                        importer.SaveAndReimport();
                    }

                    changed++;
                    log.AppendLine();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
                if (!dryRun) AssetDatabase.Refresh();
            }

            log.AppendLine($"=== RESUMEN ===");
            log.AppendLine($"Modificadas: {changed}");
            log.AppendLine($"Saltadas:    {skipped}");

            _optimizeLog = log.ToString();

            if (!dryRun)
            {
                // Update audit entries
                foreach (var e in toProcess)
                {
                    e.currentMaxSize = e.targetSettings.maxSize;
                    e.currentMipmaps = e.targetSettings.mipmapEnabled;
                    e.currentStreaming = e.targetSettings.streamingMipmaps;
                    e.needsChange = false;
                }
                Debug.Log($"[TextureOptimizer] ✅ Optimización aplicada a {changed} texturas.");
            }
            else
            {
                Debug.Log($"[TextureOptimizer] 🔍 Dry Run completado — {changed} texturas serían modificadas.");
            }
        }

        // =====================================================
        // TAB 2 — CLEANUP
        // =====================================================
        private void DrawCleanupTab()
        {
            EditorGUILayout.LabelField("Limpieza — Duplicados y Sin Uso", _headerStyle);
            EditorGUILayout.HelpBox(
                "Detecta texturas duplicadas (mismo nombre en múltiples rutas), texturas sin referencias " +
                "activas y materiales huérfanos. No elimina nada automáticamente.",
                MessageType.Info);

            if (GUILayout.Button("🔍  Escanear Duplicados y Huérfanos", GUILayout.Height(32)))
                RunCleanupScan();

            if (!_cleanupDone) return;

            _cleanupScroll = EditorGUILayout.BeginScrollView(_cleanupScroll);

            // Duplicates
            if (_duplicates.Count > 0)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField($"⚠️ Texturas Duplicadas ({_duplicates.Count} grupos)", EditorStyles.boldLabel);
                long dupTotal = _duplicates.Sum(d => d.singleSizeBytes * (d.paths.Count - 1));
                EditorGUILayout.LabelField($"Espacio desperdiciado en duplicados: {FormatBytes(dupTotal)}", _tagYellow);
                EditorGUILayout.Space(2);

                foreach (var dup in _duplicates.OrderByDescending(d => d.singleSizeBytes))
                {
                    EditorGUILayout.BeginVertical(_sectionStyle);
                    EditorGUILayout.LabelField($"📄 {dup.fileName}  ({dup.paths.Count}x · {FormatBytes(dup.singleSizeBytes)} c/u)", EditorStyles.boldLabel);
                    foreach (var p in dup.paths)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  " + p, EditorStyles.miniLabel, GUILayout.MinWidth(400));
                        if (GUILayout.Button("Seleccionar", EditorStyles.miniButton, GUILayout.Width(80)))
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture>(p);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            // Unused textures
            if (_unusedTextures.Count > 0)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField($"🗑️ Texturas sin referencia activa ({_unusedTextures.Count})", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Estas texturas no están referenciadas por ningún material ni escena. Revisar manualmente antes de eliminar.", MessageType.Warning);

                foreach (var path in _unusedTextures.Take(50))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(path, EditorStyles.miniLabel, GUILayout.MinWidth(400));
                    if (GUILayout.Button("Seleccionar", EditorStyles.miniButton, GUILayout.Width(80)))
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture>(path);
                    EditorGUILayout.EndHorizontal();
                }

                if (_unusedTextures.Count > 50)
                    EditorGUILayout.LabelField($"  ... y {_unusedTextures.Count - 50} más. Ver reporte HTML completo.", EditorStyles.miniLabel);
            }

            // Unused materials
            if (_unusedMaterials.Count > 0)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField($"🧱 Materiales sin referencia ({_unusedMaterials.Count})", EditorStyles.boldLabel);
                foreach (var path in _unusedMaterials)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(path, EditorStyles.miniLabel, GUILayout.MinWidth(400));
                    if (GUILayout.Button("Seleccionar", EditorStyles.miniButton, GUILayout.Width(80)))
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Material>(path);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void RunCleanupScan()
        {
            _duplicates.Clear();
            _unusedTextures.Clear();
            _unusedMaterials.Clear();

            // --- Duplicates by file name ---
            var allTextures = AssetDatabase.FindAssets("t:Texture", new[] { "Assets" })
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .ToList();

            var byName = allTextures
                .GroupBy(p => Path.GetFileName(p).ToLowerInvariant())
                .Where(g => g.Count() > 1);

            foreach (var group in byName)
            {
                string first = group.First();
                string absPath = Path.Combine(Application.dataPath, "..", first);
                long size = File.Exists(absPath) ? new FileInfo(absPath).Length : 0;

                _duplicates.Add(new TextureDuplicateGroup
                {
                    fileName = Path.GetFileNameWithoutExtension(first),
                    paths = group.ToList(),
                    singleSizeBytes = size
                });
            }

            // --- Find all GUIDs referenced in materials and scenes ---
            var referencedGuids = new HashSet<string>();

            // Check materials
            var matPaths = AssetDatabase.FindAssets("t:Material", new[] { "Assets" })
                .Select(g => AssetDatabase.GUIDToAssetPath(g));

            foreach (var matPath in matPaths)
            {
                var deps = AssetDatabase.GetDependencies(matPath, false);
                foreach (var dep in deps)
                {
                    var guid = AssetDatabase.AssetPathToGUID(dep);
                    if (!string.IsNullOrEmpty(guid))
                        referencedGuids.Add(guid);
                }
            }

            // Check scenes
            var scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(g => AssetDatabase.GUIDToAssetPath(g));

            foreach (var scenePath in scenePaths)
            {
                var deps = AssetDatabase.GetDependencies(scenePath, true);
                foreach (var dep in deps)
                {
                    var guid = AssetDatabase.AssetPathToGUID(dep);
                    if (!string.IsNullOrEmpty(guid))
                        referencedGuids.Add(guid);
                }
            }

            // Check prefabs
            var prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(g => AssetDatabase.GUIDToAssetPath(g));

            foreach (var prefabPath in prefabPaths)
            {
                var deps = AssetDatabase.GetDependencies(prefabPath, false);
                foreach (var dep in deps)
                {
                    var guid = AssetDatabase.AssetPathToGUID(dep);
                    if (!string.IsNullOrEmpty(guid))
                        referencedGuids.Add(guid);
                }
            }

            // Find unused textures
            foreach (var texPath in allTextures)
            {
                var guid = AssetDatabase.AssetPathToGUID(texPath);
                if (!referencedGuids.Contains(guid))
                    _unusedTextures.Add(texPath);
            }

            // Find unused materials
            var allMats = AssetDatabase.FindAssets("t:Material", new[] { "Assets" })
                .Select(g => AssetDatabase.GUIDToAssetPath(g));

            foreach (var matPath in allMats)
            {
                var guid = AssetDatabase.AssetPathToGUID(matPath);
                if (!referencedGuids.Contains(guid))
                    _unusedMaterials.Add(matPath);
            }

            _cleanupDone = true;
            Debug.Log($"[TextureOptimizer] Scan completo — {_duplicates.Count} grupos de duplicados, {_unusedTextures.Count} texturas sin uso, {_unusedMaterials.Count} materiales sin uso.");
        }

        // =====================================================
        // TAB 3 — REPORT
        // =====================================================
        private void DrawReportTab()
        {
            EditorGUILayout.LabelField("Reporte de Optimización", _headerStyle);
            EditorGUILayout.HelpBox(
                "Genera un reporte HTML completo con el estado del proyecto, cambios aplicados, " +
                "texturas modificadas, materiales reparados y espacio ahorrado.",
                MessageType.Info);

            EditorGUILayout.Space(4);

            if (!_auditDone)
            {
                EditorGUILayout.HelpBox("Ejecuta primero la Auditoría y la limpieza para generar el reporte completo.", MessageType.Warning);
            }

            GUI.backgroundColor = new Color(0.2f, 0.7f, 1f);
            if (GUILayout.Button("📊  Generar Reporte HTML", GUILayout.Height(36)))
                GenerateHtmlReport();
            GUI.backgroundColor = Color.white;

            if (!string.IsNullOrEmpty(_lastReportPath))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical(_sectionStyle);
                EditorGUILayout.LabelField("✅ Reporte generado:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(_lastReportPath, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("📂  Abrir en Explorador", GUILayout.Width(180)))
                    EditorUtility.RevealInFinder(_lastReportPath);
                if (GUILayout.Button("🌐  Abrir en Navegador", GUILayout.Width(180)))
                    Application.OpenURL($"file:///{_lastReportPath.Replace('\\', '/')}");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            _reportScroll = EditorGUILayout.BeginScrollView(_reportScroll);
            if (_auditDone)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Vista previa de estadísticas:", EditorStyles.boldLabel);

                int total = _auditEntries.Count;
                int needChange = _auditEntries.Count(e => e.needsChange);
                int optimized = _auditEntries.Count(e => !e.needsChange && e.currentMaxSize <= e.targetSettings.maxSize);
                long totalSize = _auditEntries.Sum(e => e.fileSizeBytes);

                // By category
                foreach (TextureCategory cat in Enum.GetValues(typeof(TextureCategory)))
                {
                    var catEntries = _auditEntries.Where(e => e.category == cat).ToList();
                    if (catEntries.Count == 0) continue;

                    EditorGUILayout.BeginHorizontal(_sectionStyle);
                    EditorGUILayout.LabelField(GetCategoryLabel(cat), GUILayout.Width(180));
                    EditorGUILayout.LabelField($"{catEntries.Count} texturas", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"{FormatBytes(catEntries.Sum(e => e.fileSizeBytes))}", GUILayout.Width(100));
                    int needChangeInCat = catEntries.Count(e => e.needsChange);
                    if (needChangeInCat > 0)
                        EditorGUILayout.LabelField($"⚠️ {needChangeInCat} pendientes", _tagYellow, GUILayout.Width(150));
                    else
                        EditorGUILayout.LabelField("✅ Todas OK", _tagGreen, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void GenerateHtmlReport()
        {
            string reportDir = Path.Combine(Application.dataPath, "..", "Reports");
            Directory.CreateDirectory(reportDir);
            string reportPath = Path.Combine(reportDir, $"TextureOptimization_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'>");
            sb.AppendLine("<title>Echoes of You — Texture Optimization Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:'Segoe UI',sans-serif;background:#0f1117;color:#e0e0e0;margin:0;padding:20px}");
            sb.AppendLine("h1{color:#7dd3fc;font-size:1.8em;border-bottom:2px solid #334155;padding-bottom:10px}");
            sb.AppendLine("h2{color:#a78bfa;margin-top:30px}");
            sb.AppendLine(".stats{display:flex;gap:20px;flex-wrap:wrap;margin:20px 0}");
            sb.AppendLine(".stat-card{background:#1e293b;border:1px solid #334155;border-radius:8px;padding:16px 24px;min-width:150px}");
            sb.AppendLine(".stat-card .num{font-size:2em;font-weight:700;color:#38bdf8}");
            sb.AppendLine(".stat-card .lbl{font-size:0.85em;color:#94a3b8;margin-top:4px}");
            sb.AppendLine("table{width:100%;border-collapse:collapse;margin-top:10px;font-size:0.88em}");
            sb.AppendLine("th{background:#1e293b;padding:8px 12px;text-align:left;color:#7dd3fc;border-bottom:2px solid #334155}");
            sb.AppendLine("td{padding:7px 12px;border-bottom:1px solid #1e293b}");
            sb.AppendLine("tr:hover td{background:#1a2332}");
            sb.AppendLine(".ok{color:#4ade80}.warn{color:#fbbf24}.bad{color:#f87171}");
            sb.AppendLine(".tag{display:inline-block;padding:2px 8px;border-radius:4px;font-size:0.8em;font-weight:600}");
            sb.AppendLine(".tag-env{background:#1d4ed8;color:#bfdbfe}.tag-prop{background:#15803d;color:#bbf7d0}");
            sb.AppendLine(".tag-char{background:#7c3aed;color:#ede9fe}.tag-vfx{background:#b45309;color:#fef3c7}");
            sb.AppendLine(".tag-ui{background:#be185d;color:#fce7f3}.tag-norm{background:#0f766e;color:#ccfbf1}");
            sb.AppendLine(".tag-hdri{background:#1e40af;color:#dbeafe}.tag-unk{background:#374151;color:#d1d5db}");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine($"<h1>🎮 Echoes of You — Texture Optimization Report</h1>");
            sb.AppendLine($"<p style='color:#64748b'>Generado: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Proyecto: Echoes of You URP</p>");

            // Stats
            int total = _auditEntries.Count;
            int needChange = _auditEntries.Count(e => e.needsChange);
            int ok = total - needChange;
            long totalSizeBytes = _auditEntries.Sum(e => e.fileSizeBytes);
            int dupCount = _duplicates.Count;
            long dupWaste = _duplicates.Sum(d => d.singleSizeBytes * (d.paths.Count - 1));
            int unusedTex = _unusedTextures.Count;
            int unusedMat = _unusedMaterials.Count;

            sb.AppendLine("<div class='stats'>");
            sb.AppendLine($"<div class='stat-card'><div class='num'>{total}</div><div class='lbl'>Total Texturas</div></div>");
            sb.AppendLine($"<div class='stat-card'><div class='num' style='color:#4ade80'>{ok}</div><div class='lbl'>Optimizadas OK</div></div>");
            sb.AppendLine($"<div class='stat-card'><div class='num' style='color:#fbbf24'>{needChange}</div><div class='lbl'>Pendientes de cambio</div></div>");
            sb.AppendLine($"<div class='stat-card'><div class='num'>{FormatBytes(totalSizeBytes)}</div><div class='lbl'>Tamaño total disco</div></div>");
            sb.AppendLine($"<div class='stat-card'><div class='num' style='color:#f87171'>{dupCount}</div><div class='lbl'>Grupos duplicados</div></div>");
            sb.AppendLine($"<div class='stat-card'><div class='num' style='color:#f87171'>{FormatBytes(dupWaste)}</div><div class='lbl'>Desperdicio duplicados</div></div>");
            sb.AppendLine($"<div class='stat-card'><div class='num' style='color:#f87171'>{unusedTex}</div><div class='lbl'>Texturas sin uso</div></div>");
            sb.AppendLine($"<div class='stat-card'><div class='num' style='color:#f87171'>{unusedMat}</div><div class='lbl'>Materiales sin uso</div></div>");
            sb.AppendLine("</div>");

            // Texture audit table
            sb.AppendLine("<h2>📋 Auditoría de Texturas</h2>");
            sb.AppendLine("<table><tr><th>Textura</th><th>Categoría</th><th>Tamaño disco</th><th>MaxSize actual</th><th>MaxSize objetivo</th><th>MipMaps</th><th>Streaming</th><th>Estado</th></tr>");

            foreach (var e in _auditEntries.OrderBy(x => x.category).ThenByDescending(x => x.fileSizeBytes))
            {
                string stateClass = e.needsChange ? "warn" : "ok";
                string stateLabel = e.needsChange ? "⚠️ Cambiar" : "✅ OK";
                string catTag = GetCategoryHtmlTag(e.category);

                sb.AppendLine($"<tr><td>{Path.GetFileName(e.assetPath)}</td>");
                sb.AppendLine($"<td>{catTag}</td>");
                sb.AppendLine($"<td>{FormatBytes(e.fileSizeBytes)}</td>");
                sb.AppendLine($"<td>{e.currentMaxSize}</td>");
                sb.AppendLine($"<td class='{(e.targetSettings.maxSize < e.currentMaxSize ? "ok" : "")}'>{e.targetSettings.maxSize}</td>");
                sb.AppendLine($"<td>{(e.targetSettings.mipmapEnabled ? "✅" : "❌")}</td>");
                sb.AppendLine($"<td>{(e.targetSettings.streamingMipmaps ? "✅" : "❌")}</td>");
                sb.AppendLine($"<td class='{stateClass}'>{stateLabel}</td></tr>");
            }
            sb.AppendLine("</table>");

            // Duplicates section
            if (_duplicates.Count > 0)
            {
                sb.AppendLine("<h2>⚠️ Texturas Duplicadas</h2>");
                sb.AppendLine("<table><tr><th>Nombre</th><th>Copias</th><th>Tamaño c/u</th><th>Desperdicio</th><th>Rutas</th></tr>");
                foreach (var dup in _duplicates.OrderByDescending(d => d.singleSizeBytes))
                {
                    long waste = dup.singleSizeBytes * (dup.paths.Count - 1);
                    string pathList = string.Join("<br>", dup.paths.Select(p => $"<small>{p}</small>"));
                    sb.AppendLine($"<tr><td>{dup.fileName}</td><td class='warn'>{dup.paths.Count}x</td><td>{FormatBytes(dup.singleSizeBytes)}</td><td class='bad'>{FormatBytes(waste)}</td><td>{pathList}</td></tr>");
                }
                sb.AppendLine("</table>");
            }

            // Unused textures
            if (_unusedTextures.Count > 0)
            {
                sb.AppendLine("<h2>🗑️ Texturas Sin Uso</h2>");
                sb.AppendLine("<table><tr><th>Ruta</th></tr>");
                foreach (var p in _unusedTextures)
                    sb.AppendLine($"<tr><td><small>{p}</small></td></tr>");
                sb.AppendLine("</table>");
            }

            // Unused materials
            if (_unusedMaterials.Count > 0)
            {
                sb.AppendLine("<h2>🧱 Materiales Sin Uso</h2>");
                sb.AppendLine("<table><tr><th>Ruta</th></tr>");
                foreach (var p in _unusedMaterials)
                    sb.AppendLine($"<tr><td><small>{p}</small></td></tr>");
                sb.AppendLine("</table>");
            }

            sb.AppendLine("</body></html>");

            File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
            _lastReportPath = reportPath;
            Debug.Log($"[TextureOptimizer] 📊 Reporte generado: {reportPath}");
        }

        // =====================================================
        // Helpers
        // =====================================================
        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024 * 1024) return $"{bytes / 1024f / 1024f:F1} MB";
            if (bytes >= 1024) return $"{bytes / 1024f:F1} KB";
            return $"{bytes} B";
        }

        private static string GetCategoryLabel(TextureCategory cat) => cat switch
        {
            TextureCategory.Environment => "🏛️ Environment",
            TextureCategory.PropSmall => "🪑 Props",
            TextureCategory.Character => "👤 Character",
            TextureCategory.ParticleVFX => "🎇 VFX",
            TextureCategory.UI => "🖥️ UI",
            TextureCategory.NormalMap => "🗺️ Normal Map",
            TextureCategory.HDRISkybox => "🌅 HDRI",
            _ => "❓ Unknown"
        };

        private static string GetCategoryHtmlTag(TextureCategory cat)
        {
            var (label, css) = cat switch
            {
                TextureCategory.Environment => ("Environment", "env"),
                TextureCategory.PropSmall => ("Props", "prop"),
                TextureCategory.Character => ("Character", "char"),
                TextureCategory.ParticleVFX => ("VFX", "vfx"),
                TextureCategory.UI => ("UI", "ui"),
                TextureCategory.NormalMap => ("Normal Map", "norm"),
                TextureCategory.HDRISkybox => ("HDRI", "hdri"),
                _ => ("Unknown", "unk")
            };
            return $"<span class='tag tag-{css}'>{label}</span>";
        }
    }
}
