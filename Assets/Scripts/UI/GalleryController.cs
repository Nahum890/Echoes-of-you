using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// GalleryController — carrusel de las imágenes del juego.
    ///
    /// Se engancha al panel <c>panel-gallery</c> del menú principal y monta un
    /// carrusel: imagen grande, flechas a los lados, contador, pie de foto y una
    /// tira de miniaturas navegable.
    ///
    /// El catálogo se construye en runtime con <see cref="Resources.LoadAll"/>
    /// sobre las carpetas de arte, así que añadir un .png a cualquiera de ellas lo
    /// mete en la galería sin tocar código ni UXML.
    ///
    /// Dos filtros que no son opcionales, comprobados contra los assets del repo:
    ///
    /// - <b>Tamaño mínimo.</b> Los ocho sprites de <c>UI/VN/Sprites/</c> son de
    ///   1×1 píxel: placeholders que nunca se rellenaron. Sin el filtro, la
    ///   galería enseñaba ocho cuadros vacíos. Si algún día se sustituyen por arte
    ///   de verdad, entran solas.
    /// - <b>Deduplicado por nombre.</b> <c>void_fog_bg</c> y
    ///   <c>fragmenting_silhouette</c> están tanto en <c>UI/</c> como en
    ///   <c>UI/Images/</c>, y <c>Tutorial_Level01</c> existe en .png y en .jpg.
    ///   Se queda la copia de mayor resolución.
    /// </summary>
    public class GalleryController
    {
        /// Por debajo de esto se considera un placeholder, no arte.
        const int MinPixelSize = 32;

        /// <summary>Carpetas de Resources que alimentan la galería, con la sección
        /// bajo la que se agrupan en el pie de foto.</summary>
        static readonly (string resourcePath, string section)[] Folders =
        {
            ("VN/Sprites/aiden",     "Aiden"),
            ("VN/Sprites/lyra",      "Lyra"),
            ("UI/VN/Sprites/aiden",  "Aiden"),
            ("UI/VN/Sprites/lyra",   "Lyra"),
            ("UI/Images",            "Arte del juego"),
            ("UI/Tutorial",          "La escuela"),
        };

        /// Imágenes sueltas que no viven en una carpeta propia.
        static readonly (string resourcePath, string section)[] SingleImages =
        {
            ("UI/logo", "Arte del juego"),
        };

        class Entry
        {
            public Texture2D texture;
            public string title;
            public string section;
            public int Pixels => texture != null ? texture.width * texture.height : 0;
        }

        readonly List<Entry> _entries = new List<Entry>();
        readonly List<Button> _thumbButtons = new List<Button>();

        VisualElement _panel;
        Image _image;
        Label _counter;
        Label _caption;
        Label _meta;
        Label _empty;
        Button _prev;
        Button _next;
        ScrollView _thumbs;

        int _index;
        bool _built;

        public int Count => _entries.Count;

        /// <summary>
        /// Monta la galería sobre el panel indicado. Es idempotente: llamarlo otra
        /// vez sobre el mismo panel solo refresca la vista, no duplica miniaturas
        /// ni vuelve a suscribir los botones.
        /// </summary>
        public void Attach(VisualElement panel)
        {
            if (panel == null) return;

            if (_built && _panel == panel)
            {
                Show(_index);
                return;
            }

            _panel = panel;
            _image   = panel.Q<Image>("gallery-image");
            _counter = panel.Q<Label>("lbl-gallery-counter");
            _caption = panel.Q<Label>("lbl-gallery-caption");
            _meta    = panel.Q<Label>("lbl-gallery-meta");
            _empty   = panel.Q<Label>("lbl-gallery-empty");
            _prev    = panel.Q<Button>("btn-gallery-prev");
            _next    = panel.Q<Button>("btn-gallery-next");
            _thumbs  = panel.Q<ScrollView>("gallery-thumbs");

            if (_image != null)
                _image.scaleMode = ScaleMode.ScaleToFit;

            BuildCatalog();

            if (_prev != null) _prev.clicked += Previous;
            if (_next != null) _next.clicked += Next;

            // Flechas del teclado. focusable + RegisterCallback en el propio panel:
            // así funciona sin robarle el foco a la navegación del menú.
            panel.focusable = true;
            panel.RegisterCallback<KeyDownEvent>(OnKeyDown);

            BuildThumbnails();

            _built = true;
            Show(0);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.LeftArrow)       { Previous(); evt.StopPropagation(); }
            else if (evt.keyCode == KeyCode.RightArrow) { Next();     evt.StopPropagation(); }
        }

        // ── Catálogo ──────────────────────────────────────────────────────

        void BuildCatalog()
        {
            _entries.Clear();

            // Indexado por nombre para quedarse con la copia de mayor resolución
            // cuando la misma imagen aparece en dos carpetas o en dos formatos.
            var byName = new Dictionary<string, Entry>();

            foreach (var (path, section) in Folders)
            {
                Texture2D[] textures = Resources.LoadAll<Texture2D>(path);
                if (textures == null) continue;
                foreach (Texture2D tex in textures)
                    Consider(byName, tex, section);
            }

            foreach (var (path, section) in SingleImages)
                Consider(byName, Resources.Load<Texture2D>(path), section);

            _entries.AddRange(byName.Values);

            // Orden estable: primero por sección (en el orden declarado arriba),
            // luego alfabético. Sin esto el orden depende de Resources.LoadAll y
            // cambia entre plataformas.
            _entries.Sort((a, b) =>
            {
                int sa = SectionOrder(a.section);
                int sb = SectionOrder(b.section);
                if (sa != sb) return sa.CompareTo(sb);
                return string.Compare(a.title, b.title, System.StringComparison.OrdinalIgnoreCase);
            });
        }

        void Consider(Dictionary<string, Entry> byName, Texture2D tex, string section)
        {
            if (tex == null) return;
            if (tex.width < MinPixelSize || tex.height < MinPixelSize) return;

            var entry = new Entry { texture = tex, title = PrettyTitle(tex.name), section = section };

            if (byName.TryGetValue(tex.name, out Entry existing))
            {
                if (entry.Pixels > existing.Pixels)
                    byName[tex.name] = entry;
                return;
            }

            byName[tex.name] = entry;
        }

        static int SectionOrder(string section)
        {
            switch (section)
            {
                case "Aiden": return 0;
                case "Lyra": return 1;
                case "Arte del juego": return 2;
                case "La escuela": return 3;
                default: return 4;
            }
        }

        /// <summary>
        /// "Aiden_pensativa_preocupada" → "Pensativa preocupada".
        /// El personaje ya se muestra aparte como sección, así que quitarlo del
        /// título evita leer "Aiden — Aiden pensativa".
        /// </summary>
        static string PrettyTitle(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "Sin título";

            string name = rawName;
            foreach (string prefix in new[] { "Aiden_", "Lyra_" })
            {
                if (name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(prefix.Length);
                    break;
                }
            }

            name = name.Replace('_', ' ').Replace('-', ' ').Trim();
            if (name.Length == 0) return rawName;

            return char.ToUpperInvariant(name[0]) + (name.Length > 1 ? name.Substring(1) : "");
        }

        // ── Miniaturas ────────────────────────────────────────────────────

        void BuildThumbnails()
        {
            if (_thumbs == null) return;

            _thumbs.Clear();
            _thumbButtons.Clear();

            for (int i = 0; i < _entries.Count; i++)
            {
                int captured = i; // capturar el índice, no la variable del bucle
                Entry entry = _entries[i];

                var thumb = new Button { name = "gallery-thumb-" + i };
                thumb.AddToClassList("gallery-thumb");
                thumb.tooltip = entry.title;

                var img = new Image
                {
                    image = entry.texture,
                    scaleMode = ScaleMode.ScaleAndCrop,
                    pickingMode = PickingMode.Ignore
                };
                img.AddToClassList("gallery-thumb-img");
                thumb.Add(img);

                thumb.clicked += () => Show(captured);

                _thumbs.Add(thumb);
                _thumbButtons.Add(thumb);
            }
        }

        // ── Navegación ────────────────────────────────────────────────────

        public void Next() => Show(_index + 1);
        public void Previous() => Show(_index - 1);

        public void Show(int index)
        {
            bool hasImages = _entries.Count > 0;

            _empty?.EnableInClassList("hidden", hasImages);
            _image?.EnableInClassList("hidden", !hasImages);
            if (_prev != null) _prev.SetEnabled(_entries.Count > 1);
            if (_next != null) _next.SetEnabled(_entries.Count > 1);

            if (!hasImages)
            {
                if (_counter != null) _counter.text = "0 / 0";
                if (_caption != null) _caption.text = "";
                if (_meta != null) _meta.text = "";
                return;
            }

            // Envolvente en los dos sentidos: desde la primera, "anterior" lleva a
            // la última. Un carrusel que se atasca en los extremos se siente roto.
            _index = ((index % _entries.Count) + _entries.Count) % _entries.Count;
            Entry entry = _entries[_index];

            if (_image != null) _image.image = entry.texture;
            if (_counter != null) _counter.text = (_index + 1) + " / " + _entries.Count;
            if (_caption != null) _caption.text = entry.section + " — " + entry.title;
            if (_meta != null) _meta.text = entry.texture.width + " × " + entry.texture.height;

            HighlightThumbnail(_index);
        }

        void HighlightThumbnail(int index)
        {
            for (int i = 0; i < _thumbButtons.Count; i++)
                _thumbButtons[i].EnableInClassList("gallery-thumb--active", i == index);

            if (_thumbs == null || index < 0 || index >= _thumbButtons.Count) return;

            // Traer la miniatura activa a la vista.
            //
            // ScrollTo necesita un panel vivo y un layout ya resuelto. En el primer
            // Show() todavía no hay ninguna de las dos cosas (y en un árbol clonado
            // fuera de pantalla no las habrá nunca), así que se comprueban antes:
            // el resaltado de la miniatura es lo importante, el auto-scroll es un
            // extra que nunca debe tumbar la galería.
            Button target = _thumbButtons[index];
            if (target.panel == null) return;

            Rect bounds = target.worldBound;
            if (float.IsNaN(bounds.width) || bounds.width <= 0f)
            {
                target.schedule.Execute(() =>
                {
                    if (target.panel != null) SafeScrollTo(target);
                }).ExecuteLater(0);
                return;
            }

            SafeScrollTo(target);
        }

        void SafeScrollTo(VisualElement target)
        {
            try { _thumbs.ScrollTo(target); }
            catch (System.Exception) { /* el auto-scroll no es crítico */ }
        }
    }
}
