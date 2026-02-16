---
name: "leech"
tagline: "Concurrent command-line download manager. Inspired from Leech macOS application!"
author: "Uğur Özyılmazel"
author_github: "vigo"
github_url: "https://github.com/vigo/leech"
tags: ["cli", "download-manager"]
language: "Go"
license: "MIT"
date_added: "2026-02-16"
featured: false
---

## Features

- Concurrent chunked downloads (parallel byte-range fetches)
- Multiple URL support (pipe and/or arguments)
- Progress bar with real-time terminal output
- Bandwidth limiting (shared token bucket across all downloads)
- Resume support (`.part` files, continues from where it left off)
- Single-chunk fallback for servers without `Accept-Ranges`
- Structured logging with `log/slog` (debug mode via `-verbose`)