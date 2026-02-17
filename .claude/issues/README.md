# Issue Tracking

This directory contains troubleshooting session logs.

## Format

Each issue gets a unique file: `YYYYMMDD-HHMM-short-description.md`

Example: `20260216-1430-api-startup-failure.md`

## Template

```markdown
# Issue: [Brief Description]

**Created:** YYYY-MM-DD HH:MM
**Status:** IN PROGRESS | RESOLVED | WORKAROUND
**Severity:** LOW | MEDIUM | HIGH | CRITICAL

## Symptom
[What went wrong - error messages, unexpected behavior, test failures]

## Environment
[.NET version, Node version, relevant package versions]

## Investigation
[Command outputs, findings, hypotheses - show FULL output]

## Attempted Fixes
1. [What was tried]
   - Command: `...`
   - Result: [full output]
   - Outcome: [worked/failed/partial]

## Resolution
[What ultimately fixed it, or workaround applied]

## Prevention
[What to watch for in future / how to avoid recurrence]
```

## Purpose

- Provide full context for debugging sessions
- Show complete command outputs (never suppress)
- Track what was tried and what worked
- Enable pattern recognition across sessions
