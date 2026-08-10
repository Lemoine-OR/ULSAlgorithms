# v0.29.0 — scientific provenance audit before 1.0

## Scope

This qualification block audits the public scientific metadata of all 42
runtime strategies before the 1.x compatibility baseline is frozen.

The audit covers:

- publication / historical provenance;
- normalized DOI values and intentional DOI blanks;
- documented time and space complexity;
- applicability;
- whether an implementation is a published method, a solver-backed
  formulation, a classical rule or a modern reconstruction.

## Corrections made

### Evans 1985

`wagner-whitin-evans` already contained the complete source-level citation and
DOI in `WagnerWhitinEvansSolver.cs`, but the canonical runtime catalog had an
empty DOI.

The catalog now records:

`10.1016/0272-6963(85)90009-9`

### DeMatteis 1968

The early primary Part-Period Algorithm publication is:

J. J. DeMatteis, *An Economic Lot-Sizing Technique I: The Part-Period
Algorithm*, IBM Systems Journal 7(1), 30-38, 1968.

The catalog entry `part-period-balancing` now records:

`10.1147/sj.71.0030`

### Lyu-Lee parallel complexity wording

The implementation is a modern shared-memory reconstruction. Its code evaluates
arc costs in O(1) from prefix data and partitions the triangular predecessor
scan across effective workers.

The public complexity description is therefore made implementation-specific:

`O(T²) work; O(T²/p) ideal parallel candidate span`

This avoids presenting an ambiguous historical publisher rendering as if it
were a direct complexity guarantee of the original PVM listing.

## Intentional DOI blanks

After the two verified DOI additions, exactly ten catalog entries intentionally
do not assert a DOI:

- `sadjadi-aryanezhad-sadeghi`
- `saydam-mcknew`
- `jacobs-khumawala`
- `lot-for-lot`
- `silver-meal`
- `least-unit-cost`
- `groff`
- `periodic-order-quantity`
- `freeland-colley`
- `chiu-ting-modified-part-period-balancing`

A blank means only that the library does not assert a DOI for that entry. It is
not a claim that no DOI can exist.

## Regression protection

`ScientificMetadataTests` now contains an audited baseline for every public
strategy. For all 42 stable IDs it verifies exact:

- scientific reference;
- DOI;
- time complexity;
- space complexity;
- applicability;
- implementation characterization.

It also validates source-path normalization and DOI formatting.

## Generated catalog

After changing the canonical runtime metadata,
`docs/algorithm-catalog.json` must be regenerated with the existing canonical
exporter:

```powershell
dotnet run `
    --configuration Release `
    --project .\tools\ULSAlgorithms.CatalogExporter\ULSAlgorithms.CatalogExporter.csproj `
    -- `
    --write .\docs\algorithm-catalog.json
```

The normal automation preflight subsequently proves that the generated JSON and
runtime catalog are synchronized.

No solver mathematics, stable solver ID or public API member is changed by this
audit.
