# UltraANetT 项目结构详细分析文档

## 1. 项目概述

**项目名称**: UltraANetT  
**项目类型**: 汽车网络测试管理系统  
**目标框架**: .NET Framework 4.7.2  
**开发语言**: C#  
**主要功能**: CAN/LIN通信测试管理、DTC诊断、测试报告生成、网关路由配置

---

## 2. 解决方案结构

```
UltraANetT.sln
├── UltraANetT          # 主应用程序 (WinForms)
├── CANoeEngine         # CANoe集成引擎 (类库)
├── DBEngine           # 数据库访问层 (类库)
├── FileEditor         # 文件编辑器 (WinForms)
├── Model              # 数据模型层 (类库)
├── ProcessEngine      # 业务处理引擎 (类库)
└── ReportEditor       # 报告查看器 (WinForms)
```

---

## 3. 项目详细分析

### 3.1 UltraANetT (主应用程序)

**项目类型**: Windows应用程序 (WinExe)  
**输出文件**: UltraANetT.exe

#### 3.1.1 目录结构
```
UltraANetT/
├── Form/                    # 窗体类
│   ├── Main.cs             # 主窗体
│   ├── FrmLogin.cs         # 登录窗体
│   ├── wfMain.cs           # 主工作流窗体
│   ├── Loading.cs          # 加载窗体
│   ├── Upload.cs           # 文件上传窗体
│   ├── Manual.cs           # 手动测试窗体
│   ├── EmlDTC.cs           # DTC管理窗体
│   ├── ErrorInfo.cs        # 错误信息窗体
│   ├── FaultMsg.cs         # 故障消息窗体
│   ├── GuideMain.cs        # 向导主窗体
│   ├── UserLogin.cs        # 用户登录窗体
│   └── ...                 # 其他窗体
├── Module/                  # 模块控件
│   ├── Department.cs       # 部门管理模块
│   ├── Employee.cs         # 员工管理模块
│   ├── Task.cs             # 任务管理模块
│   ├── Test.cs             # 测试模块
│   ├── TestStart.cs        # 测试启动模块
│   ├── ReportList.cs       # 报告列表模块
│   ├── EmlLibrary.cs       # 用例库模块
│   ├── ExapChapter.cs      # 用例章节模块
│   ├── FaultType.cs        # 故障类型模块
│   ├── LogInfo.cs          # 日志信息模块
│   ├── OperationLog.cs     # 操作日志模块
│   ├── NodeConfigurationBox.cs  # 节点配置盒模块
│   ├── FilePathCfg.cs      # 文件路径配置模块
│   ├── PassReportNote.cs   # 通过报告备注模块
│   ├── QuestionNote.cs     # 问题备注模块
│   ├── Segment.cs          # 网段管理模块
│   ├── Suppier.cs          # 供应商管理模块
│   ├── Tools.cs            # 工具模块
│   └── Vehicel.cs          # 车辆管理模块
├── Interface/               # 接口定义
│   ├── IDraw.cs            # 绘制接口
│   ├── INode.cs            # 节点接口
│   └── ITree.cs            # 树形结构接口
├── Properties/              # 属性资源
├── Resources/               # 资源文件
│   └── ANetT.db            # SQLite数据库
├── GlobalVar.cs            # 全局变量类
├── Program.cs              # 程序入口
└── AboutDevCompanion.cs    # 关于对话框
```

#### 3.1.2 核心类说明

**GlobalVar.cs - 全局变量类**
```csharp
public class GlobalVar
{
    // 当前登录用户信息
    public static Employee CurrentEmployee;
    public static string CurrentRole;
    
    // 报告相关数据
    public static Dictionary<string, List<List<string>>> ReportData;
    public static Dictionary<string, string> ReportCover;
    
    // 配置模板
    public static List<ConfigTemp> ConfigTemplates;
    
    // 测试状态
    public static bool IsTesting;
    public static string CurrentTaskId;
}
```

**Program.cs - 程序入口**
```csharp
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        // 初始化环境
        InitializeEnvironment();
        
        // 验证必要文件
        if (!ValidateRequiredFiles())
        {
            MessageBox.Show("必要文件缺失！");
            return;
        }
        
        // 启动登录窗体
        Application.Run(new FrmLogin());
    }
    
    static void InitializeEnvironment()
    {
        // 创建必要文件夹
        CreateRequiredDirectories();
        // 加载配置文件
        LoadConfiguration();
    }
}
```

---

### 3.2 Model (数据模型层)

**项目类型**: 类库 (Library)  
**职责**: 定义所有实体类，用于ORM映射

#### 3.2.1 实体类列表

| 类名 | 功能 | 主要属性 |
|------|------|---------|
| **Employee** | 员工信息 | 工号、姓名、角色、部门、密码、状态 |
| **Department** | 部门信息 | 部门名称、上级部门、描述 |
| **Task** | 测试任务 | 任务编号、名称、轮次、CAN通道、模块 |
| **Report** | 测试报告 | 报告编号、测试时间、类型、人员、结果 |
| **ReportInfo** | 报告详细信息 | 报告内容、测试数据、截图 |
| **Authorization** | 车型授权 | 车辆类型、配置、阶段、授权码、有效期 |
| **ConfigTemp** | 配置模板 | 模板名称、版本、内容、匹配规则 |
| **ExampleTemp** | 用例模板 | 用例名称、类型、步骤、预期结果 |
| **ExapChapter** | 用例章节 | 章节编号、名称、父章节、排序 |
| **DBC** | DBC文件信息 | 文件名、版本、网段、节点列表 |
| **FaultType** | 故障类型 | 类型编码、名称、描述、等级 |
| **NodeConfigurationBox** | 节点配置盒 | 盒编号、IP地址、通道数、状态 |
| **Segment** | 网段信息 | 网段名称、类型、波特率、节点数 |
| **Suppliers** | 供应商信息 | 供应商名称、代码、联系人、状态 |
| **Topology** | 拓扑信息 | 拓扑名称、类型、节点连接关系 |
| **UploadInfo** | 上传信息 | 文件名、路径、时间、状态 |
| **LoginLog** | 登录日志 | 用户、时间、IP、结果 |
| **OperationLog** | 操作日志 | 用户、操作、时间、结果 |
| **FileLinkByVehicel** | 车辆文件关联 | 车辆ID、文件ID、关联类型 |
| **PassReportNote** | 通过报告备注 | 报告ID、备注内容、时间 |
| **QuestionNote** | 问题备注 | 问题ID、备注内容、时间 |
| **ProjectFiles** | 项目文件 | 项目ID、文件路径、类型、版本 |

#### 3.2.2 实体类示例

**Employee.cs**
```csharp
public class Employee
{
    public virtual int Id { get; set; }
    public virtual string EmployeeNo { get; set; }      // 工号
    public virtual string Name { get; set; }            // 姓名
    public virtual string Password { get; set; }        // 密码
    public virtual string Role { get; set; }            // 角色
    public virtual Department Department { get; set; }  // 所属部门
    public virtual string Phone { get; set; }           // 电话
    public virtual string Email { get; set; }           // 邮箱
    public virtual bool IsActive { get; set; }          // 是否启用
    public virtual DateTime CreateTime { get; set; }    // 创建时间
}
```

**Task.cs**
```csharp
public class Task
{
    public virtual int Id { get; set; }
    public virtual string TaskNo { get; set; }          // 任务编号
    public virtual string TaskName { get; set; }        // 任务名称
    public virtual int Round { get; set; }              // 测试轮次
    public virtual string CANChannel { get; set; }      // CAN通道
    public virtual string Module { get; set; }          // 测试模块
    public virtual string Status { get; set; }          // 任务状态
    public virtual Employee Creator { get; set; }       // 创建人
    public virtual DateTime CreateTime { get; set; }    // 创建时间
    public virtual DateTime? StartTime { get; set; }    // 开始时间
    public virtual DateTime? EndTime { get; set; }      // 结束时间
}
```

---

### 3.3 DBEngine (数据库访问层)

**项目类型**: 类库 (Library)  
**职责**: 数据库连接管理、ORM操作封装

#### 3.3.1 核心类

**NHelper.cs - NHibernate会话工厂**
```csharp
public class NHelper
{
    private static ISessionFactory _sessionFactory;
    
    // 初始化会话工厂
    public static ISessionFactory SessionFactory
    {
        get
        {
            if (_sessionFactory == null)
            {
                _sessionFactory = CreateSessionFactory();
            }
            return _sessionFactory;
        }
    }
    
    // 创建会话工厂
    private static ISessionFactory CreateSessionFactory()
    {
        var configuration = new Configuration();
        configuration.Configure();  // 读取hibernate.cfg.xml
        configuration.AddAssembly(typeof(Model.Employee).Assembly);
        return configuration.BuildSessionFactory();
    }
    
    // 打开新会话
    public static ISession OpenSession()
    {
        return SessionFactory.OpenSession();
    }
}
```

**BaseSqlOrder.cs - 基础数据库操作**
```csharp
public class BaseSqlOrder
{
    // 根据ID获取实体
    public T GetById<T>(int id) where T : class
    {
        using (var session = NHelper.OpenSession())
        {
            return session.Get<T>(id);
        }
    }
    
    // 获取所有记录
    public IList<T> GetAll<T>() where T : class
    {
        using (var session = NHelper.OpenSession())
        {
            return session.Query<T>().ToList();
        }
    }
    
    // 保存实体
    public void Save<T>(T entity) where T : class
    {
        using (var session = NHelper.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            session.Save(entity);
            transaction.Commit();
        }
    }
    
    // 更新实体
    public void Update<T>(T entity) where T : class
    {
        using (var session = NHelper.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            session.Update(entity);
            transaction.Commit();
        }
    }
    
    // 删除实体
    public void Delete<T>(T entity) where T : class
    {
        using (var session = NHelper.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            session.Delete(entity);
            transaction.Commit();
        }
    }
    
    // 执行HQL查询
    public IList<T> Query<T>(string hql, params object[] parameters) where T : class
    {
        using (var session = NHelper.OpenSession())
        {
            var query = session.CreateQuery(hql);
            for (int i = 0; i < parameters.Length; i++)
            {
                query.SetParameter(i, parameters[i]);
            }
            return query.List<T>();
        }
    }
}
```

**ExcuteSqlCase.cs - SQL用例执行**
```csharp
public class ExcuteSqlCase
{
    // 执行原生SQL
    public void ExecuteSql(string sql)
    {
        using (var session = NHelper.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            session.CreateSQLQuery(sql).ExecuteUpdate();
            transaction.Commit();
        }
    }
    
    // 执行存储过程
    public void ExecuteStoredProcedure(string procName, Dictionary<string, object> parameters)
    {
        using (var session = NHelper.OpenSession())
        {
            var query = session.CreateSQLQuery($"exec {procName}");
            foreach (var param in parameters)
            {
                query.SetParameter(param.Key, param.Value);
            }
            query.ExecuteUpdate();
        }
    }
}
```

---

### 3.4 CANoeEngine (CANoe集成引擎)

**项目类型**: 类库 (Library)  
**职责**: 与Vector CANoe软件集成，控制测试执行

#### 3.4.1 核心类

**ProcCANoe.cs - CANoe基础控制**
```csharp
public class ProcCANoe
{
    // CANoe应用程序对象
    public CANoe.Application _mCANoeApp;
    public CANoe.Measurement _mCANoeMeasurement;
    
    private string _absoluteConfigurationPath = "";
    
    // 检查配置文件是否存在
    public bool IsExistConfiguration(string absolutePath)
    {
        if (File.Exists(absolutePath))
        {
            _absoluteConfigurationPath = absolutePath;
            return true;
        }
        return false;
    }
    
    // 打开CANoe
    public bool OpenCANoe()
    {
        _mCANoeApp = new Application();
        _mCANoeMeasurement = (Measurement)_mCANoeApp.Measurement;
        
        // 停止当前运行的测量
        if (_mCANoeMeasurement.Running)
            _mCANoeMeasurement.Stop();
        
        if (_mCANoeApp != null)
        {
            // 打开配置文件
            _mCANoeApp.Open(_absoluteConfigurationPath, true, true);
            
            // 检查打开结果
            var ocresult = _mCANoeApp.configuration.OpenConfigurationResult;
            if (ocresult.result != 0) return false;
            
            return true;
        }
        return false;
    }
    
    // 启动或停止测量
    public int StartOrStopCaNoe()
    {
        try
        {
            if (_mCANoeMeasurement == null) return 2;
            if (_mCANoeMeasurement.Running)
            {
                _mCANoeMeasurement.Stop();
                return 0;  // 停止
            }
            _mCANoeMeasurement.Start();
            return 1;  // 启动
        }
        catch (Exception ex)
        {
            return -1;  // 异常
        }
    }
    
    // 暂停测量
    public bool PauseCANoe()
    {
        try
        {
            if (_mCANoeMeasurement == null) return true;
            if (_mCANoeMeasurement.Running)
            {
                _mCANoeMeasurement.Stop();
            }
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    // 关闭CANoe
    public bool CloseCANoe()
    {
        try
        {
            if (_mCANoeApp != null)
            {
                if (_mCANoeMeasurement != null)
                {
                    if (_mCANoeMeasurement.Running)
                        _mCANoeMeasurement.Stop();
                }
                _mCANoeApp.Quit();
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

**ProcCANoeTest.cs - 测试执行控制**
```csharp
public class ProcCANoeTest
{
    private readonly ProcCANoe _canoe = new ProcCANoe();
    private readonly ProcSelfCheck _selfCheck = new ProcSelfCheck();
    private readonly ProcExample _example = new ProcExample();
    
    // 测试路径
    private string _selfCheckStr;      // 自检工程路径
    private string _exampleStr;        // 用例工程路径
    private string _strStateMachinePath;  // 状态机路径
    
    // 测试状态
    public bool endHFlag = false;      // 隐性用例结束标志
    public bool endDFlag = false;      // 显性用例结束标志
    public int ExampleTestedCount = 0; // 已测试用例数
    public int ExampleAllCount = 0;    // 总用例数
    
    // 用例缓存
    public List<string> _dictHExampleCache = new List<string>();  // 隐性用例缓存
    public List<string> _dictDExampleCache = new List<string>();  // 显性用例缓存
    public Dictionary<string, int> _dictHNameList = new Dictionary<string, int>();  // 隐性用例结果
    public Dictionary<string, int> _dictDNameList = new Dictionary<string, int>();  // 显性用例结果
    
    // 设备信息
    public Dictionary<string, string> _dictDeviceInfo = new Dictionary<string, string>();
    
    // 设置路径
    public void GetName(string selfPath, string exam, string stateMachinePath)
    {
        _selfCheckStr = selfPath;
        _exampleStr = exam;
        _strStateMachinePath = stateMachinePath;
    }
    
    // 启动设备自检
    public bool StartDeviceSelfCheck()
    {
        bool isExist = _canoe.IsExistConfiguration(_selfCheckStr);
        if (isExist)
        {
            if (_canoe.OpenCANoe())
            {
                StartStateMachine();
                _canoe.StartOrStopCaNoe();
            }
            _selfCheck._canoe = _canoe;
            _selfCheck.GetAllSelfCheckVar();
            _selfCheck.SetSelfCheckVarValue(ProcSelfCheck.SelfEnumVar.StartDeviceSelfCheck, 1);
            
            Thread thread = new Thread(ReadDeviceSelfCheckResult);
            thread.Start();
            return true;
        }
        return false;
    }
    
    // 读取设备自检结果
    private void ReadDeviceSelfCheckResult()
    {
        while (true)
        {
            int isEnd = _selfCheck.GetSelfCheckVarValue(ProcSelfCheck.SelfEnumVar.IsEndDeviceSelfCheck);
            if (isEnd == 1)
            {
                IsEndDevice = true;
                break;
            }
            Thread.Sleep(500);
        }
    }
    
    // 启动隐性用例测试
    public int StartHExampleCheck(List<string> objectName, Dictionary<string, string> dtcInfo)
    {
        GloalVar.IsHPause = true;
        
        bool isExist = _canoe.IsExistConfiguration(_exampleStr);
        if (isExist)
        {
            _example._canoe = _canoe;
            _example.GetAllExampleVar();
            
            foreach (var name in objectName)
            {
                // 解析测试次数
                int iTestCount = 1;
                string testName = name;
                if (name.Split('@').Length >= 2)
                {
                    testName = name.Split('@')[0];
                    int.TryParse(name.Split('@')[1], out iTestCount);
                }
                
                for (int i = 1; i <= iTestCount; i++)
                {
                    string exampleName = testName + "@" + i;
                    
                    // 设置DTC信息
                    if (dtcInfo.ContainsKey(testName))
                        _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.DtcTestInformation, dtcInfo[testName]);
                    
                    // 设置用例名称
                    _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestScriptName, exampleName);
                    Thread.Sleep(200);
                    
                    // 开始测试
                    _example.SetExampleVarValue(ProcExample.EmpEnumVar.StartTest, 1);
                    Thread.Sleep(200);
                    
                    // 等待测试完成
                    while (GloalVar.IsHPause)
                    {
                        int isReadFlag = _example.GetExmpVarValue(ProcExample.EmpEnumVar.BufferFlag);
                        if (isReadFlag == 1)
                        {
                            string value = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.BufferValue);
                            if (value != _exampleCache)
                            {
                                if (!_dictHExampleCache.Contains(exampleName))
                                    _dictHExampleCache.Add(exampleName + "用例：" + value);
                                _exampleCache = value;
                            }
                            _example.SetExampleVarValue(ProcExample.EmpEnumVar.BufferFlag, 0);
                        }
                        
                        int isEndTest = _example.GetExmpVarValue(ProcExample.EmpEnumVar.EndTest);
                        if (isEndTest == 1)
                        {
                            ExampleTestedCount++;
                            _example.SetExampleVarValue(ProcExample.EmpEnumVar.EndTest, 0);
                            int result = _example.GetExmpVarValue(ProcExample.EmpEnumVar.TestCaseResultState);
                            if (!_dictHNameList.ContainsKey(exampleName))
                                _dictHNameList.Add(exampleName, result);
                            break;
                        }
                        Thread.Sleep(2000);
                    }
                    Thread.Sleep(1000);
                }
            }
            
            GloalVar.IsHPause = false;
            endHFlag = true;
        }
        return 0;
    }
    
    // 启动显性用例测试
    public int StartDExampleCheck(List<string> objectName, Dictionary<string, string> dtcInfo, bool isPause)
    {
        GloalVar.IsDPause = true;
        
        bool isExist = _canoe.IsExistConfiguration(_exampleStr);
        if (isExist)
        {
            _example._canoe = _canoe;
            _example.GetAllExampleVar();
            
            foreach (var name in objectName)
            {
                // 类似隐性用例的处理逻辑
                // ...
            }
            
            GloalVar.IsDPause = false;
            endDFlag = true;
        }
        
        PauseCANoe();
        return 0;
    }
    
    // 关闭CANoe
    public void CloseCANoe()
    {
        _canoe.CloseCANoe();
        CloseStateMachine();
    }
    
    // 暂停CANoe
    public void PauseCANoe()
    {
        _canoe.PauseCANoe();
        CloseStateMachine();
    }
}
```

**ProcSelfCheck.cs - 自检功能**
```csharp
public class ProcSelfCheck
{
    private const string CaplNamespaceName = "mutualVar";
    
    // CANoe系统变量
    private CANoe.System _mCANoeSystem;
    private CANoe.Namespaces _mCANoeNamespaces;
    private CANoe.Namespace _mCANoeNamespaceGeneral;
    private CANoe.Variables _mCANoeVariablesGeneral;
    public ProcCANoe _canoe;
    
    // 自检变量
    private Variable _startDeviceSelfCheck;
    private Variable _startPrototypeSelfCheck;
    private Variable _isEndPrototySelfCheck;
    private Variable _isEndDeviceSelfCheck;
    
    // 获取所有自检变量
    public void GetAllSelfCheckVar()
    {
        _mCANoeSystem = (CANoe.System)_canoe._mCANoeApp.System;
        _mCANoeNamespaces = (CANoe.Namespaces)_mCANoeSystem.Namespaces;
        _mCANoeNamespaceGeneral = (CANoe.Namespace)_mCANoeNamespaces[CaplNamespaceName];
        _mCANoeVariablesGeneral = (CANoe.Variables)_mCANoeNamespaceGeneral.Variables;
        
        _startDeviceSelfCheck = (CANoe.Variable)_mCANoeVariablesGeneral["startDeviceSelfCheck"];
        _startPrototypeSelfCheck = (CANoe.Variable)_mCANoeVariablesGeneral["startPrototypeSelfCheck"];
        _isEndPrototySelfCheck = (CANoe.Variable)_mCANoeVariablesGeneral["isEndPrototySelfCheck"];
        _isEndDeviceSelfCheck = (CANoe.Variable)_mCANoeVariablesGeneral["isEndDeviceSelfCheck"];
    }
    
    // 获取变量值
    public int GetSelfCheckVarValue(SelfEnumVar enumVar)
    {
        switch (enumVar)
        {
            case SelfEnumVar.StartPrototypeSelfCheck:
                return _startPrototypeSelfCheck.Value;
            case SelfEnumVar.StartDeviceSelfCheck:
                return _startDeviceSelfCheck.Value;
            case SelfEnumVar.IsEndPrototySelfCheck:
                return _isEndPrototySelfCheck.Value;
            case SelfEnumVar.IsEndDeviceSelfCheck:
                return _isEndDeviceSelfCheck.Value;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 设置变量值
    public bool SetSelfCheckVarValue(SelfEnumVar enumVar, int varValue)
    {
        switch (enumVar)
        {
            case SelfEnumVar.StartDeviceSelfCheck:
                _startDeviceSelfCheck.Value = varValue;
                return true;
            case SelfEnumVar.StartPrototypeSelfCheck:
                _startPrototypeSelfCheck.Value = varValue;
                return true;
            case SelfEnumVar.IsEndDeviceSelfCheck:
                _isEndDeviceSelfCheck.Value = varValue;
                return true;
            case SelfEnumVar.IsEndPrototySelfCheck:
                _isEndPrototySelfCheck.Value = varValue;
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 自检变量枚举
    public enum SelfEnumVar
    {
        StartDeviceSelfCheck,
        PowerSupplyCheck,
        OscillographCheck,
        DigitalMultimeterCheck,
        PaNPowerSupplyCheck,
        StartPrototypeSelfCheck,
        IsDBCDepended,
        ExplicitOrImplicit,
        SendMessage,
        IsEndPrototySelfCheck,
        IsEndDeviceSelfCheck,
    }
}
```

**ProcExample.cs - 用例执行**
```csharp
public class ProcExample
{
    private const string CaplNamespaceName = "mutualVar";
    
    // CANoe系统变量
    private CANoe.System _mCANoeSystem;
    private CANoe.Namespaces _mCANoeNamespaces;
    private CANoe.Namespace _mCANoeNamespaceGeneral;
    private CANoe.Variables _mCANoeVariablesGeneral;
    public ProcCANoe _canoe;
    
    // 用例变量
    private Variable _startTest;
    private Variable _endTest;
    private Variable _testScriptName;
    private Variable _testscriptNameState;
    private Variable _bufferFlag;
    private Variable _bufferValue;
    private Variable _testCaseResultState;
    private Variable _dtcTestInformation;
    
    // 设备信息变量
    private Variable _carManufacturerECUHardwareNumber;
    private Variable _carManufacturerECUSoftware;
    private Variable _ECUBatchNumber;
    private Variable _ECUManufacturingDate;
    private Variable _softwareVersionNumber;
    private Variable _sparePartsNumberOfAutomobileManufacturers;
    private Variable _systemVendorECUHardwareNumber;
    private Variable _systemVendorECUSoftware;
    private Variable _systemVendorECUSoftwareVersionNumber;
    private Variable _systemVendorHardwareVersionNumber;
    private Variable _systemVendorNameCode;
    private Variable _VINCode;
    private Variable _startDeviceInfo;
    
    // 获取所有用例变量
    public void GetAllExampleVar()
    {
        _mCANoeSystem = (CANoe.System)_canoe._mCANoeApp.System;
        _mCANoeNamespaces = (CANoe.Namespaces)_mCANoeSystem.Namespaces;
        _mCANoeNamespaceGeneral = (CANoe.Namespace)_mCANoeNamespaces[CaplNamespaceName];
        _mCANoeVariablesGeneral = (CANoe.Variables)_mCANoeNamespaceGeneral.Variables;
        
        // 初始化所有变量引用
        _startTest = (CANoe.Variable)_mCANoeVariablesGeneral["startTest"];
        _endTest = (CANoe.Variable)_mCANoeVariablesGeneral["endTest"];
        _testScriptName = (CANoe.Variable)_mCANoeVariablesGeneral["testScriptName"];
        _testscriptNameState = (CANoe.Variable)_mCANoeVariablesGeneral["testscriptNameState"];
        _bufferFlag = (CANoe.Variable)_mCANoeVariablesGeneral["bufferFlag"];
        _bufferValue = (CANoe.Variable)_mCANoeVariablesGeneral["bufferValue"];
        _testCaseResultState = (CANoe.Variable)_mCANoeVariablesGeneral["testCaseResultState"];
        _dtcTestInformation = (CANoe.Variable)_mCANoeVariablesGeneral["dtcTestInformation"];
        
        // 设备信息变量
        _carManufacturerECUHardwareNumber = (CANoe.Variable)_mCANoeVariablesGeneral["carManufacturerECUHardwareNumber"];
        _carManufacturerECUSoftware = (CANoe.Variable)_mCANoeVariablesGeneral["carManufacturerECUSoftware"];
        _ECUBatchNumber = (CANoe.Variable)_mCANoeVariablesGeneral["ECUBatchNumber"];
        _ECUManufacturingDate = (CANoe.Variable)_mCANoeVariablesGeneral["ECUManufacturingDate"];
        _softwareVersionNumber = (CANoe.Variable)_mCANoeVariablesGeneral["softwareVersionNumber"];
        _sparePartsNumberOfAutomobileManufacturers = (CANoe.Variable)_mCANoeVariablesGeneral["sparePartsNumberOfAutomobileManufacturers"];
        _systemVendorECUHardwareNumber = (CANoe.Variable)_mCANoeVariablesGeneral["systemVendorECUHardwareNumber"];
        _systemVendorECUSoftware = (CANoe.Variable)_mCANoeVariablesGeneral["systemVendorECUSoftware"];
        _systemVendorECUSoftwareVersionNumber = (CANoe.Variable)_mCANoeVariablesGeneral["systemVendorECUSoftwareVersionNumber"];
        _systemVendorHardwareVersionNumber = (CANoe.Variable)_mCANoeVariablesGeneral["systemVendorHardwareVersionNumber"];
        _systemVendorNameCode = (CANoe.Variable)_mCANoeVariablesGeneral["systemVendorNameCode"];
        _VINCode = (CANoe.Variable)_mCANoeVariablesGeneral["VINCode"];
        _startDeviceInfo = (CANoe.Variable)_mCANoeVariablesGeneral["startDeviceInfo"];
    }
    
    // 获取整型变量值
    public int GetExmpVarValue(EmpEnumVar enumVar)
    {
        switch (enumVar)
        {
            case EmpEnumVar.StartTest:
                return _startTest.Value;
            case EmpEnumVar.EndTest:
                return _endTest.Value;
            case EmpEnumVar.BufferFlag:
                return _bufferFlag.Value;
            case EmpEnumVar.TestCaseResultState:
                return _testCaseResultState.Value;
            case EmpEnumVar.StartDeviceInfo:
                return _startDeviceInfo.Value;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 获取字符串变量值
    public string GetExmpVarValueFromStr(EmpEnumVar enumVar)
    {
        switch (enumVar)
        {
            case EmpEnumVar.TestScriptName:
                return _testScriptName.Value;
            case EmpEnumVar.TestscriptNameState:
                return _testscriptNameState.Value;
            case EmpEnumVar.BufferValue:
                return _bufferValue.Value;
            case EmpEnumVar.DtcTestInformation:
                return _dtcTestInformation.Value;
            case EmpEnumVar.CarManufacturerECUHardwareNumber:
                return _carManufacturerECUHardwareNumber.Value;
            case EmpEnumVar.CarManufacturerECUSoftware:
                return _carManufacturerECUSoftware.Value;
            case EmpEnumVar.ECUBatchNumber:
                return _ECUBatchNumber.Value;
            case EmpEnumVar.ECUManufacturingDate:
                return _ECUManufacturingDate.Value;
            case EmpEnumVar.SoftwareVersionNumber:
                return _softwareVersionNumber.Value;
            case EmpEnumVar.SparePartsNumberOfAutomobileManufacturers:
                return _sparePartsNumberOfAutomobileManufacturers.Value;
            case EmpEnumVar.SystemVendorECUHardwareNumber:
                return _systemVendorECUHardwareNumber.Value;
            case EmpEnumVar.SystemVendorECUSoftware:
                return _systemVendorECUSoftware.Value;
            case EmpEnumVar.SystemVendorECUSoftwareVersionNumber:
                return _systemVendorECUSoftwareVersionNumber.Value;
            case EmpEnumVar.SystemVendorHardwareVersionNumber:
                return _systemVendorHardwareVersionNumber.Value;
            case EmpEnumVar.SystemVendorNameCode:
                return _systemVendorNameCode.Value;
            case EmpEnumVar.VINCode:
                return _VINCode.Value;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 设置整型变量值
    public bool SetExampleVarValue(EmpEnumVar enumVar, int varValue)
    {
        try
        {
            switch (enumVar)
            {
                case EmpEnumVar.StartTest:
                    _startTest.Value = varValue;
                    return true;
                case EmpEnumVar.EndTest:
                    _endTest.Value = varValue;
                    return true;
                case EmpEnumVar.BufferFlag:
                    _bufferFlag.Value = varValue;
                    return true;
                case EmpEnumVar.TestCaseResultState:
                    _testCaseResultState.Value = varValue;
                    return true;
                case EmpEnumVar.StartDeviceInfo:
                    _startDeviceInfo.Value = varValue;
                    return true;
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
    
    // 设置字符串变量值
    public bool SetExampleVarValueByStr(EmpEnumVar enumVar, string varValue)
    {
        try
        {
            switch (enumVar)
            {
                case EmpEnumVar.TestScriptName:
                    _testScriptName.Value = varValue;
                    return true;
                case EmpEnumVar.TestscriptNameState:
                    _testscriptNameState.Value = varValue;
                    return true;
                case EmpEnumVar.BufferValue:
                    _bufferValue.Value = varValue;
                    return true;
                case EmpEnumVar.DtcTestInformation:
                    _dtcTestInformation.Value = varValue;
                    return true;
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
    
    // 用例变量枚举
    public enum EmpEnumVar
    {
        StartTest,
        EndTest,
        TestScriptName,
        TestscriptNameState,
        BufferFlag,
        BufferValue,
        TestCaseResultState,
        DtcTestInformation,
        StartDeviceInfo,
        CarManufacturerECUHardwareNumber,
        CarManufacturerECUSoftware,
        ECUBatchNumber,
        ECUManufacturingDate,
        SoftwareVersionNumber,
        SparePartsNumberOfAutomobileManufacturers,
        SystemVendorECUHardwareNumber,
        SystemVendorECUSoftware,
        SystemVendorECUSoftwareVersionNumber,
        SystemVendorHardwareVersionNumber,
        SystemVendorNameCode,
        VINCode,
    }
}
```

**GloalVar.cs - 全局变量**
```csharp
public class GloalVar
{
    public static bool IsHPause = false;  // 隐性用例暂停标志
    public static bool IsDPause = false;  // 显性用例暂停标志
}
```

---

### 3.5 ProcessEngine (业务处理引擎)

**项目类型**: 类库 (Library)  
**职责**: 核心业务逻辑处理、文件操作、报告生成

#### 3.5.1 核心类

**ProcFile.cs - 文件处理类**（2600+行，功能最复杂）
```csharp
public class ProcFile
{
    private readonly procDBC _dbcAnalysis = new procDBC();
    private readonly ProcStore _store = new ProcStore();
    private procLDF _ldf = new procLDF();
    
    #region INI文件生成
    
    // 创建CAN单节点INI文件
    public string CreateCfginiFromCANS(string folder, string configName, 
        Dictionary<string, string> dictHead,
        Dictionary<string, string> dictNody, 
        List<Dictionary<string, string>> dictLocal, 
        List<string> dictVirNode,
        string dbcPath)
    {
        // 生成INI配置文件
        var folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\configini\\" + configName + "\\";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        
        StreamWriter sw = new StreamWriter(folderPath + configName + ".ini", false, Encoding.Default);
        
        // 写入文件头
        sw.WriteLine("[PathInfo]");
        sw.WriteLine("DBCPathFile=" + AppDomain.CurrentDomain.BaseDirectory + dbcPath);
        
        // 写入DUT头信息
        WriteDUTHead(ref sw, dictHead);
        // 写入CAN节点信息
        WriteDUTCANNodeToSig(ref sw, dictNody);
        
        sw.Flush();
        sw.Close();
        
        return "";
    }
    
    // 创建网关路由INI文件
    public string CreateCfginiFromGateway(string folder, string configName, string excelPath)
    {
        // 解析Excel，生成网关路由配置
        XSSFWorkbook workbook = new XSSFWorkbook(excelPath);
        
        // 多线程解析各个Sheet
        ThreadPool.SetMinThreads(1, 1);
        ThreadPool.SetMaxThreads(5, 5);
        
        // 解析不同类型的路由表
        for (int i = 0; i < listSheetName.Count; i++)
        {
            switch (listSheetName[i].Trim().ToLower())
            {
                case "directmessage_routingtable":
                    ThreadPool.QueueUserWorkItem(AnalysisDirectMessage_Routingtable, workbook.GetSheet(listSheetName[i]));
                    break;
                case "indirectmessage_routingtable":
                    ThreadPool.QueueUserWorkItem(AnalysisIndirectMessage_Routingtable, workbook.GetSheet(listSheetName[i]));
                    break;
                // ... 其他路由表类型
            }
        }
        
        // 等待所有线程完成
        _myEvent.WaitOne();
        
        // 生成INI文件
        return CreateINI(strIniPath);
    }
    
    #endregion
    
    #region XML报告解析
    
    // 解析CANoe生成的XML报告
    public Dictionary<string, List<List<string>>> AnalysisXml(string xmlPath)
    {
        Dictionary<string, List<List<string>>> dictReport = new Dictionary<string, List<List<string>>>();
        
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlPath);
        XmlElement rootElem = doc.DocumentElement;
        
        // 获取所有testcase节点
        XmlNodeList testcaseNodes = rootElem.GetElementsByTagName("testcase");
        
        foreach (XmlNode testcaseNode in testcaseNodes)
        {
            // 解析测试步骤
            XmlNodeList desNodes = testcaseNode.SelectNodes("teststep//tabularinfo//description");
            XmlNodeList rowNodes = testcaseNode.SelectNodes("teststep//tabularinfo//row");
            string title = testcaseNode.SelectNodes("ident")[0].InnerText;
            XmlNodeList resultNodes = testcaseNode.SelectNodes("teststep");
            
            // 构建报告数据结构
            List<List<string>> rowsContent = new List<List<string>>();
            // ... 解析逻辑
            
            dictReport.Add(title, rowsContent);
        }
        
        return dictReport;
    }
    
    #endregion
    
    #region 压缩解压
    
    // 解压ZIP文件
    public string UnZipFile(string targetFile, string fileDir)
    {
        var s = new ZipInputStream(File.OpenRead(targetFile.Trim()));
        ZipEntry theEntry;
        
        while ((theEntry = s.GetNextEntry()) != null)
        {
            string fileName = Path.GetFileName(theEntry.Name);
            if (fileName != String.Empty)
            {
                FileStream streamWriter = File.Create(path + "\\" + fileName);
                byte[] data = new byte[2048];
                while (true)
                {
                    var size = s.Read(data, 0, data.Length);
                    if (size > 0)
                    {
                        streamWriter.Write(data, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
                streamWriter.Close();
            }
        }
        s.Close();
        
        return rootFile;
    }
    
    // 压缩为ZIP文件
    public void ZipFile(string strFile, string strZip)
    {
        ZipOutputStream s = new ZipOutputStream(File.Create(strZip));
        s.SetLevel(6);
        Zip(strFile, s, strFile);
        s.Finish();
        s.Close();
    }
    
    #endregion
    
    #region FTP上传
    
    // FTP文件上传
    public string UpLoadFile(Dictionary<string, string> dictUpload, string localPath)
    {
        FileInfo fileInfo = new FileInfo(localPath);
        
        // 创建FTP请求
        var reqFtp = (FtpWebRequest)WebRequest.Create(
            new Uri("ftp://" + dictUpload["IP"] + ":" + dictUpload["Port"] + "//" + 
                   dictUpload["UploadPath"] + "//" + fileInfo.Name));
        reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
        reqFtp.UseBinary = true;
        reqFtp.Credentials = new NetworkCredential(dictUpload["User"], dictUpload["Password"]);
        
        // 上传文件
        FileStream fs = fileInfo.OpenRead();
        Stream ftpStream = reqFtp.GetRequestStream();
        
        const int bufferSize = 2048;
        byte[] buffer = new byte[bufferSize];
        
        int readCount = fs.Read(buffer, 0, bufferSize);
        while (readCount > 0)
        {
            ftpStream.Write(buffer, 0, readCount);
            readCount = fs.Read(buffer, 0, bufferSize);
        }
        
        ftpStream.Close();
        fs.Close();
        
        return "8";  // 成功代码
    }
    
    #endregion
}
```

**CreateReport.cs - 报告生成**
```csharp
public class CreateReport
{
    // 生成Excel测试报告
    public void GenerateReport(Dictionary<string, List<List<string>>> reportData, 
        string outputPath)
    {
        IWorkbook workbook = new XSSFWorkbook();
        ISheet sheet = workbook.CreateSheet("测试报告");
        
        // 创建标题行
        IRow headerRow = sheet.CreateRow(0);
        headerRow.CreateCell(0).SetCellValue("测试用例");
        headerRow.CreateCell(1).SetCellValue("测试步骤");
        headerRow.CreateCell(2).SetCellValue("预期结果");
        headerRow.CreateCell(3).SetCellValue("实际结果");
        headerRow.CreateCell(4).SetCellValue("测试结果");
        
        int rowIndex = 1;
        foreach (var testCase in reportData)
        {
            foreach (var step in testCase.Value)
            {
                IRow row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(testCase.Key);
                for (int i = 0; i < step.Count; i++)
                {
                    row.CreateCell(i + 1).SetCellValue(step[i]);
                }
            }
        }
        
        // 保存文件
        FileStream fs = File.Create(outputPath);
        workbook.Write(fs);
        fs.Close();
    }
}
```

**LogicalControl.cs - 逻辑控制**
```csharp
public class LogicalControl
{
    Dictionary<string, object> _dict = new Dictionary<string, object>();
    private ProcStore _store = new ProcStore();
    
    // 角色判断
    public string RoleSelect(string userName)
    {
        string role;
        _dict.Add("ElyName", userName);
        IList<object[]> list = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Employee_role, _dict);
        
        if (list[0][2].ToString() == "超级管理员")
        {
            role = "superadminister";
        }
        else if (list[0][2].ToString() == "管理员")
        {
            role = "administer";
        }
        else if (list[0][2].ToString() == "配置员")
        {
            role = "configurator";
        }
        else
        {
            role = "tester";
        }
        return role;
    }
}
```

**EnumLibrary.cs - 枚举库**
```csharp
public class EnumLibrary
{
    // 数据库表枚举
    public enum EnumTable
    {
        Employee,
        Employee_role,
        Department,
        Task,
        Report,
        ConfigTemp,
        ExampleTemp,
        FaultType,
        // ... 其他表
    }
    
    // 测试类型枚举
    public enum TestType
    {
        CANCommunicationUnit,       // CAN通信单元
        CANCommunicationIntegration, // CAN通信集成
        DirectNMUnit,               // 直接NM单元
        DirectNMIntegration,        // 直接NM集成
        IndirectNMUnit,             // 间接NM单元
        IndirectNMIntegration,      // 间接NM集成
        LINCommunicationMaster,     // LIN通信主节点
        LINCommunicationSlave,      // LIN通信从节点
        GatewayRouting,             // 网关路由
        DTCCommunication,           // 通信DTC
        Bootloader,                 // Bootloader
        OSEKNMUnit,                 // OSEK NM单元
        OSEKNMIntegration,          // OSEK NM集成
    }
    
    // 测试结果枚举
    public enum TestResult
    {
        PASS = 0,
        FAIL = 1,
        WARN = 2,
        NT = 3,     // Not Tested
    }
}
```

---

### 3.6 FileEditor (文件编辑器)

**项目类型**: Windows应用程序 (WinExe)  
**职责**: 配置文件编辑、模板管理

#### 3.6.1 主要功能
- DBC/LDF文件解析和编辑
- 配置模板管理
- 用例模板编辑
- 网关路由配置
- DTC故障配置

---

### 3.7 ReportEditor (报告查看器)

**项目类型**: Windows应用程序 (WinExe)  
**职责**: 测试报告查看、打印、导出

#### 3.7.1 主要功能
- 查看测试报告
- 打印报告
- 导出PDF/Excel
- 报告对比

---

## 4. 技术栈分析

### 4.1 核心技术

| 技术/框架 | 版本 | 用途 |
|----------|------|------|
| .NET Framework | 4.7.2 | 基础运行环境 |
| NHibernate | 3.3.1 | ORM框架 |
| NPOI | 2.3.0 | Excel文件操作 |
| Newtonsoft.Json | 11.0.2 | JSON序列化 |
| SharpZipLib | 0.86.0 | 压缩解压 |
| AlphaFS | 2.2.6 | 文件系统操作（支持长路径） |
| DevExpress | v15.2 | UI控件库 |
| Vector CANoe COM API | - | 与CANoe集成 |

### 4.2 数据库支持

| 数据库 | 支持状态 | 说明 |
|--------|---------|------|
| SQLite | ✅ | 本地数据库 |
| MySQL | ✅ | 通过网络 |
| SQL Server | ✅ | 企业版 |

### 4.3 外部DLL依赖

| DLL名称 | 用途 |
|---------|------|
| Interop.CANoe.dll | CANoe COM接口 |
| DBCEngine.dll | DBC文件解析引擎 |
| OSEKCLASS.dll | OSEK网络管理 |
| hasp_net_windows.dll | 加密狗保护 |

---

## 5. 业务流程分析

### 5.1 测试执行流程

```
1. 用户登录
   └─> 验证用户名密码
   └─> 加载用户权限
   
2. 选择测试任务
   └─> 加载任务配置
   └─> 验证车型授权
   
3. 设备自检
   └─> 打开自检工程
   └─> 启动CANoe测量
   └─> 执行自检程序
   └─> 等待自检完成
   
4. 执行测试用例
   ├─> 隐性用例测试
   │  ├─> 设置用例参数
   │  ├─> 启动测试
   │  ├─> 监控执行状态
   │  └─> 记录测试结果
   │
   └─> 显性用例测试
      ├─> 设置用例参数
      ├─> 启动测试
      ├─> 监控执行状态
      └─> 记录测试结果

5. 生成报告
   ├─> 解析CANoe XML报告
   ├─> 生成Excel报告
   └─> 保存测试记录

6. 关闭资源
   ├─> 停止CANoe测量
   ├─> 关闭CANoe
   └─> 释放资源
```

### 5.2 数据流图

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   用户界面   │────>│  ProcessEngine │────>│  CANoeEngine │
│  (UltraANetT)│     │  (业务逻辑)   │     │  (CANoe控制) │
└─────────────┘     └─────────────┘     └─────────────┘
       │                   │                   │
       │                   │                   ▼
       │                   │            ┌─────────────┐
       │                   │            │  Vector CANoe│
       │                   │            └─────────────┘
       │                   ▼
       │            ┌─────────────┐
       └───────────>│   DBEngine   │
                    │  (数据访问)  │
                    └─────────────┘
                           │
                           ▼
                    ┌─────────────┐
                    │   Database   │
                    └─────────────┘
```

---

## 6. 关键设计模式

### 6.1 分层架构

```
┌─────────────────────────────────────┐
│           表现层 (UI)                │  UltraANetT, FileEditor, ReportViewer
├─────────────────────────────────────┤
│           业务逻辑层                 │  ProcessEngine
├─────────────────────────────────────┤
│           数据访问层                 │  DBEngine, CANoeEngine
├─────────────────────────────────────┤
│           数据模型层                 │  Model
└─────────────────────────────────────┘
```

### 6.2 工厂模式

**NHelper** - NHibernate会话工厂
```csharp
public class NHelper
{
    private static ISessionFactory _sessionFactory;
    
    public static ISessionFactory SessionFactory
    {
        get
        {
            if (_sessionFactory == null)
            {
                _sessionFactory = CreateSessionFactory();
            }
            return _sessionFactory;
        }
    }
}
```

### 6.3 枚举模式

**EnumLibrary** - 集中管理所有枚举
```csharp
public class EnumLibrary
{
    public enum EnumTable { ... }
    public enum TestType { ... }
    public enum TestResult { ... }
}
```

### 6.4 状态机模式

**ProcCANoeTest** - 测试状态管理
```csharp
public class ProcCANoeTest
{
    public bool endHFlag = false;  // 隐性用例结束
    public bool endDFlag = false;  // 显性用例结束
    
    // 通过标志位控制测试流程
}
```

---

## 7. 集成建议

### 7.1 Web API改造方案

基于以上分析，建议的Web API项目结构：

```
CANoeMiddleware.API/
├── Controllers/
│   ├── TestController.cs           # 测试控制
│   ├── ConfigController.cs         # 配置管理
│   ├── StatusController.cs         # 状态查询
│   └── ReportController.cs         # 报告管理
├── Services/
│   ├── TestExecutionService.cs     # 测试执行服务
│   ├── CanoeWrapperService.cs      # CANoe包装服务
│   └── ConfigGenerationService.cs  # 配置生成服务
├── Models/
│   ├── ApiModels.cs                # API请求/响应模型
│   └── DtoModels.cs                # 数据传输对象
└── Program.cs
```

### 7.2 核心服务设计

**TestExecutionService**
```csharp
public class TestExecutionService
{
    private readonly ProcCANoeTest _canoeTest;
    
    public async Task<TestResult> ExecuteTestAsync(TestRequest request)
    {
        // 1. 设置路径
        _canoeTest.GetName(request.SelfCheckPath, request.ConfigPath, request.StateMachinePath);
        
        // 2. 设备自检
        if (!await RunDeviceSelfCheckAsync())
            return TestResult.Failed("设备自检失败");
        
        // 3. 执行隐性用例
        if (request.ImplicitTestCases?.Any() == true)
        {
            var result = await RunImplicitTestsAsync(request.ImplicitTestCases, request.DtcInfo);
            if (!result.Success)
                return result;
        }
        
        // 4. 执行显性用例
        if (request.ExplicitTestCases?.Any() == true)
        {
            var result = await RunExplicitTestsAsync(request.ExplicitTestCases, request.DtcInfo);
            if (!result.Success)
                return result;
        }
        
        // 5. 生成报告
        var report = GenerateReport();
        
        return TestResult.Success(report);
    }
}
```

---

## 8. 总结

UltraANetT是一个功能完善的汽车网络测试管理系统，具有以下特点：

1. **分层架构清晰**: 表现层、业务逻辑层、数据访问层、模型层分离
2. **功能完整**: 覆盖测试任务管理、用例执行、报告生成全流程
3. **与CANoe深度集成**: 通过COM API实现完整的测试控制
4. **多数据库支持**: 支持SQLite、MySQL、SQL Server
5. **丰富的文件处理**: 支持INI、XML、Excel、ZIP等多种格式

**改造建议**:
- 优先复用 **CANoeEngine** 和 **ProcessEngine** 的核心功能
- 通过Web API封装原有功能，避免大规模重构
- 保持原有业务逻辑不变，仅增加API层
- 使用依赖注入管理服务对象

---
账号：123
密码：123
向日葵：634216249
o1d7tm
**文档版本**: 1.0  
**创建日期**: 2026-02-25  
**作者**: AI Assistant
