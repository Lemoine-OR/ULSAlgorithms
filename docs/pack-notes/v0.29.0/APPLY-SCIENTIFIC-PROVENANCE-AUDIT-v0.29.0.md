# Apply the v0.29.0 scientific-provenance audit

This pack is based on main commit
`616190358ff98a2075d9e737c1e269f2a6f7824d`.

1. Extract the pack at the repository root, replacing files.
2. Run:

```powershell
.\_apply-scientific-provenance-v0.29.0.ps1
```

3. Delete the one-time helper:

```powershell
Remove-Item .\_apply-scientific-provenance-v0.29.0.ps1
```

4. Run the focused validation:

```powershell
dotnet test `
    .\tests\ULSAlgorithms.Tests\ULSAlgorithms.Tests.csproj `
    --configuration Release `
    --filter "FullyQualifiedName~ScientificMetadataTests"
```

5. Then run:

```powershell
.\tools\Test-SolverCatalog.ps1
.\tools\Test-Automation.ps1
.\build\Build-Validated.ps1
```

Do not publish v0.29.0 yet. This is one qualification block in the final
pre-1.0 release.
