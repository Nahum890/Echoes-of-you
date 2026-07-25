# LOCALIZATION_SCHEMA.md — Localization Tables & String Interpolation Specs
## Spec ID: SPEC-133
## Version: 1.0 (AI-Executable)

---

### 1. PURPOSE
Defines minimum localization table structure, locale fallback chains (`es-MX` primary, `en-US` secondary), dynamic string parameter formatting, and font SDF fallbacks for *Echoes of You 2.0*.

### 2. SCOPE
Applies to UI text components, narrative prompt dialogues, menu labels, and `LocalizationManager.cs`.

### 3. AUTHORITY
Nivel 3 (Especificación Ejecutable). Subordinate to `SOURCE_OF_TRUTH.md` (`SPEC-000`) and `UI_SPEC.md` (`SPEC-008`).

### 4. DEFINITIONS
- `Primary Locale`: `es-MX` (Mexican Spanish) as the canonical source locale.
- `Dynamic Param`: Mustache string interpolation format `{{key}}` for runtime variable replacement.

### 5. INPUTS
- [UI_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/UI_SPEC.md) `[SPEC-008]`
- [LOCALIZATION_SCHEMA.yaml](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/LOCALIZATION_SCHEMA.yaml) `[SPEC-133]`

### 6. OUTPUTS
- Localized CSV tables in `Assets/Localization/` and runtime string evaluation.

### 7. RULES
- `[RULE-LOC-001]`: **Default Locale**: Primary locale MUST be set to `es-MX`.
- `[RULE-LOC-002]`: **CSV Encoding**: All localization CSV files MUST use UTF-8 encoding.
- `[RULE-LOC-003]`: **UI Character Limit**: UI localized strings MUST NOT exceed 64 characters to prevent text overflow in HUD elements.

### 8. ALGORITHMS
See canonical configuration at `Docs/Specs/LOCALIZATION_SCHEMA.yaml`.

### 9. CONSTRAINTS
- `[CONS-LOC-001]`: Prohibido hardcoding UI text strings directly in C# scripts.

### 10. VALIDATION
- `[VAL-LOC-001]`: `LevelValidator.cs` parses localization CSV tables and asserts 0 missing keys between `es-MX` and `en-US`.

### 11. EXAMPLES
- `LOCALIZATION_SCHEMA.yaml` schema definition.

### 12. FAILURE CASES
- `[FAIL-LOC-001]`: **Missing Localization Key**: Key missing in table resulting in raw key string displayed to user. Result: `FAIL-LOC-01`.

### 13. CROSS REFERENCES
- [UI_SPEC.md](file:///c:/Users/lol xdd/OneDrive/Documentos/Colegio/Echoes of you/Docs/Specs/UI_SPEC.md) `[SPEC-008]`

### 14. CHANGE HISTORY
- **v1.0 (2026-07-25)**: Created canonical SPEC-133 for Localization Schema.
