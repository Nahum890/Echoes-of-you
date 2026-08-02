using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// LoadingController — gestiona el indicador de carga (EchoLoading).
    /// 
    /// Estados: loading (progress 0-1), complete, error.
    /// Automáticamente Push/Pop vía NavigationManager (layer Loading).
    /// 
    /// Uso:
    ///   LoadingController.Instance.Show("Preparando entorno de recuerdos...", progress: 0.3f);
    ///   LoadingController.Instance.SetProgress(0.7f);
    ///   LoadingController.Instance.Complete("Completado");
    ///   LoadingController.Instance.Error("Error de carga");
    /// </summary>
    public class LoadingController : MonoBehaviour
    {
        public static LoadingController Instance { get; private set; }

        [Header("Templates")]
        [SerializeField] VisualTreeAsset _loadingTemplate; // EchoLoading.uxml

        VisualElement _root;
        VisualElement _loadingElement;
        VisualElement _fillBar;
        Label _titleLabel;
        bool _active;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Setup(VisualElement uiRoot, VisualTreeAsset template)
        {
            _root = uiRoot;
            _loadingTemplate = template;
        }

        public void Show(string title, float progress = 0f)
        {
            if (_loadingTemplate == null || _root == null) return;

            HideLoading();

            _loadingElement = _loadingTemplate.CloneTree();
            _root.Add(_loadingElement);
            _loadingElement.BringToFront();
            _loadingElement.RemoveFromClassList("hidden");
            _loadingElement.AddToClassList("echo-loading--entering");

            _titleLabel = _loadingElement.Q<Label>("loadingTitle");
            _fillBar = _loadingElement.Q("loadingFill");

            if (_titleLabel != null) _titleLabel.text = title;
            SetProgress(progress);
            _active = true;
        }

        public void SetProgress(float progress01)
        {
            if (_fillBar != null)
            {
                float value = Mathf.Clamp01(progress01);
                _fillBar.style.width = Length.Percent(value * 100f);
            }
        }

        public void Complete(string finalTitle = null)
        {
            if (_loadingElement == null) return;

            if (!string.IsNullOrEmpty(finalTitle) && _titleLabel != null)
                _titleLabel.text = finalTitle;

            SetProgress(1f);
            _loadingElement.RemoveFromClassList("echo-loading--entering");
            _loadingElement.AddToClassList("echo-loading--complete");

            StartCoroutine(HideAfter(0.8f));
            _active = false;
        }

        public void Error(string errorMsg)
        {
            if (_loadingElement == null) return;

            if (_titleLabel != null)
                _titleLabel.text = errorMsg;

            _loadingElement.RemoveFromClassList("echo-loading--entering");
            _loadingElement.AddToClassList("echo-loading--error");

            StartCoroutine(HideAfter(2f));
            _active = false;
        }

        IEnumerator HideAfter(float sec)
        {
            yield return new WaitForSecondsRealtime(sec);
            HideLoading();
        }

        void HideLoading()
        {
            if (_loadingElement != null)
            {
                _loadingElement.RemoveFromHierarchy();
                _loadingElement = null;
            }
        }

        public bool IsActive => _active;
    }
}