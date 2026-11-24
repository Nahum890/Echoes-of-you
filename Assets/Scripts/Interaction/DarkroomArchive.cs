using Echoes.UI;
using UnityEngine;

namespace Echoes.Interaction
{
    public class DarkroomArchive : MonoBehaviour, IResettableLevelObject
    {
        [SerializeField] Transform[] exposures;
        [SerializeField] GameObject[] prints;
        [SerializeField] PuzzleSignal archived;
        readonly Collider[] hits = new Collider[8];
        int layers, exposed, filed, pass, lastMark = -1;
        EchoPlayback performer;

        void Awake() => layers = LayerMask.GetMask("Echo");

        void FixedUpdate()
        {
            if (exposed == exposures.Length) return;
            for (int i = 0; i < exposures.Length; i++)
            {
                var center = exposures[i].position + Vector3.up;
                int count = Physics.OverlapBoxNonAlloc(center, new Vector3(1, 1.2f, 1), hits,
                    Quaternion.identity, layers, QueryTriggerInteraction.Collide);
                for (int j = 0; j < count; j++)
                {
                    var echo = hits[j].GetComponentInParent<EchoPlayback>();
                    if (!echo || !echo.IsPlaying) continue;
                    if (echo != performer || echo.PlaybackPass != pass)
                    {
                        ClearExposures();
                        performer = echo;
                        pass = echo.PlaybackPass;
                        lastMark = -1;
                    }
                    if (lastMark == i) return;
                    lastMark = i;
                    if (i != exposed) ClearExposures();
                    if (i == exposed)
                    {
                        prints[exposed++].SetActive(true);
                        Hint(exposed == 3 ? "Tres copias en el tendedero. Falta recomponer el archivo." : $"Revelado {exposed} de 3.");
                    }
                    else Hint("La tira salió velada. El contador empieza en I.");
                    return;
                }
            }
            lastMark = -1;
        }

        void ClearExposures()
        {
            exposed = 0;
            foreach (var print in prints) print.SetActive(false);
        }

        public void Chalk() => File(0);
        public void Boat() => File(1);
        public void Key() => File(2);

        void File(int symbol)
        {
            if (archived.IsSatisfied) return;
            if (exposed != 3) { Hint("El lomo pide una tira completa, con sus tres copias."); return; }
            if (symbol != 2 - filed)
            {
                filed = 0;
                Hint("No encajan las muescas. Las copias siguen colgadas para volver a mirarlas.");
                return;
            }
            if (++filed == 3) archived.MarkSatisfied();
            else Hint($"Copia archivada · {filed} de 3");
        }

        public void ResetLevelState()
        {
            ClearExposures();
            performer = null;
            lastMark = -1;
            filed = 0;
            archived.ResetLevelState();
        }

        static void Hint(string text) => GameHUD.Instance?.ShowToast(text, Color.white, 3f);
    }
}
