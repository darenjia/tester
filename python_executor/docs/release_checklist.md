# 发布前检查清单

## 启动前

- 确认根目录 `config.json` 已更新到目标环境配置
- 运行 `python scripts/runtime_diagnose.py` 获取当前运行摘要
- 调用 `GET /api/runtime/preflight`，确认状态为 `ready` 或已接受的 `warning`
- 检查 `GET /health` 与 `GET /metrics` 返回正常

## 关键检查项

- 配置校验无阻塞错误
- `logs/` 与 `data/` 目录可写
- `report.result_api_url` 配置有效
- 至少一个测试工具处于 `ready`
- 无异常堆积的失败报告需要人工确认

## 发布后

- 再次执行 `GET /api/runtime/diagnose`
- 观察最新任务是否能进入 `queued -> preparing -> executing -> collecting -> reporting -> finished`
- 确认失败报告数量没有异常上升
