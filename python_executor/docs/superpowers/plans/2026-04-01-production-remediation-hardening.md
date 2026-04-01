# Production Remediation Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stabilize the production executor by removing duplicated task state writes, rejecting ambiguous task dispatches, routing result reporting through a single configurable path, and restoring a runnable verification baseline.

**Architecture:** Keep the current production entrypoint and executor shape, but tighten the boundaries around task intake, queue persistence, and reporting. Prefer focused changes that preserve current behavior for valid tasks while making invalid or ambiguous tasks fail fast with explicit messages.

**Tech Stack:** Python 3.10, Flask-SocketIO, gevent, pytest, dataclasses, in-repo task queue persistence.

---

## Scope

This remediation pass focuses on the highest-risk production issues already found during review:

1. Duplicate queue writes between `main_production.py` and `core/task_executor_production.py`
2. Mixed-tool task dispatch silently choosing the first tool instead of rejecting the request
3. Hard-coded report/upload URLs inside the executor instead of using shared configuration
4. Existing baseline import breakage during pytest collection
5. Logger naming not honoring module-specific names

This pass intentionally does **not** attempt a full config-system merger or a full task-model rewrite. Those remain follow-up architecture tasks after production stabilization.

## File Map

**Primary files**
- Modify: `main_production.py`
- Modify: `core/task_executor_production.py`
- Modify: `utils/report_client.py`
- Modify: `utils/logger.py`
- Modify: `core/test_state_handlers.py`

**Possible support files**
- Modify: `config/config_manager.py`
- Modify: `config/settings.py`
- Modify: `tests/manual/test_all_modules.py`
- Create: `tests/test_task_dispatch_production.py`
- Create: `tests/test_report_client_integration.py`
- Create: `tests/test_logger_manager.py`

## Workstreams

### Workstream 1: Task Intake And Queue Ownership

**Intent:** Ensure each inbound task has a single authoritative persistence path and explicit validation before execution.

- [ ] Extract the current production dispatch flow from `main_production.py` into a small, testable helper or service function.
- [ ] Reject task payloads that resolve to multiple tool categories instead of silently picking the first one.
- [ ] Stop pre-writing `ExecutorTask` from `main_production.py` before `TaskExecutorProduction.execute_task()` runs.
- [ ] Keep queue creation inside the executor path only, so `taskNo` maps to one queue record and one status lifecycle.
- [ ] Add focused tests for:
  - single-tool task dispatch succeeds
  - mixed-tool task dispatch fails with a clear validation error
  - executor queue is written once per task

### Workstream 2: Reporting Path Consolidation

**Intent:** Route all remote result and artifact uploads through `ReportClient` or a shared configuration-backed helper.

- [ ] Remove hard-coded upload and report URLs from `core/task_executor_production.py`.
- [ ] Reuse `ReportClient` configuration values for direct result posting and report-file upload.
- [ ] Preserve failed-report persistence behavior when remote reporting fails.
- [ ] Add focused tests for:
  - config-backed URL selection
  - upload/report requests using configured endpoints
  - failed reports still persist when reporting returns errors

### Workstream 3: Baseline Fixes And Observability Cleanup

**Intent:** Restore basic test collection and improve log attribution without broad refactors.

- [ ] Fix `core/test_state_handlers.py` import usage so pytest collection no longer crashes on the stale adapter import path.
- [ ] Update `utils/logger.py` so `get_logger(name)` returns a named child logger instead of collapsing everything into the root executor logger.
- [ ] Add focused tests for logger naming behavior.
- [ ] Update any existing manual smoke tests that rely on stale import paths.

### Workstream 4: Integration Verification

**Intent:** Confirm the remediation pass leaves the executor in a better verified state than the current baseline.

- [ ] Run targeted pytest commands for the new tests first.
- [ ] Run a broader pytest pass and record any remaining pre-existing failures separately from the remediated issues.
- [ ] Review task intake, queue status APIs, and report retry code paths for regressions.
- [ ] Summarize remaining architecture follow-ups that were intentionally deferred.

## Parallelization Strategy

The following work can be done in parallel with disjoint write scopes:

1. **Task intake/queue ownership**
   - Files: `main_production.py`, `tests/test_task_dispatch_production.py`
2. **Reporting path consolidation**
   - Files: `core/task_executor_production.py`, `utils/report_client.py`, `tests/test_report_client_integration.py`
3. **Baseline fix and logger cleanup**
   - Files: `core/test_state_handlers.py`, `utils/logger.py`, `tests/test_logger_manager.py`, `tests/manual/test_all_modules.py`

Integration and final verification happen only after those three workstreams land.

## Success Criteria

- Production dispatch no longer double-writes the task queue
- Mixed-tool task payloads are rejected before execution starts
- Remote reporting uses configuration-backed endpoints only
- Pytest collection no longer fails on the stale adapter import
- Logger naming distinguishes module loggers

## Verification Commands

- `pytest tests/test_task_dispatch_production.py -q`
- `pytest tests/test_report_client_integration.py -q`
- `pytest tests/test_logger_manager.py -q`
- `pytest -q`

## Deferred Follow-Ups

These items should become a second remediation phase after the immediate stabilization work:

1. Merge `config/settings.py` and `config/config_manager.py` into a single configuration system
2. Unify `models.task.Task` and `models.executor_task.Task` behind one execution-facing model
3. Reduce adapter abstraction leakage from `AdapterWrapper`
4. Add structured task-scoped logging and metrics tags across the execution pipeline
