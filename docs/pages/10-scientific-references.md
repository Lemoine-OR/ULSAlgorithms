\page scientific_references Scientific References

# Scientific References

ULSAlgorithms treats scientific provenance as part of the public strategy
contract. A strategy named after a publication is not merely a label: its
source documentation, applicability conditions, complexity statement and
implementation note must remain consistent with the literature basis and with
the code that is actually shipped.

## v0.29.0 pre-1.0 provenance audit

Before freezing the 1.x compatibility baseline, all **42 public strategy IDs**
were reviewed against the runtime catalog, source-level documentation and,
where durable publisher metadata is available, the publication record.

The audit deliberately distinguishes three things:

1. **publication metadata** — author/year and DOI or other durable publication
   record when one can be asserted confidently;
2. **implementation metadata** — what this C# implementation actually does;
3. **complexity/applicability metadata** — claims about the implementation,
   not an automatic copy of an abstract or historical program listing.

A blank DOI is intentional and means **ULSAlgorithms does not assert a DOI for
that catalog entry**. It must not be interpreted as a claim that no DOI exists.

The v0.29.0 audit made three concrete metadata corrections/clarifications:

- Evans (1985) now records DOI `10.1016/0272-6963(85)90009-9`, already present
  in the source-level documentation but previously absent from the runtime
  catalog.
- DeMatteis (1968), the early primary Part-Period Algorithm reference, now
  records DOI `10.1147/sj.71.0030`.
- The Lyu-Lee parallel strategy now states the complexity of the **library
  implementation** explicitly: `O(T²)` total work with ideal
  `O(T²/p)` parallel candidate-evaluation span. The implementation remains
  documented as a modern shared-memory reconstruction rather than a
  transliteration of the historical PVM code.

No solver mathematics or public solver identifier is changed by this audit.

## Audited strategy matrix

| Stable strategy ID | Scientific / historical provenance | DOI recorded by ULSAlgorithms | Documented time |
|---|---|---|---|
| `adaptive-exact` | Wagelmans, Van Hoesel & Kolen (1992); Federgruen & Tzur (1991) | `10.1287/opre.40.1.S145` | O(T) in the NSM case; O(T log T) in the general case |
| `wagner-whitin-classical` | Wagner & Whitin (1958) | `10.1287/mnsc.5.1.89` | O(T²) |
| `wagner-whitin-evans` | Evans (1985) | `10.1016/0272-6963(85)90009-9` | O(T²) |
| `wagner-whitin-linear` | Wagelmans, Van Hoesel & Kolen (1992) | `10.1287/opre.40.1.S145` | O(T) |
| `wagelmans-general` | Wagelmans, Van Hoesel & Kolen (1992) | `10.1287/opre.40.1.S145` | O(T log T) |
| `federgruen-tzur-general` | Federgruen & Tzur (1991) | `10.1287/mnsc.37.8.909` | O(T log T) |
| `federgruen-tzur-nsm` | Federgruen & Tzur (1991) | `10.1287/mnsc.37.8.909` | O(T) |
| `federgruen-tzur-nondecreasing-setup` | Federgruen & Tzur (1991) | `10.1287/mnsc.37.8.909` | O(T) |
| `aggarwal-park` | Aggarwal & Park (1993), Improved Algorithms for Economic Lot Size Problems, Operations Research 41(3), 549-571 | `10.1287/opre.41.3.549` | O(T log T) |
| `bahl-taj-planning-horizon` | Bahl & Taj (1991) | `10.1016/0360-8352(91)90033-3` | O(T²) worst case |
| `heady-zhu` | Heady & Zhu (1994) | `10.1111/j.1937-5956.1994.tb00109.x` | O(T²) worst case |
| `chowdhury-baki-azab` | Chowdhury, Baki & Azab (2018) | `10.1016/j.cie.2018.01.010` | O(T) |
| `sadjadi-aryanezhad-sadeghi` | Sadjadi, Aryanezhad & Sadeghi (2009) | *not asserted* | O(T²) worst case |
| `lyu-lee-parallel` | Lyu & Lee (2001) | `10.1016/S0360-8352(01)00047-X` | O(T²) work; O(T²/p) ideal parallel candidate span |
| `saydam-mcknew` | Saydam & McKnew (1987) | *not asserted* | O(T²) |
| `jacobs-khumawala` | Jacobs & Khumawala (1987) | *not asserted* | O(T²) |
| `zangwill-network` | Zangwill (1969) | `10.1287/mnsc.15.9.506` | O(T²) |
| `aggregate-inventory-formulation` | Wagner & Whitin (1958); Brahimi et al. (2006) | `10.1287/mnsc.5.1.89` | Solver-dependent |
| `facility-location-formulation` | Krarup & Bilde (1977); Brahimi et al. (2006) | `10.1007/978-3-0348-5936-3_10` | Solver-dependent |
| `shortest-path-formulation` | Zangwill (1969); Brahimi et al. (2006) | `10.1287/mnsc.15.9.506` | Solver-dependent |
| `inventory-eliminated-formulation` | Brahimi et al. (2006) | `10.1016/j.ejor.2004.01.054` | Solver-dependent |
| `general-ls-cutting-plane` | Barany, Van Roy & Wolsey (1984) | `10.1007/BFb0121006` | O(T²) separation per root iteration + solver |
| `wagner-whitin-ls-cutting-plane` | Pochet & Wolsey (1994) | `10.1007/BF01582225` | O(T²) separation per root iteration + solver |
| `lot-for-lot` | Classical MRP rule | *not asserted* | O(T) |
| `silver-meal` | Silver & Meal (1973) | *not asserted* | O(T) |
| `least-unit-cost` | Classical LUC rule | *not asserted* | O(T) |
| `part-period-balancing` | DeMatteis (1968) | `10.1147/sj.71.0030` | O(T) |
| `groff` | Groff (1979) | *not asserted* | O(T) |
| `periodic-order-quantity` | Classical POQ rule | *not asserted* | O(T) |
| `freeland-colley` | Freeland & Colley (1982) | *not asserted* | O(T) |
| `patterson-laforge-incremental-part-period` | Patterson & LaForge (1985) | `10.1111/j.1745-493X.1985.tb00132.x` | O(T) |
| `wemmerlov-modified-ppb` | Wemmerlöv (1983) | `10.1016/0272-6963(83)90023-2` | O(T) |
| `wemmerlov-ppb-lalb` | Wemmerlöv (1983) | `10.1016/0272-6963(83)90023-2` | O(T) |
| `wemmerlov-modified-ppb-lalb` | Wemmerlöv (1983) | `10.1016/0272-6963(83)90023-2` | O(T) |
| `part-period-simplified` | DeMatteis (1968); Baciarello et al. (2013) | `10.5772/56004` | O(T) |
| `segerstedt-reformulated-silver-meal` | Segerstedt, Abdul-Jalbar & Samuelsson (2023) | `10.3390/axioms12070661` | O(T) |
| `chiu-modified-least-unit-cost` | Chiu (2004) | `10.1080/09720510.2004.10701115` | O(T) |
| `chiu-ting-modified-part-period-balancing` | Chiu, Ting & Chiu (2005) | *not asserted* | O(T) |
| `ho-chang-solis-net-least-period-cost` | Ho, Chang & Solis (2006) | `10.1057/palgrave.jors.2602076` | O(T) |
| `ho-chang-solis-improved-net-least-period-cost` | Ho, Chang & Solis (2006) | `10.1057/palgrave.jors.2602076` | O(T) |
| `mclaren-order-moment` | McLaren (1977); Baciarello et al. (2013) | `10.5772/56004` | O(T) |
| `karni-maximum-part-period-gain` | Karni (1981); Baciarello et al. (2013) | `10.5772/56004` | O(T log T) |

## Foundational exact methods

- H. M. Wagner and T. M. Whitin (1958), *Dynamic Version of the Economic Lot
  Size Model*, Management Science 5(1), 89-96.
  DOI: https://doi.org/10.1287/mnsc.5.1.89
- J. R. Evans (1985), *An Efficient Implementation of the Wagner-Whitin
  Algorithm for Dynamic Lot-Sizing*, Journal of Operations Management 5(2),
  229-235.
  DOI: https://doi.org/10.1016/0272-6963(85)90009-9
- W. I. Zangwill (1969), *A Backlogging Model and a Multi-Echelon Model of a
  Dynamic Economic Lot Size Production System--A Network Approach*,
  Management Science 15(9), 506-527.
  DOI: https://doi.org/10.1287/mnsc.15.9.506
- A. Federgruen and M. Tzur (1991), *A Simple Forward Algorithm to Solve
  General Dynamic Lot Sizing Models with n Periods in O(n log n) or O(n)
  Time*, Management Science 37(8), 909-925.
  DOI: https://doi.org/10.1287/mnsc.37.8.909
- A. Wagelmans, S. van Hoesel and A. Kolen (1992), *Economic Lot Sizing:
  An O(n log n) Algorithm That Runs in Linear Time in the Wagner-Whitin
  Case*, Operations Research 40(1 supplement), S145-S156.
  DOI: https://doi.org/10.1287/opre.40.1.S145
- A. Aggarwal and J. K. Park (1993), *Improved Algorithms for Economic Lot
  Size Problems*, Operations Research 41(3), 549-571.
  DOI: https://doi.org/10.1287/opre.41.3.549

## Data-dependent, implementation-oriented and parallel exact methods

- H. C. Bahl and S. Taj (1991), *A data-dependent efficient implementation of
  the Wagner-Whitin algorithm for lot-sizing*, Computers & Industrial
  Engineering 20(2), 289-291.
  DOI: https://doi.org/10.1016/0360-8352(91)90033-3
- R. B. Heady and Z. Zhu (1994), *An Improved Implementation of the
  Wagner-Whitin Algorithm*, Production and Operations Management 3(1), 55-63.
  DOI: https://doi.org/10.1111/j.1937-5956.1994.tb00109.x
- N. T. Chowdhury, M. F. Baki and A. Azab (2018), *Dynamic Economic
  Lot-Sizing Problem: A new O(T) Algorithm for the Wagner-Whitin Model*,
  Computers & Industrial Engineering 117, 6-18.
  DOI: https://doi.org/10.1016/j.cie.2018.01.010
- S. J. Sadjadi, M. B. Gh. Aryanezhad and H. A. Sadeghi (2009),
  *An Improved WAGNER-WHITIN Algorithm*, International Journal of Industrial
  Engineering & Production Research 20(3), 117-123. No DOI is asserted by the
  runtime catalog.
- J.-J. Lyu and M.-C. Lee (2001), *A parallel algorithm for the dynamic
  lot-sizing problem*, Computers & Industrial Engineering 41(2), 127-134.
  DOI: https://doi.org/10.1016/S0360-8352(01)00047-X
- C. Saydam and M. McKnew (1987), *A Fast Microcomputer Program for Ordering
  Using the Wagner-Whitin Algorithm*, Production and Inventory Management
  Journal 28(4), 15-19. No DOI is asserted by the runtime catalog.
- F. R. Jacobs and B. M. Khumawala (1987), *A Simplified Procedure for Optimal
  Single-Level Lot Sizing*, Production and Inventory Management 28(3), 39-43.
  No DOI is asserted by the runtime catalog.

## Mathematical formulations and cutting planes

- J. Krarup and O. Bilde (1977), *Plant location, Set Covering and Economic Lot
  Size: An O(mn)-Algorithm for Structured Problems*, in *Numerische Methoden
  bei Optimierungsaufgaben Band 3*, pp. 155-180.
  DOI: https://doi.org/10.1007/978-3-0348-5936-3_10
- N. Brahimi, S. Dauzere-Peres, N. M. Najid and A. Nordli (2006),
  *Single item lot sizing problems*, European Journal of Operational Research
  168(1), 1-16.
  DOI: https://doi.org/10.1016/j.ejor.2004.01.054
- I. Barany, T. Van Roy and L. A. Wolsey (1984), *Uncapacitated lot-sizing:
  The convex hull of solutions*, Mathematical Programming Study 22, 32-43.
  DOI: https://doi.org/10.1007/BFb0121006
- Y. Pochet and L. A. Wolsey (1994), *Polyhedra for lot-sizing with
  Wagner-Whitin costs*, Mathematical Programming 67, 297-323.
  DOI: https://doi.org/10.1007/BF01582225

For solver-backed formulations, the catalog may cite the publication that most
directly identifies the represented formulation, while the implementation
documentation can additionally cite the Brahimi et al. survey used to
cross-check formulation families.

## Classical and modern heuristics

- E. A. Silver and H. C. Meal (1973), *A heuristic for selecting lot size
  quantities for the case of a deterministic time-varying demand rate and
  discrete opportunities for replenishment*, Production and Inventory
  Management 14(2), 64-74. No DOI is asserted by the runtime catalog.
- J. J. DeMatteis (1968), *An Economic Lot-Sizing Technique I: The Part-Period
  Algorithm*, IBM Systems Journal 7(1), 30-38.
  DOI: https://doi.org/10.1147/sj.71.0030
- G. K. Groff (1979), *A Lot-Sizing Rule for Time-Phased Component Demand*,
  Production and Inventory Management 20(1), 47-53. No DOI is asserted by the
  runtime catalog.
- J. R. Freeland and J. L. Colley Jr. (1982), *A Simple Heuristic Method for
  Lot-Sizing in a Time-Phased Reorder System*, Production and Inventory
  Management 23(1), 15-22. No DOI is asserted by the runtime catalog.
- J. W. Patterson and R. L. LaForge (1985), *The Incremental Part-Period
  Algorithm: An Alternative to EOQ*, Journal of Purchasing and Materials
  Management 21(2), 28-33.
  DOI: https://doi.org/10.1111/j.1745-493X.1985.tb00132.x
- U. Wemmerlov (1983), *The Part-Period Balancing Algorithm and Its Look
  Ahead-Look Back Feature*, Journal of Operations Management 4(1), 23-39.
  DOI: https://doi.org/10.1016/0272-6963(83)90023-2
- L. Baciarello, M. D'Avino, R. Onori and M. M. Schiraldi (2013),
  *Lot Sizing Heuristics Performance*, International Journal of Engineering
  Business Management 5.
  DOI: https://doi.org/10.5772/56004
- A. Segerstedt, B. Abdul-Jalbar and B. Samuelsson (2023), *Reformulated
  Silver-Meal and Similar Lot Sizing Techniques*, Axioms 12(7), 661.
  DOI: https://doi.org/10.3390/axioms12070661
- Y. P. Chiu (2004), *A modification of the least unit cost lot-sizing
  heuristic*, Journal of Statistics and Management Systems 7(1), 197-207.
  DOI: https://doi.org/10.1080/09720510.2004.10701115
- S. W. Chiu, C.-K. Ting and Y.-S. P. Chiu (2005), *A modified version of the
  part period lot-sizing heuristic*, International Journal for Engineering
  Modelling 18(1-2), 59-64. No DOI is asserted by the runtime catalog.
- J. C. Ho, Y.-L. Chang and A. O. Solis (2006), *Two modifications of the
  least cost per period heuristic for dynamic lot-sizing*, Journal of the
  Operational Research Society 57(8), 1005-1013.
  DOI: https://doi.org/10.1057/palgrave.jors.2602076

Lot-for-Lot, classical Least Unit Cost and Periodic Order Quantity are cataloged
as classical rules rather than being assigned a single paper DOI.

## Provenance policy

A publication name on a public class or catalog entry is treated as a
provenance claim.

When the original program listing is unavailable and the implementation
reconstructs the published architecture, the documentation says **modern
reconstruction** (or equivalent wording) rather than implying line-by-line
transcription.

The audited unit-test baseline now locks, for every public strategy ID:

- scientific/historical reference;
- normalized DOI value, including intentional blanks;
- documented time and space complexity;
- applicability conditions;
- implementation characterization.

Any future change to one of these fields is therefore deliberate and must update
both the runtime catalog and the scientific audit baseline.
