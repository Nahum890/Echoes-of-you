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
            // El ancla era "PlacaExploracion", que NO existe en Level_02: el
            // prop caia al fallback (la posicion del jugador) y aparecia junto
            // al spawn en vez de en el aula. Y siendo artefacto de Lyra, era de
            // los que mas importa encontrar donde toca. "AulaDerecha" si existe
            // y es lo que el propio nombre del prop dice (Prop_Foto_AulaDer).
            new Def { name = "Prop_Foto_AulaDer", displayName = "Marco de foto", commentKey = "interaction.mirror_08", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.7f, 0.7f, 0.7f), prefabPath = "Props/Prop_PhotoFrame", anchorName = "AulaDerecha", offset = new Vector3(-0.8f, 1.5f, 0f), rotation = new Vector3(0f, -90f, 0f) },
            new Def { name = "Prop_Radiador_Corredor", displayName = "Radiador", commentKey = "interaction.n01_radiator", category = InteractableCategory.Ambient, triggerRadius = 4f, scale = new Vector3(1.4f, 0.9f, 0.7f), prefabPath = "", anchorName = "PlacaJugador_Corredor", offset = new Vector3(4.5f, 0.4f, 0.5f), rotation = Vector3.zero },
            new Def { name = "Prop_Reloj_Roto", displayName = "Reloj roto", commentKey = "interaction.n01_broken_clock", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.9f, 0.9f, 0.9f), prefabPath = "Props/Prop_StoppedClock", anchorName = "PuertaAula", offset = new Vector3(0f, 1.6f, -1f), rotation = Vector3.zero },
        };

        public static readonly Def[] Level03 =
        {
            // El ancla era "PlacaShadowEco", que NO existe en Level_03, asi que
            // el instalador omitia el prop y el nivel se quedaba sin cuaderno.
            // La placa de eco de este nivel se llama "PlacaEco_AulaLyra" — el
            // equivalente de la "PlacaEco_Aula" que usan L01 y L02.
            new Def { name = "Prop_Cuaderno_Sombra", displayName = "Cuaderno", commentKey = "interaction.notebook_05", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.6f, 0.6f, 0.6f), prefabPath = "Props/Prop_Notebook", anchorName = "PlacaEco_AulaLyra", offset = new Vector3(0.9f, 0.6f, 0.5f), rotation = Vector3.zero },
            new Def { name = "Prop_Registros_Estrella", displayName = "Tablero de registros", commentKey = "interaction.n01_records_board", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(1.1f, 1.1f, 1.1f), prefabPath = "Props/Prop_RecordsBoard", anchorName = "AulaEco", offset = new Vector3(-0.4f, 1.5f, 0f), rotation = new Vector3(0f, 90f, 0f) },
            new Def { name = "Prop_Radiador_Hall", displayName = "Radiador", commentKey = "interaction.n01_radiator", category = InteractableCategory.Ambient, triggerRadius = 4f, scale = new Vector3(1.4f, 0.9f, 0.7f), prefabPath = "", anchorName = "EstatuaFundador", offset = new Vector3(1.8f, 0.4f, 0f), rotation = Vector3.zero },
            new Def { name = "Prop_Foto_Lyra", displayName = "Marco de foto", commentKey = "interaction.mirror_08", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.7f, 0.7f, 0.7f), prefabPath = "Props/Prop_PhotoFrame", anchorName = "AulaLyra", offset = new Vector3(0.4f, 1.5f, 0f), rotation = new Vector3(0f, -90f, 0f) },
        };

        // ---------------------------------------------------------------
        // Niveles 04-06. Estaban VACIOS: la mitad del bloque jugable no tenia
        // un solo prop, y como lyraArtifactsSeen alimenta el score que decide
        // el final (BlockEndingResolver), no habia forma de ganar comprension
        // en la segunda mitad de la partida.
        //
        // Sobre las anclas: ResolveAnchorPosition DESCARTA la altura del ancla
        // (fuerza y = 0.1) y luego suma el offset, asi que anclar a algo
        // elevado deja el prop flotando en el aire. Por eso aqui solo se usan
        // anclas a nivel de suelo — en L05 se evita ControlLedge (y=4) a
        // proposito — y los offsets laterales se mantienen dentro de la huella
        // de la plataforma para que nada quede sobre el vacio.
        // Las posiciones y tamanos salen de Assets/Data/Levels/Level_0X_Blueprint.asset.
        // ---------------------------------------------------------------

        /// <summary>N04 "Cruce doble": pasillo recto z=0..32 con placas A/B/C
        /// alternas. Tema del capitulo II: coordinacion y orden exacto.</summary>
        public static readonly Def[] Level04 =
        {
            // StartPlatform es 12x10: un offset de -4.2 deja la mochila en el
            // borde, visible desde el spawn pero fuera de la linea de carrera.
            new Def { name = "Prop_Mochila_Olvidada", displayName = "Mochila olvidada", commentKey = "interaction.n04_mochila_olvidada", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.8f, 0.8f, 0.8f), prefabPath = "Props/Prop_Backpack", anchorName = "StartPlatform", offset = new Vector3(-4.2f, 0.3f, 2.5f), rotation = new Vector3(0f, 35f, 0f) },
            new Def { name = "Prop_Horario_Lyra", displayName = "Horario de clases", commentKey = "interaction.n04_horario", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.9f, 0.9f, 0.9f), prefabPath = "Props/Prop_RecordsBoard", anchorName = "PlatePlatA", offset = new Vector3(-2.4f, 1.5f, 0f), rotation = new Vector3(0f, 90f, 0f) },
            new Def { name = "Prop_Cronometro_Ensayo", displayName = "Cronometro", commentKey = "interaction.n04_cronometro_ensayo", category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.7f, 0.7f, 0.7f), prefabPath = "Props/Prop_StoppedClock", anchorName = "PlatePlatB", offset = new Vector3(2.2f, 0.45f, 0f), rotation = new Vector3(0f, -90f, 0f) },
            // Sin prefab: el instalador genera un mesh, y una caja aplastada
            // lee como marcas de tiza en el suelo.
            new Def { name = "Prop_Marcas_Suelo", displayName = "Marcas en el suelo", commentKey = "interaction.n04_marcas_suelo", category = InteractableCategory.Ambient, triggerRadius = 3.5f, scale = new Vector3(1.3f, 0.04f, 1.3f), prefabPath = "", anchorName = "PlatePlatC", offset = new Vector3(-2.2f, 0.12f, 0f), rotation = Vector3.zero },
        };

        /// <summary>N05 "Caos controlado": cortina de energia en z=13 y repisa
        /// de control elevada. Tema: control y soltar el control.</summary>
        public static readonly Def[] Level05 =
        {
            new Def { name = "Prop_Banco_Espera", displayName = "Banco de espera", commentKey = "interaction.n05_banco_espera", category = InteractableCategory.Ambient, triggerRadius = 4f, scale = new Vector3(1.6f, 0.5f, 0.6f), prefabPath = "", anchorName = "StartPlatform", offset = new Vector3(-5.5f, 0.35f, 0f), rotation = new Vector3(0f, 90f, 0f) },
            new Def { name = "Prop_Cartel_Normas", displayName = "Cartel de normas", commentKey = "interaction.n05_cartel_normas", category = InteractableCategory.Ambient, triggerRadius = 3.5f, scale = new Vector3(0.8f, 0.8f, 0.8f), prefabPath = "Props/Prop_RecordsBoard", anchorName = "StartPlatform", offset = new Vector3(5.5f, 1.6f, 2f), rotation = new Vector3(0f, -90f, 0f) },
            // Float_2 esta DESPUES de la cortina: la carta es lo que se gana al
            // cruzar, no algo que se encuentre antes de decidirse.
            new Def { name = "Prop_Carta_SinEnviar", displayName = "Carta sin enviar", commentKey = "interaction.n05_carta_sin_enviar", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.5f, 0.5f, 0.5f), prefabPath = "Props/Prop_Notebook", anchorName = "Float_2", offset = new Vector3(2f, 0.4f, 0f), rotation = new Vector3(0f, 20f, 0f) },
            new Def { name = "Prop_Extintor", displayName = "Extintor", commentKey = "interaction.n05_extintor", category = InteractableCategory.Ambient, triggerRadius = 3.5f, scale = new Vector3(0.35f, 0.9f, 0.35f), prefabPath = "", anchorName = "ExitPlatform", offset = new Vector3(-5f, 0.8f, -1.5f), rotation = Vector3.zero },
        };

        /// <summary>N06 "Dominio": biblioteca (zona de lectura, pasillo de
        /// estanterias) y el puente espectral. Es el ULTIMO nivel del bloque,
        /// asi que aqui se concentran los artefactos de Lyra: es la ultima
        /// ocasion de subir la comprension antes de que se resuelva el final.
        /// Las ZonaC_* son abismos (size.y = 0.2) y no se usan como ancla.</summary>
        public static readonly Def[] Level06 =
        {
            new Def { name = "Prop_Silla_Lectura", displayName = "Silla vacia", commentKey = "interaction.n06_silla_lectura", category = InteractableCategory.Ambient, triggerRadius = 3.5f, scale = new Vector3(0.5f, 0.9f, 0.5f), prefabPath = "", anchorName = "ZonaA_Lectura", offset = new Vector3(-3f, 0.45f, 1.5f), rotation = new Vector3(0f, 25f, 0f) },
            new Def { name = "Prop_Libro_Marcado", displayName = "Libro marcado", commentKey = "interaction.n06_libro_marcado", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.55f, 0.55f, 0.55f), prefabPath = "Props/Prop_Notebook", anchorName = "ZonaA_Lectura", offset = new Vector3(3f, 0.5f, -1f), rotation = new Vector3(0f, -15f, 0f) },
            // ZonaB es estrecha (4 de ancho): offsets de +-1.6 la dejan pegada a
            // las estanterias sin bloquear el paso.
            new Def { name = "Prop_Estanteria_Hueco", displayName = "Hueco en la estanteria", commentKey = "interaction.n06_estanteria_hueco", category = InteractableCategory.Ambient, triggerRadius = 3.5f, scale = new Vector3(0.3f, 0.5f, 0.9f), prefabPath = "", anchorName = "ZonaB_PasilloEstanterias", offset = new Vector3(-1.6f, 1.4f, 0f), rotation = Vector3.zero },
            new Def { name = "Prop_Ficha_Prestamo", displayName = "Ficha de prestamo", commentKey = "interaction.n06_ficha_prestamo", isLyraArtifact = true, category = InteractableCategory.Narrative, triggerRadius = 4f, scale = new Vector3(0.8f, 0.8f, 0.8f), prefabPath = "Props/Prop_RecordsBoard", anchorName = "ZonaB_PasilloEstanterias", offset = new Vector3(1.6f, 1.2f, 3f), rotation = new Vector3(0f, -90f, 0f) },
            // Ultimo prop del bloque: se mira el patio justo antes de salir.
            new Def { name = "Prop_Ventanal_Llegada", displayName = "Ventanal", commentKey = "interaction.n06_ventanal_llegada", category = InteractableCategory.Narrative, triggerRadius = 4.5f, scale = new Vector3(2.2f, 1.8f, 0.15f), prefabPath = "", anchorName = "ZonaD_Llegada", offset = new Vector3(0f, 1.6f, 3.2f), rotation = Vector3.zero },
        };

        public static Def[] GetDefs(int levelNum)
        {
            return levelNum switch
            {
                1 => Level01,
                2 => Level02,
                3 => Level03,
                4 => Level04,
                5 => Level05,
                6 => Level06,
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