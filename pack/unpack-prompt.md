# Prompt: Build a Static Unpacker Web App (GitHub Pages)

## Context

I have a custom text-based packaging format that bundles multiple files (`.cs`, `.md`, etc.) from a folder into a single `.txt` file. I need a **static web app** (no backend) deployable to **GitHub Pages** that lets anyone unpack these `.txt` files.

## Pack Format Spec

Each file in the `.txt` is separated by a one-line delimiter:

```
<!-- ==== FILE: phase-b(New)/GUIDE.md ==== -->
(full raw file content follows)

<!-- ==== FILE: phase-b(New)/Scripts/0-Core/GameEvents.cs ==== -->
(full raw file content follows)
```

- Delimiter regex: `^<!-- ==== FILE: (.+) ==== -->$`
- The path after `FILE:` is always relative (e.g. `phase-b(New)/Scripts/...`)
- File content is everything between one delimiter and the next (trim trailing blank line)
- Files can be `.cs`, `.md`, `.txt`, `.json`, or any text format
- Content may contain any characters including triple backticks, HTML tags, etc.

## What to Build

A **single-page static web app** with:

### 1. Upload Area
- Drag-and-drop zone + file picker button
- Accepts only `.txt` files
- Reads the file **entirely in the browser** (no server upload)

### 2. Parsed Tree View
- After parsing, display a **collapsible folder tree** showing the full directory structure
- Show file count and total size
- Clicking a file shows a **preview panel** with the file content (syntax highlighted if possible)

### 3. Download as ZIP
- A button that creates a `.zip` file **in the browser** using JSZip
- The `.zip` must preserve the full subfolder structure from the delimiter paths
- Trigger download via browser (FileSaver.js or `<a download>`)

### 4. Reset
- A button to clear and start over with a new file

## Tech Constraints

- **100% client-side** — no server, no API calls, no backend
- **Deployable to GitHub Pages** — static files only (HTML, CSS, JS)
- Use **vanilla HTML/CSS/JS** or a lightweight framework. No heavy build tools required.
- Dependencies (use via CDN, no npm build step):
  - **JSZip** — for creating the zip
  - **FileSaver.js** — for triggering the download (optional, can use `<a download>`)
- Must work in modern browsers (Chrome, Edge, Firefox)

## UI Requirements

- Clean, modern, minimal design
- Dark theme preferred
- Responsive (works on mobile too)
- Show a loading state while parsing large files
- Show error state if the file doesn't match the expected format

## Deliverables

Provide a **single `index.html`** file (inline CSS + JS) that I can drop or paste as .txt too (into a GitHub Pages web) and it just works. No build step, no npm, no bundler.

## Example Output Structure

If the packed `.txt` contains:
```
<!-- ==== FILE: phase-b(New)/GUIDE.md ==== -->
# Guide content here

<!-- ==== FILE: phase-b(New)/Scripts/0-Core/GameEvents.cs ==== -->
using System;
public static class GameEvents { }
```

The downloaded `.zip` should contain:
```
phase-b(New)/
├── GUIDE.md
└── Scripts/
    └── 0-Core/
        └── GameEvents.cs
```

With each file containing its exact original content.
