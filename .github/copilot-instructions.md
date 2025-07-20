# GitHub Copilot Instructions for RimWorld Expanded Roofing Mod

## Mod Overview and Purpose

The Expanded Roofing Mod aims to enrich the roofing system in RimWorld by introducing new types of roofs and maintaining systems. This mod allows players to build custom roofs with specific characteristics, such as solar and transparent roofs, adding more depth and strategy to base construction and resource management.

## Key Features and Systems

1. **Custom Roof Types**: Introduces various roof types like solar roofs, thick stone roofs, and transparent roofs, providing diverse options for players' structures.

2. **Roof Maintenance**: Implements a maintenance system for roofs, ensuring they require periodic upkeep which adds an element of resource management and planning.

3. **Solar Power Integration**: Solar roofs not only act as a covering but also generate power, contributing to the colony's energy resources dynamically depending on weather and light conditions.

4. **Construction and Designation Tools**: New designators for roofing enable intuitive construction and prioritizing of the different roof types.

5. **Harmony Patches**: These are utilized extensively to seamlessly integrate the features without directly modifying the core game codes, ensuring maximum compatibility.

## Coding Patterns and Conventions

- **Static Classes for Tooling**: Many of the utility functions are encapsulated within static classes, following the pattern of static method collections to provide clear, accessible utility functions (e.g., `InjectedDefHasher`).

- **Inheriting Core Classes**: Classes like `CompCustomRoof` inherit from base classes like `ThingComp`, showcasing an object-oriented approach that leverages inheritance to extend functionality with minimal redundancy.

- **Internal Access Modifiers**: Many helper classes and methods are marked as internal to encapsulate functionality within the assembly and maintain clear boundaries.

- **Method Overloading and Overrides**: Methods are carefully overloaded and sometimes overridden to ensure correct behavior tailored to different roof types (e.g., `Designator_BuildCustomRoof`).

## XML Integration

The mod involves XML definitions for new roof types, research projects, and designators. Typically, these XML files declare new items by defining their properties, enabling the game's engine to recognize and use them. Unfortunately, the specifics of the XML files were unavailable due to parsing issues, but integrating XML changes is a standard procedure for extending RimWorld’s def-based architecture.

## Harmony Patching

Harmony is extensively used to avoid overwriting the base game code. The key elements include:

- **HarmonyPatches.cs**: The core location where all the Harmony patches are applied to the game.
  
- **Prefixes, Transpilers, and Postfixes**: Often utilized to modify method behavior before, during, or after method execution without direct code alteration.

- **Patch Classes**: Many classes like `RoofMaintenance_Patches` implement patches that adjust how roofing systems integrate with gameplay mechanics.

## Suggestions for Copilot

- **Helper Method Completion**: Given the repetitive nature of utility functions, Copilot can be used to propose method signatures based on existing patterns.

- **Class and Method Suggestions**: Add suggestions for filling in logic within classes, leveraging standard RimWorld modding patterns for designing `Comps` and `Def Mod Extensions`.

- **Harmony Transpiler/Formulation**: Provide recommendations for gear transpiler methods using IL code, ensuring correctness and preventing game-breaking bugs.

- **XML Template Completion**: While not detailed here, once XML files are clear, Copilot can assist in generating def templates for new roofs, enabling faster XML modification and experimentation.

- **Safeguard Incorporation**: Automatically suggest safeguards in methods, preventing null references and common runtime exceptions found in modding environments.

By following these guidelines and utilizing GitHub Copilot, developers can streamline mod development, ensuring consistency and reliability in mod features.
