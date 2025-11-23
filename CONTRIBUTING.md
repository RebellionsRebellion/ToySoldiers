# Contributing to Toy Soldiers

These guidelines outline the conventions and standards that are expected to be upheld throughout 
development. Following them ensures a clean, consistent project structure and smooth developer experience

## Table of Contents
- [Styleguides](#styleguides)
  - [Project Structure](#project-structure)
  - [Asset Considerations](#asset-considerations)
  - [Models and Textures](#models-and-textures)
  - [Scripts](#scripts)
  - [Audio](#audio)
  - [Commit Messages](#commit-messages)
- [Version Control](#version-control)
  - [Commit Types](#commit-types)
  - [Scopes](#scopes)
  - [Example Commits](#example-commits)
  - [Branches](#branches)
  - [Jira Integration](#jira-integration)
  - [Pull Requests](#pull-requests)

## Styleguides

### Project Structure
A well-structured project ensures that assets are easy to locate and work with. This structure *may* evolve
as the project grows, but contributors should aim to keep additions consistent. Please frequently
check the related document on the Confluence to ensure that there are no differing changes.

Avoid working directly in Unity scenes unless necessary and prefer working in prefabs whenever possible.

---

### Asset Considerations

To ensure compatibility with GitHub:
- **Assets should be kept under ~75MB**
  
    Github rejects files above **100MB**. This 75MB guideline provides a buffer.

- **Optimise assets where possible**

    Large files should be either compressed or optimised before commiting, ask your leads on help if
    you can not figure out which option is best for your case

---

### Models and Textures

- **Textures must not be baked into FBX files**

    Keep models and textures separate to minimise file size

- **Recommended texture size: 1024x1024px, unless a larger resolution is logically suitable**

- **Before exporting models**
  - Ensure topology is clean
  - Ensure face normals are correct
  - Remove unused mesh data

---

### Scripts

Scripts should be understandable _without_ relying on comments.

When comments _are_ needed, they must explain **why** the code exists, not restate what the code literally does.

**Example**
```csharp
// BAD: Vague and useless
cachedObjects.Clear(); // Clear cached object array

// GOOD: explains intent
cachedObjects.Clear(); // Clear array to allow reallocation
```

---

### Audio

_(tbd)_

---

### Commit Messages

Commit messages must follow the project standardised format:

```csharp
<JIRA-TICKET-ID> <type>(<scope>): <short summary>

[optional body]

[optional footer]
```

#### Updating Jira Status via commit

To automatically move a Jira ticket to a board category (e.g., in-progress):
```
<JIRA-TICKET-ID> #<category-name> <type>(<scope>): <short summary>

[optional body]

[optional footer]
```

#### Short Summary
  - The summary is a one-line TL;DR of the commit
  - If it exceeds oe line, shorten it and place details in the commit body instead

#### Optional Body
Used when additional detail is needed:
What changed, why it changed, and any important notes for designers or other contributors

#### Optional Footer

For related metadata or secondary information that doesn't belong in the body

---

#### Commit types

| Type       | Description                                                                |
| ---------- | -------------------------------------------------------------------------- |
| `feat`     | A new feature (e.g., adding a new system or gameplay mechanic)             |
| `fix`      | A bug fix                                                                  |
| `refactor` | Code refactoring that doesn’t change behavior (e.g., structure or cleanup) |
| `chore`    | Non-code changes (e.g., updating `.editorconfig`, README, or metadata)     |
| `docs`     | Documentation-only changes                                                 |
| `style`    | Code style or formatting changes (no logic changes)                        |
| `perf`     | Performance improvements                                                   |
| `build`    | Changes to build scripts or dependencies                                   |

---

#### Scopes

Scopes describe the main area of the project being modified. The can be quite arbitrary, so do not worry if
one of these aren't applicable, just come up with your own:

Common scopes include:
`scripts`, `prefabs`, `ui`, `vfx`, `input`, `player`, `audio`, `ghameplay`

---

#### Example Commits

```
TOYS-2 feat(player): add climbing ability to player controller

TOYS-4 fix(ui): correct alignment of health bar

TOYS-62 refactor(scripts): split GameManager into smaller components

TOYS-12 #in-progress docs: add contributing guide and naming conventions

chore: update README.md and fix trailing whitespace
```

NOTE: Add a blank line between the summary and body to keep commits ✨ clean ✨

---

### Version Control

#### Branches

Follow the branching rules below to keep development stable and Jira integration functional

| Branch     | Purpose                                                                                   |
|------------|-------------------------------------------------------------------------------------------|
| `main`     | Stable, production-ready, Update only via PRs from `dev` or force merge from a maintainer |
| `dev`      | Active development Branch. All features and fixes merge here first                        |
| `art`      | Dedicated art branch which merges into `dev`                                              |
| `feature/*`| Feature development branches. Deleted after merge into `dev`                              |
| `fix/*`    | Bugfix branches from `dev`. Deleted ater merge into `dev`                                 |
| `hotfix/*` | Urgent patches from `main`. Merged intto botth `main` and `dev`                           |

#### Jira integration

Branches should include the Jira ticket ID in the name so that the associated ticket automatically updates:

```csharp
feature/TOYS-123-add-audio-system
fix/TOYS-104-fix-collision-glitch
hotfix/TOYS-201-stop-file-corruption
```

---

#### Pull Requests

Before opening a PR, please ensure the fopllowing

**General**
  - Based on the latest version of `dev`
  - PR title folloows commit message format
  - Links to a JIra ticket (e.g., Closes TOYS-123)

**Code Quality**
  - Project compiles and runs
  - No console errors and _minimal_ warnings
  - Follows naming conventions and styleguide
  - No leftover `Debug.Log` spam
  - Comments included where necessary (especially in longer classes)