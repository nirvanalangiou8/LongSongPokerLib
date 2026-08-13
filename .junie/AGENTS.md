# Project Guidelines: LongSongPokerLib

This file provides relevant details for future development on the `LongSongPokerLib` project.

## Build/Configuration Instructions

- **Framework**: .NET 8.0
- **Language**: C# 12.0
- **Solution File**: `LongSongPokerLib.sln`
- **Main Projects**:
  - `LongSongPokerLibCore`: The core library containing poker logic.
  - `UnitTest`: NUnit test project.
- **Build Command**: 
  - Using CLI: `dotnet build`
  - Using IDE: Build the solution in Rider or Visual Studio.

## Testing Information

### Configuring and Running Tests
- **Framework**: NUnit 3.
- **CLI**: `dotnet test`
- **IDE**: Use the built-in Test Runner in Rider or Visual Studio.
- **Scoped Tests**: Use the Fully Qualified Name (FQN) to run specific tests, e.g., `dotnet test --filter FullyQualifiedName=UnitTest.SimplePokerTest`.

### Guidelines for Adding and Executing New Tests
- **Location**: Add new test files to the `UnitTest` project.
- **Conventions**:
  - Use the `[TestFixture]` attribute for test classes.
  - Use the `[Test]` attribute for test methods.
  - For data-driven tests, use `[TestCase]` or `[TestCaseSource]`.
- **Validation**: Ensure all tests pass before submitting changes. Use `[DEBUG_LOG]` prefix for messages intended for debugging during test runs.

### Test Process Demonstration
To verify the setup, you can create a simple test like the one below:
```csharp
using GenericPoker.EightCard;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class SanityTest
    {
        [Test]
        public void TestInitialization()
        {
            var calculator = PokerHandCalculator.CreateInstance("A♠️,K♠️,Q♠️,J♠️,10♠️,9♠️,8♠️,7♠️");
            Assert.That(calculator, Is.Not.Null);
        }
    }
}
```

## Additional Development Information

### Code Style
- **Naming**: Use PascalCase for classes and methods, camelCase for local variables and private fields (often prefixed with `_`).
- **Structure**: Follow the existing directory structure under `LongSongPokerLibCore\GenericPoker`.
- **Poker Card Representation**: Cards are represented by strings like `A♠️`, `10❤️`, `Joker`.
- **Core Logic**: `PokerHandCalculator` is the central class for hand evaluation and splitting in Eight Card poker.

### Instruction Files for Junie
Junie (the AI agent) can be guided using Markdown files at different levels:

1. **Global Guidelines**: 
   - Path: `%USERPROFILE%\.junie\AGENTS.md` (Windows).
   - Use this for personal preferences or organization-wide rules that apply to all projects.

2. **Project-Level Guidelines**:
   - Path: `.junie/AGENTS.md` (recommended) or `AGENTS.md` in the project root.
   - Use this for project-specific context, build instructions, and testing guidelines.

3. **Lower-Level (Nested) Guidelines**:
   - Path: `AGENTS.md` inside any subdirectory (e.g., `LongSongPokerLibCore/GenericPoker/EightCard/AGENTS.md`).
   - Use this for specific modules or subprojects. Files closer to the working directory take precedence over those higher up in the tree.

Junie automatically reads and incorporates these guidelines into its context for every task.
