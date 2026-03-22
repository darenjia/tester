using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace UltraANetT.NetworkTask
{
    /// <summary>
    /// 网络任务配置
    /// </summary>
    public class NetworkTaskConfig
    {
        private const string SectionName = "networkTask";

        /// <summary>
        /// 是否启用网络任务
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// 是否自动连接
        /// </summary>
        public bool AutoConnect { get; set; }

        /// <summary>
        /// 重连间隔(秒)
        /// </summary>
        public int ReconnectInterval { get; set; }

        /// <summary>
        /// 最大重连次数
        /// </summary>
        public int MaxReconnectAttempts { get; set; }

        /// <summary>
        /// 心跳间隔(秒)
        /// </summary>
        public int HeartbeatInterval { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备位置
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 通信协议 (http/https)
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// 轮询间隔(秒)
        /// </summary>
        public int PollInterval { get; set; }

        /// <summary>
        /// HTTP服务监听地址
        /// </summary>
        public string ListenUrl { get; set; }

        /// <summary>
        /// 远程调度服务器地址(用于上报结果)
        /// </summary>
        public string SchedulerUrl { get; set; }

        /// <summary>
        /// 是否启用HTTP服务
        /// </summary>
        public bool HttpServerEnabled { get; set; }

        /// <summary>
        /// 最大并发任务数
        /// </summary>
        public int MaxConcurrentTasks { get; set; }

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

        /// <summary>
        /// 构造函数
        /// </summary>
        public NetworkTaskConfig()
        {
            // 默认值
            Enabled = false;
            Protocol = "http";
            ServerUrl = "http://localhost:8080/api";
            ListenUrl = "http://+:8081/";
            SchedulerUrl = "http://localhost:8080";
            HttpServerEnabled = false;
            MaxConcurrentTasks = 5;
            AutoConnect = true;
            ReconnectInterval = 5;
            MaxReconnectAttempts = 10;
            HeartbeatInterval = 30;
            PollInterval = 10; // HTTP轮询默认10秒
            DeviceId = null;
            DeviceName = null;
            Location = "实验室";
            AutoExecute = true;
            ReportApiPath = "/api/python/report";
            PollingIntervalMs = 1000;
            DeviceBinding = new DeviceBindingConfig();
            TaskCategoryMapping = new Dictionary<string, string>();
        }

        /// <summary>
        /// 从配置文件加载
        /// </summary>
        public static NetworkTaskConfig Load()
        {
            var config = new NetworkTaskConfig();

            try
            {
                var section = ConfigurationManager.GetSection(SectionName) as System.Collections.Specialized.NameValueCollection;
                
                if (section != null)
                {
                    if (bool.TryParse(section["enabled"], out bool enabled))
                    {
                        config.Enabled = enabled;
                    }

                    if (!string.IsNullOrEmpty(section["protocol"]))
                    {
                        config.Protocol = section["protocol"];
                    }

                    if (!string.IsNullOrEmpty(section["serverUrl"]))
                    {
                        config.ServerUrl = section["serverUrl"];
                    }

                    if (int.TryParse(section["pollInterval"], out int pollInterval))
                    {
                        config.PollInterval = pollInterval;
                    }

                    if (bool.TryParse(section["autoConnect"], out bool autoConnect))
                    {
                        config.AutoConnect = autoConnect;
                    }

                    if (int.TryParse(section["reconnectInterval"], out int reconnectInterval))
                    {
                        config.ReconnectInterval = reconnectInterval;
                    }

                    if (int.TryParse(section["maxReconnectAttempts"], out int maxReconnectAttempts))
                    {
                        config.MaxReconnectAttempts = maxReconnectAttempts;
                    }

                    if (int.TryParse(section["heartbeatInterval"], out int heartbeatInterval))
                    {
                        config.HeartbeatInterval = heartbeatInterval;
                    }

                    if (!string.IsNullOrEmpty(section["deviceId"]))
                    {
                        config.DeviceId = section["deviceId"];
                    }

                    if (!string.IsNullOrEmpty(section["deviceName"]))
                    {
                        config.DeviceName = section["deviceName"];
                    }

                    if (!string.IsNullOrEmpty(section["location"]))
                    {
                        config.Location = section["location"];
                    }

                    if (!string.IsNullOrEmpty(section["listenUrl"]))
                    {
                        config.ListenUrl = section["listenUrl"];
                    }

                    if (!string.IsNullOrEmpty(section["schedulerUrl"]))
                    {
                        config.SchedulerUrl = section["schedulerUrl"];
                    }

                    if (bool.TryParse(section["httpServerEnabled"], out bool httpServerEnabled))
                    {
                        config.HttpServerEnabled = httpServerEnabled;
                    }

                    if (int.TryParse(section["maxConcurrentTasks"], out int maxConcurrentTasks))
                    {
                        config.MaxConcurrentTasks = maxConcurrentTasks;
                    }
                }
            }
            catch (Exception ex)
            {
                // 加载失败时使用默认配置
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskConfig] 加载配置失败: {ex.Message}");
            }

            return config;
        }

        /// <summary>
        /// 保存到配置文件
        /// </summary>
        public bool Save()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                
                // 移除旧配置
                if (config.Sections[SectionName] != null)
                {
                    config.Sections.Remove(SectionName);
                }

                // 创建新的AppSettingsSection
                var section = new AppSettingsSection();
                section.Settings.Add("enabled", Enabled.ToString());
                section.Settings.Add("protocol", Protocol);
                section.Settings.Add("serverUrl", ServerUrl);
                section.Settings.Add("pollInterval", PollInterval.ToString());
                section.Settings.Add("autoConnect", AutoConnect.ToString());
                section.Settings.Add("reconnectInterval", ReconnectInterval.ToString());
                section.Settings.Add("maxReconnectAttempts", MaxReconnectAttempts.ToString());
                section.Settings.Add("heartbeatInterval", HeartbeatInterval.ToString());
                section.Settings.Add("deviceId", DeviceId ?? "");
                section.Settings.Add("deviceName", DeviceName ?? "");
                section.Settings.Add("location", Location ?? "");
                section.Settings.Add("listenUrl", ListenUrl ?? "http://+:8081/");
                section.Settings.Add("schedulerUrl", SchedulerUrl ?? "http://localhost:8080");
                section.Settings.Add("httpServerEnabled", HttpServerEnabled.ToString());
                section.Settings.Add("maxConcurrentTasks", MaxConcurrentTasks.ToString());

                // 添加到配置
                config.Sections.Add(SectionName, section);
                
                // 保存
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(SectionName);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkTaskConfig] 保存配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            errorMessage = null;

            if (!Enabled)
            {
                return true; // 未启用时不验证
            }

            if (string.IsNullOrWhiteSpace(ServerUrl))
            {
                errorMessage = "服务器地址不能为空";
                return false;
            }

            if (!ServerUrl.StartsWith("http://") && !ServerUrl.StartsWith("https://"))
            {
                errorMessage = "服务器地址必须是有效的 HTTP 地址 (http:// 或 https://)";
                return false;
            }

            if (ReconnectInterval < 1)
            {
                errorMessage = "重连间隔必须大于等于 1 秒";
                return false;
            }

            if (MaxReconnectAttempts < 0)
            {
                errorMessage = "最大重连次数不能为负数";
                return false;
            }

            if (HeartbeatInterval < 5)
            {
                errorMessage = "心跳间隔必须大于等于 5 秒";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 重置为默认配置
        /// </summary>
        public void ResetToDefault()
        {
            Enabled = false;
            Protocol = "http";
            ServerUrl = "http://localhost:8080/api";
            ListenUrl = "http://+:8081/";
            SchedulerUrl = "http://localhost:8080";
            HttpServerEnabled = false;
            MaxConcurrentTasks = 5;
            AutoConnect = true;
            ReconnectInterval = 5;
            MaxReconnectAttempts = 10;
            HeartbeatInterval = 30;
            PollInterval = 10;
            DeviceId = null;
            DeviceName = null;
            Location = "实验室";
            AutoExecute = true;
            ReportApiPath = "/api/python/report";
            PollingIntervalMs = 1000;
            DeviceBinding = new DeviceBindingConfig();
            TaskCategoryMapping = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// NameValueSectionHandler 实现
    /// </summary>
    public class NameValueSectionHandler : System.Configuration.IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            var collection = new System.Collections.Specialized.NameValueCollection();
            
            foreach (System.Xml.XmlNode node in section.ChildNodes)
            {
                if (node.NodeType == System.Xml.XmlNodeType.Element && node.Name == "add")
                {
                    var key = node.Attributes["key"]?.Value;
                    var value = node.Attributes["value"]?.Value;
                    
                    if (!string.IsNullOrEmpty(key))
                    {
                        collection[key] = value;
                    }
                }
            }
            
            return collection;
        }
    }
}
