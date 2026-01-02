# GitHub Copilot Instructions for Expanded Roofing (Continued)

## Mod Overview and Purpose

The Expanded Roofing (Continued) mod enhances the roofing system in RimWorld by introducing customizable roof types, including transparent greenhouse roofs and photovoltaic solar roofs. Originally developed by Vendan and later updated and continued by multiple developers, this mod adds new strategic depth to building design, enabling players to manage light penetration and power generation more effectively.

## Key Features and Systems

- **Transparent Roofs**: Designed to allow almost all light to pass through, ideal for greenhouses.
- **Solar Roofs**: Generate power without occupying ground space. Each solar tile can produce up to 200w and is managed via controllers.
- **Roof Maintenance**: Thick roofs require maintenance over time to prevent collapses, integrating a dynamic upkeep system.
- **Research**: Unlock new roofing types and capabilities through four new technological advancements.
- **Compatibility**: Seamlessly integrates with save games created in version B19 or later, with no additional roofing mods.
- **Localization**: The mod supports multiple languages, including English, Turkish, Russian, French, Chinese, and Japanese.

### Solar Roofing and Controllers

- Controllers manage the power output of connected solar roofing tiles.
- A controller's maximum output is capped at 2500w but can manage unlimited tile connections.
  
### Roof Maintenance

- Thick roofs deteriorate over time (approximately every 167 days) and require maintenance.
- After 250 days without maintenance, roofs risk collapse in a mean time between (MTB) event of 3.5 days.

## Coding Patterns and Conventions

- **Namespace Structuring**: Maintain modular and clear code by categorizing classes according to functionality and features (e.g., `CompCustomRoof`, `CompPowerPlantSolarController`).
- **Coding Style**: Use PascalCase for class names and method declarations; use camelCase for local variables and method parameters.
- **File Organization**: Logical splitting of functionalities into multiple classes and files to promote maintainability (e.g., `RoofGrid_SetRoof.cs`, `GlowGrid_GroundGlowAt.cs`).

## XML Integration

- Utilize XML for defining roof types and related data within the mod, employing `ThingDef`, `RoofDef`, and custom `CompProperties`.
- Designators for new roofing types are specified and available in the game under the `Zone` tab.

## Harmony Patching

- Embrace Harmony for non-destructive method patching, ensuring compatibility with other mods and vanilla updates.
- **Common Harmony Classes**:
  - `HarmonyPatches`: Implement patches in this class to alter or extend game behavior effectively.
  - Specific examples include manipulation of construction blocking (`GenConstruct_BlocksConstruction`) and roof grid updates (`RoofGrid_SetRoof`).

## Suggestions for Copilot

To maximize efficiency and accuracy when using GitHub Copilot:
- **Prompt Suggestions**: Start code comments or method stubs to receive more accurate autocompletions based on context.
- **Harmony Examples**: Provide examples of patches and scenarios where Harmony can alter game logic or visuals.
- **XML Data**: When creating or modifying XML defs, prompt Copilot with existing `Def` examples to explore similar pattern options.
- **Localization Enhancements**: Provide key phrases or terms for use in translations and localizations, leveraging Copilot's abilities to suggest translated strings based on these inputs.

## Note

Whenever you encounter issues:
- Test with only this mod enabled alongside its requirements.
- Use the provided Log Uploader for error reports.
- Direct issues to the Discord channel for quicker support; GitHub is preferred for solutions and contributions.

With these instructions, contributors should be well-equipped to develop and expand upon the Expanded Roofing mod for RimWorld.
