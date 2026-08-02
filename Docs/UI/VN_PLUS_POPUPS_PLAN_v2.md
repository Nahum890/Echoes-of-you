# PLAN VISUAL NOVEL + POP-UPS — Echoes of You 2.0 (Reescritura Narrativa Dual)

## Estado del Documento
- **Versión**: 2.0 (post reescritura narrativa)
- **Fecha**: 2026-08-02
- **Dependencias**: `ECHOES_BIBLE.md v3.2`, `DESIGN_PHILOSOPHY.md v3.1`, `NARRATIVA_INTERNA.md v1.0`, `VN_ENDINGS_REDEFINED.yaml v2.0`, `INSPECT_POPUP_SPEC.md v1.0`
- **Instancia Unity**: `Echoes of you@c3e3be837858e94e` (Unity 6000.4.3f1, URP, Cinemachine 3.x)

---

## RESUMEN NARRATIVO (post-rewrite)

**Aiden** es una chica en proceso de procesar errores que cometió con alguien querido (**Lyra**). La naturaleza del vínculo es AMBIGUA por regla (ANTI-BIB-004). El entorno es **la mente de Aiden**: cada aula, corredor y objeto es una estructura cognitiva en tensión.

### Mensaje Dual (irresoluble en texto — Anti-Bib-005)
1. **Aceptar el pasado**: el pasado no se puede cambiar.
2. **Mejorar como persona**: aceptar no es resignarse, es comprender para dejar de repetir.

### Evolución de Aiden (regla de voz interna + Catch-22)
- **N01-N04 (Convicción)**: Aiden cree tener razón. Tono defensivo.
- **N05-N08 (Culpa)**: primer quiebre. Tono pesado.
- **N09-N12 (Realización)**: empieza a ver el panorama. Tono tentativo.
- **N13-N15 (Aceptación)**: sostiene sin apretar. Tono calmo.

**Catch-22**: Si el jugador no acumula comprensión (inspecciones amber + elecciones "abrir"), Aiden se queda atascada en una etapa anterior aunque esté en un nivel avanzado. La voz lo confirme negándose a evolucionar.

### 5 Endings (renombrados a etapas psicológicas)

| Ending | Brazo psicológico | `salir_del_colegio` |
|---|---|---|
| Aislamiento | Negación persistente | false |
| Ruminación | Culpa / autosabotaje | false |
| Negociación | Realización parcial evasiva | partial |
| Desesperación | Comprensión pero pavor | false |
| Aceptación | Aceptación + mejora activa | **true** (únicamente) |

---

## ARQUITECTURA TÉCNICA

### Capa 1 — Interacción con Objetos (Pop-ups, durante gameplay)

| Archivo | Líneas est. | Responsabilidad |
|---|---|---|
| `Assets/Scripts/Interaction/InteractableObject.cs` | 80 | Componente adjunto a props: `commentKey`, `isLyraArtifact`, `requireEchoActive`, `cooldown=3s`, `triggerRadius=2.5m`. Auto-registro `OnTriggerEnter/Exit`. |
| `Assets/Scripts/Interaction/InteractionSystem.cs` | 120 | Singleton: nearest picker, Input.GetKeyDown(E), cooldown dict, dispatcher → `GameHUD.ShowInspection()`. |
| `Assets/Scripts/UI/AidenStageResolver.cs` | 60 | `ResolveForCurrentLevel()` → aplica Catch-22 con `VN_StageThresholds.ForLevel(N)` y `VN_EndingFlags.ComprehensionScore`. |
| `GameHUD.cs` (mod) | +10 | Añadir `ShowInspection(title, text)` → wrapper `ShowChalkboard(title, text)` existente. |
| `Assets/Data/Localization/VN_Text.es.yaml` (nuevo) | 200+ | Sección `interaction:*`: catálogo de objetos × 4 etapas (Convicción/Culpa/Realización/Aceptación). + sección `choice:*`: 20 VN nodes. |

### Capa 2 — VN Decision Gate (post-LevelExit, gating psicológico)

| Archivo | Líneas est. | Responsabilidad |
|---|---|---|
| `Assets/Scripts/VN/VN_ChoiceNode.cs` | 30 | struct: `nodeId`, `levelIndex`, `promptKey`, `cyanLabel`, `amberLabel`, `isMicroChoice`. |
| `Assets/Scripts/VN/VN_ChoiceRegistry.cs` | 60 | ScriptableObject: `VN_ChoiceNode[] nodes` (20 entries). `GetNode(levelIndex, isMicro=false)`. |
| `Assets/Scripts/VN/VN_EndingFlags.cs` | 80 | Singleton MonoBehaviour persistente. `Dictionary<string,bool> flags`. `comprehension_score` getter. `BumpLyraArtifactSeen()`. |
| `Assets/Scripts/VN/VN_StageThresholds.cs` | 40 | Static lookup table reading from `VN_ENDINGS_REDEFINED.yaml` `stage_threshold_by_level`. |
| `Assets/Scripts/VN/VN_EndingResolver.cs` | 120 | `Resolve(flagsDict)` → `EndingID`. Algoritmo deter. 6 pasos (ver VN_ENDINGS_REDEFINED yaml §resolution_algorithm). |
| `Assets/Scripts/VN/VN_ChoiceGateController.cs` | 150 | UI controller. `Show(levelIndex, isMicro, onComplete)`: fade overlay, 2 EchoButton (cyan/amber), input A/D, setFlag + bumpScore, fade out, invoke callback. |
| `Assets/UI/VN/VN_ChoiceGateUI.uxml/.uss` | 100 | Overlay full-screen: figura Aiden de espaldas + polaroids + 2 EchoButton + hint "A = abrir / D = mantener". |
| `LevelRuntimeController.cs` (mod) | +15 | En `OnLevelCompleted()`: si `VN_ChoiceGateController.Instance != null` → bloquear carga de siguiente → mostrar gate → onComplete → `SceneManager.LoadScene`. |
| `LevelExit.cs` (mod) | +10 | En `LoadNext()`: si gate activo → no cargar siguiente. |
| `Assets/Scripts/VN/VN_EpilogueController.cs` | 80 | En `CreditsScene.unity OnEnable()`: ending = `VN_EndingResolver.Resolve()` → `LoadSceneAdditive("Epilogue_" + ending)` → countdown + "Continuar" → MainMenu. |

### Capa 3 — Epílogos y Créditos

| Archivo | Responsabilidad |
|---|---|
| `Assets/Scenes/Epilogue_Aislamiento.unity` | Aiden en corredor N01, sin salir. |
| `Assets/Scenes/Epilogue_Ruminacion.unity` | Aiden en aula Lyra repitiendo Eco 12s-12s. |
| `Assets/Scenes/Epilogue_Negociacion.unity` | Aiden en puerta, sale y regresa. |
| `Assets/Scenes/Epilogue_Desperacion.unity` | Aiden en vestíbulo, ni sale ni entra. |
| `Assets/Scenes/Epilogue_Aceptacion.unity` | Aiden fuera del colegio mirando hacia adelante. |
| `Assets/Scripts/VN/EpilogueController.cs` | Hooked en cada escena: lee `vn.epilogue.<ending_id>`, muestra voz final 1 vez, **clamp salir_del_colegio = true solo en Aceptación**. |
| `Assets/Scenes/CreditsScene.unity` (mod) | Añadir `VN_EpilogueController` → `LoadSceneAdditive("Epilogue_" + ending)` antes del scroll de créditos. |

### Testing Crítico

| Archivo | Cubre |
|---|---|
| `Assets/Tests/EditMode/VN_EndingResolverTests.cs` | 32 canonical_test_paths (ver VN_ENDINGS_REDEFINED.yaml) |
| `Assets/Tests/EditMode/AmbiguityPoliceTests.cs` | 0 vocabulario relacional prohibido en textos VN |
| `Assets/Tests/EditMode/DualThesisPoliceTests.cs` | 0 auto-justificación ("tenía razón", etc.) |
| `Assets/Tests/EditMode/VN_EndingFlagsTests.cs` | `salir_del_colegio = true` SOLO en Aceptación |
| `Assets/Tests/EditMode/VN_DeterminismTests.cs` | MISMO score+flags → MISMO ending |
| `Assets/Tests/EditMode/TextInspector/AmbiguityPoliceTests.cs` | `interaction.*` regex persona, `≤42 chars`, 0 vocab prohibido, 0 pasado evocativo |
| `Assets/Tests/EditMode/TextInspector/ToneByStageTests.cs` | `tone_by_stage` matches Catch-22 |
| `Assets/Tests/EditMode/TextInspector/InteractionSystemTests.cs` | Render solo en Chalkboard, cooldown 3s, dismissal 2.5s |

---

## FASES DE EJECUCIÓN

### Fase 1 — Pop-ups de Inspección (Base)
1. Crear `InteractableObject.cs` + `InteractionSystem.cs` + `AidenStageResolver.cs`
2. Modificar `GameHUD.cs` con `ShowInspection()`
3. Crear `VN_Text.es.yaml` sección `interaction:*` (catálogo inicial: 20-30 objetos × 4 etapas)
4. Crear `VN_EndingFlags.cs` (singleton) y `VN_StageThresholds.cs`
5. Tags todos los objetos interactuables en escenas existentes con `InteractableObject` + `isLyraArtifact=true` donde toque
6. **Validación**: Player + E cerca objeto → Chalkboard 2.5s → texto ≤42 chars → 1ª persona → tono correcto → cooldown 3s

### Fase 2 — VN Decision Registry & Gate
1. Crear `VN_ChoiceNode.cs` + `VN_ChoiceRegistry.cs` (ScriptableObject .asset con 20 entries)
2. Crear `VN_ChoiceGateController.cs` + `VN_ChoiceGateUI.uxml/.uss`
3. Modificar `LevelRuntimeController.OnLevelCompleted()` y `LevelExit.LoadNext()`
4. Crear sección `choice:*` en `VN_Text.es.yaml` (20 nodos)
5. **Validación**: Nivel completo → overlay cyan/amber → A/D → flag seteado → comprehension_score bumped si "abrir" → next level

### Fase 3 — Ending Resolver
1. Crear `VN_EndingResolver.cs` con algoritmo de 6 pasos (ver VN_ENDINGS_REDEFINED.yaml)
2. Tests EditMode: 32 paths + Ambiguity + DualThesis + Determinism + Exit-flag
3. **Validación**: `VN_EndingResolverTests.cs` todos los 32 path tests verde

### Fase 4 — Epílogos + Créditos
1. Crear 5 escenas `Epilogue_*.unity` con `EpilogueController.cs`
2. Modificar `CreditsScene.unity` con `VN_EpilogueController`
3. Build flow end-to-end: N15 final_choice → Reload → CreditsScene → LoadSceneAdditive(Epilogue_*) → Scroll → MainMenu
4. **Validación**: 5 endings reproducibles, Aceptación = `salir_del_colegio = true`

### Fase 5 — QA Final
QA-01 | inspección: 1ª persona, ≤42 chars, tono por etapa, cooldown, amber bumps comprehension
QA-02 | Catch-22: con comprehension_score=2 en N13, tono = Realization (NO Aceptación)
QA-03 | 20 VN choices funcionan, flags setteados, comprehension actualizada
QA-04 | 32 paths resolver deterministamente
QA-05 | Ambiguity police: 0 vocab relacionales en runtime
QA-06 | DualThesis police: 0 autojustificación
QA-07 | `salir_del_colegio = true` SOLO en Aceptación
QA-08 | 5 epílogos reproducibles sin tween-cut abrupto
QA-09 | Aceptación NO dice "perdoné" ni "fue bueno"
QA-10 | Build Windows .exe end-to-end: menú → N15 → ending → créditos → menú

---

## PROMPT FINAL PARA AGENTE FRESCO — VN + POP-UPS v2.0

```
[PROMPT AGENTE FRESCO — VISUAL NOVEL + POP-UPS NARRATIVA DUAL v2.0]

═══════════════════════════════════════════════════════════════
CONTEXTO
═══════════════════════════════════════════════════════════════
Echoes of You 2.0 — Unity 6000.4.3f1, URP 17.4, Cinemachine 3.x
Instancia: Echoes of you@c3e3be837858e94e
Pin primero: unityMCP_set_active_instance(instance="Echoes of you@c3e3be837858e94e")

═══════════════════════════════════════════════════════════════
NARRATIVA — LEER ANTES DE EMPEZAR
═══════════════════════════════════════════════════════════════
  Aiden: Chica. Protagonista. Procesando errores con Lyra.
  Lyra: Alguien querido. Naturaleza AMBIGUA por regla.
  Entorno: La mente de Aiden (cada aula = estructura cognitiva).
  Mensaje DUAL:
    A) El pasado no se puede cambiar — aceptar sin negar.
    B) Aceptar no es resignarse — comprender para dejar de repetir.
  4 ETAPAS: Convicción (N01-N04) → Culpa (N05-N08) →
            Realización (N09-N12) → Aceptación (N13-N15)
  Catch-22: Si comprehension_score < threshold → tono clampsúa.
  5 ENDINGS: Aislamiento / Ruminación / Negociación / Desesperación / Aceptación
            Solo Aceptación settea salir_del_colegio = true.

═══════════════════════════════════════════════════════════════
RESTRICCIONES INQUEBRANTABLES
═══════════════════════════════════════════════════════════════
- BIB-TXT-003: Cero diálogos externos. Solo pop-ups (voz interna Aiden 1a).
- BIB-AIN-006: Tono variable por etapa. No tiempo pasado evocativo.
- ANTI-BIB-004: 0 "amiga" "novia" "pareja" "relación" "amistad" en texts.
- ANTI-BIB-005: 0 "tenía razón" / "tenía la culpa" / "perdoné" / etc.
- RULE-INSPECT-001: Regex ^(Yo|Quisiera|Esto|Puedo|Aún|Tanto|Pude|Tal vez|Nunca|Siempre|A veces|Me)
- RULE-INSPECT-002: ≤42 chars tras interpolación.
- RULE-INSPECT-003: 0 pasado evocativo ("era", "fui", "tuve", "hice", "dije", etc.)
- RULE-PHI-005: Tono por etapa + Catch-22 clamp.
- Sistema canónico: UI Toolkit (UIDocument). 0 uGUI.

═══════════════════════════════════════════════════════════════
DOCUMENTOS DE AUTORIDAD OBLIGATORIOS (Lee primero)
═══════════════════════════════════════════════════════════════
1. Docs/Authority/SOURCE_OF_TRUTH.md
2. Docs/GameDesign/ECHOES_BIBLE.md (v3.2)
3. Docs/GameDesign/DESIGN_PHILOSOPHY.md (v3.1)
4. Docs/GameDesign/NARRATIVA_INTERNA.md (DOC-102)
5. Docs/Specs/UI_SPEC.md
6. Docs/ExecutableSpecs/narrative/emotional_arc.yaml (v2.0)
7. Docs/ExecutableSpecs/narrative/DIALOGUE_TREE_SCHEMA.yaml (v2.0)
8. Docs/ExecutableSpecs/narrative/VN_ENDINGS_REDEFINED.yaml (v2.0)
9. Docs/UI/INSPECT_POPUP_SPEC.md (v1.0)
10. Docs/AI/AI_AGENT_CONTRACTS.md (VisualNovelAgent section)

═══════════════════════════════════════════════════════════════
FASE 1 — POP-UPS DE INSPECCIÓN (Días 1-2)
═══════════════════════════════════════════════════════════════

Crear:
- Assets/Scripts/Interaction/InteractableObject.cs
- Assets/Scripts/Interaction/InteractionSystem.cs
- Assets/Scripts/UI/AidenStageResolver.cs
- Assets/Scripts/VN/VN_EndingFlags.cs (singleton)
- Assets/Scripts/VN/VN_StageThresholds.cs

Modificar:
- Assets/Scripts/GameHUD.cs (añadir ShowInspection(string title, string text))

Crear YAML:
- Assets/Data/Localization/VN_Text.es.yaml:
    interaction:
      locker_lyra:
        is_lyra_artifact: true
        title: "Taquilla de Lyra"
        tone:
          conviction: { text: "Yo no la miro. No ahora." }       # 29
          guilt:      { text: "Toco la chapa. Aún duele." }       # 31
          realization: { text: "Esto pesa. Puedo dejar que pese." } # 37
          acceptance: { text: "Puedo apartar la mano sin odiarla." } # 39
      # ... 20-30 objetos × 4 etapas siguiendo INSPECT_POPUP_SPEC §9

Tags en escenas existentes:
- Añadir componente InteractableObject a 20-30 props en los 15 niveles.
- is_lyra_artifact=true en los objetos del aula de Lyra (N05, N10, N13).

VALIDACIÓN FASE 1:
- Play mode: Player + E cerca objeto → Chalkboard 2.5s → 1ª persona
  → text ≤42 chars → tono matches (level, comprehension_score)
- Cooldown 3s verificado
- Inspección amber inyecta +1 comprehension_score
- 0 Build warnings en consola

═══════════════════════════════════════════════════════════════
FASE 2 — VN DECISION GATE (Días 3-4)
═══════════════════════════════════════════════════════════════

Crear:
- Assets/Scripts/VN/VN_ChoiceNode.cs (struct)
- Assets/Scripts/VN/VN_ChoiceRegistry.cs (ScriptableObject)
- Assets/Scripts/VN/VN_ChoiceGateController.cs
- Assets/UI/VN/VN_ChoiceGateUI.uxml + VN_ChoiceGateUI.uss
- Assets/Data/VN/VN_ChoiceRegistry.asset (20 entries) — usar VN_ENDINGS_REDEFINED.yaml flag_catalog
- Assets/Data/Localization/VN_Text.es.yaml (extender sección choice:*)

Registrar 20 VN_ChoiceNode (ver NARRATIVA_INTERNA §6 y VN_ENDINGS_REDEFINED flag_catalog):
  N1-N15 base (17 nodes) + N3/N7/N13 micro (3 micro).

Modificar:
- Assets/Scripts/LevelRuntimeController.cs — en OnLevelCompleted():
    if (VN_ChoiceGateController.Instance != null) {
        GameHUD.Instance.SetVisible(false);
        VN_ChoiceGateController.Instance.Show(levelIndex, false, choice => {
            VN_EndingFlags.Instance.SetFlag(node.nodeId, choice);
            if (is_openness_flag) VN_EndingFlags.Instance.BumpComprehension();
            SceneManager.LoadScene("Level_" + (levelIndex+1));
        });
        return;
    }
- LevelExit.cs análogo en LoadNext().

VALIDACIÓN FASE 2:
- Nivel completo → fade overlay → A/D → flag set → score bumped → next level
- Pulse cyan/amber animación 200ms
- 0 warnings clips null

═══════════════════════════════════════════════════════════════
FASE 3 — ENDING RESOLVER (Días 5-6)
═══════════════════════════════════════════════════════════════

Crear:
- Assets/Scripts/VN/VN_EndingResolver.cs
  public enum EndingID { Aislamiento, Ruminacion, Negociacion, Desesperacion, Aceptacion }
  Implementar algoritmo 6 pasos (VN_ENDINGS_REDEFINED.yaml resolution_algorithm)

Crear tests EditMode:
- Assets/Tests/EditMode/VN_EndingResolverTests.cs (32 canonical paths)
- Assets/Tests/EditMode/AmbiguityPoliceTests.cs (0 vocab relacional en VN_Text.es.yaml)
- Assets/Tests/EditMode/DualThesisPoliceTests.cs (0 auto-justificación)
- Assets/Tests/EditMode/VN_EndingFlagsTests.cs (salir_del_colegio solo Aceptacion)
- Assets/Tests/EditMode/VN_DeterminismTests.cs (A != B pero score_A = score_B → same ending)
- Assets/Tests/EditMode/TextInspector/PersonaRegexTests.cs (interaction.* todos cumplen regex)
- Assets/Tests/EditMode/TextInspector/CharLimitTests.cs (≤42 chars)
- Assets/Tests/EditMode/TextInspector/ToneByStageTests.cs (Catch-22)
- Assets/Tests/EditMode/TextInspector/ForbiddenPastTenseTests.cs (0 pasado evocativo)

Correr tests:
unityMCP_run_tests mode=EditMode

VALIDACIÓN FASE 3:
- Todos los 32 VN_EndingResolverTests verde
- Todos los TextInspector tests verde
- Ambiguity y DualThesis police verde

═══════════════════════════════════════════════════════════════
FASE 4 — EPÍLOGOS + CRÉDITOS (Días 7-8)
═══════════════════════════════════════════════════════════════

Crear 5 escenas:
- Assets/Scenes/Epilogue_Aislamiento.unity (Aiden en corredor N01 sin salir)
- Assets/Scenes/Epilogue_Ruminacion.unity (Aiden en aula Lyra repitiendo Eco)
- Assets/Scenes/Epilogue_Negociacion.unity (Aiden en puerta, sale-regresa)
- Assets/Scenes/Epilogue_Desperacion.unity (Aiden en vestíbulo indecisa)
- Assets/Scenes/Epilogue_Aceptacion.unity (Aiden fuera, hacia adelante)

Crear Assets/Scripts/VN/EpilogueController.cs por escena:
  void Start() {
      var ending = VN_EndingResolver.Resolve(VN_EndingFlags.Instance.Flags);
      assertion: ending matches scene name
      VN_TextTable.Get($"vn.epilogue.{ending}.voice_final") → mostrar 1 vez
      if (ending == "Aceptacion")
          VN_EndingFlags.Instance.SetFlag("salir_del_colegio", true);
      Botón "Continuar" → SceneManager.LoadScene("CreditsScene");
  }

Modificar Assets/Scenes/CreditsScene.unity:
  Añadir VN_EpilogueController:
  void OnEnable() {
      var ending = VN_EndingResolver.Resolve(...);
      SceneManager.LoadSceneAdditive($"Epilogue_{ending}");
  }
  Scroll de créditos + botón "Volver al Cuaderno" → MainMenu.

VALIDACIÓN FASE 4:
- Build N15 → resolver → CreditsScene → LoadSceneAdditive(Epilogue_*)
- Cada ending reproduce su epílogo
- Aceptación: salir_del_colegio = true
- Otros 4 endings: salir_del_colegio = false o partial
- 0 enteros ambient en dialogs/epilogues (solo Voice Final)

═══════════════════════════════════════════════════════════════
FASE 5 — QA FINAL (Día 9)
═══════════════════════════════════════════════════════════════

QA-01 inspección: 1ª persona, ≤42 chars, tono por etapa, cooldown OK
QA-02 Catch-22: comprehension_score=2 en N13 → tono Realization (NO Aceptación)
QA-03 20 VN choices funcionan, flags setteados, comprehension bumped
QA-04 32 paths resolver deterministamente (tests verde)
QA-05 Ambiguity: 0 "amiga" "novia" etc. en runtime / build
QA-06 DualThesis: 0 "tenía razón" "perdoné" etc. en runtime / build
QA-07 salir_del_colegio=true SOLO en Aceptación (test verde)
QA-08 5 epílogos reproducibles sin tween-cut abrupto
QA-09 Aceptación NO dice "perdoné" ni "fue bueno"
QA-10 Build Windows .exe end-to-end menú→N15→ending→créditos→menú
QA-11 Compilación limpia: 0 errors, 0 warnings en consola Unity

Build:
unityMCP_manage_build action=build target=windows64 output_path=Builds/Windows/EchoesOfYou.exe

═══════════════════════════════════════════════════════════════
REGLAS DE EJECUCIÓN
═══════════════════════════════════════════════════════════════
1. NO modificar SOURCE_OF_TRUTH, CONSTANTS_REGISTRY, ANTI_PATTERNS.
2. Compilación limpia (refresh_unity force + read_console errors=0) tras cada fase.
3. Si algo falla: analizar, fix puntual, re-valida fase. NO avanzar al verde.
4. Reporte al final de cada fase: archivos modificados, validaciones pass/fail.
5. No avances a fase siguiente sin confirmación explícita.

═══════════════════════════════════════════════════════════════
PRIMER COMANDO
═══════════════════════════════════════════════════════════════

unityMCP_set_active_instance instance="Echoes of you@c3e3be837858e94e"
# Luego: leer Docs/GameDesign/NARRATIVA_INTERNA.md COMPLETO
# Luego: crear InteractableObject.cs + InteractionSystem.cs (FASE 1)
```

---

## ENTREGABLES DEL PLAN

1. Scripts: 10 nuevos + 2 modificados (Fases 1-4)
2. YAML: `VN_Text.es.yaml` (interaction.* + choice.*), `VN_ChoiceRegistry.asset`
3. Escenas: 5 nuevos Epilogue_*.unity + CreditsScene modificado
4. Tests: 9 suites EditMode (32 paths + police + catch22 + tone + persona)
5. Documentación: este plan + NARRATIVA_INTERNA + VN_ENDINGS_REDEFINED + INSPECT_POPUP_SPEC

## CAMBIO HISTORIA

- **v1.0** (2026-07-25): Plan original — 20 nodes, 5 endings (Void/Obsession/etc.)
- **v2.0** (2026-08-02): Reescritura narrativa dual — Aiden chica, mensaje dual, etapas psicológicas, Catch-22, endings renombrados (Aislamiento/Ruminación/Negociación/Desesperación/Aceptación), specs NARRATIVA_INTERNA + VN_ENDINGS_REDEFINED + INSPECT_POPUP_SPEC dependencia obligatoria.
