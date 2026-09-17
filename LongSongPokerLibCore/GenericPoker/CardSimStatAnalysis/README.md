# Card Simulation and Hand Split Probability Analysis (`CardSimStatAnalysis`)

This document provides a comprehensive analysis and reference for the **Card Simulation and Hand Split Probability Analysis** subsystem (`InitEightCardHandSplitProbAna.cs` and related components in `LongSongPokerLibCore/GenericPoker/CardSimStatAnalysis`).

This utility simulates, evaluates, and calculates the probability distribution of different hand ranks in multi-card poker variants (such as **Eight-Card** and **Nine-Card Poker**), specifically focusing on how composite hands are broken down, assembled, and split into a **Front Hand** and a **Back Hand**.

---

## Table of Contents
1. [Overview & System Architecture](#1-overview--system-architecture)
2. [Input and Data Source](#2-input-and-data-source)
3. [Hand Parsing & Component Power](#3-hand-parsing--component-power)
4. [Component Breakdown & Assembly Subsystem](#4-component-breakdown--assembly-subsystem)
   - [4.1. Atomic Breakdown Rules (`PokerComponents.BreakDown`)](#41-atomic-breakdown-rules-pokercomponentsbreakdown)
   - [4.2. Multi-Group Cartesian Product (`CartesianProduct`)](#42-multi-group-cartesian-product-cartesianproduct)
   - [4.3. Rule Hierarchy & Hand Rank Assembly (`ISimCardRule`)](#43-rule-hierarchy--hand-rank-assembly-isimcardrule)
   - [4.4. Hand Splitting Pipeline (`InitEightCardHandSplitProbAna.SplitHand`)](#44-hand-splitting-pipeline-initeightcardhandsplitprobana-splithand)
   - [4.5. Pareto-Optimal & Dominated Solution Filtering](#45-pareto-optimal--dominated-solution-filtering)
5. [Hand Splitting Rules & Strategy](#5-hand-splitting-rules--strategy)
6. [Multi-Solution Appearance Count Treatment](#6-multi-solution-appearance-count-treatment)
7. [Output Statistics & Cumulative Win Probabilities](#7-output-statistics--cumulative-win-probabilities)
8. [Concrete Examples](#8-concrete-examples)
9. [Verification & Test Suites](#9-verification--test-suites)
10. [Summary](#10-summary)

---

## 1. Overview & System Architecture

In multi-card poker games (e.g., 8-card or 9-card poker), a player's dealt cards are evaluated and split into two hands:
- **Front Hand**: Typically 3 cards (or fewer).
- **Back Hand**: Typically 5 cards (or more).
- **Fundamental Rule**: The **Back Hand must be stronger than or equal to the Front Hand** (`BackHand >= FrontHand`).

To systematically evaluate all valid front/back hand arrangements from raw frequency data:
1. **Raw Simulation Input**: Frequency counts of all dealt hand combinations are ingested from simulation data files (`stats_result.csv`).
2. **Component Decomposition (`PokerComponents.BreakDown`)**: Composite card structures (e.g., a 9-card flush, 4-of-a-kind) are decomposed into atomic building blocks according to game rules (`ISimCardRule`).
3. **Multi-Group Cartesian Product (`PokerComponents.CartesianProduct`)**: When multiple independent component groups exist in a hand, cross-group combinations are computed.
4. **Partitioning & Assembly (`ISimCardRule.AssembleHandRank` / `AssemblyComponent.AssembleHandRank`)**: Subsets of components are assigned to Front and Back hands, mapped to overall ranks (`SimCardOverAllHandRank`), and validated against game legality rules.
5. **Statistical Aggregation**: Appearance frequencies are distributed across valid split solutions, and cumulative winning probabilities are calculated.

```
+-------------------------------------------------------------+
|                      Full Card Hand                         |
|             (e.g., 9-Card Flush, 4-of-a-Kind, etc.)         |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|             PokerComponents.BreakDown(rule)                 |
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
|           rule.AssembleHandRank() (via ISimCardRule)        |
|   Front Rank: Pair                                          |
|   Back Rank:  FullHouse (ThreeOfKind + Pair)                |
|   Legality:   Back >= Front (Valid)                         |
+-------------------------------------------------------------+
```

---

## 2. Input and Data Source

The analysis pipeline consumes input data generated by exhaustive simulations or large-scale Monte Carlo runs of all possible dealt card combinations:
- **File Name**: `stats_result.csv` (or game-specific variants like `stats_result_8cards.csv`, `stats_result_9cards.csv`).
- **Data Structure**:
  - **Hand Type**: A string representing the constituent hand components (e.g., `Pair*2_ThreeOfKind`, `FiveCardsFlush_Pair`, `FourOfKind`).
  - **Count**: An integer representing the total number of occurrences of this hand across all simulated deals.

---

## 3. Hand Parsing & Component Power

### 3.1. Parsing Hand Component Strings
The utility parses raw string identifiers into structured `PokerComponents` objects:
- Example: `Pair*2_ThreeOfKind` is parsed into `[Pair, Pair, ThreeOfKind]`.

### 3.2. Component Metadata & Power Mapping
Each `PokerComponents` instance contains:
- `CompType`: Component type enum (`SimCardsCompType`).
- `CardCount`: Number of cards constituting the component (e.g., 2 for `Pair`, 3 for `ThreeOfKind`, 5 for `FiveCardsFlush`).
- `Power`: Integer value reflecting relative component strength, used for sorting and comparison.
- `Rule`: Associated `ISimCardRule` providing threshold constraints and assembly logic.

#### Component Power Table (Sample)

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

## 4. Component Breakdown & Assembly Subsystem

The breakdown and assembly subsystem handles decomposing composite hand structures and assembling subsets of components into evaluated overall hand ranks.

### 4.1. Atomic Breakdown Rules (`PokerComponents.BreakDown`)
`PokerComponents.BreakDown()` employs **mathematical integer partition generation** filtered by minimum card thresholds defined in `ISimCardRule` (`rule.MinFlushCount`, `rule.MinFlushStraightCount`, `rule.MinStraightCount`, `rule.MinKindCount`):

1. **Integer Partitions**: For a component with $N$ cards, all integer partitions $N = p_1 + p_2 + \dots + p_k$ ($p_1 \ge p_2 \ge \dots \ge p_k \ge 1$) are generated.
2. **Constraint Filtering**:
   - **Flush Straights**: Filtered by `rule.MinFlushStraightCount` (default: 3). For example, an 8-card flush straight $(8)$ produces $[8]$, $[7]$ ($1$ ineligible), $[6]$ ($2$ ineligible), $[5, 3]$, $[4, 4]$, $[4, 3]$, $[3, 3]$, etc.
   - **Flushes**: Filtered by `rule.MinFlushCount` (default: 5). For example, an 8-card flush produces $[8]$, $[7]$, $[6]$, $[5]$ ($3$ is ineligible when $\text{min}=5$).
   - **Straights**: Filtered by `rule.MinStraightCount` (default: 5).
   - **Sets / Multiples (Kinds)**: Filtered by `rule.MinKindCount` (default: 2). For example, `FourOfKind` (4 cards) decomposes into `[FourOfKind]`, `[ThreeOfKind]`, `[Pair, Pair]`, `[Pair]`.
3. **Deduplication**: Partitions yielding identical eligible atomic components are deduplicated while preserving canonical descending order.

### 4.2. Multi-Group Cartesian Product (`CartesianProduct`)
When a hand contains components across independent groups (e.g., Group 1: 9-card flush; Group 2: Four-of-a-kind), each group independently produces candidate breakdowns. `CartesianProduct` generates all combinations by selecting one candidate breakdown from each group:

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

### 4.3. Rule Hierarchy & Hand Rank Assembly (`ISimCardRule`)
Hand rank assembly logic and threshold configurations are encapsulated in a polymorphic rule hierarchy (`ISimCardRule`), allowing game-specific customization for 8-card, 9-card, or future variations:

```
                  +-------------------+
                  |   ISimCardRule    |
                  +-------------------+
                  | + MinStraight     |
                  | + MinFlush        |
                  | + MinFlushStraight|
                  | + MinKind         |
                  | + AssembleHandRank|
                  +-------------------+
                            ^
                            |
                  +-------------------+
                  |  BaseSimCardRule  |
                  +-------------------+
                     ^             ^
                     |             |
        +--------------------+     +--------------------+
        |  EightCardSimRule  |     |  NineCardSimRule   |
        +--------------------+     +--------------------+
```

#### Assembly Mapping Rules (`AssemblyComponent.AssembleHandRank`):
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

### 4.4. Hand Splitting Pipeline (`InitEightCardHandSplitProbAna.SplitHand`)
The complete hand splitting pipeline decomposes components, performs Cartesian product expansion, partitions candidate subsets into front/back hands, and evaluates legal splits:

```csharp
public static List<(SimCardOverAllHandRank, SimCardOverAllHandRank)> SplitHand(
    List<PokerComponents> comps,
    ISimCardRule? rule = null,
    int minFlushStraightCards = -1,
    int minFlushCards = -1,
    int minStraightCards = -1,
    int minKindCards = -1)
{
    if (comps == null || comps.Count == 0) return new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>();

    var effectiveRule = rule ?? comps.FirstOrDefault(c => c.Rule != null)?.Rule ?? EightCardSimRule.Default;

    // 1. Break down each component into atomic sub-components based on card constraints.
    var breakdownSequences = comps
        .Select(c => c.BreakDown(effectiveRule, minFlushStraightCards, minFlushCards, minStraightCards, minKindCards))
        .ToList();

    // 2. Generate Cartesian Product of all candidate breakdowns across components.
    var candidateCombinations = PokerComponents.CartesianProduct(breakdownSequences);

    var solutions = new List<(SimCardOverAllHandRank, SimCardOverAllHandRank)>();

    foreach (var combination in candidateCombinations)
    {
        var atomicComps = combination.SelectMany(c => c).ToList();
        atomicComps.Sort((a, b) => b.Power.CompareTo(a.Power));

        int maxFrontCount = atomicComps.Count / 2;

        for (int selectCount = 0; selectCount <= maxFrontCount; selectCount++)
        {
            var possibleGroups = UtilFunc.GetPermutationAllowedDuplicated(atomicComps, selectCount);
            foreach (var group in possibleGroups)
            {
                var frontGroup = group.Selected;
                var backGroup = group.Remaining;

                var frontRank = effectiveRule.AssembleHandRank(frontGroup);
                var backRank = effectiveRule.AssembleHandRank(backGroup);

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

### 4.5. Pareto-Optimal & Dominated Solution Filtering
`FilterDominatedSolutions` removes suboptimal ("strictly worse") split solutions:
- A split solution $(F_2, B_2)$ is dominated by $(F_1, B_1)$ if $F_1 \ge F_2$ and $B_1 \ge B_2$ with at least one strict inequality ($F_1 > F_2$ or $B_1 > B_2$).
- For example, with an 8-card flush straight:
  - `(Nothing, 8FS)` dominates `(Nothing, 7FS)` and `(Nothing, 6FS)`.
  - `(3FS, 5FS)` and `(4FS, 4FS)` dominate `(3FS, 4FS)` and `(3FS, 3FS)`.
- The final output retains only the Pareto-optimal frontier of split options.

---

## 5. Hand Splitting Rules & Strategy

When splitting a full hand into Front and Back sub-hands:

1. **First Rule (`BackHand >= FrontHand`)**:
   - The Back Hand must always be greater than or equal to the Front Hand in rank strength.
2. **Universal Hand Ranking (Elimination of Separate Front Hand Validation)**:
   - A separate "valid front hand list" is not required because Rule 1 (`BackHand >= FrontHand`) combined with universal hand rank evaluation inherently prevents illegal placements.
   - *Example*: If a hand contains `FullHouse + Pair`, placing `FullHouse` in front and `Pair` in back violates `BackHand >= FrontHand` and is automatically disqualified.
3. **Combinatorial Permutation Exploration**:
   - For $N$ components, the algorithm examines selections of $k$ components for the Front Hand ($0 \le k \le \lfloor N/2 \rfloor$).
   - Symmetrical combinations (e.g., $C(N, k)$ vs $C(N, N-k)$) are covered by evaluating the partition and swapping if `Front > Back`.
4. **Symmetry & Deduplication**:
   - Deduplication is applied to eliminate identical split solutions arising from symmetric combinations.

---

## 6. Multi-Solution Appearance Count Treatment

When a particular hand type produces multiple valid split solutions ($M > 1$), the appearance count $X$ is divided equally among the $M$ solutions:
- Each valid solution receives an equal weight of $\frac{X}{M}$.
- $\frac{X}{M}$ is added to the corresponding Front Hand rank statistic.
- $\frac{X}{M}$ is added to the corresponding Back Hand rank statistic.

### Example:
Consider an input entry of `ThreeOfKind + 2 pairs` with an appearance count of $X = 1000$.
This hand produces two distinct valid split solutions:
1. **Solution 1**: Front = `Pair`, Back = `FullHouse` (ThreeOfKind + Pair)
2. **Solution 2**: Front = `TwoPairs` (Pair + Pair), Back = `ThreeOfKind`

**Statistical Allocation**:
- **Front Hand Stats**:
  - `Pair` receives $+ \frac{1000}{2} = +500$ counts.
  - `TwoPairs` receives $+ \frac{1000}{2} = +500$ counts.
- **Back Hand Stats**:
  - `FullHouse` receives $+ \frac{1000}{2} = +500$ counts.
  - `ThreeOfKind` receives $+ \frac{1000}{2} = +500$ counts.

---

## 7. Output Statistics & Cumulative Win Probabilities

### 7.1. Individual Rank Probabilities
Once all counts are accumulated across front and back positions:
$$P(\text{Rank}_{\text{Front}}) = \frac{\text{Total Count}(\text{Rank}_{\text{Front}})}{\sum \text{Front Counts}}$$
$$P(\text{Rank}_{\text{Back}}) = \frac{\text{Total Count}(\text{Rank}_{\text{Back}})}{\sum \text{Back Counts}}$$

### 7.2. Cumulative Winning Probabilities ("Win/NoLose Probability")
The cumulative probability represents the probability of achieving a hand of a given rank or weaker, reflecting the overall winning/tying potential when playing that rank in the respective position:
$$\text{Cumulative Prob}(\text{Rank}_k) = \sum_{i \le k} P(\text{Rank}_i)$$

#### Practical Interpretation & Interpolation:
- If `Nothing` in the front hand has an individual probability of $72\%$, and `Pair` has $26\%$:
  - A player with `Nothing` has a baseline win/tie probability of up to $72\%$.
  - A player holding a `Pair` in front beats all `Nothing` hands ($72\%$) plus ties/beats a portion of `Pair` hands, giving a cumulative probability window of $72\%$ to $98\%$ ($72\% + 26\%$).
  - Downstream consumers (e.g., AI battle decision engines) can interpolate the exact winning rate within $[72\%, 98\%]$ based on the specific card rank of the pair (e.g., Pair of 2s vs. Pair of Aces).

### 7.3. Output Data Files
Results are exported to CSV files for use in runtime game engines and AI decision models:
- `front_back_stats.csv`
- `twohands_prob_8cards.csv`
- `twohands_prob_9cards.csv`

---

## 8. Concrete Examples

### Example 1: Splitting Full House Components
**Input**: `[ThreeOfKind, Pair]`
- **Candidate Split 1**: Front `[Pair]`, Back `[ThreeOfKind]`
  - Front rank: `Pair`
  - Back rank: `ThreeOfKind`
  - Validity: `ThreeOfKind >= Pair` $\rightarrow$ Legal split: `(Pair, ThreeOfKind)`
- **Candidate Split 2**: Front `[]` (`Nothing`), Back `[ThreeOfKind, Pair]`
  - Front rank: `Nothing`
  - Back rank: `FullHouse`
  - Validity: `FullHouse >= Nothing` $\rightarrow$ Legal split: `(Nothing, FullHouse)`

### Example 2: Decomposing a 9-Card Flush
```csharp
var nineFlush = new PokerComponents(SimCardsCompType.NineCardsFlush, NineCardSimRule.Default);
var breakdowns = nineFlush.BreakDown();
```
**Candidate Decompositions**:
1. `[FiveCardsFlush, FourCardsFlush]`
2. `[SixCardsFlush, ThreeCardsFlush]`
3. `[NineCardsFlush]`

When partitioned across Front and Back hands, the `[FiveCardsFlush, FourCardsFlush]` candidate yields:
- Front: `FourCardsFlush`
- Back: `FiveCardsFlush`
- Result: Legal split `(FourCardsFlush, FiveCardsFlush)`

---

## 9. Verification & Test Suites

The pipeline and component architecture are validated by automated test suites in `UnitTest/`:

- **`UnitTest/SimCardRuleTest.cs`**:
  - Validates `ISimCardRule` defaults, minimum threshold constraints, custom rule overrides, and hand assembly logic.
- **`UnitTest/SplitHandBreakAndAssemblyTest.cs`**:
  - `TestFlushBreakdown`: Validates 9-card flush decomposition into 5-card + 4-card flushes.
  - `TestMultiplesBreakdown`: Validates four-of-a-kind decomposition into pair + pair.
  - `TestCartesianProductOfBreakdowns`: Validates multi-group cross-product decomposition.
  - `TestAssembleHandRank`: Validates single and composite hand assembly (`FullHouse`, `Mansion`, `TwoPairs`).
  - `TestSplitHandIntegration`: Validates end-to-end split generation with rule injection.
- **`UnitTest/SimHandSplitProbAnaTest.cs`**:
  - `TestEightCardHandSplitProbAna`: Validates 8-card probability analysis and split outputs against historical baseline.
  - `TestNineCardHandSplitProbAna`: Validates 9-card probability analysis and split outputs against historical baseline.

---

## 10. Summary

- **Decoupled Architecture**: `ISimCardRule` abstracts game-specific card counts and assembly logic, supporting Eight-Card, Nine-Card, and future poker game modes.
- **Integer Partition Breakdown**: Mathematical partition generation dynamically breaks down oversized flushes, straights, and multiples into atomic components.
- **Pareto-Optimal Hand Splitting**: Explores the full combinatorial space of valid split hands, enforces `BackHand >= FrontHand`, and filters out dominated solutions.
- **Weighted Multi-Solution Statistics**: Fairly divides occurrence frequencies across multiple split solutions to yield accurate cumulative winning probability tables for game AI.
