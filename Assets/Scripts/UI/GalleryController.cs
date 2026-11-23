using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    public class GalleryController
    {
        static readonly (string path, string section)[] Folders =
        {
            ("VN/Sprites/aiden", "Aiden"),
            ("VN/Sprites/lyra", "Lyra"),
            ("UI/VN/Sprites/aiden", "Aiden"),
            ("UI/VN/Sprites/lyra", "Lyra"),
            ("UI/Images", "Arte del juego"),
            ("UI/Tutorial", "La escuela"),
        };

        static readonly string[] Sections = { "Aiden", "Lyra", "La escuela", "Arte del juego" };

        class Entry
        {
            public Texture2D Texture;
            public Sprite Sprite;
            public string Title;
            public string Section;
            public int Width => Sprite != null ? (int)Sprite.rect.width : Texture.width;
            public int Height => Sprite != null ? (int)Sprite.rect.height : Texture.height;

            public void Display(Image image)
            {
                if (Sprite != null)
                    image.sprite = Sprite;
                else
                    image.image = Texture;
            }
        }

        readonly List<Entry> _catalog = new();
        readonly List<Entry> _visibleEntries = new();
        readonly List<Button> _tiles = new();

        VisualElement _panel;
        Image _image;
        Label _counter;
        Label _caption;
        Label _meta;
        Label _empty;
        Button _prev;
        Button _next;
        ScrollView _indexScroll;
        VisualElement _grid;
        int _index;

        public int Count => _catalog.Count;

        public void Attach(VisualElement panel)
        {
            if (panel == null || panel == _panel) return;

            _panel = panel;
            _image = panel.Q<Image>("gallery-image");
            _counter = panel.Q<Label>("lbl-gallery-counter");
            _caption = panel.Q<Label>("lbl-gallery-caption");
            _meta = panel.Q<Label>("lbl-gallery-meta");
            _empty = panel.Q<Label>("lbl-gallery-empty");
            _prev = panel.Q<Button>("btn-gallery-prev");
            _next = panel.Q<Button>("btn-gallery-next");
            _indexScroll = panel.Q<ScrollView>("gallery-index");
            _grid = panel.Q("gallery-grid");

            _prev.clicked += () => Show(_index - 1);
            _next.clicked += () => Show(_index + 1);
            _panel.focusable = true;
            _panel.RegisterCallback<KeyDownEvent>(OnKeyDown);

            var filters = panel.Q("gallery-filters");
            AddFilter(filters, "Todas", null);
            foreach (string section in Sections)
                AddFilter(filters, section, section);

            if (_catalog.Count == 0)
                BuildCatalog();
            SelectSection(null);
        }

        void AddFilter(VisualElement filters, string title, string section)
        {
            var button = new Button { text = title, userData = section };
            button.AddToClassList("gallery-filter");
            button.clicked += () => SelectSection(section);
            filters.Add(button);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.target is VisualElement target && target.ClassListContains("gallery-filter")) return;
            if (evt.keyCode != KeyCode.LeftArrow && evt.keyCode != KeyCode.RightArrow) return;
            Show(_index + (evt.keyCode == KeyCode.LeftArrow ? -1 : 1));
            evt.StopPropagation();
        }

        void BuildCatalog()
        {
            var byName = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            foreach (var (path, section) in Folders)
            {
                var spriteTextures = new HashSet<Texture2D>();
                foreach (var sprite in Resources.LoadAll<Sprite>(path))
                {
                    spriteTextures.Add(sprite.texture);
                    AddEntry(byName, sprite.name, section, sprite.texture, sprite);
                }

                foreach (var texture in Resources.LoadAll<Texture2D>(path))
                    if (!spriteTextures.Contains(texture))
                        AddEntry(byName, texture.name, section, texture);
            }

            var logo = Resources.Load<Texture2D>("UI/logo");
            if (logo != null) AddEntry(byName, logo.name, "Arte del juego", logo);

            _catalog.AddRange(byName.Values);
            _catalog.Sort((left, right) =>
            {
                int section = Array.IndexOf(Sections, left.Section).CompareTo(Array.IndexOf(Sections, right.Section));
                return section != 0 ? section : string.Compare(left.Title, right.Title, StringComparison.OrdinalIgnoreCase);
            });
        }

        static void AddEntry(Dictionary<string, Entry> entries, string name, string section, Texture2D texture, Sprite sprite = null)
        {
            var entry = new Entry { Texture = texture, Sprite = sprite, Title = PrettyTitle(name), Section = section };
            if (entry.Width <= 1 || entry.Height <= 1) return;

            string key = section + "/" + name;
            if (!entries.TryGetValue(key, out var existing) ||
                entry.Width * entry.Height > existing.Width * existing.Height)
                entries[key] = entry;
        }

        static string PrettyTitle(string name)
        {
            if (name.StartsWith("Aiden_", StringComparison.OrdinalIgnoreCase))
                name = name.Substring("Aiden_".Length);
            else if (name.StartsWith("Lyra_", StringComparison.OrdinalIgnoreCase))
                name = name.Substring("Lyra_".Length);

            name = name.Replace('_', ' ').Replace('-', ' ').Trim();
            return name.Length > 0 ? char.ToUpperInvariant(name[0]) + name.Substring(1) : "Sin título";
        }

        void SelectSection(string section)
        {
            foreach (var filter in _panel.Q("gallery-filters").Children())
                filter.EnableInClassList("gallery-filter--active", Equals(filter.userData, section));

            _grid.Clear();
            _tiles.Clear();
            _visibleEntries.Clear();
            foreach (var entry in _catalog)
            {
                if (section != null && entry.Section != section) continue;

                int index = _visibleEntries.Count;
                _visibleEntries.Add(entry);

                var tile = new Button(() => Show(index)) { tooltip = entry.Section + " · " + entry.Title };
                tile.AddToClassList("gallery-tile");
                var image = new Image { scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
                image.AddToClassList("gallery-tile-image");
                entry.Display(image);
                tile.Add(image);

                var caption = new Label(entry.Title) { pickingMode = PickingMode.Ignore };
                caption.AddToClassList("gallery-tile-label");
                tile.Add(caption);

                _tiles.Add(tile);
                _grid.Add(tile);
            }

            _indexScroll.scrollOffset = Vector2.zero;
            Show(0);
        }

        void Show(int index)
        {
            int count = _visibleEntries.Count;
            _empty.EnableInClassList("hidden", count > 0);
            _image.EnableInClassList("hidden", count == 0);
            _prev.SetEnabled(count > 1);
            _next.SetEnabled(count > 1);
            _caption.text = string.Empty;
            _meta.text = string.Empty;
            if (count == 0)
            {
                _counter.text = "0 imágenes";
                return;
            }

            _index = (index % count + count) % count;
            _counter.text = $"{_index + 1} / {count}";
            Entry entry = _visibleEntries[_index];
            entry.Display(_image);
            _caption.text = entry.Title;
            _meta.text = entry.Section;

            for (int i = 0; i < _tiles.Count; i++)
                _tiles[i].EnableInClassList("gallery-tile--active", i == _index);

            var selected = _tiles[_index];
            selected.schedule.Execute(() =>
            {
                if (selected.panel != null && selected == _tiles[_index])
                    _indexScroll.ScrollTo(selected);
            });
        }
    }
}
