using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UltraANetT.NetworkTask
{
    /// <summary>
    /// 网络任务管理器
    /// </summary>
    public class NetworkTaskManager : IDisposable
    {
        private HttpServerManager _httpServer;
        private TaskQueueManager _taskQueue;
        private ResultReporter _resultReporter;
        private NetworkTaskExecutor _executor;
        private readonly NetworkTaskConfig _config;
        private bool _isRunning;
        private bool _isDisposed;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 是否已连接（与IsRunning相同，用于兼容性）
        /// </summary>
        public bool IsConnected => _isRunning;

        /// <summary>
        /// HTTP服务是否运行中
        /// </summary>
        public bool IsHttpServerRunning => _httpServer?.IsRunning ?? false;

        /// <summary>
        /// 任务队列管理器
        /// </summary>
        public TaskQueueManager TaskQueue => _taskQueue;

        /// <summary>
        /// 配置
        /// </summary>
        public NetworkTaskConfig Config => _config;

        /// <summary>
        /// 任务接收事件
        /// </summary>
        public event EventHandler<TaskReceivedEventArgs> TaskReceived;

        /// <summary>
        /// 连接状态变更事件
        /// </summary>
        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        /// <summary>
        /// 错误发生事件
        /// </summary>
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// 构造函数
        /// </summary>
        public NetworkTaskManager(NetworkTaskConfig config = null)
        {
            _config = config ?? NetworkTaskConfig.Load();
            _taskQueue = new TaskQueueManager();
        }

        /// <summary>
        /// 启动网络任务管理器
        /// </summary>
        public async Task<bool> StartAsync()
        {
            if (_isRunning)
            {
                return true;
            }

            if (!_config.HttpServerEnabled)
            {
                System.Diagnostics.Debug.WriteLine("[NetworkTaskManager] HTTP服务未启用");
                return false;
            }

            try
            {
                // 创建HTTP服务器
                _httpServer = new HttpServerManager(_config, _taskQueue);
                
                // 创建结果上报客户端
                _resultReporter = new ResultReporter(_config.SchedulerUrl);

                // 启动HTTP服务器
                await _httpServer.StartAsync();
                
                _isRunning = true;
                
                // 触发连接状态变更事件
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                {
                    IsConnected = true
                });

                // 启动执行引擎（如果配置了自动执行）
                if (_config.AutoExecute)
                {
                    _executor = new NetworkTaskExecutor(_taskQueue, this, _config.PollingIntervalMs);

                    // 订阅执行器事件
                    _executor.TaskStarted += (s, e) => System.Diagnostics.Debug.WriteLine($"[NetworkTaskManager] 任务开始: {e.TaskNo}");
                    _executor.TaskCompleted += (s, e) => System.Diagnostics.Debug.WriteLine($"[NetworkTaskManager] 任务完成: {e.TaskNo}");
                    _executor.TaskFailed += (s, e) => System.Diagnostics.Debug.WriteLine($"[NetworkTaskManager] 任务失败: {e.TaskNo} - {e.Message}");
                    _executor.ErrorOccurred += (s, e) => ErrorOccurred?.Invoke(this, e);

                    if (_executor.Start())
                    {
                        System.Diagnostics.Debug.WriteLine("[NetworkTaskManager] 执行引擎已启动");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[NetworkTaskManager] 执行引擎启动失败");
                    }
                }

                System.Diagnostics.Debug.WriteLine("[NetworkTaskManager] 网络任务管理器已启动");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskManager] 启动失败: {ex.Message}");
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex, $"启动网络任务管理器失败: {ex.Message}"));
                return false;
            }
        }

        /// <summary>
        /// 停止网络任务管理器
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            // 停止执行引擎
            if (_executor != null)
            {
                _executor.Stop();
                _executor.Dispose();
                _executor = null;
                System.Diagnostics.Debug.WriteLine("[NetworkTaskManager] 执行引擎已停止");
            }

            _isRunning = false;

            if (_httpServer != null)
            {
                await _httpServer.StopAsync();
                _httpServer.Dispose();
                _httpServer = null;
            }

            if (_resultReporter != null)
            {
                _resultReporter.Dispose();
                _resultReporter = null;
            }

            // 触发连接状态变更事件
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
            {
                IsConnected = false
            });

            System.Diagnostics.Debug.WriteLine("[NetworkTaskManager] 网络任务管理器已停止");
        }

        /// <summary>
        /// 上报任务结果到远程调度服务器
        /// </summary>
        public async Task<bool> ReportResultToScheduler(string taskNo, TaskResult result)
        {
            if (_resultReporter == null)
            {
                System.Diagnostics.Debug.WriteLine("[NetworkTaskManager] 结果上报客户端未初始化");
                return false;
            }

            if (string.IsNullOrEmpty(taskNo) || result == null)
            {
                System.Diagnostics.Debug.WriteLine("[NetworkTaskManager] 任务编号或结果不能为空");
                return false;
            }

            try
            {
                result.TaskNo = taskNo;
                return await _resultReporter.ReportResultAsync(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskManager] 上报任务结果失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 上报任务状态到远程调度服务器
        /// </summary>
        public async Task<bool> ReportStatusToScheduler(string taskNo, TaskStatus status, int progress = 0, string message = "")
        {
            if (_resultReporter == null)
            {
                return false;
            }

            try
            {
                return await _resultReporter.ReportStatusAsync(taskNo, status, progress, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskManager] 上报任务状态失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 上报任务状态（别名方法）
        /// </summary>
        public async Task<bool> ReportTaskStatusAsync(string taskNo, TaskStatus status, int progress = 0, string message = "")
        {
            return await ReportStatusToScheduler(taskNo, status, progress, message);
        }

        /// <summary>
        /// 上报任务结果（别名方法）
        /// </summary>
        public async Task<bool> ReportTaskResultAsync(string taskNo, TaskResult result)
        {
            return await ReportResultToScheduler(taskNo, result);
        }

        /// <summary>
        /// 将网络任务转换为本地任务格式
        /// </summary>
        public Dictionary<string, object> ConvertToLocalTask(NetworkTask networkTask)
        {
            // 从测试项中提取模块列表
            var moduleList = new List<string>();
            if (networkTask.TestItems != null && networkTask.TestItems.Count > 0)
            {
                // 根据测试项名称或类型推断模块
                foreach (var item in networkTask.TestItems)
                {
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        // 提取模块名称（假设测试项名称格式为：模块名_用例名）
                        var parts = item.Name.Split('_');
                        if (parts.Length > 0 && !moduleList.Contains(parts[0]))
                        {
                            moduleList.Add(parts[0]);
                        }
                    }
                }
            }

            // 如果没有提取到模块，使用默认值
            if (moduleList.Count == 0)
            {
                moduleList.Add("Default");
            }

            // 生成符合现有系统格式的任务编号
            // 格式: NET-{项目编号}-{时间戳}-{随机数}
            string localTaskNo;
            if (!string.IsNullOrEmpty(networkTask.TaskNo) && networkTask.TaskNo.StartsWith("NET_"))
            {
                // 如果已经是NET_格式，转换为NET-XXX-XXX-XXX格式
                var parts = networkTask.TaskNo.Split('_');
                if (parts.Length >= 2)
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd");
                    var random = new Random().Next(100, 999).ToString();
                    localTaskNo = $"NET-{parts[1]}-{timestamp}-{random}";
                }
                else
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd");
                    var random = new Random().Next(100, 999).ToString();
                    localTaskNo = $"NET-{networkTask.ProjectNo ?? "PROJ"}-{timestamp}-{random}";
                }
            }
            else
            {
                // 生成新的网络任务编号
                var timestamp = DateTime.Now.ToString("yyyyMMdd");
                var random = new Random().Next(100, 999).ToString();
                localTaskNo = $"NET-{networkTask.ProjectNo ?? "PROJ"}-{timestamp}-{random}";
            }

            var dictTask = new Dictionary<string, object>
            {
                ["TaskNo"] = localTaskNo,
                ["TaskRound"] = "Round1",
                ["TaskName"] = networkTask.TaskName,
                ["CANRoad"] = "CAN1",
                ["Module"] = moduleList,  // 使用列表格式，与原有代码兼容
                ["TestType"] = networkTask.ToolType,
                ["CreateTime"] = DateTime.Now.ToString("yyyy-MM-dd"),
                ["Creater"] = "Network",
                ["AuthTester"] = GlobalVar.UserName,
                ["AuthorizedFromDept"] = GlobalVar.UserDept,
                ["Supplier"] = "",
                ["AuthorizationTime"] = DateTime.Now.ToString("yyyy-MM-dd"),
                ["InvalidTime"] = DateTime.Now.AddYears(20).ToString("yyyy-MM-dd"),
                ["Remark"] = $"来自网络任务: {networkTask.ProjectNo}",
                ["IsNetworkTask"] = true,
                ["NetworkTaskData"] = networkTask,
                ["ConfigPath"] = networkTask.ConfigPath,
                ["TestItems"] = networkTask.TestItems,
                ["Variables"] = networkTask.Variables,
                ["Timeout"] = networkTask.Timeout
            };

            return dictTask;
        }

        /// <summary>
        /// 将本地结果转换为网络结果格式
        /// </summary>
        public TaskResult ConvertToNetworkResult(string taskNo, object localResult)
        {
            var result = new TaskResult
            {
                TaskNo = taskNo,
                Status = TaskStatus.Completed,
                CompletedAt = DateTime.Now
            };

            // TODO: 根据实际本地结果格式进行转换
            // 这里需要根据 UltraANetT 的实际结果格式进行调整

            return result;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            try
            {
                StopAsync().Wait(TimeSpan.FromSeconds(5));
                _httpServer?.Dispose();
                _resultReporter?.Dispose();
                _executor?.Dispose();
            }
            catch
            {
                // 忽略释放时的异常
            }
        }
    }
}
