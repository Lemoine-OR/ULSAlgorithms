# ULSAlgorithms 0.4.0 - Wagner-Whitin classical + Evans

Apply this package only after v0.3.0 has been released.

Copy the bundle contents directly into the repository root and replace
`version.json` with the included 0.4.0 file.

Added public solvers:

- `WagnerWhitinClassicalSolver`
- `WagnerWhitinEvansSolver`

Existing public solver retained:

- `WagnerWhitinSolver` (Wagelmans linear-time specialization)

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Optionally run `WagnerWhitinFamilyBenchmarks`.
4. Commit + push.
5. Wait for CI and documentation to be green.
6. Publish v0.4.0 through Create Release.

This package does not yet implement Heady-Zhu (1994), Saydam (1987),
Evans-Saydam-McKnew (1989), Sajadi et al. (2009), or Chowdhury-Baki-Azab
(2018). They remain on the explicit implementation roadmap rather than being
silently omitted.
