---
inclusion: auto
---

# Response Protocol — Mandatory Codebase Inspection

## Rule

Before answering ANY question — whether about a bug, feature, how something works, or what's missing — you MUST:

1. **Read the relevant source code files** in the codebase first
2. **Inspect the actual implementation** — never guess or assume
3. **Verify your answer against the real code** before responding
4. **Show evidence** of what you found (file paths, code snippets)

## Process

1. Identify which files/folders are relevant to the question
2. Read those files
3. If the question involves API endpoints — check both backend controller AND frontend service
4. If the question involves Graph integration — verify the interface AND implementation
5. Only THEN formulate your answer based on what you actually found

## Never Do

- Never say "it should work" without verifying
- Never say "the issue is probably X" without reading the code first
- Never claim a fix works without building and testing
- Never assume an endpoint exists without checking the controller
- Never assume a Graph service exists without checking the interface

## Always Do

- Read before answering
- Build after changing
- Test after building
- Verify the fix works
- Commit only confirmed working code
