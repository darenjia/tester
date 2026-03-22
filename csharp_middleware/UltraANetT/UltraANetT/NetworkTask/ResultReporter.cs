using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UltraANetT.NetworkTask
{
    /// <summary>
    /// 结果上报客户端
    /// 负责向远程调度服务器上报任务执行结果
    /// </summary>
    public class ResultReporter : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _schedulerUrl;
        private readonly int _maxRetries;
        private readonly int _retryDelayMs;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ResultReporter(string schedulerUrl, int maxRetries = 3, int retryDelayMs = 1000)
        {
            _schedulerUrl = schedulerUrl ?? throw new ArgumentNullException(nameof(schedulerUrl));
            _maxRetries = maxRetries;
            _retryDelayMs = retryDelayMs;
            
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// 上报任务结果
        /// </summary>
        public async Task<bool> ReportResultAsync(TaskResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (string.IsNullOrEmpty(result.TaskNo))
            {
                System.Diagnostics.Debug.WriteLine("[ResultReporter] 任务编号不能为空");
                return false;
            }

            var url = $"{_schedulerUrl}/api/tasks/{result.TaskNo}/result";
            var json = JsonConvert.SerializeObject(result);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[ResultReporter] 正在上报任务 {result.TaskNo} 结果 (尝试 {attempt}/{_maxRetries})");
                    
                    var response = await _httpClient.PostAsync(url, content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var responseObj = JsonConvert.DeserializeObject<ReportResponse>(responseBody);
                        
                        if (responseObj?.Success == true)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ResultReporter] 任务 {result.TaskNo} 结果上报成功");
                            return true;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[ResultReporter] 任务 {result.TaskNo} 结果上报失败: {responseObj?.Message}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ResultReporter] 任务 {result.TaskNo} 结果上报失败: HTTP {(int)response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ResultReporter] 任务 {result.TaskNo} 结果上报异常 (尝试 {attempt}): {ex.Message}");
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < _maxRetries)
                {
                    await Task.Delay(_retryDelayMs * attempt);
                }
            }

            System.Diagnostics.Debug.WriteLine($"[ResultReporter] 任务 {result.TaskNo} 结果上报最终失败");
            return false;
        }

        /// <summary>
        /// 批量上报任务结果
        /// </summary>
        public async Task<bool> ReportResultsAsync(List<TaskResult> results)
        {
            if (results == null || results.Count == 0)
                return true;

            bool allSuccess = true;
            
            foreach (var result in results)
            {
                var success = await ReportResultAsync(result);
                if (!success)
                {
                    allSuccess = false;
                }
                
                // 短暂延迟，避免请求过快
                await Task.Delay(100);
            }

            return allSuccess;
        }

        /// <summary>
        /// 上报任务状态
        /// </summary>
        public async Task<bool> ReportStatusAsync(string taskNo, TaskStatus status, int progress = 0, string message = "")
        {
            if (string.IsNullOrEmpty(taskNo))
            {
                System.Diagnostics.Debug.WriteLine("[ResultReporter] 任务编号不能为空");
                return false;
            }

            var url = $"{_schedulerUrl}/api/tasks/{taskNo}/status";
            var statusInfo = new
            {
                TaskNo = taskNo,
                Status = status.ToString(),
                Progress = progress,
                Message = message,
                Timestamp = DateTime.Now
            };
            
            var json = JsonConvert.SerializeObject(statusInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ResultReporter] 上报任务状态失败: {ex.Message}");
                return false;
            }
        }

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

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// 上报响应
    /// </summary>
    public class ReportResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

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
}
