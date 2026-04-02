"""TSMaster execution strategy."""

from typing import Any

from .base import ExecutionStrategy


class TSMasterExecutionStrategy(ExecutionStrategy):
    """Strategy placeholder for TSMaster execution flow."""

    strategy_name = "tsmaster"

    def prepare(self, plan: Any, adapter: Any):
        has_configuration = adapter.get_capability("configuration") is not None
        has_measurement = adapter.get_capability("measurement") is not None
        has_rpc = adapter.get_capability("rpc_execution") is not None

        if not has_configuration or not has_measurement:
            return False, "Missing TSMaster capability: configuration/measurement"
        if not has_rpc:
            return False, "Missing TSMaster RPC capability"
        return True, None

    def run(self, plan: Any, adapter: Any, executor: Any = None, config_path: str | None = None) -> Any:
        if executor is None:
            raise RuntimeError("TSMasterExecutionStrategy requires executor context")

        if config_path:
            executor._load_configuration_by_path(config_path)

        try:
            executor._start_test_execution(plan)
            return executor._collect_tsmaster_results(plan)
        finally:
            executor._stop_measurement(plan)
