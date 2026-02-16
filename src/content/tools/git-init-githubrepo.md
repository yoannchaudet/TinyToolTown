---
name: "git-init-githubrepo"
tagline: "Create git repository for GitHub style"
author: "Uğur Özyılmazel"
author_github: "vigo"
github_url: "https://github.com/vigo/git-init-githubrepo"
tags: ["cli", "git", "github"]
language: "Go"
license: "MIT"
date_added: "2026-02-16"
featured: false
---

# GitHub Friendly Repo Creator/Initializer

Create git repository for GitHub style:

- `README.md` (as seen here!)
- `LICENSE`
- `CODE_OF_CONDUCT.md` (optional)
- `.bumpversion.toml` (optional)
- `SECURITY.md` (optional)
- `.github/CODEOWNERS` (optional)
- `.github/FUNDING.yml` (optional)
- `.github/pull_request_template.md` (optional)

According to `--project-style` (currently only `go` available)

- `.github/workflows/go-test.yml`
- `.github/workflows/go-lint.yml`
- `.github/dependabot.yml`
- `.golangci.yml`