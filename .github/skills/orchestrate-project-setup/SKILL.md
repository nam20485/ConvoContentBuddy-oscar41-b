---
name: orchestrate-project-setup
description: 'Orchestrate the project-setup-upgraded dynamic workflow to fully initialize a new repository. Use when bootstrapping a new project: sets up repo structure, app plan, application foundation and structure, testing infrastructure, deployment infrastructure, and documentation — with automatic validation gating after every assignment.'
argument-hint: 'Optional: additional workflow inputs (e.g., tech stack override)'
---

# Orchestrate Project Setup

## When to Use

Invoke this skill when asked to:

- Set up, initialize, or bootstrap a new project or repository
- Run the `project-setup-upgraded` dynamic workflow
- Execute `/orchestrate-project-setup` as the first action on a fresh repo

## Mandatory Instruction-Reading Chain

Before orchestrating, read these files in order. Refuse to proceed if any file is unreadable.

1. `ai-core-instructions.md` — Core protocol and rules
2. `dynamic-workflow-syntax.md` — Event system (`pre-assignment-begin`, etc.)
3. `ai-workflow-assignments.md` — Assignment protocol
4. `orchestrate-dynamic-workflow.md` — Orchestration rules and guardrails
5. `dynamic-workflows/project-setup-upgraded.md` — The specific workflow script
6. Each assignment file before delegating to it

The branch value for constructing raw URLs is in `AGENTS.md` under `<configuration><branch>`.

URL translation: `https://github.com/nam20485/agent-instructions/blob/{branch}/...`
→ `https://raw.githubusercontent.com/nam20485/agent-instructions/{branch}/...`
(remove `blob/`, replace `github.com/` with `raw.githubusercontent.com/`)

## Procedure

### 1. Preflight

- Resolve and print the full resolution trace with URLs:
  `orchestrate-dynamic-workflow → dynamic-workflows/project-setup-upgraded.md → [assignments] → ai-workflow-assignments/<assignment>.md`
- Verify all files are readable; abort if any are missing.
- Confirm required GitHub credentials/permissions are available.
- Determine whether the default branch is protected; plan feature-branch + PR route if so.

### 2. Parse the Workflow Script

The `project-setup-upgraded` workflow defines these assignments (executed in order):

| # | Assignment | Purpose |
| --- | --- | --- |
| 1 | `init-existing-repository` | Initialize git repo and basic structure |
| 2 | `create-app-plan` | Create comprehensive application plan and architecture docs |
| 3 | `create-application-foundation` | Set up dependencies, build system, code quality tools, directory structure |
| 4 | `create-application-structure` | Create entry point, core architecture, configuration, utilities |
| 5 | `create-testing-infrastructure` | Set up test framework, directory structure, smoke tests |
| 6 | `create-deployment-infrastructure` | Docker, CI/CD workflows, deployment scripts, setup scripts |
| 7 | `pr-approval-and-merge` | Open PR, request review, merge on approval |
| 8 | `debrief-and-document` | Produce 12-section debriefing report with lessons learned and metrics |

### 3. Event Handlers

Execute these event handlers at each lifecycle point — **do not skip**:

#### `pre-assignment-begin` (before EACH assignment)

Run `gather-context` assignment. Record output as `#events.pre-assignment-begin.gather-context`.

#### `on-assignment-failure` (when ANY assignment fails)

Run `recover-from-error` assignment. Record output as `#events.on-assignment-failure.recover-from-error`.
Stop execution and report failure details after recovery attempt.

#### `post-assignment-complete` (after EACH assignment succeeds)

Run these in order:

1. `create-repository-summary` — Record as `#events.post-assignment-complete.create-repository-summary`
2. `validate-assignment-completion` — Record as `#events.post-assignment-complete.validate-assignment-completion`
   - **Gate**: If validation FAILS, halt the workflow and request manual intervention.
   - **Gate**: If validation PASSES, continue.
3. `report-progress` — Record as `#events.post-assignment-complete.report-progress`

### 4. Execute Assignments

For each assignment in the list above:

1. Fire `pre-assignment-begin` event.
2. Fetch and read the assignment file from the remote canonical repository.
3. Extract: Inputs, Detailed Steps, and Acceptance Criteria.
4. Validate the delegation prompt is data-complete before delegating (no placeholder text).
5. Delegate to the appropriate agent/subagent and await completion.
6. If the assignment fails, fire `on-assignment-failure` and stop.
7. Fire `post-assignment-complete` events (summary → **validate (gated)** → progress report).
8. Only proceed to the next assignment after validation PASSES.

### 5. Acceptance Criteria Gating

Extract Acceptance Criteria from each assignment file. Treat every criterion as a
must-pass gate — **do not declare success on self-reported completion**. Require
independent evidence (repo queries, file checks, test results) for each criterion.

All Acceptance Criteria must pass, or the assignment fails. No exceptions.

### 6. Run Report

Produce a final Run Report after all assignments complete:

- **Header**: Assignment = `orchestrate-dynamic-workflow`, Inputs = `project-setup-upgraded`
- **Resolution Trace**: Ordered list of files used with URLs
- **Events**: Which events ran and at which lifecycle points
- **Actions Executed**: Keyed actions per assignment
- **Acceptance Criteria Results**: Each criterion with PASS/FAIL and evidence link
- **Deviations**: Any variance from specs with rationale
- **Outcome**: Success only when all criteria PASS

## Inputs

| Input | Required | Description |
| --- | --- | --- |
| `$workflow_name` | Auto-set | Always `project-setup-upgraded` |
| Additional | Optional | Any override parameters supported by the workflow's Inputs section |

## Completion Criteria

The workflow is complete when:

- All 8 assignments have been executed and approved
- All event handlers have run at their designated lifecycle points
- `validate-assignment-completion` has PASSED after each assignment
- The final Run Report is produced and contains no unresolved FAILs
- The debriefing document is committed to the repository
