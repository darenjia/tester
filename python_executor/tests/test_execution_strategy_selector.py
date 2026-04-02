from types import SimpleNamespace

import pytest


def test_selector_returns_canoe_strategy():
    from core.execution_strategies.selector import ExecutionStrategySelector

    selector = ExecutionStrategySelector()
    plan = SimpleNamespace(tool_type="canoe")

    strategy = selector.select(plan)

    assert strategy.strategy_name == "canoe"


def test_selector_returns_tsmaster_strategy():
    from core.execution_plan import ExecutionPlan
    from core.execution_strategies.selector import ExecutionStrategySelector

    selector = ExecutionStrategySelector()
    plan = ExecutionPlan(task_no="TASK-001", tool_type="tsmaster")

    strategy = selector.select(plan)

    assert strategy.strategy_name == "tsmaster"


def test_selector_returns_ttworkbench_strategy():
    from core.execution_strategies.selector import ExecutionStrategySelector

    selector = ExecutionStrategySelector()
    plan = SimpleNamespace(tool_type="ttworkbench")

    strategy = selector.select(plan)

    assert strategy.strategy_name == "ttworkbench"


def test_selector_rejects_unsupported_tool_type():
    from core.execution_strategies.selector import ExecutionStrategySelector

    selector = ExecutionStrategySelector()
    plan = SimpleNamespace(tool_type="unknown")

    with pytest.raises(ValueError, match="Unsupported tool type"):
        selector.select(plan)
