# Expanded Roofing (Continued) GitHub Copilot Instructions

## Overview
Expanded Roofing (Continued) is a RimWorld mod that adds customizable roofing options, including transparent greenhouse roofing and photovoltaic solar roofing. It also reworks the thick roof system and adds research technologies for each feature. The mod is compatible with existing B19 saves and recommends loading early in the mod load order.

## Key Features and Systems
- **Transparent Roofs**: Allows almost all light to pass through, ideal for greenhouse environments.
- **Solar Roofs and Controllers**: Enables the generation of power using solar tiles and controllers.
- **Thick Roofs**: Requires maintenance and can collapse if not maintained.
- **Research Technologies**: Four new techs for each roofing feature to unlock.
- **Designators**: New designators in the Zone tab for easier re-roofing.

## Coding Patterns and Conventions
- **Code Structure**: C# files are organized into classes and methods that handle specific functionalities, such as roof maintenance and power generation.
- **Naming Conventions**: Classes and methods follow PascalCase, and variables follow camelCase.
- **Documentation**: Inline comments and XML documentation are used to explain complex logic and APIs.

## XML Integration
XML files define various aspects of the mod, such as building definitions, job definitions, and research projects. Key XML files include:
- `Building_SolarController.xml`: Defines the solar controller building.
- `Jobs_Work.xml`: Defines jobs related to roofing.
- `ResearchProjectDefs.xml`: Defines research projects for each roofing feature.
- `ResearchTab.xml`: Defines research tabs for organizing research projects.
- `Roofs.xml`: Defines roof types and spawner definitions.

## Harmony Patching
The mod uses Pardeike's Harmony library for non-destructive patching. Key Harmony patches include:
- Modifying building behavior to add solar power generation.
- Adjusting maintenance and collapse mechanics for thick roofs.
- Integrating new research technologies into the game.

## Suggestions for Copilot
- **Code Generation**: When writing new components or systems, Copilot can generate the basic structure and methods based on existing patterns.
- **XML Snippets**: For XML integration, Copilot can suggest snippets for defining new buildings, jobs, or research projects.
- **Harmony Patches**: When implementing Harmony patches, Copilot can provide examples of how to modify existing game mechanics without interfering with core functionality.
- **Localization**: For adding translations, Copilot can suggest structures for defining new languages and translations based on existing files.

## Example GitHub Copilot Instructions File


# Expanded Roofing (Continued) GitHub Copilot Instructions

## Overview
Expanded Roofing (Continued) is a RimWorld mod that adds customizable roofing options, including transparent greenhouse roofing and photovoltaic solar roofing. It also reworks the thick roof system and adds research technologies for each feature. The mod is compatible with existing B19 saves and recommends loading early in the mod load order.

## Key Features and Systems
- **Transparent Roofs**: Allows almost all light to pass through, ideal for greenhouse environments.
- **Solar Roofs and Controllers**: Enables the generation of power using solar tiles and controllers.
- **Thick Roofs**: Requires maintenance and can collapse if not maintained.
- **Research Technologies**: Four new techs for each roofing feature to unlock.
- **Designators**: New designators in the Zone tab for easier re-roofing.

## Coding Patterns and Conventions
- **Code Structure**: C# files are organized into classes and methods that handle specific functionalities, such as roof maintenance and power generation.
- **Naming Conventions**: Classes and methods follow PascalCase, and variables follow camelCase.
- **Documentation**: Inline comments and XML documentation are used to explain complex logic and APIs.

## XML Integration
XML files define various aspects of the mod, such as building definitions, job definitions, and research projects. Key XML files include:
- `Building_SolarController.xml`: Defines the solar controller building.
- `Jobs_Work.xml`: Defines jobs related to roofing.
- `ResearchProjectDefs.xml`: Defines research projects for each roofing feature.
- `ResearchTab.xml`: Defines research tabs for organizing research projects.
- `Roofs.xml`: Defines roof types and spawner definitions.

## Harmony Patching
The mod uses Pardeike's Harmony library for non-destructive patching. Key Harmony patches include:
- Modifying building behavior to add solar power generation.
- Adjusting maintenance and collapse mechanics for thick roofs.
- Integrating new research technologies into the game.

## Suggestions for Copilot
- **Code Generation**: When writing new components or systems, Copilot can generate the basic structure and methods based on existing patterns.
- **XML Snippets**: For XML integration, Copilot can suggest snippets for defining new buildings, jobs, or research projects.
- **Harmony Patches**: When implementing Harmony patches, Copilot can provide examples of how to modify existing game mechanics without interfering with core functionality.
- **Localization**: For adding translations, Copilot can suggest structures for defining new languages and translations based on existing files.


This detailed file provides a comprehensive guide for contributors and developers using GitHub Copilot with the Expanded Roofing (Continued) mod, ensuring consistency and efficiency in development.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

