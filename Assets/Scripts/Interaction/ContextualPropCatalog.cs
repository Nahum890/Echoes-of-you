using UnityEngine;

namespace Echoes.Interaction
{
    /// <summary>
    /// Catálogo único de props contextuales por nivel.
    /// Lo usan el spawner de runtime (LevelEnvironmentBootstrap) y el instalador
    /// de editor (ContextPropsSceneInstaller) para que los props existan tanto en
    /// la escena (edit mode) como al jugar.
    ///
    /// Coordenadas: el mundo se escala x2 en runtime (LevelGeometryScale) pero los
    /// props NO se escalan; por eso las posiciones se resuelven SIEMPRE a partir de
    /// un objeto ancla de la escena (ya escalado) + un offset en metros reales,
    /// relativos al jugador (PlayerHeight = 2.2).
    /// </summary>
    public static class ContextualPropCatalog
    {
        [System.Serializable]
        public struct Def
        {
            public string name;
            public string displayName;
            public string commentKey;
            public bool isLyraArtifact;
            public InteractableCategory category;
            public float triggerRadius;
            public Vector3 scale;
            public string prefabPath;   // Resources path opcional ("Props/Prop_Notebook")
            public string anchorName;   // objeto de la escena al que anclar
            public Vector3 offset;      // offset en metros (offset.y = altura sobre el suelo aprox.)
            public Vector3 rotation;    // euler opcional para props de pared
        }

        public static readonly Def[] Level01 =
        {
            new Def { name = "Prop_Cuaderno_Eco", displayName = "Cuaderno", commentKey = "interaction.notebook_05", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.6f, 0.6f, 0.6f), prefabPath = "Props/Prop_Notebook", anchorName = "PlacaEco_Aula", offset = new Vector3(0.9f, 0.6f, 0.5f), rotation = Vector3.zero },
            new Def { name = "Prop_Cinta_Lyra", displayName = "Cinta de audio", commentKey = "interaction.cassette_tape", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.6f, 0.6f, 0.6f), prefabPath = "Props/Prop_CoffeeCups", anchorName = "PlacaEco_Aula", offset = new Vector3(-0.8f, 0.4f, 0.4f), rotation = Vector3.zero },
            new Def { name = "Prop_Reloj_Detenido", displayName = "Reloj detenido", commentKey = "interaction.n01_stopped_clock_corridor", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.9f, 0.9f, 0.9f), prefabPath = "Props/Prop_StoppedClock", anchorName = "AulaAusente", offset = new Vector3(7.5f, 1.6f, 0f), rotation = new Vector3(0f, 90f, 0f) },
            new Def { name = "Prop_Radiador", displayName = "Radiador", commentKey = "interaction.n01_radiator", category = InteractableCategory.Ambient, triggerRadius = 4f, scale = new Vector3(1.4f, 0.9f, 0.7f), prefabPath = "", anchorName = "PasilloA", offset = new Vector3(4.5f, 0.4f, 4f), rotation = Vector3.zero },
            new Def { name = "Prop_Mochila_Puerta", displayName = "Mochila", commentKey = "interaction.n01_coat", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.8f, 0.8f, 0.8f), prefabPath = "Props/Prop_Backpack", anchorName = "PuertaAula", offset = new Vector3(0.7f, 0.2f, -1.5f), rotation = Vector3.zero },
        };

        public static readonly Def[] Level02 =
        {
            new Def { name = "Prop_Cuaderno_AulaIzq", displayName = "Cuaderno", commentKey = "interaction.notebook_05", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.6f, 0.6f, 0.6f), prefabPath = "Props/Prop_Notebook", anchorName = "PlacaEco_Aula", offset = new Vector3(0.9f, 0.6f, 0.5f), rotation = Vector3.zero },
            new Def { name = "Prop_Foto_AulaDer", displayName = "Marco de foto", commentKey = "interaction.mirror_08", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.7f, 0.7f, 0.7f), prefabPath = "Props/Prop_PhotoFrame", anchorName = "PlacaExploracion", offset = new Vector3(0.8f, 0.5f, 0.5f), rotation = Vector3.zero },
            new Def { name = "Prop_Radiador_Corredor", displayName = "Radiador", commentKey = "interaction.n01_radiator", category = InteractableCategory.Ambient, triggerRadius = 4f, scale = new Vector3(1.4f, 0.9f, 0.7f), prefabPath = "", anchorName = "PlacaJugador_Corredor", offset = new Vector3(4.5f, 0.4f, 0.5f), rotation = Vector3.zero },
            new Def { name = "Prop_Reloj_Roto", displayName = "Reloj roto", commentKey = "interaction.n01_broken_clock", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.9f, 0.9f, 0.9f), prefabPath = "Props/Prop_StoppedClock", anchorName = "PuertaAula", offset = new Vector3(0f, 1.6f, -1f), rotation = Vector3.zero },
        };

        public static readonly Def[] Level03 =
        {
            new Def { name = "Prop_Cuaderno_Sombra", displayName = "Cuaderno", commentKey = "interaction.notebook_05", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.6f, 0.6f, 0.6f), prefabPath = "Props/Prop_Notebook", anchorName = "PlacaShadowEco", offset = new Vector3(0.9f, 0.6f, 0.5f), rotation = Vector3.zero },
            new Def { name = "Prop_Registros_Estrella", displayName = "Tablero de registros", commentKey = "interaction.n01_records_board", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(1.1f, 1.1f, 1.1f), prefabPath = "Props/Prop_RecordsBoard", anchorName = "AulaEco", offset = new Vector3(-0.4f, 1.5f, 0f), rotation = new Vector3(0f, 90f, 0f) },
            new Def { name = "Prop_Radiador_Hall", displayName = "Radiador", commentKey = "interaction.n01_radiator", category = InteractableCategory.Ambient, triggerRadius = 4f, scale = new Vector3(1.4f, 0.9f, 0.7f), prefabPath = "", anchorName = "EstatuaFundador", offset = new Vector3(1.8f, 0.4f, 0f), rotation = Vector3.zero },
            new Def { name = "Prop_Foto_Lyra", displayName = "Marco de foto", commentKey = "interaction.mirror_08", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.7f, 0.7f, 0.7f), prefabPath = "Props/Prop_PhotoFrame", anchorName = "AulaLyra", offset = new Vector3(0.4f, 1.5f, 0f), rotation = new Vector3(0f, -90f, 0f) },
        };

        public static Def[] GetDefs(int levelNum)
        {
            return levelNum switch
            {
                1 => Level01,
                2 => Level02,
                3 => Level03,
                _ => null
            };
        }

        /// <summary>
        /// Posición del prop: ancla (con y en el suelo) + offset en metros reales.
        /// Si no existe el ancla, se usa el fallback (posición del jugador).
        /// </summary>
        public static Vector3 ResolveAnchorPosition(Def def, Vector3 fallback)
        {
            if (string.IsNullOrEmpty(def.anchorName))
                return fallback;

            GameObject anchor = GameObject.Find(def.anchorName);
            if (anchor == null)
                return fallback;

            Vector3 pos = anchor.transform.position;
            pos.y = 0.1f; // suelo aproximado del mundo escalado
            return pos + def.offset;
        }

        public static Quaternion ResolveRotation(Def def) => Quaternion.Euler(def.rotation);
    }
}