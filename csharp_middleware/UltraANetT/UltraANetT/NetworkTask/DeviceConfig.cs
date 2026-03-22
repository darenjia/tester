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