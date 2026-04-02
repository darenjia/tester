# TTworkbench Strategy Alignment Design

## Goal

Align `TTworkbench` with the same execution architecture already used by `CANoe` and
`TSMaster`:

- `ExecutionPlan -> ExecutionStrategy -> Adapter Capabilities -> Tool Runtime`
- raw adapters expose low-level capabilities
- tool-specific execution semantics live in strategies
- adapter-level `execute_test_item()` remains legacy-only and must not be the main path

## Current Problems

`TTworkbenchAdapter` is still an old-model island:

- execution semantics are centered on `execute_test_item(clf_test / batch_test)`
- no dedicated execution strategy exists
- no stable capability registration exists
- selector does not recognize `ttworkbench`
- executor therefore cannot treat `TTworkbench` as a first-class tool runtime

## Target Shape

### Strategy

Introduce `TTworkbenchExecutionStrategy`.

Responsibilities:

- validate required capabilities
- load `.clf` configuration when needed
- run single or batch clf execution
- normalize adapter results into `TestResult`
- collect report/log artifacts

### Capabilities

`TTworkbenchAdapter` should register:

- `configuration`
  - validate/load clf file path
- `measurement`
  - minimal lifecycle hooks, even if implemented as no-op / process preparation
- `artifact`
  - resolve generated report/log paths and parsed log details
- `ttworkbench_execution`
  - execute a single clf file
  - execute multiple clf files

Unlike CANoe/TSMaster, this tool does not need to fake unrelated capabilities such as
system variables or RPC control.

## Execution Model

The internal `ExecutionPlan` should carry TTworkbench cases as execution-oriented cases.

Recommended interpretation:

- `case_type = "clf_test"` for one clf file
- `case_type = "batch_test"` for a grouped execution request when explicitly needed

Preferred steady-state:

- strategy iterates plan cases and calls a single-clf execution capability
- batch execution becomes a strategy concern instead of a special adapter API shape

## Adapter Changes

`TTworkbenchAdapter` should be adjusted so that:

- capability registration happens in `__init__`
- core execution logic is exposed through capability callables
- `execute_test_item()` becomes legacy-only, consistent with `BaseTestAdapter`
- old `clf_test / batch_test` branches remain only as a thin compatibility wrapper if still needed during migration

## Selector Changes

`ExecutionStrategySelector` must recognize `tool_type == "ttworkbench"` and return
`TTworkbenchExecutionStrategy`.

## Executor Boundary

No new tool-specific branches should be added back into `TaskExecutorProduction`.

If TTworkbench needs special handling, that handling belongs in:

- `TTworkbenchExecutionStrategy`
- TTworkbench-specific capability implementation

## Migration Order

1. Add TTworkbench capability registration
2. Add `TTworkbenchExecutionStrategy`
3. Extend selector
4. Add tests covering selector + strategy + adapter capability behavior
5. Reduce adapter-level `execute_test_item()` to legacy compatibility only

## Done Criteria

- selector supports `ttworkbench`
- TTworkbench executes through strategy + capability
- no new executor tool-branch logic is introduced
- adapter-level `execute_test_item()` is not used by the main execution path
- full pytest suite remains green
