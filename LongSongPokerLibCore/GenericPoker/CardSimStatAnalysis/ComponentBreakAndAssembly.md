# Poker Component Breakdown and Assembly

This document explains the architecture and mechanics of breaking composite poker hands into atomic components and assembling them into evaluated hand ranks for multi-hand splitting (such as Eight-Card and Nine-Card poker games).

---

## 1. Overview & Architecture

In multi-card games (e.g., 8-card or 9-card poker), hands are evaluated and split into front and back hands (where the back hand must be at least as strong as the front hand: `BackHand >= FrontHand`).

To systematically evaluate all valid front/back hand arrangements:
1. **Component Decomposition (`PokerComponents.BreakDown`)**: Large/oversized structures (such as a 9-card flush, an 8-card straight, or four-of-a-kind) are broken down into valid atomic building blocks.
2. **Multi-Group Cartesian Product (`PokerComponents.CartesianProduct`)**: When a hand has multiple component groups (e.g., a flush group and a pair group), candidate decompositions from each group are combined across groups to yield all complete atomic hand representations.
3. **Hand Assembly (`AssemblyComponent.AssembleHandRank`)**: Candidate subsets of atomic components are mapped and assembled into composite overall hand ranks (`SimCardOverAllHandRank`), such as `FullHouse`, `TwoPairs`, `Mansion`, or individual component ranks.
4. **Permutation & Splitting (`InitEightCardHandSplitProbAna.SplitHand`)**: Combinations of components are distributed into front and back hands, assembled into ranks, validated (`BackHand >= FrontHand`), and deduplicated.

```
+-------------------------------------------------------------+
|                      Full Card Hand                         |
|             (e.g., 9-Card Flush, 4-of-a-Kind, etc.)         |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|             PokerComponents.BreakDown()                     |
|  - 9-Card Flush  -> [5-Card Flush + 4-Card Flush], etc.     |
|  - FourOfKind    -> [FourOfKind], [Pair + Pair]             |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|             PokerComponents.CartesianProduct()              |
| Combines candidate breakdowns across independent groups     |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|          Permutation Partitioning into Front / Back         |
|     (e.g., Front: [Pair], Back: [ThreeOfKind])              |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|            AssemblyComponent.AssembleHandRank()             |
|   Front Rank: Pair                                          |
|   Back Rank:  FullHouse (ThreeOfKind + Pair)                |
|   Legality:   Back >= Front (Valid)                         |
+-------------------------------------------------------------+
```

---

## 2. Component Power & Metadata (`PokerComponents`)

The `PokerComponents` class encapsulates individual hand components with metadata and power rankings:

- `CompType`: The type of component (`SimCardsCompType`).
- `CardCount`: Number of cards making up the component (e.g., 2 for `Pair`, 3 for `ThreeOfKind`, 5 for `FiveCardsFlush`).
- `Power`: An integer value reflecting relative component strength, used for balanced sorting and ranking comparisons.

### Component Power Table (Sample)

| Component Type | Card Count | Power |
| :--- | :---: | :---: |
| `Pair` | 2 | 1 |
| `ThreeCardsStraight` | 3 | 2 |
| `ThreeCardsFlush` | 3 | 3 |
| `FourCardStraight` | 4 | 4 |
| `FourCardsFlush` | 4 | 5 |
| `ThreeOfKind` | 3 | 10 |
| `ThreeCardsFlushStraight` | 3 | 15 |
| `FiveCardsStraight` | 5 | 18 |
| `FiveCardsFlush` | 5 | 20 |
| `FourCardsFlushStraight` | 4 | 25 |
| `FourOfKind` | 4 | 30 |
| `FiveCardsFlushStraight` | 5 | 35 |

---

## 3. Atomic Breakdown Rules (`BreakDown`)

The `PokerComponents.BreakDown()` method utilizes **mathematical integer partition generation** filtered by minimum card constraints (`_minFlushCards`, `_minFlushStraightCards`, `_minStraightCards`, `_minKindCards`):

1. **Integer Partitions**: For a component of $N$ cards, all integer partitions $N = p_1 + p_2 + \dots + p_k$ ($p_1 \ge p_2 \ge \dots \ge p_k \ge 1$) are mathematically generated.
2. **Constraint Filtering**: Each part $p_i$ is tested against the corresponding component minimum threshold:
   - **Flush Straights**: Filtered by `_minFlushStraightCards` (default: 3). E.g., for an 8-card flush straight, $(8) \to [8]$, $(7,1) \to [7]$ (1 is ineligible), $(6,2) \to [6]$ (2 is ineligible), $(5,3) \to [5,3]$, $(4,4) \to [4,4]$, $(4,3,1) \to [4,3]$, $(3,3,2) \to [3,3]$, etc.
   - **Flushes**: Filtered by `_minFlushCards` (default: 5). E.g., for an 8-card flush, $(8) \to [8]$, $(7,1) \to [7]$, $(6,2) \to [6]$, $(5,3) \to [5]$ (3 is ineligible when min=5), etc.
   - **Straights**: Filtered by `_minStraightCards` (default: 5).
   - **Sets / Multiples (Kinds)**: Filtered by `_minKindCards` (default: 2). E.g., `FourOfKind` (4 cards) breaks into `[FourOfKind]`, `[ThreeOfKind]`, `[Pair, Pair]`, `[Pair]`.
3. **Deduplication**: Partitions resulting in the same set of eligible atomic components are deduplicated while preserving canonical descending order.

---

## 4. Multi-Group Cartesian Product (`CartesianProduct`)

When a hand contains components across different groups (for example, Group 1: 9-card flush; Group 2: Four-of-a-kind), each group independently produces multiple candidate component breakdown options.

The `CartesianProduct` method generates all combinations by selecting exactly one candidate breakdown from each group:

```csharp
private static List<List<List<PokerComponents>>> CartesianProduct(List<List<List<PokerComponents>>> sequences)
{
    var result = new List<List<List<PokerComponents>>> { new() };

    foreach (var sequence in sequences)
    {
        var temp = new List<List<List<PokerComponents>>>();
        foreach (var existing in result)
        {
            foreach (var item in sequence)
            {
                var copy = new List<List<PokerComponents>>(existing) { item };
                temp.Add(copy);
            }
        }
        result = temp;
    }

    return result;
}
```

Flattening each combination produces a complete candidate component list for the entire hand.

---

## 5. Hand Rank Assembly (`AssemblyComponent.AssembleHandRank`)

`AssemblyComponent.AssembleHandRank` evaluates a list of atomic components and maps them to an overall hand rank (`SimCardOverAllHandRank`):

```csharp
public static SimCardOverAllHandRank AssembleHandRank(IEnumerable<PokerComponents>? components)
```

### Assembly Mapping Rules:
1. **Single Component**:
   - `Pair` $\rightarrow$ `Pair`
   - `ThreeOfKind` $\rightarrow$ `ThreeOfKind`
   - `FourOfKind` $\rightarrow$ `FourOfKind`
   - `FiveCardsFlush` $\rightarrow$ `FiveCardsFlush`
   - Direct 1-to-1 mapping to matching `SimCardOverAllHandRank`.
2. **Two Components**:
   - `ThreeOfKind` + `Pair` $\rightarrow$ `FullHouse`
   - `ThreeCardsFlushStraight` + `Pair` $\rightarrow$ `Mansion`
   - `Pair` + `Pair` $\rightarrow$ `TwoPairs`
3. **Invalid / Unmatched Combinations**:
   - Returns `SimCardOverAllHandRank.None` (indicating an illegal split or non-combinable components).

---

## 6. Hand Splitting Pipeline (`InitEightCardHandSplitProbAna.SplitHand`)

The split algorithm breaks down components into atomic subsets, forms candidate combinations across groups via Cartesian product, partitions each candidate set into all legal front and back hand pairs, and removes dominated ("obviously loser") splits:

```csharp
public static List<(SimCardOverAllHandRank, SimCardOverAllHandRank)> SplitHand(
    List<PokerComponents> comps,
    int minFlushStraightCards = -1,
    int minFlushCards = -1,
    int minStraightCards = -1,
    int minKindCards = -1)
{
    if (comps == null || comps.Count == 0) return new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>();

    // 1. Break down each component into atomic sub-components based on card constraints.
    var breakdownSequences = comps
        .Select(c => c.BreakDown(minFlushStraightCards, minFlushCards, minStraightCards, minKindCards))
        .ToList();

    // 2. Generate Cartesian Product of all candidate breakdowns across components.
    var candidateCombinations = PokerComponents.CartesianProduct(breakdownSequences);

    var solutions = new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>();

    foreach (var combination in candidateCombinations)
    {
        // Flatten the candidate components
        var atomicComps = combination.SelectMany(c => c).ToList();

        // Sort atomic components by power descending
        atomicComps.Sort((a, b) => b.Power.CompareTo(a.Power));

        // Explore all possible split component groups (up to half total count for front hand)
        int maxFrontCount = atomicComps.Count / 2;

        for (int selectCount = 0; selectCount <= maxFrontCount; selectCount++)
        {
            var possibleGroups = UtilFunc.GetPermutationAllowedDuplicated(atomicComps, selectCount);
            foreach (var group in possibleGroups)
            {
                var frontGroup = group.Selected;
                var backGroup = group.Remaining;

                var frontRank = AssemblyComponent.AssembleHandRank(frontGroup);
                var backRank = AssemblyComponent.AssembleHandRank(backGroup);

                // If either rank is None, it is an invalid split; skip it.
                if (frontRank == SimCardOverAllHandRank.None || backRank == SimCardOverAllHandRank.None) continue;

                // Ensure back rank >= front rank (swap if necessary)
                if ((int)frontRank > (int)backRank)
                {
                    var swappedFrontRank = backRank;
                    var swappedBackRank = frontRank;

                    if ((int)swappedBackRank >= (int)swappedFrontRank)
                    {
                        solutions.Add((swappedFrontRank, swappedBackRank));
                    }
                }
                else
                {
                    solutions.Add((frontRank, backRank));
                }
            }
        }
    }

    var uniqueSolutions = solutions.Distinct().ToList();
    return FilterDominatedSolutions(uniqueSolutions);
}
```

### Dominated Solution Filtering (`FilterDominatedSolutions`)
Eliminates dominated candidate splits. A candidate split $(F_2, B_2)$ is dominated by $(F_1, B_1)$ if $F_1 \ge F_2$ and $B_1 \ge B_2$ with at least one strict inequality.
- For example, with an 8-card flush straight: `(Nothing, 8FS)` dominates `(Nothing, 7FS)` and `(Nothing, 6FS)`; `(3FS, 5FS)` and `(4FS, 4FS)` dominate `(3FS, 4FS)` and `(3FS, 3FS)`.
- The filtered result retains only the Pareto-optimal split hand solutions.

---

## 7. Examples

### Example 1: Splitting Full House Components
Input: `[ThreeOfKind, Pair]`
- Candidate split 1: Front `[Pair]`, Back `[ThreeOfKind]`
  - Front rank: `Pair`
  - Back rank: `ThreeOfKind`
  - Valid: `ThreeOfKind >= Pair` $\rightarrow$ `(Pair, ThreeOfKind)`
- Candidate split 2: Front `[]` (Nothing), Back `[ThreeOfKind, Pair]`
  - Front rank: `Nothing`
  - Back rank: `FullHouse`
  - Valid: `FullHouse >= Nothing` $\rightarrow$ `(Nothing, FullHouse)`

### Example 2: Decomposing a 9-Card Flush
```csharp
var nineFlush = new PokerComponents(SimCardsCompType.NineCardsFlush);
var breakdowns = nineFlush.BreakDown();
```
Output candidate decompositions:
1. `[FiveCardsFlush, FourCardsFlush]`
2. `[SixCardsFlush, ThreeCardsFlush]`
3. `[NineCardsFlush]`

When distributed into front and back hands, the `[FiveCardsFlush, FourCardsFlush]` candidate enables:
- Front: `FourCardsFlush`
- Back: `FiveCardsFlush`
- Result: Legal split `(FourCardsFlush, FiveCardsFlush)`

---

## 8. Verification & Tests

The implementation is verified by test suites in:
- `UnitTest/PokerComponentBreakingAndAssemblyTest.cs`:
  - `TestFlushBreakdown`: Tests 9-card flush decomposition into 5-card + 4-card flushes.
  - `TestMultiplesBreakdown`: Tests four-of-a-kind decomposition into pair + pair.
  - `TestCartesianProductOfBreakdowns`: Tests multi-group cross-product decomposition.
  - `TestAssembleHandRank`: Tests single and composite hand assembly (`FullHouse`, `Mansion`, `TwoPairs`).
  - `TestSplitHandIntegration`: Tests complete end-to-end split generation.
- `UnitTest/SimHandSplitProbAnaTest.cs`:
  - `TestEightCardHandSplitProbAna`: Validates 8-card probability analysis and split outputs against historical baseline.
  - `TestNineCardHandSplitProbAna`: Validates 9-card probability analysis and split outputs against historical baseline.
