using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// TutorialController — Mensajes 1 acción, máx 2 líneas.
    /// Desaparecen al ejecutar acción. No reaparecen (PlayerPrefs flag por tutorial).
    /// Accesible desde Pausa → "Controles".
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        [Header("Templates")]
        [SerializeField] VisualTreeAsset _tutorialTemplate; // TutorialUI.uxml

        VisualElement _root;
        VisualElement _tutorialBox;
        Label _tutorialTitle;
        Label _tutorialText;
        string _currentTutorialId;

        void Awake()
        {
            // Singleton for scene access
        }

        public void Setup(VisualElement container, VisualTreeAsset template)
        {
            _tutorialTemplate = template;
            container.Clear();
            var panel = _tutorialTemplate.CloneTree();
            container.Add(panel);
            _root = panel;
            _root.RemoveFromClassList("hidden");
        }

        public void ShowTutorial(string tutorialId, string title, string text)
        {
            // Check if already shown (PlayerPrefs flag)
            string flag = $"TutorialShown_{tutorialId}";
            if (PlayerPrefs.GetInt(flag, 0) == 1) return;

            if (_root == null || _tutorialTemplate == null) return;

            _tutorialBox = _root.Q("tutorialBox");
            _tutorialTitle = _root.Q<Label>("tutorialTitle");
            _tutorialText = _root.Q<Label>("tutorialText");

            if (_tutorialTitle != null) _tutorialTitle.text = title;
            if (_tutorialText != null) _tutorialText.text = text;
            _currentTutorialId = tutorialId;

            _root.RemoveFromClassList("hidden");
            _tutorialBox?.RemoveFromClassList("hidden");
        }

        public void MarkAsShown(string tutorialId = null)
        {
            string id = tutorialId ?? _currentTutorialId;
            if (!string.IsNullOrEmpty(id))
            {
                PlayerPrefs.SetInt($"TutorialShown_{id}", 1);
                PlayerPrefs.Save();
            }
            Hide();
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.AddToClassList("hidden");
            }
        }

        // Static helpers for common tutorials
        public void ShowRecordTutorial()
        {
            ShowTutorial("record", "Grabar", "Mantén [R] para grabar tus movimientos.");
        }

        public void ShowPlaybackTutorial()
        {
            ShowTutorial("playback", "Proyectar Eco", "Pulsa [R] para proyectar el Eco.");
        }

        public void ShowPlateTutorial()
        {
            ShowTutorial("plate", "Placa de Presión", "Tu Eco puede pisar la placa por ti.");
        }

        public void ShowDoorTutorial()
        {
            ShowTutorial("door", "Puerta", "Activa la placa para abrir la puerta.");
        }

        public void ShowControlsReference()
        {
            // Shows a summary of controls - accessible from Pause menu
            string text = "Grabar: Mantener [R]\nProyectar Eco: [R]\nMover: [WASD] / [Stick]\nSaltar: [Espacio] / [A]\nPausa: [Esc] / [Start]";
            ShowTutorial("controls_ref", "Controles", text);
        }
    }
}