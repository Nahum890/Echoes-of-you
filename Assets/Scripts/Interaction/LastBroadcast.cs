using Echoes.UI;
using UnityEngine;

namespace Echoes.Interaction
{
    public class LastBroadcast : MonoBehaviour, IResettableLevelObject
    {
        [System.Serializable]
        public class Track
        {
            public Transform[] contacts;
            public Light[] indicators;
            [System.NonSerialized] public bool powered;
            [System.NonSerialized] public EchoPlayback take;
            int beat, last = -1, pass;
            EchoPlayback reader;

            public void Clear()
            {
                beat = 0;
                last = -1;
                take = reader = null;
                foreach (var lamp in indicators) lamp.enabled = false;
            }

            public void Listen(Collider[] buffer, int mask, string name, EchoPlayback otherTake)
            {
                if (!powered) return;
                if (take && take.IsPlaying) return;
                if (beat == contacts.Length) Clear();
                for (int i = 0; i < contacts.Length; i++)
                {
                    int count = Physics.OverlapBoxNonAlloc(contacts[i].position + Vector3.up,
                        new Vector3(.96f, 1.1f, .96f), buffer, Quaternion.identity, mask, QueryTriggerInteraction.Collide);
                    for (int j = 0; j < count; j++)
                    {
                        var echo = buffer[j].GetComponentInParent<EchoPlayback>();
                        if (!echo || echo == otherTake || !echo.IsPlaying) continue;
                        if (reader != echo || pass != echo.PlaybackPass)
                        {
                            Clear();
                            reader = echo;
                            pass = echo.PlaybackPass;
                        }
                        if (last == i) return;
                        last = i;
                        if (i != beat)
                        {
                            if (beat > 0) Hint(name + ": la frase se cortó. El eco vuelve a intentarlo en la próxima vuelta.");
                            beat = 0;
                            foreach (var lamp in indicators) lamp.enabled = false;
                        }
                        if (i == beat)
                        {
                            indicators[beat++].enabled = true;
                            if (beat == contacts.Length)
                            {
                                take = echo;
                                Hint(name + ": toma completa. El canal queda en memoria.");
                            }
                        }
                        return;
                    }
                }
                last = -1;
            }
        }

        public Track air, reply;
        public InteractableObject reel, patch, mixer, microphone;
        public PuzzleSignal transmitted;
        readonly Collider[] hits = new Collider[12];
        int mask;
        bool mixed;
        void Awake() => mask = LayerMask.GetMask("Echo");

        void FixedUpdate()
        {
            if (mixed) return;
            air.Listen(hits, mask, "Patio", reply.take);
            reply.Listen(hits, mask, "Cabina", air.take);
        }

        public void ThreadTape() { air.powered = true; Inspect(reel); }
        public void ConnectReturn() { reply.powered = true; Inspect(patch); }
        public void Mix()
        {
            if (!air.powered || !reply.powered) { Hint("Falta alimentar un canal. Revisá el carrete y el retorno de auriculares."); return; }
            if (!mixed && (!air.take || !air.take.IsPlaying || !reply.take || !reply.take.IsPlaying))
            { Hint("Cada fila necesita sus tres pilotos. Las dos tomas pueden entrar en vueltas distintas."); return; }
            if (!mixed && air.take == reply.take) { Hint("Las dos pistas tienen la misma voz. Hace falta otra grabación."); return; }
            mixed = true;
            Inspect(mixer);
        }
        public void SignOff()
        {
            if (!mixed) { Hint("El micrófono espera la mezcla de los dos canales."); return; }
            transmitted.MarkSatisfied();
            Inspect(microphone);
        }
        public void ResetLevelState()
        {
            air.Clear(); reply.Clear();
            air.powered = reply.powered = mixed = false;
            transmitted.ResetLevelState();
        }
        static void Inspect(InteractableObject o) => InteractionSystem.Instance.RequestInspection(o);
        static void Hint(string text) => GameHUD.Instance?.ShowToast(text, Color.white, 4f);
    }
}
