using Echoes.UI;
using UnityEngine;

namespace Echoes.Interaction
{
    public class RehearsalLights : MonoBehaviour, IResettableLevelObject
    {
        [SerializeField] InteractableObject program, tape, intercom;
        [SerializeField] Light rain, moon, dawn;
        [SerializeField] Transform leftCurtain, rightCurtain;
        [SerializeField] Transform[] actingMarks;
        [SerializeField] Light[] footlights;
        [SerializeField] PuzzleSignal finale;
        Vector3 leftClosed, rightClosed;
        bool readProgram, foundTape, loaded;
        int completed;
        bool waitingForActor;
        float waitHintAt;
        readonly Collider[] actors = new Collider[8];
        int echoLayers;

        void Awake()
        {
            leftClosed = leftCurtain.localPosition;
            rightClosed = rightCurtain.localPosition;
            echoLayers = LayerMask.GetMask("Echo");
        }

        public void ReadProgram()
        {
            readProgram = true;
            UpdateSearchHint();
            InteractionSystem.Instance.RequestInspection(program);
        }

        public void TakeTape()
        {
            if (foundTape) return;
            foundTape = true;
            UpdateSearchHint();
            InteractionSystem.Instance.RequestInspection(tape);
            tape.gameObject.SetActive(false);
            Hint("Cinta recogida · Ensayo del jueves");
        }

        public void PlayTape()
        {
            if (!foundTape) { Hint("La casetera está vacía. En la tapa dice «segunda fila»."); return; }
            loaded = true;
            UpdateSearchHint();
            InteractionSystem.Instance.RequestInspection(intercom);
        }

        void UpdateSearchHint()
        {
            if (finale.IsSatisfied) return;
            string clue = !readProgram ? "El atril de la entrada todavía tiene el programa."
                : !foundTape ? "Lyra guardaba sus grabaciones entre las butacas."
                : !loaded ? "El casete lleva una etiqueta de la cabina de arriba."
                : "El programa guarda el final. La cinta recuerda el principio.";
            LevelRuntimeController.Instance?.SetObjective(clue);
        }

        public void Rain() => Cue(0);
        public void Moon() => Cue(1);
        public void Dawn() => Cue(2);

        void Cue(int cue)
        {
            if (finale.IsSatisfied) { Hint("Ya dieron el saludo final."); return; }
            if (!loaded) { Hint("El piloto de cabina está apagado. Sólo se oye el carrete vacío."); return; }
            if (!readProgram) { Hint("Falta la hoja de cues. El atril de la entrada tiene el sello del reparto."); return; }
            if (waitingForActor) { Hint("La candileja azul sigue encendida. Espera los pasos que quedaron grabados."); return; }
            if (cue != completed)
            {
                completed = 0;
                UpdateLamps();
                Hint("Los focos se apagan. La cinta vuelve al comienzo de la escena.");
                return;
            }
            waitingForActor = true;
            waitHintAt = Time.time + 12f;
            footlights[completed].enabled = true;
            Hint("Foco preparado. Una de las marcas azules acaba de iluminarse.");
        }

        void FixedUpdate()
        {
            if (!waitingForActor) return;
            if (Time.time >= waitHintAt)
            {
                waitHintAt = float.PositiveInfinity;
                Hint("Mi sombra pisa la cruz y no pasa nada. ¿Qué pasaría si volviera a pasar por acá en la grabación?");
            }
            Vector3 center = actingMarks[completed].position + Vector3.up;
            int count = Physics.OverlapBoxNonAlloc(center, new Vector3(.95f, 1.15f, .95f),
                actors, Quaternion.identity, echoLayers, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                var playback = actors[i].GetComponentInParent<EchoPlayback>();
                if (playback == null || !playback.IsPlaying) continue;
                FinishCue();
                break;
            }
        }

        void FinishCue()
        {
            waitingForActor = false;
            footlights[completed].enabled = false;
            completed++;
            UpdateLamps();
            if (completed == 3)
            {
                finale.MarkSatisfied();
                Hint("En la cinta se oye un aplauso. El telón se abre.");
            }
            else Hint(completed == 1 ? "La lluvia repica sobre las tablas." : "Una luna blanca asoma entre los charcos.");
        }

        void UpdateLamps()
        {
            rain.enabled = completed >= 1;
            moon.enabled = completed >= 2;
            dawn.enabled = completed >= 3;
        }

        void Update()
        {
            if (!finale.IsSatisfied) return;
            leftCurtain.localPosition = Vector3.MoveTowards(leftCurtain.localPosition, leftClosed + Vector3.left * 2.8f, Time.deltaTime * 2);
            rightCurtain.localPosition = Vector3.MoveTowards(rightCurtain.localPosition, rightClosed + Vector3.right * 2.8f, Time.deltaTime * 2);
        }

        public void ResetLevelState()
        {
            readProgram = foundTape = loaded = false;
            tape.gameObject.SetActive(true);
            waitingForActor = false;
            completed = 0;
            foreach (var lamp in footlights) lamp.enabled = false;
            UpdateLamps();
            finale.ResetLevelState();
            leftCurtain.localPosition = leftClosed;
            rightCurtain.localPosition = rightClosed;
            UpdateSearchHint();
        }

        static void Hint(string text) => GameHUD.Instance?.ShowToast(text, Color.white, 3f);
    }
}
