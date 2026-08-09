# ULSAlgorithms 0.10.0 - Heady-Zhu Economic-Part-Period solver

Apply this package after release v0.9.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.10.0 file.

Added public solver:

- `HeadyZhuEconomicPartPeriodSolver`

Algorithmic identity:

- Wagner-Whitin exact dynamic programming;
- Planning Horizon Theorem;
- Economic-Part-Period pruning;
- constant setup / production / relevant holding-cost specialization;
- O(n^2) worst-case time;
- O(n) auxiliary memory;
- data-dependent practical work.

Important provenance note:

The original Heady-Zhu publisher record exposes the abstract but not the full
algorithm text. The fixed-cost DPP=A/H cutoff in this implementation is
therefore also tied explicitly to the later openly accessible primary
derivation by Sadjadi, Aryanezhad and Sadeghi (2009). Source XML comments and
the Doxygen page state this distinction.

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. If a test fails, report the exact test name and full assertion/exception.
5. Commit + push only after the complete suite is green.
6. Wait for Build and Test + Build Documentation.
7. Publish v0.10.0 through Create Release.
