# Execution Observability Refactor Design

## Summary

This spec defines the third-stage production hardening work for the Python executor: a state-machine-aware observability refactor for execution flow. The goal is to make task lifecycle state, structured logs, metrics, and health/monitoring endpoints tell one coherent story for the same task, without changing task protocol, adapter abstractions, or the unified config system completed in phase two.

The implementation will introduce a lightweight execution observability layer centered on a shared execution context. Runtime components will use that context to emit lifecycle-aware logs and metrics across task dispatch, queueing, execution, reporting, and completion.

## Goals

- Introduce a single execution observability model shared by logs, metrics, and health/status output.
- Make task lifecycle explicit and machine-readable across main dispatch and production execution paths.
- Add task-scoped structured logging fields for production troubleshooting.
- Add business metrics that reflect queueing, execution, reporting, failures, and stage durations.
- Upgrade `/health`, `/status`, and `/metrics` to expose business-health information instead of only low-level process/system state.
- Add automated tests for lifecycle behavior, structured logging, and monitoring responses.

## Non-Goals

- Do not redesign the TDM2.0 task protocol.
- Do not unify or replace the task model in this stage.
- Do not refactor adapter abstractions or their public interfaces.
- Do not replace the unified config system implemented in `config/unified_config.py`.
- Do not introduce an external observability backend such as Prometheus, OpenTelemetry, or ELK in this stage.

## Current Problems

The current production executor has useful logging and metrics primitives, but the execution path is not observability-driven:

- Lifecycle phases are implicit in code flow rather than explicitly modeled.
- Logs are mostly free-form strings and often omit task identity, stage, and error metadata.
- Metrics exist, but they are fragmented and skew toward raw point collection rather than business-health summaries.
- Health and metrics endpoints expose process health but do not clearly answer whether task execution is healthy.
- Failures are visible, but not normalized into a consistent “which stage failed, why, and whether it is retryable” model.

This makes incident diagnosis slower and increases the chance of different views (`logs`, `/health`, `/metrics`, queue state) disagreeing with each other.

## Proposed Architecture

The refactor will add a lightweight execution observability layer rather than replacing the existing executor architecture.

### 1. Execution Context

Add a shared execution context object that represents one task’s observability identity and current state. It should include at least:

- `task_no`
- `device_id`
- `tool_type`
- `current_stage`
- `attempt`
- `error_code`
- `error_message`
- `started_at`
- `last_transition_at`
- `report_status`

This context is created at task receipt and updated as the task progresses. It becomes the source of truth for log field injection, stage metrics, and health snapshots.

### 2. Explicit Lifecycle Model

The execution flow will be normalized into these stages:

1. `received`
2. `validated`
3. `queued`
4. `preparing`
5. `executing`
6. `reporting`
7. `finished`

Each transition performs the same three responsibilities:

- emit a structured lifecycle log
- record lifecycle metrics
- update the current observable task snapshot

Failures are modeled as stage-aware failures rather than only free-form exceptions. A failed task must preserve:

- failed stage
- error code
- human-readable error message
- retryability signal when known

### 3. Execution Observer Layer

Add a small observer/coordinator layer rather than rewriting the executor state machine. This layer will wrap existing execution nodes in `main_production.py` and `core/task_executor_production.py`.

Responsibilities:

- create execution context for incoming tasks
- expose helper methods for stage transitions
- emit task-scoped log events
- emit metrics derived from stage transitions
- expose the latest task health snapshot to HTTP endpoints

This is intentionally additive. The production executor continues to own actual task execution, queue behavior, and reporting behavior.

## Component Design

### Logging

`utils/logger.py` will be extended to support task context injection without replacing Python logging.

Preferred design:

- retain the current `get_logger()` entrypoint
- add a lightweight context-aware logger helper or binding method
- inject context fields into log records in a backward-compatible way

Required structured fields for task-scoped logs:

- `task_no`
- `device_id`
- `tool_type`
- `stage`
- `attempt`
- `error_code`

The file logger and in-memory logger should preserve these fields in a way that can be asserted in tests and inspected during incidents.

### Metrics

`utils/metrics.py` will be upgraded to support both raw point recording and business summaries.

Retained metrics:

- `system.cpu_percent`
- `system.memory_percent`
- `system.disk_percent`

New business metrics:

- `task.received`
- `task.validated`
- `task.queued`
- `task.preparing`
- `task.executing`
- `task.reporting`
- `task.finished`
- `task.failed`
- `task.report.success`
- `task.report.failure`
- `task.stage.duration`
- `task.queue.depth`
- `task.active.count`

Metrics should carry labels when useful, especially:

- `task_no`
- `tool_type`
- `stage`
- `status`

### Health And Monitoring Endpoints

`main_production.py` endpoints will be extended as follows:

- `/health`
  - returns overall service health plus business-health flags
  - includes configuration validity
  - includes active task state
  - includes failure-report backlog signal when available
  - can downgrade health when a task is stuck in one stage beyond a threshold

- `/status`
  - returns current execution snapshot and lifecycle summary
  - includes current queue/active-task observability state

- `/metrics`
  - returns raw metrics and business summaries
  - includes queue depth, active task count, report success/failure counts, recent failed task count, and stage duration aggregates

## Data Flow

### Task Dispatch

When `TASK_DISPATCH` arrives in `main_production.py`:

1. create execution context at `received`
2. validate input and transition to `validated`
3. after successful enqueue, transition to `queued`
4. if dispatch fails, mark failure at the appropriate stage

### Task Execution

When the worker begins real execution in `core/task_executor_production.py`:

1. transition to `preparing`
2. once runtime/tool/config preparation is complete, transition to `executing`
3. once execution finishes and result assembly/upload begins, transition to `reporting`
4. once result handling is complete, transition to `finished`

### Failures

Failures do not bypass observability. Every failure path must:

- update the execution context
- emit a structured error log
- record failure metrics
- surface the latest failed stage in the health snapshot

## Testing Strategy

The implementation must add or update automated tests in three groups.

### 1. Lifecycle Tests

Validate that lifecycle transitions occur in the correct order:

- successful dispatch path
- successful execution path
- failure during validation
- failure during prepare/execute/report stages

Assertions must verify stage ordering and final stage state.

### 2. Structured Logging Tests

Validate that task-scoped logs contain expected context fields:

- `task_no`
- `device_id`
- `tool_type`
- `stage`
- `error_code` for failures

The test should not rely on reading a real log file if in-memory handlers can validate the fields more deterministically.

### 3. Monitoring Endpoint Tests

Validate that `/health`, `/status`, and `/metrics` return business summaries that reflect:

- queued task count
- active task snapshot
- recent failure state
- reporting success/failure counters
- stage duration aggregates

## Rollout Constraints

This stage must preserve behavior compatibility:

- existing task execution flow must still pass current regression tests
- existing API routes must remain available
- logging changes must not break current log retrieval code paths
- configuration lookup must continue using the phase-two unified config implementation

## Acceptance Criteria

The stage is complete when all of the following are true:

- task lifecycle stages are explicit and consistently updated
- task-scoped logs contain structured execution context fields
- `/health`, `/status`, and `/metrics` expose business-health summaries
- lifecycle, structured logging, and monitoring endpoint tests are present and passing
- full project `pytest` still passes with only the intended skips

