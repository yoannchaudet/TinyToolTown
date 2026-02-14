---
name: "statoo"
tagline: "`statoo` is a super simple http GET tool for checking site health"
author: "Uğur Özyılmazel"
author_github: "vigo"
github_url: "https://github.com/vigo/statoo"
tags: ["cli", "health-check", "monitoring", "http-client"]
language: "Go"
license: "MIT"
date_added: "2026-02-14"
featured: false
ai_summary: "A pocket-sized HTTP poke that fires a single GET and returns status timing and optional JSON so quick health checks and CI scripts stay delightfully simple. | FEATURES: 🔥 Simple single GET status reporting with gzip header injection | ⚡ Machine-friendly JSON output with elapsed timing and status fields | 🎯 Custom request headers basic auth timeout and skip certificate options | 🧭 Body text search and response header lookup for targeted checks"
---

A super basic http tool that makes only `GET` request to given URL and returns
status code of the response. Well, if you are `curl` or `http` (*httpie*) user,
you can make the same kind of request and get a kind-of same response since
`statoo` is way better simple :)