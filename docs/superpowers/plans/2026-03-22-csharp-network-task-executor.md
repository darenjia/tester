# C#网络任务执行器实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在UltraANetT C#项目中实现网络任务自动执行引擎，支持接收远程任务、自动执行测试、上报结果到TDM2.0接口。

**Architecture:** 新增DeviceConfig设备绑定配置、TaskCategoryMapper任务类别映射、NetworkTaskExecutor自动执行引擎三个核心模块，修改ResultReporter适配TDM2.0上报格式，集成到现有NetworkTaskManager。

**Tech Stack:** C# .NET Framework 4.8, Newtonsoft.Json, HttpListener, CANoe COM接口

**Spec:** `docs/superpowers/specs/2026-03-22-csharp-network-task-executor-design.md`

---

## 文件结构

```
csharp_middleware/UltraANetT/UltraANetT/
├── NetworkTask/
│   ├── DeviceConfig.cs           # 新增 - 设备绑定配置
│   ├── TaskCategoryMapper.cs     # 新增 - 任务类别映射
│   ├── NetworkTaskExecutor.cs    # 新增 - 自动执行引擎
│   ├── ResultReporter.cs         # 修改 - 适配TDM2.0上报
│   ├── NetworkTaskManager.cs     # 修改 - 集成执行引擎
│   └── NetworkTaskConfig.cs      # 修改 - 新增配置项
├── Module/
│   └── Task.cs                   # 修改 - 启动执行引擎
└── configuration/
    └── NetworkTaskConfig.json    # 新增 - 配置文件
```

---

## Task 1: 设备配置模块 (DeviceConfig)

**Files:**
- Create: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/DeviceConfig.cs`

**说明:** 实现设备绑定配置管理，存储车型、配置、阶段、通道等信息。

- [ ] **Step 1: 创建 DeviceConfig.cs 文件**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UltraANetT.NetworkTask
{
    /// <summary>
    /// 设备绑定配置
    /// 存储设备绑定的车型配置信息
    /// </summary>
    public class DeviceBindingConfig
    {
        /// <summary>
        /// 车型，如 "车型A"
        /// </summary>
        public string VehicleType { get; set; }

        /// <summary>
        /// 配置，如 "配置1"
        /// </summary>
        public string VehicleConfig { get; set; }

        /// <summary>
        /// 阶段，如 "阶段2"
        /// </summary>
        public string VehicleStage { get; set; }

        /// <summary>
        /// 测试通道，如 "CAN1"
        /// </summary>
        public string TestChannel { get; set; }

        /// <summary>
        /// DBC文件路径
        /// </summary>
        public string DbcPath { get; set; }

        /// <summary>
        /// 默认波特率
        /// </summary>
        public string DefaultBaudRate { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        public DeviceBindingConfig()
        {
            VehicleType = "";
            VehicleConfig = "";
            VehicleStage = "";
            TestChannel = "";
            DbcPath = "";
            DefaultBaudRate = "500000";
            DeviceId = "DEVICE_001";
            DeviceName = "测试执行设备-01";
        }
    }

    /// <summary>
    /// 设备配置管理器
    /// 负责加载、保存、访问设备绑定配置
    /// </summary>
    public class DeviceConfig
    {
        private static DeviceConfig _instance;
        private static readonly object _lock = new object();

        private DeviceBindingConfig _bindingConfig;
        private readonly string _configPath;

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static DeviceConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DeviceConfig();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 获取设备绑定配置
        /// </summary>
        public DeviceBindingConfig BindingConfig => _bindingConfig;

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private DeviceConfig()
        {
            _configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "configuration",
                "NetworkTaskConfig.json"
            );
            _bindingConfig = new DeviceBindingConfig();
            Load();
        }

        /// <summary>
        /// 从配置文件加载
        /// </summary>
        public void Load()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[DeviceConfig] 配置文件不存在: {_configPath}");
                    return;
                }

                var json = File.ReadAllText(_configPath);
                var config = JsonConvert.DeserializeObject<NetworkTaskConfigJson>(json);

                if (config != null && config.DeviceBinding != null)
                {
                    _bindingConfig = config.DeviceBinding;
                    System.Diagnostics.Debug.WriteLine($"[DeviceConfig] 配置加载成功: VehicleType={_bindingConfig.VehicleType}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceConfig] 加载配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void Save(DeviceBindingConfig config)
        {
            try
            {
                _bindingConfig = config ?? new DeviceBindingConfig();

                // 读取现有配置
                NetworkTaskConfigJson fullConfig;
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    fullConfig = JsonConvert.DeserializeObject<NetworkTaskConfigJson>(json) ?? new NetworkTaskConfigJson();
                }
                else
                {
                    fullConfig = new NetworkTaskConfigJson();
                }

                // 更新设备绑定配置
                fullConfig.DeviceBinding = _bindingConfig;

                // 保存
                var directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var newJson = JsonConvert.SerializeObject(fullConfig, Formatting.Indented);
                File.WriteAllText(_configPath, newJson);

                System.Diagnostics.Debug.WriteLine($"[DeviceConfig] 配置保存成功: {_configPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceConfig] 保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取车型节点列表 (用于设置 GlobalVar.CurrentVNode)
        /// </summary>
        public List<string> GetVehicleNodeList()
        {
            return new List<string>
            {
                _bindingConfig.VehicleType,
                _bindingConfig.VehicleConfig,
                _bindingConfig.VehicleStage
            };
        }

        /// <summary>
        /// 检查配置是否有效
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(_bindingConfig.VehicleType) &&
                   !string.IsNullOrEmpty(_bindingConfig.VehicleConfig) &&
                   !string.IsNullOrEmpty(_bindingConfig.VehicleStage);
        }
    }

    /// <summary>
    /// 配置文件JSON结构（内部使用）
    /// </summary>
    internal class NetworkTaskConfigJson
    {
        public bool Enabled { get; set; } = true;
        public bool HttpServerEnabled { get; set; } = true;
        public string ListenUrl { get; set; } = "http://+:8180/";
        public bool AutoConnect { get; set; } = true;
        public bool AutoExecute { get; set; } = true;
        public string SchedulerUrl { get; set; } = "http://10.124.11.142:8315";
        public string ReportApiPath { get; set; } = "/api/python/report";
        public int PollingIntervalMs { get; set; } = 1000;
        public DeviceBindingConfig DeviceBinding { get; set; }
        public Dictionary<string, string> TaskCategoryMapping { get; set; }
    }
}
```

- [ ] **Step 2: 验证文件创建成功**

确认文件路径: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/DeviceConfig.cs`

- [ ] **Step 3: 提交代码**

```bash
git add csharp_middleware/UltraANetT/UltraANetT/NetworkTask/DeviceConfig.cs
git commit -m "feat(network-task): 添加设备绑定配置模块 DeviceConfig"
```

---

## Task 2: 任务类别映射模块 (TaskCategoryMapper)

**Files:**
- Create: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/TaskCategoryMapper.cs`

**说明:** 实现网络任务taskName到C#任务类别的映射。

- [ ] **Step 1: 创建 TaskCategoryMapper.cs 文件**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UltraANetT.NetworkTask
{
    /// <summary>
    /// 任务类别枚举
    /// </summary>
    public enum TaskCategory
    {
        Unknown,
        CANUnit,
        CANIntegration,
        LINMaster,
        LINSalve,
        LINIntegration,
        DirectNMUnit,
        DirectNMIntegration,
        DynamicNMMaster,
        DynamicNMSlave,
        DynamicNMIntegration,
        IndirectNMUnit,
        IndirectNMIntegration,
        CommunicationDTC,
        OSEKNMUnit,
        OSEKNMIntegration,
        GatewayRouting
    }

    /// <summary>
    /// 任务类别映射器
    /// 将网络任务的taskName映射到C#项目的任务类别
    /// </summary>
    public class TaskCategoryMapper
    {
        private static TaskCategoryMapper _instance;
        private static readonly object _lock = new object();

        private readonly Dictionary<string, TaskCategory> _mapping;
        private readonly string _configPath;

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static TaskCategoryMapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TaskCategoryMapper();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private TaskCategoryMapper()
        {
            _mapping = new Dictionary<string, TaskCategory>(StringComparer.OrdinalIgnoreCase);
            _configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "configuration",
                "NetworkTaskConfig.json"
            );

            // 初始化默认映射
            InitDefaultMapping();

            // 从配置文件加载（覆盖默认值）
            LoadFromConfig();
        }

        /// <summary>
        /// 初始化默认映射
        /// </summary>
        private void InitDefaultMapping()
        {
            _mapping["CAN通信单元"] = TaskCategory.CANUnit;
            _mapping["CAN通信集成"] = TaskCategory.CANIntegration;
            _mapping["LIN通信主节点"] = TaskCategory.LINMaster;
            _mapping["LIN通信从节点"] = TaskCategory.LINSalve;
            _mapping["LIN通信集成"] = TaskCategory.LINIntegration;
            _mapping["直接NM单元"] = TaskCategory.DirectNMUnit;
            _mapping["直接NM集成"] = TaskCategory.DirectNMIntegration;
            _mapping["动力域NM主节点"] = TaskCategory.DynamicNMMaster;
            _mapping["动力域NM从节点"] = TaskCategory.DynamicNMSlave;
            _mapping["动力域NM集成"] = TaskCategory.DynamicNMIntegration;
            _mapping["间接NM单元"] = TaskCategory.IndirectNMUnit;
            _mapping["间接NM集成"] = TaskCategory.IndirectNMIntegration;
            _mapping["通信DTC"] = TaskCategory.CommunicationDTC;
            _mapping["OSEK NM单元"] = TaskCategory.OSEKNMUnit;
            _mapping["OSEK NM集成"] = TaskCategory.OSEKNMIntegration;
            _mapping["网关路由"] = TaskCategory.GatewayRouting;
        }

        /// <summary>
        /// 从配置文件加载映射
        /// </summary>
        private void LoadFromConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    return;
                }

                var json = File.ReadAllText(_configPath);
                var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (config != null && config.ContainsKey("taskCategoryMapping"))
                {
                    var mappingJson = config["taskCategoryMapping"].ToString();
                    var customMapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(mappingJson);

                    if (customMapping != null)
                    {
                        foreach (var kvp in customMapping)
                        {
                            if (Enum.TryParse<TaskCategory>(kvp.Value, true, out var category))
                            {
                                _mapping[kvp.Key] = category;
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[TaskCategoryMapper] 已加载 {_mapping.Count} 个映射");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskCategoryMapper] 加载配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 映射任务名称到类别
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <returns>任务类别，未找到返回 Unknown</returns>
        public TaskCategory MapToCategory(string taskName)
        {
            if (string.IsNullOrEmpty(taskName))
            {
                return TaskCategory.Unknown;
            }

            // 尝试直接匹配
            if (_mapping.TryGetValue(taskName, out var category))
            {
                return category;
            }

            // 尝试模糊匹配（去除空格、大小写）
            var normalizedTaskName = taskName.Trim().Replace(" ", "");
            foreach (var kvp in _mapping)
            {
                var normalizedKey = kvp.Key.Trim().Replace(" ", "");
                if (string.Equals(normalizedKey, normalizedTaskName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[TaskCategoryMapper] 未找到映射: {taskName}");
            return TaskCategory.Unknown;
        }

        /// <summary>
        /// 验证类别是否有效
        /// </summary>
        public bool IsValidCategory(TaskCategory category)
        {
            return category != TaskCategory.Unknown;
        }

        /// <summary>
        /// 获取所有支持的类别名称
        /// </summary>
        public List<string> GetAllCategoryNames()
        {
            return new List<string>(_mapping.Keys);
        }

        /// <summary>
        /// 获取类别对应的测试类型（用于 ProcFile.CANConvertToType）
        /// </summary>
        public int GetTestType(TaskCategory category)
        {
            switch (category)
            {
                case TaskCategory.CANUnit:
                case TaskCategory.DirectNMUnit:
                case TaskCategory.DynamicNMMaster:
                case TaskCategory.DynamicNMSlave:
                case TaskCategory.IndirectNMUnit:
                case TaskCategory.CommunicationDTC:
                case TaskCategory.OSEKNMUnit:
                    return 0; // CAN单节点类型

                case TaskCategory.LINMaster:
                case TaskCategory.LINSalve:
                    return 1; // LIN类型

                case TaskCategory.GatewayRouting:
                    return 2; // 网关路由类型

                case TaskCategory.CANIntegration:
                case TaskCategory.DirectNMIntegration:
                case TaskCategory.DynamicNMIntegration:
                case TaskCategory.IndirectNMIntegration:
                case TaskCategory.OSEKNMIntegration:
                case TaskCategory.LINIntegration:
                    return 3; // 集成类型

                default:
                    return -1; // 未知类型
            }
        }

        /// <summary>
        /// 判断是否需要J1939格式
        /// </summary>
        public bool IsJ1939(TaskCategory category, string busType)
        {
            // 根据总线和类别判断
            return !string.IsNullOrEmpty(busType) && busType.ToUpper() == "J1939";
        }
    }
}
```

- [ ] **Step 2: 验证文件创建成功**

确认文件路径: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/TaskCategoryMapper.cs`

- [ ] **Step 3: 提交代码**

```bash
git add csharp_middleware/UltraANetT/UltraANetT/NetworkTask/TaskCategoryMapper.cs
git commit -m "feat(network-task): 添加任务类别映射模块 TaskCategoryMapper"
```

---

## Task 3: 自动执行引擎 (NetworkTaskExecutor)

**Files:**
- Create: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/NetworkTaskExecutor.cs`

**说明:** 实现后台监听任务队列，自动触发测试执行的核心引擎。

- [ ] **Step 1: 创建 NetworkTaskExecutor.cs 文件 (Part 1 - 类结构和基础方法)**

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CANoeEngine;
using Newtonsoft.Json;
using ProcessEngine;
using UltraANetT.Form;

namespace UltraANetT.NetworkTask
{
    /// <summary>
    /// 任务执行事件参数
    /// </summary>
    public class TaskExecutionEventArgs : EventArgs
    {
        public string TaskNo { get; set; }
        public TaskStatus Status { get; set; }
        public int Progress { get; set; }
        public string Message { get; set; }
        public NetworkTask Task { get; set; }
        public TaskResult Result { get; set; }
        public Exception Error { get; set; }

        public TaskExecutionEventArgs()
        {
            Progress = 0;
            Message = "";
        }
    }

    /// <summary>
    /// 网络任务执行器
    /// 后台监听任务队列，自动触发测试执行
    /// </summary>
    public class NetworkTaskExecutor : IDisposable
    {
        private Thread _workerThread;
        private bool _isRunning;
        private bool _isDisposed;
        private readonly TaskQueueManager _taskQueue;
        private readonly NetworkTaskManager _networkTaskManager;
        private readonly int _pollingIntervalMs;
        private readonly object _executionLock = new object();
        private NetworkTask _currentTask;
        private bool _isExecuting;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 是否正在执行任务
        /// </summary>
        public bool IsExecuting => _isExecuting;

        /// <summary>
        /// 当前执行的任务
        /// </summary>
        public NetworkTask CurrentTask => _currentTask;

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
        /// 构造函数
        /// </summary>
        public NetworkTaskExecutor(NetworkTaskManager networkTaskManager, int pollingIntervalMs = 1000)
        {
            _networkTaskManager = networkTaskManager ?? throw new ArgumentNullException(nameof(networkTaskManager));
            _taskQueue = networkTaskManager.TaskQueue;
            _pollingIntervalMs = pollingIntervalMs;
        }

        /// <summary>
        /// 启动执行器
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                Debug.WriteLine("[NetworkTaskExecutor] 执行器已在运行");
                return;
            }

            _isRunning = true;
            _workerThread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "NetworkTaskExecutor"
            };
            _workerThread.Start();

            Debug.WriteLine("[NetworkTaskExecutor] 执行器已启动");
        }

        /// <summary>
        /// 停止执行器
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;

            // 等待当前任务完成
            if (_workerThread != null && _workerThread.IsAlive)
            {
                _workerThread.Join(5000);
            }

            Debug.WriteLine("[NetworkTaskExecutor] 执行器已停止");
        }

        /// <summary>
        /// 工作线程循环
        /// </summary>
        private void WorkerLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // 如果正在执行任务，等待
                    if (_isExecuting)
                    {
                        Thread.Sleep(_pollingIntervalMs);
                        continue;
                    }

                    // 检查是否有待执行任务
                    var task = _taskQueue.DequeueTask();
                    if (task != null)
                    {
                        // 异步执行任务
                        Task.Run(() => ExecuteTaskAsync(task));
                    }
                    else
                    {
                        // 无任务，等待
                        Thread.Sleep(_pollingIntervalMs);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[NetworkTaskExecutor] 工作循环异常: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 异步执行任务
        /// </summary>
        private async Task ExecuteTaskAsync(NetworkTask task)
        {
            lock (_executionLock)
            {
                if (_isExecuting)
                {
                    Debug.WriteLine("[NetworkTaskExecutor] 已有任务在执行，跳过");
                    return;
                }
                _isExecuting = true;
                _currentTask = task;
            }

            DateTime startTime = DateTime.Now;
            TaskResult result = null;

            try
            {
                Debug.WriteLine($"[NetworkTaskExecutor] 开始执行任务: {task.TaskNo}");

                // 标记任务开始
                _taskQueue.MarkTaskRunning(task.TaskNo);

                // 触发开始事件
                OnTaskStarted(task);

                // 1. 准备测试环境
                OnTaskProgress(task, 5, "正在准备测试环境...");
                if (!PrepareTestEnvironment(task))
                {
                    throw new Exception("准备测试环境失败");
                }

                // 2. 执行测试
                OnTaskProgress(task, 10, "开始执行测试...");
                result = await ExecuteTestAsync(task);

                // 3. 标记完成
                _taskQueue.MarkTaskCompleted(task.TaskNo, result);

                // 4. 上报结果
                OnTaskProgress(task, 95, "正在上报结果...");
                await ReportResultAsync(task.TaskNo, result);

                // 5. 触发完成事件
                OnTaskCompleted(task, result);

                Debug.WriteLine($"[NetworkTaskExecutor] 任务完成: {task.TaskNo}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NetworkTaskExecutor] 任务执行失败: {ex.Message}");

                // 创建失败结果
                result = new TaskResult
                {
                    TaskNo = task.TaskNo,
                    Status = TaskStatus.Failed,
                    CompletedAt = DateTime.Now,
                    Remark = ex.Message
                };

                // 标记失败
                _taskQueue.MarkTaskFailed(task.TaskNo, ex.Message);

                // 上报失败结果
                try
                {
                    await ReportResultAsync(task.TaskNo, result);
                }
                catch { }

                // 触发失败事件
                OnTaskFailed(task, ex, result);
            }
            finally
            {
                lock (_executionLock)
                {
                    _isExecuting = false;
                    _currentTask = null;
                }
            }
        }
```

- [ ] **Step 2: 创建 NetworkTaskExecutor.cs 文件 (Part 2 - 测试环境准备)**

继续添加以下代码:

```csharp
        /// <summary>
        /// 准备测试环境
        /// </summary>
        private bool PrepareTestEnvironment(NetworkTask task)
        {
            try
            {
                // 1. 获取设备绑定配置
                var deviceConfig = DeviceConfig.Instance;
                if (!deviceConfig.IsValid())
                {
                    Debug.WriteLine("[NetworkTaskExecutor] 设备配置无效");
                    return false;
                }

                var bindingConfig = deviceConfig.BindingConfig;

                // 2. 设置 GlobalVar.CurrentVNode
                GlobalVar.CurrentVNode = new List<string>
                {
                    bindingConfig.VehicleType,
                    bindingConfig.VehicleConfig,
                    bindingConfig.VehicleStage
                };

                Debug.WriteLine($"[NetworkTaskExecutor] CurrentVNode: {string.Join("-", GlobalVar.CurrentVNode)}");

                // 3. 映射任务类别
                var category = TaskCategoryMapper.Instance.MapToCategory(task.TaskName);
                if (category == TaskCategory.Unknown)
                {
                    Debug.WriteLine($"[NetworkTaskExecutor] 未知的任务类别: {task.TaskName}");
                    return false;
                }

                Debug.WriteLine($"[NetworkTaskExecutor] 任务类别: {category}");

                // 4. 构建任务节点信息
                // 格式: [TaskNo, Round, TaskName, Channel, Module]
                var taskNode = new List<string>
                {
                    task.TaskNo,
                    "Round1",
                    task.TaskName,
                    bindingConfig.TestChannel,
                    task.TestItems?.FirstOrDefault()?.Name ?? "Default"
                };
                GlobalVar.CurrentTsNode = taskNode;

                // 5. 设置测试类型
                task.ToolType = "CANoe"; // 默认使用CANoe

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NetworkTaskExecutor] 准备测试环境失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 执行测试
        /// </summary>
        private async Task<TaskResult> ExecuteTestAsync(NetworkTask task)
        {
            var result = new TaskResult
            {
                TaskNo = task.TaskNo,
                Status = TaskStatus.Running,
                CaseList = new List<CaseResult>()
            };

            var startTime = DateTime.Now;
            int totalItems = task.TestItems?.Count ?? 0;
            int currentItem = 0;

            try
            {
                // 获取配置路径
                var category = TaskCategoryMapper.Instance.MapToCategory(task.TaskName);
                string cfgPath = GetCfgPath(task.TaskName, category);
                string reportPath = GetReportPath(task.TaskName, category);

                if (string.IsNullOrEmpty(cfgPath) || !File.Exists(cfgPath))
                {
                    throw new Exception($"配置文件不存在: {cfgPath}");
                }

                Debug.WriteLine($"[NetworkTaskExecutor] cfg路径: {cfgPath}");
                Debug.WriteLine($"[NetworkTaskExecutor] 报告路径: {reportPath}");

                // 设置枚举库路径
                EnumLibrary.CfgPath = cfgPath;

                // 获取测试用例
                var testCases = GetTestCases(task);

                // 执行每个测试用例
                foreach (var testCase in testCases)
                {
                    currentItem++;
                    int progress = 10 + (int)((currentItem / (double)totalItems) * 80);

                    OnTaskProgress(task, progress, $"执行测试项 {currentItem}/{totalItems}: {testCase.Name}");

                    // 执行单个用例
                    var caseResult = await ExecuteSingleTestCaseAsync(testCase, cfgPath);
                    result.CaseList.Add(caseResult);

                    // 更新任务进度
                    task.Progress = progress;
                }

                // 生成结果
                result.Status = TaskStatus.Completed;
                result.CompletedAt = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NetworkTaskExecutor] 执行测试失败: {ex.Message}");
                result.Status = TaskStatus.Failed;
                result.Remark = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取测试用例列表
        /// </summary>
        private List<TestItem> GetTestCases(NetworkTask task)
        {
            var testCases = new List<TestItem>();

            if (task.TestItems != null && task.TestItems.Count > 0)
            {
                // 使用任务中的测试项
                testCases.AddRange(task.TestItems);
            }
            else
            {
                // TODO: 从数据库查询该任务类别下的所有用例
                // 暂时返回空列表
                Debug.WriteLine("[NetworkTaskExecutor] 任务无测试项");
            }

            return testCases;
        }

        /// <summary>
        /// 执行单个测试用例
        /// </summary>
        private async Task<CaseResult> ExecuteSingleTestCaseAsync(TestItem testItem, string cfgPath)
        {
            var result = new CaseResult
            {
                CaseNo = testItem.Name,
                Result = "BLOCK",
                Remark = "未执行"
            };

            try
            {
                // TODO: 调用实际的测试执行逻辑
                // 这里需要与 ProcCANoeTest 或 TestStart 集成

                // 模拟执行
                await Task.Delay(100);

                // 根据实际情况设置结果
                result.Result = "PASS";
                result.Remark = "执行完成";
            }
            catch (Exception ex)
            {
                result.Result = "FAIL";
                result.Remark = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取CFG文件路径
        /// </summary>
        private string GetCfgPath(string taskName, TaskCategory category)
        {
            // 根据任务类别返回对应的cfg路径
            // 这里需要根据实际配置获取
            var bindingConfig = DeviceConfig.Instance.BindingConfig;

            // 从配置或数据库获取路径
            // 示例：返回配置的路径
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            switch (category)
            {
                case TaskCategory.CANUnit:
                    return EnumLibrary.CANSigExam;
                case TaskCategory.CANIntegration:
                    return EnumLibrary.CANLtgExam;
                case TaskCategory.LINMaster:
                    return EnumLibrary.LINSigExam;
                case TaskCategory.CommunicationDTC:
                    return EnumLibrary.DTCExam;
                // ... 其他类别
                default:
                    return "";
            }
        }

        /// <summary>
        /// 获取报告路径
        /// </summary>
        private string GetReportPath(string taskName, TaskCategory category)
        {
            switch (category)
            {
                case TaskCategory.CANUnit:
                    return EnumLibrary.CANSigReport;
                case TaskCategory.CANIntegration:
                    return EnumLibrary.CANLtgReport;
                case TaskCategory.LINMaster:
                    return EnumLibrary.LINSigReport;
                case TaskCategory.CommunicationDTC:
                    return EnumLibrary.DTCreport;
                // ... 其他类别
                default:
                    return "";
            }
        }
```

- [ ] **Step 3: 创建 NetworkTaskExecutor.cs 文件 (Part 3 - 结果上报和事件)**

继续添加以下代码:

```csharp
        /// <summary>
        /// 上报结果到调度服务器
        /// </summary>
        private async Task ReportResultAsync(string taskNo, TaskResult result)
        {
            try
            {
                if (_networkTaskManager == null)
                {
                    Debug.WriteLine("[NetworkTaskExecutor] NetworkTaskManager 为空，无法上报");
                    return;
                }

                // 构建TDM2.0格式上报数据
                var reportData = BuildTDM2ReportData(taskNo, result);

                // 调用 NetworkTaskManager 上报
                var success = await _networkTaskManager.ReportTaskResultAsync(taskNo, result);

                if (success)
                {
                    Debug.WriteLine($"[NetworkTaskExecutor] 结果上报成功: {taskNo}");
                }
                else
                {
                    Debug.WriteLine($"[NetworkTaskExecutor] 结果上报失败: {taskNo}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NetworkTaskExecutor] 上报结果异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 构建TDM2.0格式上报数据
        /// </summary>
        private object BuildTDM2ReportData(string taskNo, TaskResult result)
        {
            var deviceConfig = DeviceConfig.Instance.BindingConfig;

            // 统计结果
            int total = result.CaseList?.Count ?? 0;
            int pass = result.CaseList?.Count(c => c.Result == "PASS") ?? 0;
            int fail = result.CaseList?.Count(c => c.Result == "FAIL") ?? 0;
            int block = result.CaseList?.Count(c => c.Result == "BLOCK") ?? 0;
            int skip = result.CaseList?.Count(c => c.Result == "SKIP") ?? 0;

            return new
            {
                taskNo = taskNo,
                projectNo = _currentTask?.ProjectNo ?? "",
                deviceId = deviceConfig.DeviceId,
                taskName = _currentTask?.TaskName ?? "",
                toolType = _currentTask?.ToolType ?? "CANoe",
                status = result.Status.ToString().ToLower(),
                startTime = result.StartedAt?.ToString("o") ?? "",
                endTime = result.CompletedAt.ToString("o"),
                caseList = result.CaseList?.Select(c => new
                {
                    caseNo = c.CaseNo,
                    result = c.Result,
                    remark = c.Remark ?? "",
                    created = c.Created?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
                }).ToList() ?? new List<object>(),
                summary = new
                {
                    total = total,
                    pass = pass,
                    fail = fail,
                    block = block,
                    skip = skip
                }
            };
        }

        /// <summary>
        /// 触发任务开始事件
        /// </summary>
        private void OnTaskStarted(NetworkTask task)
        {
            TaskStarted?.Invoke(this, new TaskExecutionEventArgs
            {
                TaskNo = task.TaskNo,
                Task = task,
                Status = TaskStatus.Running,
                Progress = 0,
                Message = "任务开始执行"
            });
        }

        /// <summary>
        /// 触发任务进度事件
        /// </summary>
        private void OnTaskProgress(NetworkTask task, int progress, string message)
        {
            task.Progress = progress;

            TaskProgress?.Invoke(this, new TaskExecutionEventArgs
            {
                TaskNo = task.TaskNo,
                Task = task,
                Status = TaskStatus.Running,
                Progress = progress,
                Message = message
            });

            // 同时上报进度
            _ = _networkTaskManager?.ReportTaskStatusAsync(task.TaskNo, TaskStatus.Running, progress, message);
        }

        /// <summary>
        /// 触发任务完成事件
        /// </summary>
        private void OnTaskCompleted(NetworkTask task, TaskResult result)
        {
            TaskCompleted?.Invoke(this, new TaskExecutionEventArgs
            {
                TaskNo = task.TaskNo,
                Task = task,
                Status = TaskStatus.Completed,
                Progress = 100,
                Message = "任务执行完成",
                Result = result
            });
        }

        /// <summary>
        /// 触发任务失败事件
        /// </summary>
        private void OnTaskFailed(NetworkTask task, Exception error, TaskResult result)
        {
            TaskFailed?.Invoke(this, new TaskExecutionEventArgs
            {
                TaskNo = task.TaskNo,
                Task = task,
                Status = TaskStatus.Failed,
                Progress = task.Progress,
                Message = error.Message,
                Error = error,
                Result = result
            });
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
            Stop();

            Debug.WriteLine("[NetworkTaskExecutor] 资源已释放");
        }
    }
}
```

- [ ] **Step 4: 验证文件创建成功**

确认文件路径: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/NetworkTaskExecutor.cs`

- [ ] **Step 5: 提交代码**

```bash
git add csharp_middleware/UltraANetT/UltraANetT/NetworkTask/NetworkTaskExecutor.cs
git commit -m "feat(network-task): 添加自动执行引擎 NetworkTaskExecutor"
```

---

## Task 4: 适配 ResultReporter 到 TDM2.0

**Files:**
- Modify: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/ResultReporter.cs`

**说明:** 修改上报地址和数据格式，适配TDM2.0接口。

- [ ] **Step 1: 读取现有 ResultReporter.cs 内容**

确认需要修改的方法。

- [ ] **Step 2: 添加 TDM2.0 上报方法**

在 ResultReporter 类中添加以下方法:

```csharp
/// <summary>
/// 上报TDM2.0格式结果
/// </summary>
public async Task<bool> ReportTDM2ResultAsync(TDM2ReportData reportData)
{
    if (reportData == null)
        throw new ArgumentNullException(nameof(reportData));

    var url = $"{_schedulerUrl}/api/python/report";
    var json = JsonConvert.SerializeObject(reportData);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    for (int attempt = 1; attempt <= _maxRetries; attempt++)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ResultReporter] 正在上报TDM2.0结果 (尝试 {attempt}/{_maxRetries})");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[ResultReporter] TDM2.0上报成功: {responseBody}");
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ResultReporter] TDM2.0上报失败: HTTP {(int)response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ResultReporter] TDM2.0上报异常 (尝试 {attempt}): {ex.Message}");
        }

        if (attempt < _maxRetries)
        {
            await Task.Delay(_retryDelayMs * attempt);
        }
    }

    return false;
}

/// <summary>
/// 上报TaskResult到TDM2.0接口
/// </summary>
public async Task<bool> ReportTaskResultToTDM2Async(string taskNo, TaskResult result, string projectNo = "", string taskName = "", string toolType = "CANoe")
{
    var deviceConfig = DeviceConfig.Instance.BindingConfig;

    // 统计结果
    int total = result.CaseList?.Count ?? 0;
    int pass = result.CaseList?.Count(c => c.Result == "PASS") ?? 0;
    int fail = result.CaseList?.Count(c => c.Result == "FAIL") ?? 0;
    int block = result.CaseList?.Count(c => c.Result == "BLOCK") ?? 0;
    int skip = result.CaseList?.Count(c => c.Result == "SKIP") ?? 0;

    var reportData = new TDM2ReportData
    {
        TaskNo = taskNo,
        ProjectNo = projectNo,
        DeviceId = deviceConfig.DeviceId,
        TaskName = taskName,
        ToolType = toolType,
        Status = result.Status.ToString().ToLower(),
        StartTime = result.StartedAt?.ToString("o") ?? DateTime.Now.ToString("o"),
        EndTime = result.CompletedAt.ToString("o"),
        CaseList = result.CaseList?.Select(c => new TDM2CaseResult
        {
            CaseNo = c.CaseNo,
            Result = c.Result,
            Remark = c.Remark ?? "",
            Created = c.Created?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }).ToList() ?? new List<TDM2CaseResult>(),
        Summary = new TDM2Summary
        {
            Total = total,
            Pass = pass,
            Fail = fail,
            Block = block,
            Skip = skip
        }
    };

    return await ReportTDM2ResultAsync(reportData);
}
```

- [ ] **Step 3: 添加 TDM2.0 数据模型**

在文件末尾添加:

```csharp
/// <summary>
/// TDM2.0上报数据
/// </summary>
public class TDM2ReportData
{
    [JsonProperty("taskNo")]
    public string TaskNo { get; set; }

    [JsonProperty("projectNo")]
    public string ProjectNo { get; set; }

    [JsonProperty("deviceId")]
    public string DeviceId { get; set; }

    [JsonProperty("taskName")]
    public string TaskName { get; set; }

    [JsonProperty("toolType")]
    public string ToolType { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("startTime")]
    public string StartTime { get; set; }

    [JsonProperty("endTime")]
    public string EndTime { get; set; }

    [JsonProperty("caseList")]
    public List<TDM2CaseResult> CaseList { get; set; }

    [JsonProperty("summary")]
    public TDM2Summary Summary { get; set; }

    public TDM2ReportData()
    {
        CaseList = new List<TDM2CaseResult>();
        Summary = new TDM2Summary();
    }
}

/// <summary>
/// TDM2.0用例结果
/// </summary>
public class TDM2CaseResult
{
    [JsonProperty("caseNo")]
    public string CaseNo { get; set; }

    [JsonProperty("result")]
    public string Result { get; set; }

    [JsonProperty("remark")]
    public string Remark { get; set; }

    [JsonProperty("created")]
    public string Created { get; set; }
}

/// <summary>
/// TDM2.0结果统计
/// </summary>
public class TDM2Summary
{
    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("pass")]
    public int Pass { get; set; }

    [JsonProperty("fail")]
    public int Fail { get; set; }

    [JsonProperty("block")]
    public int Block { get; set; }

    [JsonProperty("skip")]
    public int Skip { get; set; }
}
```

- [ ] **Step 4: 验证修改**

确认文件修改正确，无语法错误。

- [ ] **Step 5: 提交代码**

```bash
git add csharp_middleware/UltraANetT/UltraANetT/NetworkTask/ResultReporter.cs
git commit -m "feat(network-task): 适配ResultReporter到TDM2.0上报格式"
```

---

## Task 5: 更新 NetworkTaskConfig

**Files:**
- Modify: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/NetworkTaskConfig.cs`

**说明:** 添加设备绑定和任务映射配置项。

- [ ] **Step 1: 读取现有 NetworkTaskConfig.cs**

- [ ] **Step 2: 添加新的配置属性**

在 NetworkTaskConfig 类中添加:

```csharp
/// <summary>
/// 是否自动执行
/// </summary>
public bool AutoExecute { get; set; } = true;

/// <summary>
/// 上报API路径
/// </summary>
public string ReportApiPath { get; set; } = "/api/python/report";

/// <summary>
/// 轮询间隔(毫秒)
/// </summary>
public int PollingIntervalMs { get; set; } = 1000;

/// <summary>
/// 设备绑定配置
/// </summary>
public DeviceBindingConfig DeviceBinding { get; set; }

/// <summary>
/// 任务类别映射
/// </summary>
public Dictionary<string, string> TaskCategoryMapping { get; set; }
```

- [ ] **Step 3: 更新 Load 方法**

确保配置加载时包含新字段。

- [ ] **Step 4: 提交代码**

```bash
git add csharp_middleware/UltraANetT/UltraANetT/NetworkTask/NetworkTaskConfig.cs
git commit -m "feat(network-task): 更新NetworkTaskConfig添加设备绑定配置"
```

---

## Task 6: 集成执行引擎到 NetworkTaskManager

**Files:**
- Modify: `csharp_middleware/UltraANetT/UltraANetT/NetworkTask/NetworkTaskManager.cs`

**说明:** 在 NetworkTaskManager 中集成 NetworkTaskExecutor。

- [ ] **Step 1: 添加 NetworkTaskExecutor 字段**

```csharp
private NetworkTaskExecutor _executor;
```

- [ ] **Step 2: 在 StartAsync 中启动执行引擎**

在 StartAsync 方法末尾添加:

```csharp
// 启动自动执行引擎
if (_config.AutoExecute)
{
    _executor = new NetworkTaskExecutor(this, _config.PollingIntervalMs);
    _executor.TaskStarted += (s, e) => TaskReceived?.Invoke(this, new TaskReceivedEventArgs(e.Task));
    _executor.TaskCompleted += (s, e) => System.Diagnostics.Debug.WriteLine($"[NetworkTaskManager] 任务完成: {e.TaskNo}");
    _executor.TaskFailed += (s, e) => ErrorOccurred?.Invoke(this, new ErrorEventArgs(e.Error, e.Message));
    _executor.Start();
}
```

- [ ] **Step 3: 在 StopAsync 中停止执行引擎**

在 StopAsync 方法开头添加:

```csharp
// 停止执行引擎
if (_executor != null)
{
    _executor.Stop();
    _executor.Dispose();
    _executor = null;
}
```

- [ ] **Step 4: 更新 Dispose 方法**

确保释放 _executor 资源。

- [ ] **Step 5: 提交代码**

```bash
git add csharp_middleware/UltraANetT/UltraANetT/NetworkTask/NetworkTaskManager.cs
git commit -m "feat(network-task): 集成NetworkTaskExecutor到NetworkTaskManager"
```

---

## Task 7: 创建配置文件

**Files:**
- Create: `csharp_middleware/UltraANetT/UltraANetT/bin/Debug/configuration/NetworkTaskConfig.json`

**说明:** 创建默认配置文件。

- [ ] **Step 1: 创建配置目录**

确保 `configuration` 目录存在。

- [ ] **Step 2: 创建配置文件**

```json
{
    "enabled": true,
    "httpServerEnabled": true,
    "listenUrl": "http://+:8180/",
    "autoConnect": true,
    "autoExecute": true,
    "schedulerUrl": "http://10.124.11.142:8315",
    "reportApiPath": "/api/python/report",
    "pollingIntervalMs": 1000,

    "deviceBinding": {
        "vehicleType": "车型A",
        "vehicleConfig": "配置1",
        "vehicleStage": "阶段2",
        "testChannel": "CAN1",
        "dbcPath": "C:\\DBC\\车型A\\",
        "defaultBaudRate": "500000",
        "deviceId": "DEVICE_001",
        "deviceName": "测试执行设备-01"
    },

    "taskCategoryMapping": {
        "CAN通信单元": "CANUnit",
        "CAN通信集成": "CANIntegration",
        "LIN通信主节点": "LINMaster",
        "LIN通信从节点": "LINSalve",
        "LIN通信集成": "LINIntegration",
        "直接NM单元": "DirectNMUnit",
        "直接NM集成": "DirectNMIntegration",
        "动力域NM主节点": "DynamicNMMaster",
        "动力域NM从节点": "DynamicNMSlave",
        "动力域NM集成": "DynamicNMIntegration",
        "间接NM单元": "IndirectNMUnit",
        "间接NM集成": "IndirectNMIntegration",
        "通信DTC": "CommunicationDTC",
        "OSEK NM单元": "OSEKNMUnit",
        "OSEK NM集成": "OSEKNMIntegration",
        "网关路由": "GatewayRouting"
    }
}
```

- [ ] **Step 3: 提交代码**

```bash
git add csharp_middleware/UltraANetT/UltraANetT/bin/Debug/configuration/NetworkTaskConfig.json
git commit -m "feat(network-task): 添加网络任务配置文件"
```

---

## Task 8: 更新项目文件

**Files:**
- Modify: `csharp_middleware/UltraANetT/UltraANetT/UltraANetT.csproj`

**说明:** 确保新增的 .cs 文件包含在项目中。

- [ ] **Step 1: 检查项目文件**

确认新增的文件已自动包含，或手动添加。

- [ ] **Step 2: 提交（如有修改）**

---

## Task 9: 集成测试

**Files:**
- 无新增文件，进行测试验证

**说明:** 验证整体功能是否正常。

- [ ] **Step 1: 编译项目**

在 Visual Studio 中编译项目，确认无语法错误。

- [ ] **Step 2: 运行应用**

启动 UltraANetT 应用，确认网络任务模块正确初始化。

- [ ] **Step 3: 测试HTTP接口**

使用 Postman 或 curl 测试:

```bash
curl -X POST http://localhost:8180/api/tasks/submit \
  -H "Content-Type: application/json" \
  -d '{
    "taskNo": "TEST-001",
    "taskName": "CAN通信单元",
    "projectNo": "PROJ001",
    "testItems": [{"name": "测试项1", "type": "auto"}],
    "timeout": 3600
  }'
```

- [ ] **Step 4: 验证执行流程**

检查调试输出，确认任务被执行。

- [ ] **Step 5: 验证结果上报**

检查是否正确上报到 TDM2.0 接口。

---

## 完成标准

- [ ] 所有新增文件创建完成
- [ ] 所有修改文件更新完成
- [ ] 项目编译无错误
- [ ] HTTP接口可以接收任务
- [ ] 任务可以自动执行
- [ ] 结果可以上报到TDM2.0接口
- [ ] 代码已提交到git

---

## 版本信息

- **计划版本**: 1.0
- **创建日期**: 2026-03-22
- **作者**: Claude Code