using UnityEngine;
using UnityEngine.Events;
using Echoes.UI;

namespace Echoes.Interaction
{
    /// <summary>
    /// Categoría A (Gameplay): acciones de juego al interactuar (puerta, pista, toggle, custom).
    /// Se auto-registra en el UnityEvent OnInteracted del InteractableObject.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public class GameplayInteraction : MonoBehaviour
    {
        public enum ActionKind { Hint, Door, Toggle, Custom }

        [Header("Tipo de Acción (Categoría A)")]
        [SerializeField] ActionKind actionKind = ActionKind.Hint;

        [Header("Hint (texto contextual)")]
        [SerializeField] string hintTitle = "Pista";
        [TextArea]
        [SerializeField] string hintText = "Algo ocurrió aquí.";

        [Header("Door (requiere DoorController)")]
        [SerializeField] DoorController doorController;
        [SerializeField] bool openOnInteract = true;

        [Header("Toggle (alterna un estado serializado)")]
        [SerializeField] bool initialToggle = false;

        [Header("Custom (eventos conectados en el Inspector)")]
        [SerializeField] UnityEvent customActions = new UnityEvent();

        [Header("Feedback")]
        [SerializeField] bool playActionSound = true;

        InteractableObject _io;
        bool _toggleState;

        void Awake()
        {
            _io = GetComponent<InteractableObject>();
            if (_io != null)
                _io.OnInteracted.AddListener(Execute);

            if (actionKind == ActionKind.Door && doorController == null)
                doorController = GetComponentInParent<DoorController>() ?? GetComponent<DoorController>();

            _toggleState = initialToggle;
        }

        void Execute()
        {
            switch (actionKind)
            {
                case ActionKind.Door:
                    if (doorController != null)
                    {
                        doorController.SetOpenState(openOnInteract);
                        if (playActionSound && GameFeelController.Instance != null)
                            GameFeelController.Instance.PlayDoorMove(transform.position);
                    }
                    break;

                case ActionKind.Toggle:
                    _toggleState = !_toggleState;
                    customActions.Invoke();
                    if (playActionSound && GameFeelController.Instance != null)
                        GameFeelController.Instance.PlayMechanicTick(transform.position, 0.7f);
                    break;

                case ActionKind.Custom:
                    customActions.Invoke();
                    if (playActionSound && GameFeelController.Instance != null)
                        GameFeelController.Instance.PlayMechanicTick(transform.position, 0.8f);
                    break;

                default:
                    if (playActionSound && GameFeelController.Instance != null)
                        GameFeelController.Instance.PlayMechanicTick(transform.position, 0.6f);
                    GameHUD hud = FindAnyObjectByType<GameHUD>();
                    if (hud != null)
                        hud.ShowInspection(hintTitle, hintText);
                    break;
            }
        }
    }
}