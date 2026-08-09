\page saydam_mcknew Saydam-McKnew fast Wagner-Whitin implementation

# Saydam-McKnew fast Wagner-Whitin implementation

Public class:

`SaydamMcKnewFastWagnerWhitinSolver`

Reference:

C. Saydam and M. McKnew (1987),
*A Fast Microcomputer Program for Ordering Using the Wagner-Whitin Algorithm*,
Production and Inventory Management Journal 28(4), 15-19.

## Publication identity

The author-uploaded article record describes the program as a very fast
implementation of the **full original Wagner-Whitin algorithm** and explicitly
contrasts it with Evans (1985): Evans uses less array storage and is recommended
when storage is in severe shortage.

ULSAlgorithms preserves that architectural distinction.

## Modern implementation

This class materializes every regeneration interval cost before the DP phase.

The triangular matrix is flattened into one contiguous pooled `double[]`,
avoiding row objects and preserving sequential memory access.

For period `i` and terminal period `j`, the precomputation incrementally updates:

- demand accumulated in the lot;
- delivered unit cost from period `i`;
- regeneration interval variable cost.

The second phase performs the original forward Wagner-Whitin recursion using
only table lookups and additions.

## Complexity

- precomputation: O(T²);
- forward DP: O(T²);
- total time: O(T²);
- working memory: O(T²).

This deliberate memory-for-speed trade-off is the principal distinction from
`WagnerWhitinEvansSolver`, which uses O(T) memory.
