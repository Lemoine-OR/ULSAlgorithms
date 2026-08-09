# ULSAlgorithms 0.6.0 - Federgruen-Tzur general solver

Apply this package after release v0.5.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.6.0 file.

Added public solver:

- `FedergruenTzurSolver`

Implementation:

- forward Minimal Optimal Predecessor algorithm;
- array-backed AVL balanced binary tree;
- O(n log n) time;
- O(n) auxiliary space;
- ArrayPool-backed primitive storage;
- no LINQ in the hot path.

Validation:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. Commit + push.
5. Wait for Build and Test + Build Documentation.
6. Publish v0.6.0 through Create Release.

The two linear-time specializations in the same Federgruen-Tzur paper are
deliberately not folded into this class. They are scheduled as separate
public solvers in the next package:

- nondecreasing setup costs;
- no speculative inventory motive.
