Here's the prompt — ready to copy-paste into a future Claude session:

---

# Prompt: Build a Static Phase Viewer Web App (Single `index.html`)

## Goal

Build a **single standalone `index.html`** file — no build step, no server, no npm. Deployable to **GitHub Pages** as a static site. Opens in any modern browser.

## What It Does

A file viewer/explorer for a custom `.txt` packed format. The user provides a packed `.txt` file via **one of three input methods**, and the app displays a navigable file tree with syntax-highlighted content.

## Input Methods (all three must work)

### 1. Drag & Drop / File Picker
- Drag-drop zone + "Browse" button
- Accepts `.txt` files from local disk
- Uses browser `FileReader` API

### 2. Paste
- A "Paste" button or auto-detect `Ctrl+V`
- User copies entire `.txt` content and pastes
- Parses from clipboard text

### 3. Raw URL
- Text input field for a URL (e.g. raw GitHub link like `https://raw.githubusercontent.com/user/repo/main/phase-b(New).txt`)
- Fetches via `fetch()` and parses the response text
- Show loading spinner while fetching
- Handle CORS errors gracefully with a user-friendly message

## Pack Format Spec

Each file in the `.txt` is separated by a one-line delimiter:

```
<!-- ==== FILE: phase-b(New)/GUIDE.md ==== -->
(full raw file content follows, can contain ANY characters)

<!-- ==== FILE: phase-b(New)/Scripts/0-Core/GameEvents.cs ==== -->
(full raw file content follows)
```

- **Delimiter regex:** `^<!-- ==== FILE: (.+) ==== -->$`
- Path after `FILE:` is always relative (e.g. `phase-b(New)/Scripts/...`)
- File content = everything between one delimiter and the next (trim ONE trailing blank line if present)
- Files can be `.cs`, `.md`, `.txt`, `.json`, or any text format
- Content may contain **any characters** including triple backticks, HTML tags, `<!-- -->` comments — only the exact delimiter pattern marks file boundaries
- The first line of the `.txt` is always a delimiter (no preamble)

## UI Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  📦 Phase Viewer       [input area: drop/paste/url]  [⬇ ZIP]   │
├─────────────────┬───────────────────────────────────────────────┤
│                 │  ┌─ breadcrumb: phase-b(New) > Scripts > ... ┐│
│  Collapsible    │  │                                           ││
│  Folder Tree    │  │  Syntax-highlighted file content          ││
│                 │  │  or rendered Markdown                     ││
│  📂 phase-b(New)│  │                                           ││
│  ├── GUIDE.md   │  │                                           ││
│  ├── 📂 Scripts │  │                                           ││
│  │  ├── 📂 0-Co │  │                                           ││
│  │  │  └── Ga… │  │                                           ││
│  │  ├── 📂 1-Ma │  │                                           ││
│  │  ...        │  │                                           ││
│                 │  └───────────────────────────────────────────┘│
├─────────────────┴───────────────────────────────────────────────┤
│  Files: 44  │  Viewing: GameEvents.cs  │  Lang: csharp         │
└─────────────────────────────────────────────────────────────────┘
```

## Features

### File Tree (Left Panel)
- Collapsible/expandable folder nodes (click folder to toggle)
- File icons by type: 📄 for `.cs`, 📝 for `.md`, 📄 for others
- Folder icons: 📂 open / 📁 closed
- Clicking a **file** loads its content in the right panel
- Highlight the currently selected file
- Tree sorted: folders first, then files, alphabetical within each

### Content Viewer (Right Panel)
- **`.cs` files** → syntax highlighted as C# (highlight.js with `csharp` language)
- **`.md` files** → rendered Markdown (marked.js) with a **toggle button** to switch between "Rendered" and "Raw" view (raw = syntax highlighted as markdown)
- **`.json` files** → syntax highlighted as JSON
- **Other files** → plain text, monospace
- Line numbers on the left (for code files)
- Breadcrumb path at top showing full relative path
- Copy button (copies raw file content to clipboard)

### Download as ZIP
- Button in header: "⬇ Download ZIP"
- Creates a `.zip` using JSZip preserving full folder structure
- Filename = root folder name + `.zip` (e.g. `phase-b(New).zip`)
- Triggers browser download via `<a download>`

### Reset
- Button to clear current file and return to input screen

## Tech Constraints

- **100% client-side** — no backend, no API keys, no server
- **Single `index.html`** — all CSS and JS inline (no external `.css` or `.js` files except CDN)
- **CDN dependencies only** (loaded via `<script src>` / `<link href>`):
  - **highlight.js** — syntax highlighting (include `csharp`, `markdown`, `json` languages + `github-dark` theme)
  - **marked.js** — Markdown rendering
  - **JSZip** — ZIP creation
- Must work offline after first load if CDN is cached (no runtime API calls except optional URL fetch)
- Works in Chrome, Edge, Firefox (latest)

## UI Style

- **Dark theme** (dark background, light text — similar to VS Code dark)
- Split pane: resizable or fixed 250px left / rest right
- Monospace font for code (`JetBrains Mono` via Google Fonts CDN, fallback `Consolas, monospace`)
- Smooth transitions on folder expand/collapse
- Responsive: on narrow screens, tree collapses to hamburger menu
- Status bar at bottom with file count and current file info

## Edge Cases to Handle

- Empty `.txt` file → show "No files found" message
- `.txt` with no valid delimiters → show "Invalid format" error
- URL fetch fails (CORS/404) → show error with suggestion to download and use drag-drop instead
- Very large files (>10K lines) → virtual scroll or truncation with "Show all" button
- File with no extension → treat as plain text
- Paste with no valid content → show "No valid packed content detected"

## Deliverable

A **single `index.html`** file. I drop it into a GitHub Pages repo (`docs/index.html` or root), push, and it works. Nothing else needed.

## Example

If the packed `.txt` contains:
```
<!-- ==== FILE: phase-b(New)/GUIDE.md ==== -->
# Phase B — Player Controller
Some **markdown** content with `code`.

<!-- ==== FILE: phase-b(New)/Scripts/0-Core/GameEvents.cs ==== -->
using System;
public static partial class GameEvents
{
    public static event Action OnToolSwitched;
}
```

Then:
- Tree shows: `📂 phase-b(New)` → `📝 GUIDE.md` + `📂 Scripts` → `📂 0-Core` → `📄 GameEvents.cs`
- Clicking [GUIDE.md](cci:7://file:///c:/Users/PAVANK1/Documents/ig/ig-1-main/ig-1-main/minemgl/learn/phase-b%28New%29/GUIDE.md:0:0-0:0) → rendered Markdown with bold, inline code styled
- Clicking [GameEvents.cs](cci:7://file:///c:/Users/PAVANK1/Documents/ig/ig-1-main/ig-1-main/minemgl/learn/phase-b%28New%29/Scripts/0-Core/GameEvents.cs:0:0-0:0) → C# syntax highlighted with line numbers
- ZIP download creates `phase-b(New)/GUIDE.md` and `phase-b(New)/Scripts/0-Core/GameEvents.cs`

---

Save this as your prompt file. Copy-paste the entire block into a future Claude session and it should produce the complete `index.html` in one shot.
