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