from core.execution_plan import ExecutionPlan, PlannedCase
from core.task_executor_production import TaskExecutorProduction

def _build_plan(tool_type="canoe"):
    return ExecutionPlan(
        task_no="TASK-STRATEGY-1",
        device_id="DEVICE-1",
        tool_type=tool_type,
        timeout_seconds=30,
        cases=[PlannedCase(case_no="CASE-1", case_name="Case 1", case_type="test_module")],
    )


def test_execute_plan_rejects_when_strategy_prepare_fails(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    queued = []

    class _FakeStrategy:
        def prepare(self, plan, adapter):
            return False, "missing capability"

    class _FakeSelector:
        def select(self, plan):
            return _FakeStrategy()

    monkeypatch.setattr(
        "core.task_executor_production.create_adapter",
        lambda tool_type, config=None: object(),
    )
    monkeypatch.setattr(
        executor._task_queue,
        "put",
        lambda plan: queued.append(plan) or True,
    )
    executor._strategy_selector = _FakeSelector()

    assert executor.execute_plan(_build_plan()) is False
    assert queued == []


def test_execute_plan_runs_strategy_prepare_before_queuing(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    selected = {}

    class _FakeStrategy:
        def prepare(self, plan, adapter):
            selected["tool_type"] = plan.tool_type
            selected["adapter"] = adapter
            return True, None

    class _FakeSelector:
        def select(self, plan):
            return _FakeStrategy()

    class _FakeAdapter:
        pass

    monkeypatch.setattr(
        "core.task_executor_production.create_adapter",
        lambda tool_type, config=None, singleton=True: _FakeAdapter(),
    )
    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.add",
        lambda task, overwrite=True: True,
    )
    executor._strategy_selector = _FakeSelector()

    assert executor.execute_plan(_build_plan(tool_type="tsmaster")) is True
    assert selected["tool_type"] == "tsmaster"
    assert isinstance(selected["adapter"], _FakeAdapter)


def test_run_strategy_execution_delegates_to_selected_strategy(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    observed = {}

    class _FakeStrategy:
        def run(self, plan, adapter, executor=None, config_path=None):
            observed["plan"] = plan.task_no
            observed["adapter"] = adapter
            observed["executor"] = executor
            observed["config_path"] = config_path
            return ["ok"]

    class _FakeSelector:
        def select(self, plan):
            observed["tool_type"] = plan.tool_type
            return _FakeStrategy()

    monkeypatch.setattr(
        "core.task_executor_production.record_metric",
        lambda *args, **kwargs: None,
    )
    monkeypatch.setattr(
        "core.task_executor_production._ensure_observability_context",
        lambda task: type("_Obs", (), {"transition": staticmethod(lambda *args, **kwargs: None)})(),
    )
    executor._strategy_selector = _FakeSelector()

    adapter = object()
    result = executor._run_strategy_execution(_build_plan(), adapter=adapter, config_path="D:/cfgs/main.cfg")

    assert result == ["ok"]
    assert observed["tool_type"] == "canoe"
    assert observed["plan"] == "TASK-STRATEGY-1"
    assert observed["adapter"] is adapter
    assert observed["executor"] is executor
    assert observed["config_path"] == "D:/cfgs/main.cfg"


def test_load_configuration_by_path_prefers_configuration_capability():
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    loaded = []

    class _Collector:
        def add_log(self, *args, **kwargs):
            return None

    class _Controller:
        def get_capability(self, name, default=None):
            if name == "configuration":
                return type("_Cfg", (), {"load": staticmethod(lambda path: loaded.append(path) or True)})()
            return default

    executor.current_collector = _Collector()
    executor.controller = _Controller()

    executor._load_configuration_by_path("D:/cfgs/capability.cfg")

    assert loaded == ["D:/cfgs/capability.cfg"]


def test_start_measurement_prefers_measurement_capability():
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    calls = []

    class _Collector:
        def add_log(self, *args, **kwargs):
            return None

    class _Controller:
        def get_capability(self, name, default=None):
            if name == "measurement":
                return type(
                    "_Measurement",
                    (),
                    {
                        "start": staticmethod(lambda: calls.append("start") or True),
                        "stop": staticmethod(lambda: calls.append("stop") or True),
                    },
                )()
            return default

    executor.current_collector = _Collector()
    executor.controller = _Controller()

    executor._start_measurement(_build_plan())
    executor._stop_measurement(_build_plan())

    assert calls == ["start", "stop"]


def test_execute_task_production_keeps_raw_adapter_on_controller(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    plan = _build_plan(tool_type="canoe")
    plan.config_path = "D:/cfgs/demo.cfg"

    class _Adapter:
        last_error = None

        def connect(self):
            return True

        def disconnect(self):
            return True

        def get_capability(self, name, default=None):
            if name == "configuration":
                return type("_Cfg", (), {"load": staticmethod(lambda path: True)})()
            if name == "measurement":
                return type("_Measurement", (), {"start": staticmethod(lambda: True), "stop": staticmethod(lambda: True)})()
            if name == "test_module":
                return type("_TM", (), {"execute_module": staticmethod(lambda module_name, timeout=None: {"verdict": "PASS"})})()
            return default

    monkeypatch.setattr(
        "core.task_executor_production.create_adapter",
        lambda tool_type, config=None, singleton=True: _Adapter(),
    )
    monkeypatch.setattr("core.task_executor_production.record_metric", lambda *args, **kwargs: None)
    monkeypatch.setattr(
        "core.task_executor_production._ensure_observability_context",
        lambda task: type(
            "_Obs",
            (),
            {
                "transition": staticmethod(lambda *args, **kwargs: None),
                "finish": staticmethod(lambda *args, **kwargs: None),
            },
        )(),
    )
    monkeypatch.setattr(executor, "_update_task_status", lambda *args, **kwargs: None)
    monkeypatch.setattr(executor, "_complete_task", lambda *args, **kwargs: None)
    monkeypatch.setattr(executor, "_cleanup", lambda: None)
    monkeypatch.setattr(executor._task_queue, "mark_processing", lambda *args, **kwargs: None)
    monkeypatch.setattr(executor._task_queue, "mark_completed", lambda *args, **kwargs: None)

    executor._execute_task_production(plan)

    assert isinstance(executor.controller, _Adapter)
    assert executor.controller.__class__ is _Adapter


def test_execute_plan_uses_non_singleton_raw_adapter(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    observed = {}

    class _FakeStrategy:
        def prepare(self, plan, adapter):
            return True, None

    class _FakeSelector:
        def select(self, plan):
            return _FakeStrategy()

    monkeypatch.setattr(
        executor._task_queue,
        "put",
        lambda plan: True,
    )
    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.add",
        lambda task, overwrite=True: True,
    )

    def _fake_create_adapter(tool_type, config=None, singleton=True):
        observed["singleton"] = singleton
        return object()

    monkeypatch.setattr(
        "core.task_executor_production.create_adapter",
        _fake_create_adapter,
    )
    executor._strategy_selector = _FakeSelector()

    assert executor.execute_plan(_build_plan(tool_type="canoe")) is True
    assert observed["singleton"] is False


def test_task_executor_no_longer_exposes_legacy_signal_execution_helpers():
    executor = TaskExecutorProduction(message_sender=lambda _: None)

    assert not hasattr(executor, "_controller_get_signal")
    assert not hasattr(executor, "_controller_set_signal")
    assert not hasattr(executor, "_execute_test_items")
    assert not hasattr(executor, "_execute_single_item")


def test_controller_execute_test_module_prefers_capability_and_never_falls_back_to_execute_test_item():
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    calls = []

    class _Capability:
        @staticmethod
        def execute_module(module_name, timeout=None):
            calls.append((module_name, timeout))
            return {"verdict": "PASS", "module": module_name, "timeout": timeout}

    class _Controller:
        def get_capability(self, name, default=None):
            if name == "test_module":
                return _Capability()
            return default

        def execute_test_item(self, item):
            raise AssertionError("legacy execute_test_item fallback should not be used")

    executor.controller = _Controller()

    result = executor._controller_execute_test_module("SmokeModule", timeout=15)

    assert result["module"] == "SmokeModule"
    assert calls == [("SmokeModule", 15)]
