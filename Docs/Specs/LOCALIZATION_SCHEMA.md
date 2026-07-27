# LOCALIZATION_SCHEMA.md — Localization Tables & String Interpolation Specs
## Spec ID: SPEC-133
## Version: 2.0 (AI-Executable)

---

### 1. PURPOSE
Defines minimum localization table structure, locale fallback chains (`es-MX` primary, `en-US` secondary, `ja-JP` tertiary), dynamic string parameter formatting, font SDF fallbacks, text bounds, and overflow handling for *Echoes of You 2.0*.

### 2. SCOPE
Applies to UI text components, narrative prompt dialogues, menu labels, and `LocalizationManager.cs`.

### 3. AUTHORITY
Level 4 (Declarative Specs). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `UI_SPEC.md` (`SPEC-008`). Runtime config in `Docs/Specs/LOCALIZATION_SCHEMA.yaml` (`SPEC-133`).

### 4. DEFINITIONS
- `Primary Locale`: `es-MX` (Mexican Spanish) as the canonical source locale.
- `Fallback Locale`: `en-US` (English US) as secondary, `ja-JP` (Japanese) as tertiary.
- `Dynamic Param`: ICU MessageFormat interpolation syntax `{variable}` for runtime variable replacement.
- `Wrap Width`: Maximum characters per line before wrapping/truncation.
- `Overflow Handling`: Strategy when text exceeds bounds.

### 5. INPUTS
- [UI_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/UI_SPEC.md) `[SPEC-008]`
- [LOCALIZATION_SCHEMA.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCALIZATION_SCHEMA.yaml) `[SPEC-133]`

### 6. OUTPUTS
- Localized CSV tables in `Assets/Localization/` and runtime string evaluation.

### 7. RULES
- `[RULE-LOC-001]`: **Default Locale** — Primary locale MUST be set to `es-MX`.
- `[RULE-LOC-002]`: **CSV Encoding** — All localization CSV files MUST use UTF-8 encoding.
- `[RULE-LOC-003]`: **Interpolation Syntax** — Dynamic parameters MUST use ICU MessageFormat `{variable}` (curly braces). Mustache `{{key}}` is PROHIBITED.
- `[RULE-LOC-004]`: **Wrap Width** — Subtitle line wrap width MUST be `42` characters.
- `[RULE-LOC-005]`: **Overflow Handling** — Overflow strategy MUST be `truncate_ellipsis` (append `...`).
- `[RULE-LOC-006]`: **Text Bounds** — UI button max `18` chars, HUD label max `24` chars, subtitle max `42` chars/line, max `2` subtitle lines.

### 8. ALGORITHMS
Configuration defined in `LOCALIZATION_SCHEMA.yaml`:

```yaml
localization:
  supported_locales: ["es-MX", "en-US", "ja-JP"]
  string_format: "ICU MessageFormat"
  fallback_locale: "en-US"
  interpolation_syntax: "{variable}"
  wrap_width_chars: 42
  overflow: "truncate_ellipsis"
  text_bounds:
    ui_button_max_chars: 18
    hud_label_max_chars: 24
    subtitle_max_chars_per_line: 42
    subtitle_max_lines: 2
  font_fallbacks:
    - "Inter"
    - "Noto Sans JP"
    - "Noto Sans SC"
```

### 9. CONSTRAINTS
- `[CONS-LOC-001]`: Prohibido hardcoding UI text strings directly in C# scripts.
- `[CONS-LOC-002]`: Prohibido using `{{key}}` mustache syntax. Use `{variable}` only.

### 10. VALIDATION
- `[VAL-LOC-001]`: `LevelValidator.cs` parses localization CSV tables and asserts 0 missing keys between `es-MX`, `en-US`, and `ja-JP`.
- `[VAL-LOC-002]`: Automated test asserts all localized strings respect text_bounds limits.

### 11. EXAMPLES
See `LOCALIZATION_SCHEMA.yaml` for canonical schema definition.

### 12. FAILURE CASES
- `[FAIL-LOC-001]`: **Missing Localization Key** — Key missing in table resulting in raw key string displayed to user. Result: `FAIL-LOC-01`.

### 13. CROSS REFERENCES
- [UI_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/UI_SPEC.md) `[SPEC-008]`
- [LOCALIZATION_SCHEMA.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCALIZATION_SCHEMA.yaml) `[SPEC-133]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-133 for Localization Schema.
- **v2.0 (2026-07-25)**: Added ICU MessageFormat interpolation, wrap_width=42, truncate_ellipsis overflow, text_bounds for UI/HUD/subtitles, font fallbacks (Inter, Noto Sans JP, Noto Sans SC).

(End of file - total 68 lines)