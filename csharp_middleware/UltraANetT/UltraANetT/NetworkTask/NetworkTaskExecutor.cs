using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UltraANetT.NetworkTask
{
    /// <summary>
    /// 任务执行事件参数
    /// </summary>
    public class TaskExecutionEventArgs : EventArgs
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public string TaskNo { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// 执行进度 (0-100)
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 任务结果
        /// </summary>
        public TaskResult Result { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 详细信息
        /// </summary>
        public Dictionary<string, object> Details { get; set; }

        public TaskExecutionEventArgs()
        {
            Timestamp = DateTime.Now;
            Details = new Dictionary<string, object>();
        }

        public TaskExecutionEventArgs(NetworkTask task) : this()
        {
            TaskNo = task?.TaskNo;
            TaskName = task?.TaskName;
            Status = task?.Status ?? TaskStatus.Unknown;
            Progress = task?.Progress ?? 0;
        }
    }

    /// <summary>
    /// 网络任务执行引擎
    /// 后台监听任务队列，自动触发测试执行
    /// </summary>
    public class NetworkTaskExecutor : IDisposable
    {
        #region 私有字段

        private static NetworkTaskExecutor _instance;
        private static readonly object _lock = new object();

        private readonly TaskQueueManager _taskQueue;
        private readonly NetworkTaskManager _taskManager;
        private readonly ResultReporter _resultReporter;
        private readonly DeviceConfig _deviceConfig;
        private readonly TaskCategoryMapper _categoryMapper;

        private Thread _workerThread;
        private bool _isRunning;
        private bool _isDisposed;
        private bool _isExecuting;
        private readonly int _pollingIntervalMs;
        private readonly int _maxConcurrentTasks;
        private int _currentRunningCount;

        private NetworkTask _currentTask;
        private readonly object _taskLock = new object();

        #endregion

        #region 事件定义

        /// <summary>
        /// 任务开始事件
        /// </summary>
        public event EventHandler<TaskExecutionEventArgs> TaskStarted;

        /// <summary>
        /// 任务进度更新事件
        /// </summary>
        public event EventHandler<TaskExecutionEventArgs> TaskProgress;

        /// <summary>
        /// 任务完成事件
        /// </summary>
        public event EventHandler<TaskExecutionEventArgs> TaskCompleted;

        /// <summary>
        /// 任务失败事件
        /// </summary>
        public event EventHandler<TaskExecutionEventArgs> TaskFailed;

        /// <summary>
        /// 错误发生事件
        /// </summary>
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        #endregion

        #region 属性

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static NetworkTaskExecutor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new NetworkTaskExecutor();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 是否正在执行任务
        /// </summary>
        public bool IsExecuting => _isExecuting;

        /// <summary>
        /// 当前任务
        /// </summary>
        public NetworkTask CurrentTask
        {
            get
            {
                lock (_taskLock)
                {
                    return _currentTask;
                }
            }
        }

        /// <summary>
        /// 任务队列管理器
        /// </summary>
        public TaskQueueManager TaskQueue => _taskQueue;

        #endregion

        #region 构造函数

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private NetworkTaskExecutor()
        {
            _taskQueue = new TaskQueueManager();
            _taskManager = new NetworkTaskManager();
            _resultReporter = new ResultReporter(NetworkTaskConfig.Load().SchedulerUrl);
            _deviceConfig = DeviceConfig.Instance;
            _categoryMapper = TaskCategoryMapper.Instance;

            _pollingIntervalMs = 1000; // 默认1秒轮询
            _maxConcurrentTasks = 1;   // 默认单任务执行
            _currentRunningCount = 0;
            _isRunning = false;
            _isExecuting = false;
        }

        /// <summary>
        /// 使用指定参数构造
        /// </summary>
        public NetworkTaskExecutor(TaskQueueManager taskQueue, NetworkTaskManager taskManager, int pollingIntervalMs = 1000)
        {
            _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
            _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
            _resultReporter = new ResultReporter(NetworkTaskConfig.Load().SchedulerUrl);
            _deviceConfig = DeviceConfig.Instance;
            _categoryMapper = TaskCategoryMapper.Instance;

            _pollingIntervalMs = pollingIntervalMs > 0 ? pollingIntervalMs : 1000;
            _maxConcurrentTasks = 1;
            _currentRunningCount = 0;
            _isRunning = false;
            _isExecuting = false;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 启动执行引擎
        /// </summary>
        public bool Start()
        {
            if (_isRunning)
            {
                System.Diagnostics.Debug.WriteLine("[NetworkTaskExecutor] 执行引擎已在运行中");
                return true;
            }

            try
            {
                // 验证设备配置
                if (!_deviceConfig.IsValid())
                {
                    var errorMsg = "设备配置无效，请先配置设备绑定信息";
                    System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] {errorMsg}");
                    OnError(new ErrorEventArgs(errorMsg));
                    return false;
                }

                _isRunning = true;
                _isExecuting = false;

                // 创建并启动工作线程
                _workerThread = new Thread(WorkerLoop)
                {
                    IsBackground = true,
                    Name = "NetworkTaskExecutor-Worker"
                };
                _workerThread.Start();

                System.Diagnostics.Debug.WriteLine("[NetworkTaskExecutor] 执行引擎已启动");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 启动失败: {ex.Message}");
                OnError(new ErrorEventArgs(ex, "启动执行引擎失败"));
                _isRunning = false;
                return false;
            }
        }

        /// <summary>
        /// 停止执行引擎
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;

            // 等待当前任务完成（最多等待30秒）
            int waitCount = 0;
            while (_isExecuting && waitCount < 300)
            {
                Thread.Sleep(100);
                waitCount++;
            }

            // 如果还在执行，强制中断
            if (_workerThread != null && _workerThread.IsAlive)
            {
                _workerThread.Join(5000);
            }

            System.Diagnostics.Debug.WriteLine("[NetworkTaskExecutor] 执行引擎已停止");
        }

        /// <summary>
        /// 添加任务到执行队列
        /// </summary>
        public void EnqueueTask(NetworkTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            _taskQueue.EnqueueTask(task);
            System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 任务 {task.TaskNo} 已添加到队列");
        }

        /// <summary>
        /// 取消当前任务
        /// </summary>
        public bool CancelCurrentTask()
        {
            lock (_taskLock)
            {
                if (_currentTask == null)
                {
                    return false;
                }

                return _taskQueue.CancelTask(_currentTask.TaskNo);
            }
        }

        /// <summary>
        /// 获取执行器状态信息
        /// </summary>
        public Dictionary<string, object> GetStatus()
        {
            return new Dictionary<string, object>
            {
                ["IsRunning"] = _isRunning,
                ["IsExecuting"] = _isExecuting,
                ["PendingCount"] = _taskQueue.PendingCount,
                ["RunningCount"] = _taskQueue.RunningCount,
                ["CompletedCount"] = _taskQueue.CompletedCount,
                ["CurrentTaskNo"] = _currentTask?.TaskNo,
                ["DeviceId"] = _deviceConfig.BindingConfig.DeviceId,
                ["DeviceName"] = _deviceConfig.BindingConfig.DeviceName
            };
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 工作线程循环
        /// </summary>
        private void WorkerLoop()
        {
            System.Diagnostics.Debug.WriteLine("[NetworkTaskExecutor] 工作线程已启动");

            while (_isRunning)
            {
                try
                {
                    // 如果正在执行任务，等待后继续
                    if (_isExecuting)
                    {
                        Thread.Sleep(_pollingIntervalMs);
                        continue;
                    }

                    // 检查是否达到最大并发数
                    if (_currentRunningCount >= _maxConcurrentTasks)
                    {
                        Thread.Sleep(_pollingIntervalMs);
                        continue;
                    }

                    // 从队列取出任务
                    var task = _taskQueue.DequeueTask();

                    if (task != null)
                    {
                        // 标记为执行中
                        _isExecuting = true;
                        _currentRunningCount++;

                        // 异步执行任务
                        Task.Run(async () =>
                        {
                            try
                            {
                                await ExecuteTaskAsync(task);
                            }
                            finally
                            {
                                _isExecuting = false;
                                _currentRunningCount--;
                            }
                        });
                    }
                    else
                    {
                        // 队列为空，等待后继续轮询
                        Thread.Sleep(_pollingIntervalMs);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 工作线程异常: {ex.Message}");
                    OnError(new ErrorEventArgs(ex, "工作线程执行异常"));
                    Thread.Sleep(_pollingIntervalMs);
                }
            }

            System.Diagnostics.Debug.WriteLine("[NetworkTaskExecutor] 工作线程已退出");
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        private async Task ExecuteTaskAsync(NetworkTask task)
        {
            DateTime startTime = DateTime.Now;
            TaskResult result = null;

            try
            {
                // 设置当前任务
                lock (_taskLock)
                {
                    _currentTask = task;
                }

                // 标记任务开始执行
                task.Status = TaskStatus.Running;
                task.StartedAt = DateTime.Now;
                _taskQueue.MarkTaskRunning(task.TaskNo);

                // 触发任务开始事件
                OnTaskStarted(new TaskExecutionEventArgs(task)
                {
                    Message = $"开始执行任务: {task.TaskName}"
                });

                // 上报状态：开始执行
                await ReportStatusAsync(task.TaskNo, TaskStatus.Running, 0, "开始执行任务");

                // 1. 准备测试环境
                var prepareResult = PrepareTestEnvironment(task);
                if (!prepareResult.Success)
                {
                    throw new Exception($"准备测试环境失败: {prepareResult.Message}");
                }

                // 更新进度
                UpdateProgress(task, 10, "测试环境准备完成");
                await ReportStatusAsync(task.TaskNo, TaskStatus.Running, 10, "测试环境准备完成");

                // 2. 执行测试
                var testResult = await ExecuteTestAsync(task);
                if (!testResult.Success)
                {
                    throw new Exception($"测试执行失败: {testResult.Message}");
                }

                // 更新进度
                UpdateProgress(task, 80, "测试执行完成");
                await ReportStatusAsync(task.TaskNo, TaskStatus.Running, 80, "测试执行完成");

                // 3. 生成测试结果
                result = BuildTaskResult(task, testResult.Data);
                result.Duration = DateTime.Now - startTime;

                // 更新进度
                UpdateProgress(task, 90, "生成测试结果");
                await ReportStatusAsync(task.TaskNo, TaskStatus.Running, 90, "生成测试结果");

                // 4. 上报结果
                var reportSuccess = await ReportResultAsync(task.TaskNo, result);

                // 标记任务完成
                task.Status = TaskStatus.Completed;
                task.CompletedAt = DateTime.Now;
                task.Progress = 100;
                _taskQueue.MarkTaskCompleted(task.TaskNo, result);

                // 触发完成事件
                OnTaskCompleted(new TaskExecutionEventArgs(task)
                {
                    Result = result,
                    Message = reportSuccess ? "任务执行完成" : "任务执行完成，但结果上报失败"
                });
            }
            catch (Exception ex)
            {
                // 标记任务失败
                task.Status = TaskStatus.Failed;
                task.ErrorMessage = ex.Message;
                task.CompletedAt = DateTime.Now;
                _taskQueue.MarkTaskFailed(task.TaskNo, ex.Message);

                // 上报失败状态
                await ReportStatusAsync(task.TaskNo, TaskStatus.Failed, task.Progress, ex.Message);

                // 触发失败事件
                OnTaskFailed(new TaskExecutionEventArgs(task)
                {
                    Exception = ex,
                    Message = $"任务执行失败: {ex.Message}"
                });

                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 任务 {task.TaskNo} 执行失败: {ex.Message}");
            }
            finally
            {
                // 清除当前任务
                lock (_taskLock)
                {
                    _currentTask = null;
                }
            }
        }

        /// <summary>
        /// 准备测试环境
        /// </summary>
        private (bool Success, string Message) PrepareTestEnvironment(NetworkTask task)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 准备测试环境: {task.TaskNo}");

                // 1. 设置 GlobalVar.CurrentVNode (车型节点)
                var vehicleNodes = _deviceConfig.GetVehicleNodeList();
                GlobalVar.CurrentVNode = vehicleNodes;
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 设置车型节点: {string.Join(" > ", vehicleNodes)}");

                // 2. 映射任务类别
                var category = _categoryMapper.MapToCategory(task.TaskName);
                var testType = _categoryMapper.GetTestType(category);
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 任务类别: {category}, 测试类型: {testType}");

                // 3. 验证配置文件
                if (!string.IsNullOrEmpty(task.ConfigPath) && !System.IO.File.Exists(task.ConfigPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 警告: 配置文件不存在: {task.ConfigPath}");
                }

                // 4. 设置变量
                if (task.Variables != null && task.Variables.Count > 0)
                {
                    foreach (var kvp in task.Variables)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 变量: {kvp.Key} = {kvp.Value}");
                    }
                }

                // 5. 检查测试项
                if (task.TestItems == null || task.TestItems.Count == 0)
                {
                    return (false, "任务没有测试项");
                }

                return (true, "测试环境准备成功");
            }
            catch (Exception ex)
            {
                return (false, $"准备测试环境异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行测试
        /// </summary>
        private async Task<(bool Success, string Message, Dictionary<string, object> Data)> ExecuteTestAsync(NetworkTask task)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 执行测试: {task.TaskNo}");

                var testData = new Dictionary<string, object>
                {
                    ["TaskNo"] = task.TaskNo,
                    ["TaskName"] = task.TaskName,
                    ["ProjectNo"] = task.ProjectNo,
                    ["TestItems"] = task.TestItems,
                    ["StartTime"] = DateTime.Now
                };

                // 映射任务类别
                var category = _categoryMapper.MapToCategory(task.TaskName);
                testData["Category"] = category.ToString();
                testData["TestType"] = _categoryMapper.GetTestType(category);

                // 模拟测试执行（实际项目中调用真实测试执行逻辑）
                int totalItems = task.TestItems?.Count ?? 0;
                int completedItems = 0;
                var itemResults = new List<Dictionary<string, object>>();

                foreach (var testItem in task.TestItems ?? new List<TestItem>())
                {
                    // 模拟执行每个测试项
                    await Task.Delay(100); // 模拟执行时间

                    var itemResult = new Dictionary<string, object>
                    {
                        ["Name"] = testItem.Name,
                        ["Type"] = testItem.Type,
                        ["Status"] = "PASS",
                        ["Duration"] = 0.1
                    };

                    itemResults.Add(itemResult);
                    completedItems++;

                    // 更新进度
                    int progress = 10 + (int)((double)completedItems / totalItems * 70);
                    UpdateProgress(task, progress, $"执行测试项: {testItem.Name}");
                }

                testData["ItemResults"] = itemResults;
                testData["EndTime"] = DateTime.Now;
                testData["TotalItems"] = totalItems;
                testData["PassedItems"] = itemResults.Count; // 简化：假设全部通过

                // 实际项目中，这里应该调用真实的测试执行逻辑
                // 例如：调用 TestStart 模块执行测试

                return (true, "测试执行成功", testData);
            }
            catch (Exception ex)
            {
                return (false, $"测试执行异常: {ex.Message}", null);
            }
        }

        /// <summary>
        /// 构建任务结果
        /// </summary>
        private TaskResult BuildTaskResult(NetworkTask task, Dictionary<string, object> testData)
        {
            var result = new TaskResult
            {
                TaskNo = task.TaskNo,
                Status = TaskStatus.Completed,
                CompletedAt = DateTime.Now
            };

            // 从测试数据中提取结果
            if (testData != null)
            {
                // 构建用例结果列表
                if (testData.ContainsKey("ItemResults"))
                {
                    var itemResults = testData["ItemResults"] as List<Dictionary<string, object>>;
                    if (itemResults != null)
                    {
                        result.CaseList = new List<CaseResult>();

                        foreach (var item in itemResults)
                        {
                            var caseResult = new CaseResult
                            {
                                CaseNo = item.ContainsKey("Name") ? item["Name"]?.ToString() : "",
                                Result = item.ContainsKey("Status") ? item["Status"]?.ToString() : "UNKNOWN",
                                Duration = item.ContainsKey("Duration") ? Convert.ToDouble(item["Duration"]) : 0,
                                Remark = ""
                            };
                            result.CaseList.Add(caseResult);
                        }
                    }
                }
            }

            // 计算统计数据
            int totalCount = result.CaseList?.Count ?? 0;
            int passedCount = result.CaseList?.FindAll(c => c.Result == "PASS").Count ?? 0;
            int failedCount = result.CaseList?.FindAll(c => c.Result == "FAIL").Count ?? 0;

            result.Remark = $"总用例: {totalCount}, 通过: {passedCount}, 失败: {failedCount}";

            return result;
        }

        /// <summary>
        /// 上报任务结果
        /// </summary>
        private async Task<bool> ReportResultAsync(string taskNo, TaskResult result)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 上报任务结果: {taskNo}");

                // 使用 ResultReporter 上报
                var success = await _resultReporter.ReportResultAsync(result);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 任务结果上报成功: {taskNo}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 任务结果上报失败: {taskNo}");
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 上报任务结果异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 上报任务状态
        /// </summary>
        private async Task ReportStatusAsync(string taskNo, TaskStatus status, int progress, string message)
        {
            try
            {
                if (_taskManager != null)
                {
                    await _taskManager.ReportTaskStatusAsync(taskNo, status, progress, message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 上报任务状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新任务进度
        /// </summary>
        private void UpdateProgress(NetworkTask task, int progress, string message)
        {
            task.Progress = progress;

            OnTaskProgress(new TaskExecutionEventArgs(task)
            {
                Progress = progress,
                Message = message
            });
        }

        #endregion

        #region 事件触发方法

        protected virtual void OnTaskStarted(TaskExecutionEventArgs e)
        {
            TaskStarted?.Invoke(this, e);
            System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 任务开始: {e.TaskNo}");
        }

        protected virtual void OnTaskProgress(TaskExecutionEventArgs e)
        {
            TaskProgress?.Invoke(this, e);
        }

        protected virtual void OnTaskCompleted(TaskExecutionEventArgs e)
        {
            TaskCompleted?.Invoke(this, e);
            System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 任务完成: {e.TaskNo}");
        }

        protected virtual void OnTaskFailed(TaskExecutionEventArgs e)
        {
            TaskFailed?.Invoke(this, e);
            System.Diagnostics.Debug.WriteLine($"[NetworkTaskExecutor] 任务失败: {e.TaskNo} - {e.Message}");
        }

        protected virtual void OnError(ErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }

        #endregion

        #region IDisposable 实现

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            try
            {
                Stop();
                _resultReporter?.Dispose();
                _taskManager?.Dispose();
            }
            catch
            {
                // 忽略释放时的异常
            }

            System.Diagnostics.Debug.WriteLine("[NetworkTaskExecutor] 资源已释放");
        }

        #endregion
    }
}