# INSPECT_POPUP_SPEC.md — Pop-up de Inspección (Voz Interna de Aiden)
## Spec ID: SPEC-UI-009
## Version: 1.0 (AI-Executable)
## Authority: Level 3 (Especificación Ejecutable)

---

### 1. PURPOSE
Especifica el sistema de pop-ups de inspección diegéticos en *Echoes of You 2.0*. El pop-up muestra la **voz interna de Aiden en 1ª persona** cuando el jugador interactúa (`tecla E`) con un objeto del entorno. El sistema es el único canal narrativo permitido durante gameplay (excepción de BIB-TXT-003), y su tono evoluciona por etapa psicológica (RULE-PHI-005) con clamp Catch-22 según el `comprehension_score` acumulado.

### 2. SCOPE
Aplica a: `Assets/Scripts/Interaction/InteractableObject.cs`, `Assets/Scripts/Interaction/InteractionSystem.cs`, `Assets/Data/Localization/VN_Text.es.yaml` (sección `interaction:*`), el subsistema `Chalkboard` de `GameHUD.cs`, y todo objeto del nivel etiquetado con `InteractableObject`.

### 3. AUTHORITY
Level 3 (Especificación Ejecutable). Subordinado a `SOURCE_OF_TRUTH.md`, `ECHOES_BIBLE.md`, `DESIGN_PHILOSOPHY.md`, `NARRATIVA_INTERNA.md`. Hereda `BIB-TXT-003`, `BIB-AIN-006`, `RULE-PHI-004`, `RULE-PHI-005`, `ANTI-BIB-004`, `ANTI-BIB-005`.

### 4. DEFINITIONS
- **Pop-up**: Renderizado contextual en `HUD::Chalkboard` (no nuevo componente UI). Solo se muestra al interactuar; nunca permanente.
- **Voz Interna**: Texto ≤42 chars en 1ª persona que representa lo que Aiden piensa-ahora-mismo al ver/tocar el objeto.
- **Tono por Etapa**: Cuatro etapas (Convicción / Culpa / Realización / Aceptación) con vocabulario y persona gramatical distintos cada una.
- **Catch-22 Clamp**: Si `comprehension_score < stage_threshold[level_index]`, el tono efectivo se clampea a la etapa más baja permitida por el comprehension actual (ver `NARRATIVA_INTERNA.md` §5.1 y `DESIGN_PHILOSOPHY.md` Algorithm 8.2).
- **Inspección Amber**: Objeto etiquetado `is_lyra_artifact = true`. Al inspeccionar inyecta +1 al `comprehension_score` (vía `lyra_artifact_seen_count`). Sin ella no se puede llegar a Aceptación.

### 5. INPUTS
- `NARRATIVA_INTERNA.md [DOC-102]` — Definiciones de Aiden, mensaje dual, etapas, Catch-22.
- `dialogue_tree_schema.yaml` v2.0 — `introspection_node` schema, regex de persona, validaciones.
- `emotional_arc.yaml` v2.0 — `tone_by_stage` por nivel.
- `VN_ENDINGS_REDEFINED.yaml` — Flags, comprehension_tiers, stage_threshold_by_level.

### 6. OUTPUTS
- `Assets/Scripts/Interaction/InteractableObject.cs` — Componente adjunto a props.
- `Assets/Scripts/Interaction/InteractionSystem.cs` — Manager singleton.
- `Assets/Data/Localization/VN_Text.es.yaml` (sección `interaction:*`).
- Modificación de `GameHUD.cs` con método `ShowInspection(string title, string text)`.

### 7. REGLAS EJECUTABLES

- `[RULE-INSPECT-001]`: **Persona Gramatical Estricta**: Todo `text` de un `interaction.*` DEBE comenzar con uno de los tokens del regex `^(Yo|Quisiera|Esto|Puedo|Aún|Tanto|Pude|Tal vez|Nunca|Siempre|A veces|Me)`. Cualquier texto que falle el regex → `FAIL-INSPECT-01`.
- `[RULE-INSPECT-002]`: **Longitud Máxima**: El `text` resuelto (tras interpolación) DEBE medir ≤ 42 caracteres (espacios inclusive). Exceder → `truncate_ellipsis` y `FAIL-INSPECT-02`.
- `[RULE-INSPECT-003]`: **Tiempo Verbal Restringido**: Prohibido cualquier conjugación de pasado simple evocativo: "era", "fui", "tuve", "hice", "dije", "estaba", "podía", "quería". Solo presente ("soy", "puedo", "veo", "siento", "pienso") y futuro hipotético ("pudiese", "quedará", "será", "tendría"). Cualquier ocurrencia de pasado evocativo → `FAIL-INSPECT-03`.
- `[RULE-INSPECT-004]`: **Auto-Justificación Prohibida**: El `text` NUNCA puede contener frases conclusivas como ["tenía razón", "ella tenía la culpa", "yo tenía la culpa", "estuvo bien", "fui inocente", "perdoné"]. `FAIL-INSPECT-04` (violación de ANTI-BIB-005).
- `[RULE-INSPECT-005]`: **Vocabulario Relacional Prohibido**: 0 ocurrencias de ["amiga", "novia", "pareja", "relación", "amistad", "amamos", "nos queríamos"]. `FAIL-INSPECT-05` (violación de ANTI-BIB-004).
- `[RULE-INSPECT-006]`: **Tono por Etapa**: El `tone_by_stage` declarado en el `interaction.*` debe coincidir con `min(stage_by_level[N], stage_by_comprehension[score])`. Discrepancia → `FAIL-INSPECT-06`.
- `[RULE-INSPECT-007]`: **Duración y Auto-Dismiss**: El pop-up se muestra por 2.5s (±1.0s). Lo elimina automáticamente el `InteractionSystem`. No requiere input del jugador para dismissal.
- `[RULE-INSPECT-008]`: **Cooldown Único**: Tras inspeccionar un `InteractableObject`, NO se puede re-inspeccionar el mismo objeto por 3.0s. Reduce repetición trivial; permite relectura posterior.
- `[RULE-INSPECT-009]`: **Solo en Chalkboard HUD**: El pop-up renderiza EXCLUSIVAMENTE en el subsistema `Chalkboard` de `GameHUD`. Prohibido usar `Label` standalone, `DialogBox`, `TextMeshPro` worldspace, o cualquier UI emergente nueva.
- `[RULE-INSPECT-010]`: **No en Cutscene**: Durante un `LevelRuntimeController.isInCutscene == true`, los pop-ups se desactivan. La introspección es gameplay, no narración dirigida.

### 8. COMPONENTES

#### 8.1 InteractableObject.cs (MonoBehaviour)

```csharp
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private string commentKey;     // ej: "interaction.locker_lyra"
    [SerializeField] private bool isLyraArtifact;   //true → inyecta comprehension
    [SerializeField] private bool requireEchoActive; // true → solo si Eco activo
    [SerializeField] private float cooldown = 3.0f;
    [SerializeField] private float triggerRadius = 2.5f;

    // Auto-registro OnTriggerEnter, auto-unregister OnTriggerExit.
    // public void Interact() → InteractionSystem.Instance.RequestInspection(this)

    public string CommentKey => commentKey;
    public bool IsLyraArtifact => isLyraArtifact;
    public bool RequireEchoActive => requireEchoActive;
    public float Cooldown => cooldown;
    public float TriggerRadius => triggerRadius;
}
```

#### 8.2 InteractionSystem.cs (Singleton)

```csharp
public class InteractionSystem : MonoBehaviour
{
    public static InteractionSystem Instance { get; private set; }
    private readonly List<InteractableObject> _nearby = new();
    private readonly Dictionary<string, float> _lastSeenAt = new();

    void Update()
    {
        var nearest = PickNearest();
        if (nearest == null) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (nearest.RequireEchoActive && !EchoRecorder.Instance.IsRecording && !EchoPlayback.Instance.IsPlaying) return;
        if (_lastSeenAt.TryGetValue(nearest.CommentKey, out var t) && Time.time - t < nearest.Cooldown) return;
        RequestInspection(nearest);
        _lastSeenAt[nearest.CommentKey] = Time.time;
    }

    public void RequestInspection(InteractableObject obj)
    {
        var stage = AidenStageResolver.ResolveForCurrentLevel();
        var entry = VN_TextTable.Get(obj.CommentKey, stage);
        GameHUD.Instance.ShowInspection(entry.title, entry.text);
        if (obj.IsLyraArtifact) VN_EndingFlags.Instance.BumpLyraArtifactSeen();
    }

    public void Register(InteractableObject o) { _nearby.Add(o); }
    public void Unregister(InteractableObject o) { _nearby.Remove(o); }
}
```

#### 8.3 AidenStageResolver

Resuelve la etapa efectiva aplicando el Catch-22:

```csharp
public static class AidenStageResolver
{
    public enum Stage { Conviction, Guilt, Realization, Acceptance }

    public static Stage ResolveForCurrentLevel()
    {
        int N = LevelRuntimeController.Instance.CurrentLevelIndex; // 1..15
        int score = VN_EndingFlags.Instance.ComprehensionScore;
        int threshold = VN_StageThresholds.ForLevel(N);
        Stage byLevel = StageByLevel(N);
        Stage byComprehension = StageByScore(score);
        // Catch-22: si no hay suficiente comprehension, clampear hacia abajo
        return (score >= threshold) ? byLevel : byComprehension;
    }
}
```

### 9. CATÁLOGO INICIAL — Ejemplos por Etapa

El YAML `VN_Text.es.yaml` tiene una entrada por (objeto, etapa). Ejemplo:

```yaml
interaction:
  locker_lyra:
    is_lyra_artifact: true
    title: "Taquilla de Lyra"
    tone:
      conviction:
        text: "Yo no la miro. No ahora."        # 29 chars
      guilt:
        text: "Toco la chapa. Aún duele."        # 31 chars
      realization:
        text: "Esto pesa. Puedo dejar que pese." # 37 chars
      acceptance:
        text: "Puedo apartar la mano sin odiarla." # 39 chars

  desk_01:
    is_lyra_artifact: false
    title: "Escritorio"
    tone:
      conviction:
        text: "Esto es solo un pupitre."          # 24 chars
      guilt:
        text: "Me senté demasiado cerca, a veces."# 39 chars
      realization:
        text: "Tal vez ella también se acercaba."# 35 chars
      acceptance:
        text: "Esto nos sostuvo a ambos un día."  # 35 chars

  chalkboard_07:
    is_lyra_artifact: false
    title: "Pizarra"
    tone:
      conviction:
        text: "Yo no escribí nada aquí."           # 25 chars
      guilt:
        text: "Pude haber dicho algo. No lo hice." # 41 chars
      realization:
        text: "El silencio también es una elección." # 41 chars
      acceptance:
        text: "Puedo callar sin que sea cobardía."  # 38 chars

  cassette_tape:
    is_lyra_artifact: true
    title: "Cinta"
    tone:
      conviction:
        text: "Esto no me concierne."              # 24 chars
      guilt:
        text: "Tal vez debería escucharla. Aún no." # 41 chars
      realization:
        text: "Esto la recuerda a ella y a mí."     # 32 chars
      acceptance:
        text: "Puedo escucharla sin romperme."      # 34 chars
```

### 10. VALIDACIÓN

| ID | Tipo | Severidad | Regla | Validator |
|---|---|---|---|---|
| VAL-INSPECT-001 | unit | error | Regex persona 1ª: `(Yo\|Quisiera\|Esto\|Puedo\|Aún\|Tanto\|Pude\|Tal vez\|Nunca\|Siempre\|A veces\|Me)` | `TextInspectorTests.cs#persona_regex` |
| VAL-INSPECT-002 | unit | error | `text.length <= 42` | `TextInspectorTests.cs#char_limit` |
| VAL-INSPECT-003 | unit | error | 0 ocurrencias de pasado evocativo: `["era", "fui", "tuve", "hice", "dije", "estaba", "podía", "quería"]` | `TextInspectorTests.cs#no_past_tense` |
| VAL-INSPECT-004 | unit | error | 0 frases conclusivas prohibidas (ANTI-BIB-005) | `TextInspectorTests.cs#no_auto_justification` |
| VAL-INSPECT-005 | unit | error | 0 vocabulario relacional prohibido (ANTI-BIB-004) | `TextInspectorTests.cs#no_relational_terms` |
| VAL-INSPECT-006 | integration | error | `tone_by_stage` matches `min(stage_by_level, stage_by_comprehension)` | `ToneByStageTests.cs#catch22_clamp` |
| VAL-INSPECT-007 | playmode | advisory | Cooldown 3s + dismissal 2.5s ±1s | `InteractionSystemTests.cs#cooldown_timing` |
| VAL-INSPECT-008 | playmode | error | Pop-up renderiza en `HUD::Chalkboard` (no en nuevo UI) | `InteractionSystemTests.cs#render_channel` |

### 11. CASOS DE USO CANÓNICOS

#### 11.1 Inspección normal en N02 (Convicción)
```
Player camina cerca de desk_01 en N02 (comprehension_score = 0)
→ InteractionSystem detecta E + proximidad
→ AidenStageResolver.ResolveForCurrentLevel() = Conviction
→ entry = VN_TextTable.Get("interaction.desk_01", Conviction)
→ GameHUD.ShowInspection("Escritorio", "Esto es solo un pupitre.")
→ Render en Chalkboard 2.5s → auto-dismiss
```

#### 11.2 Inspección amber en N05 (Culpa, comprehension aún baja)
```
Player camina cerca de locker_lyra en N05 (comprehension_score = 2)
→ threshold para N05 = 2 → score (2) >= threshold (2) → byLevel = Guilt
→ entry = VN_TextTable.Get("interaction.locker_lyra", Guilt)
→ texto = "Toco la chapa. Aún duele."
→ Al ser amber → VN_EndingFlags.BumpLyraArtifactSeen() → comprehension_score += 1 (vía 0.5 weight)
```

#### 11.3 Catch-22 en N13 (comprehension baja)
```
Player en N13 con comprehension_score = 6 (threshold N13 = 10)
→ score (6) < threshold (10) → Catch-22 activa
→ byComprehension (score=6) = Realization
→ entry = VN_TextTable.Get("interaction.<obj>", Realization)
→ texto refleja Realización, NO Aceptación — aunque estemos en N13
→ El jugador siente cómo la voz de Aiden noeca evolucionar
```

### 12. CROSS REFERENCES
- [ECHOES_BIBLE.md `[DOC-101]`] — BIB-TXT-003, BIB-AIN-006, ANTI-BIB-004, ANTI-BIB-005
- [DESIGN_PHILOSOPHY.md `[SPEC-001]`] — RULE-PHI-004, RULE-PHI-005, Algorithm 8.2
- [NARRATIVA_INTERNA.md `[DOC-102]`] — Sección 5 (Catch-22), Sección 8 (voces de pop-up), Sección 10 (validación cruzada)
- [DIALOGUE_TREE_SCHEMA.yaml] v2.0 — `introspection_node` schema
- [UI_SPEC.md `[SPEC-008]`] — `HUD::Chalkboard` subsystem
- [VN_ENDINGS_REDEFINED.yaml] — `stage_threshold_by_level`, `comprehension_tiers`, `flag_catalog`

### 13. CHANGE HISTORY
- **v1.0 (2026-08-02)**: Creación. Documento nuevo requerido por la reescritura narrativa dual. Reglas RULE-INSPECT-001 a 010. Componentes script. Catálogo de ejemplo (4 objetos × 4 etapas). Tests identificados. Catch-22 binding formal con NARRATIVA_INTERNA y VN_ENDINGS_REDEFINED.
