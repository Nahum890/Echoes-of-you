using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// LevelSelectController — Grid 15 EchoCard (01-15).
    /// Estados: locked / available / completed / current.
    /// Metadatos por tarjeta: nombre, ecos, quiebres, tiempo.
    /// Niveles 11-15 visibles y jugables (fix GameProgress.TotalLevels = 15).
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        [Header("Templates")]
        [SerializeField] VisualTreeAsset _levelSelectTemplate; // LevelSelectUI.uxml
        [SerializeField] VisualTreeAsset _cardTemplate; // EchoCard.uxml (reutiliza componente)

        VisualElement _root;
        VisualElement _cardGrid;
        Button _btnBack;

        void Awake()
        {
            // Singleton pattern for scene access
        }

        public void Setup(VisualElement container, VisualTreeAsset template, VisualTreeAsset cardTemplate)
        {
            _levelSelectTemplate = template;
            _cardTemplate = cardTemplate;

            container.Clear();
            var panel = _levelSelectTemplate.CloneTree();
            container.Add(panel);
            _root = panel;

            InitializeUI();
            PopulateLevelCards();
        }

        void InitializeUI()
        {
            if (_root == null) return;

            _cardGrid = _root.Q("levelCardGrid");
            _btnBack = _root.Q<Button>("btnBack");

            if (_btnBack != null)
            {
                _btnBack.clicked += OnBack;
            }
        }

        void PopulateLevelCards()
        {
            if (_cardGrid == null || _cardTemplate == null) return;
            _cardGrid.Clear();

            for (int i = 1; i <= GameProgress.TotalLevels; i++)
            {
                string sceneName = $"Level_{i:D2}";
                var card = _cardTemplate.CloneTree();
                card.name = $"card-{i:D2}";

                // Configure card text
                var nameLabel = card.Q<Label>("name") ?? card.Q<Label>(className: "echo-card__name");
                if (nameLabel != null)
                    nameLabel.text = $"Capítulo {i:D2}";

                bool unlocked = GameProgress.IsSceneUnlocked(sceneName);
                bool completed = GameProgress.IsSceneCompleted(sceneName);

                // Remove old state classes
                card.RemoveFromClassList("echo-card--locked");
                card.RemoveFromClassList("echo-card--available");
                card.RemoveFromClassList("echo-card--completed");
                card.RemoveFromClassList("echo-card--current");

                // Apply state
                if (!unlocked)
                {
                    card.AddToClassList("echo-card--locked");
                }
                else if (completed)
                {
                    card.AddToClassList("echo-card--completed");
                }
                else if (sceneName == GameProgress.GetContinueSceneName())
                {
                    card.AddToClassList("echo-card--current");
                }
                else
                {
                    card.AddToClassList("echo-card--available");
                }

                // Add metadata
                if (unlocked)
                {
                    int deaths = GameProgress.GetSceneDeathCount(sceneName);
                    float time = GameProgress.GetScenePlayTimeSeconds(sceneName);
                    int echoes = GameProgress.GetSceneEchoesCreated(sceneName);

                    var meta = card.Q("meta");
                    if (meta != null)
                    {
                        var echoLabel = meta.Q<Label>(className: "echoes");
                        var deathLabel = meta.Q<Label>(className: "deaths");
                        var timeLabel = meta.Q<Label>(className: "time");
                        if (echoLabel != null) echoLabel.text = $"Ecos: {echoes}";
                        if (deathLabel != null) deathLabel.text = deaths > 0 ? $"Colapsos: {deaths}" : "Colapsos: 0";
                        if (timeLabel != null) timeLabel.text = $"Tiempo: {GameProgress.FormatPlayTime(time)}";
                    }
                }

                // Click handler
                if (unlocked)
                {
                    string capturedScene = sceneName;
                    card.RegisterCallback<ClickEvent>(_ => LoadLevel(capturedScene));
                    card.AddToClassList("cursor-pointer");
                }

                _cardGrid?.Add(card);
            }
        }

        void LoadLevel(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            if (!Application.CanStreamedLevelBeLoaded(sceneName)) return;

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(sceneName);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        void OnBack()
        {
            if (NavigationManager.Instance != null)
                NavigationManager.Instance.Pop();
        }
    }
}