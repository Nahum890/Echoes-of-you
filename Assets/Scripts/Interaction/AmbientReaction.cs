using System.Collections;
using UnityEngine;

namespace Echoes.Interaction
{
    /// <summary>
    /// Categoría C (Ambient): reacciona a la interacción SIN abrir interfaz.
    /// Reproduce un sonido sutil y un pequeño wobble, con cooldown anti-spam.
    /// Se auto-registra en el UnityEvent OnInteracted del InteractableObject.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public class AmbientReaction : MonoBehaviour
    {
        [Header("Reacción Ambiental (Categoría C)")]
        [Tooltip("Inclinación máxima del wobble en grados.")]
        [SerializeField] float wobbleAmount = 10f;
        [SerializeField] float wobbleSpeed = 14f;
        [SerializeField] float wobbleDecay = 5f;

        [Header("Sonido")]
        [SerializeField] AudioClip reactionSound;
        [SerializeField] float volume = 0.45f;
        [SerializeField] bool randomizePitch = true;

        [Header("Anti-spam")]
        [SerializeField] float cooldown = 2.5f;

        float _lastTriggerTime = -100f;
        Coroutine _wobbleRoutine;

        void Awake()
        {
            if (reactionSound == null)
                reactionSound = EchoesAudioAssets.Get(EchoesAudioAssets.UiInteractionAvailable);

            InteractableObject io = GetComponent<InteractableObject>();
            if (io != null)
                io.OnInteracted.AddListener(TriggerReaction);
        }

        public void TriggerReaction()
        {
            if (Time.time - _lastTriggerTime < cooldown)
                return;
            _lastTriggerTime = Time.time;

            PlaySound();
            StartWobble();
        }

        void StartWobble()
        {
            if (_wobbleRoutine != null)
                StopCoroutine(_wobbleRoutine);
            _wobbleRoutine = StartCoroutine(WobbleRoutine());
        }

        IEnumerator WobbleRoutine()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * wobbleDecay;
                float angle = Mathf.Sin(t * wobbleSpeed * Mathf.Deg2Rad * 20f) * wobbleAmount * (1f - t);
                transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                yield return null;
            }
            transform.localRotation = Quaternion.identity;
            _wobbleRoutine = null;
        }

        void PlaySound()
        {
            if (reactionSound == null)
                return;

            GameObject go = new GameObject("AmbientReactionSFX");
            go.transform.position = transform.position;
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;   // sin esto suena vacia antes de asignar el clip
            src.clip = reactionSound;
            src.spatialBlend = 1f;
            src.minDistance = 2f;
            src.maxDistance = 18f;
            src.volume = volume;
            src.pitch = randomizePitch ? Random.Range(0.9f, 1.1f) : 1f;
            src.Play();
            Destroy(go, reactionSound.length + 0.1f);
        }
    }
}