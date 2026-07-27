# Executable Specs Protocol

## Entry point

Every IA starts with:

```text
Docs/ExecutableSpecs/manifest.yaml
```

## Mandatory sequence

```text
manifest.yaml
authority.yaml
decisions.yaml
documents.yaml
task_schema.yaml
task-specific catalogs and rules
validator reports
ai report
```

## Status meanings

```text
implemented  = verified by code or runtime evidence
partial      = some implementation exists but contract is incomplete
unverified   = design exists without proof in runtime
violated     = current code contradicts the active decision
blocked      = IA must stop before modifying related systems
```

## Non-negotiable behavior

An IA must stop and report a blocker when a required asset, code field, profile,
validator or decision cannot be resolved. It must not replace missing evidence
with an assumption.
