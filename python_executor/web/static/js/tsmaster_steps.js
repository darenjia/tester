/**
 * TSMaster 测试步骤编辑器
 * 
 * 提供 TSMaster 测试步骤的配置和执行功能
 */

// 测试步骤类型定义
const TestStepTypes = {
    signal_check: { label: '信号检查', icon: 'fa-wave-square', color: '#3b82f6' },
    signal_set: { label: '信号设置', icon: 'fa-edit', color: '#10b981' },
    lin_signal_check: { label: 'LIN信号检查', icon: 'fa-wave-square', color: '#8b5cf6' },
    lin_signal_set: { label: 'LIN信号设置', icon: 'fa-edit', color: '#f59e0b' },
    sysvar_check: { label: '系统变量检查', icon: 'fa-eye', color: '#06b6d4' },
    sysvar_set: { label: '系统变量设置', icon: 'fa-cog', color: '#ec4899' },
    message_send: { label: '发送报文', icon: 'fa-paper-plane', color: '#6366f1' },
    wait: { label: '等待', icon: 'fa-clock', color: '#9ca3af' },
    condition: { label: '条件判断', icon: 'fa-code-branch', color: '#f97316' }
};

// 全局状态
let tsmasterSteps = [];
let tsmasterConfig = {
    use_rpc: true,
    rpc_app_name: 'TSMasterTest',
    fallback_to_traditional: true,
    start_timeout: 30,
    stop_timeout: 10,
    project_path: '',
    auto_start_simulation: true,
    auto_stop_simulation: true
};
let stepTemplates = {};
let isExecuting = false;

// 默认测试步骤
const defaultTestSteps = [
    {
        id: 'step_001',
        name: '检查系统变量',
        type: 'sysvar_check',
        enabled: true,
        parameters: {
            var_name: 'Var0',
            expected_value: '1000'
        },
        timeout: 5000,
        retry_count: 0,
        on_failure: 'continue'
    },
    {
        id: 'step_002',
        name: '设置信号值',
        type: 'signal_set',
        enabled: true,
        parameters: {
            signal_name: 'EngineSpeed',
            value: 3000
        },
        timeout: 5000,
        retry_count: 0,
        on_failure: 'continue'
    },
    {
        id: 'step_003',
        name: '等待响应',
        type: 'wait',
        enabled: true,
        parameters: {
            duration: 2000
        },
        timeout: 3000,
        retry_count: 0,
        on_failure: 'continue'
    },
    {
        id: 'step_004',
        name: '验证信号值',
        type: 'signal_check',
        enabled: true,
        parameters: {
            signal_name: 'VehicleSpeed',
            expected_value: 60,
            tolerance: 5
        },
        timeout: 5000,
        retry_count: 0,
        on_failure: 'stop'
    }
];

/**
 * 初始化 TSMaster 测试步骤编辑器
 */
function initTSMasterStepsEditor() {
    loadStepTemplates();
    renderTSMasterConfig();
    renderTSMasterSteps();
    bindTSMasterEvents();
}

/**
 * 加载步骤模板
 */
async function loadStepTemplates() {
    try {
        const response = await fetch('/api/functional-tests/tsmaster/step-templates');
        const result = await response.json();
        
        if (result.success) {
            stepTemplates = result.data.templates;
            console.log('步骤模板加载成功:', stepTemplates);
        } else {
            console.error('加载步骤模板失败:', result.message);
        }
    } catch (error) {
        console.error('加载步骤模板异常:', error);
    }
}

/**
 * 渲染 TSMaster 配置面板
 */
function renderTSMasterConfig() {
    const container = document.getElementById('tsmaster-config-panel');
    if (!container) return;
    
    container.innerHTML = `
        <div class="tsmaster-config-section">
            <h4><i class="fas fa-microchip"></i> RPC 配置</h4>
            <div class="config-row">
                <label class="config-checkbox">
                    <input type="checkbox" id="tsm-use-rpc" ${tsmasterConfig.use_rpc ? 'checked' : ''}>
                    <span>启用 RPC 模式（高性能）</span>
                </label>
            </div>
            <div class="config-row">
                <label>应用名称:</label>
                <input type="text" id="tsm-rpc-app-name" value="${tsmasterConfig.rpc_app_name || ''}" 
                       placeholder="TSMasterTest">
            </div>
            <div class="config-row">
                <label class="config-checkbox">
                    <input type="checkbox" id="tsm-fallback" ${tsmasterConfig.fallback_to_traditional ? 'checked' : ''}>
                    <span>RPC 失败时回退到传统模式</span>
                </label>
            </div>
            <div class="config-row">
                <button class="btn btn-secondary btn-sm" onclick="testRPCConnection()">
                    <i class="fas fa-plug"></i> 测试连接
                </button>
            </div>
        </div>
        
        <div class="tsmaster-config-section">
            <h4><i class="fas fa-cog"></i> 工程配置</h4>
            <div class="config-row">
                <label>工程文件路径:</label>
                <input type="text" id="tsm-project-path" value="${tsmasterConfig.project_path || ''}" 
                       placeholder="C:/TestWorkspace/test.tproj">
            </div>
            <div class="config-row">
                <label class="config-checkbox">
                    <input type="checkbox" id="tsm-auto-start" ${tsmasterConfig.auto_start_simulation ? 'checked' : ''}>
                    <span>自动启动仿真</span>
                </label>
            </div>
            <div class="config-row">
                <label class="config-checkbox">
                    <input type="checkbox" id="tsm-auto-stop" ${tsmasterConfig.auto_stop_simulation ? 'checked' : ''}>
                    <span>自动停止仿真</span>
                </label>
            </div>
        </div>
        
        <div class="tsmaster-config-section">
            <h4><i class="fas fa-clock"></i> 超时配置</h4>
            <div class="config-row">
                <label>启动超时:</label>
                <input type="number" id="tsm-start-timeout" value="${tsmasterConfig.start_timeout}" min="1" max="300"> 秒
            </div>
            <div class="config-row">
                <label>停止超时:</label>
                <input type="number" id="tsm-stop-timeout" value="${tsmasterConfig.stop_timeout}" min="1" max="300"> 秒
            </div>
        </div>
    `;
}

/**
 * 渲染 TSMaster 测试步骤列表
 */
function renderTSMasterSteps() {
    const container = document.getElementById('tsmaster-steps-list');
    console.log('renderTSMasterSteps - 容器:', container ? '找到' : '未找到', '步骤数:', tsmasterSteps.length);
    
    if (!container) return;
    
    if (tsmasterSteps.length === 0) {
        container.innerHTML = `
            <div class="empty-steps">
                <i class="fas fa-list-ol"></i>
                <p>暂无测试步骤</p>
                <span>点击"添加步骤"创建测试步骤</span>
            </div>
        `;
        return;
    }
    
    let html = `
        <div class="steps-table-header">
            <span class="step-col-index">#</span>
            <span class="step-col-enable">启用</span>
            <span class="step-col-name">名称</span>
            <span class="step-col-type">类型</span>
            <span class="step-col-params">参数</span>
            <span class="step-col-actions">操作</span>
        </div>
    `;
    
    tsmasterSteps.forEach((step, index) => {
        const typeInfo = TestStepTypes[step.type] || { label: step.type, icon: 'fa-question', color: '#9ca3af' };
        const paramsSummary = getParamsSummary(step);
        
        html += `
            <div class="step-row ${step.enabled ? '' : 'disabled'}" data-index="${index}">
                <span class="step-col-index">${index + 1}</span>
                <span class="step-col-enable">
                    <input type="checkbox" ${step.enabled ? 'checked' : ''} 
                           onchange="toggleStepEnabled(${index})">
                </span>
                <span class="step-col-name">${step.name || '未命名'}</span>
                <span class="step-col-type">
                    <i class="fas ${typeInfo.icon}" style="color: ${typeInfo.color}"></i>
                    ${typeInfo.label}
                </span>
                <span class="step-col-params" title="${paramsSummary}">${paramsSummary}</span>
                <span class="step-col-actions">
                    <button class="btn-icon" onclick="editStep(${index})" title="编辑">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn-icon" onclick="moveStepUp(${index})" title="上移" ${index === 0 ? 'disabled' : ''}>
                        <i class="fas fa-arrow-up"></i>
                    </button>
                    <button class="btn-icon" onclick="moveStepDown(${index})" title="下移" ${index === tsmasterSteps.length - 1 ? 'disabled' : ''}>
                        <i class="fas fa-arrow-down"></i>
                    </button>
                    <button class="btn-icon btn-danger" onclick="deleteStep(${index})" title="删除">
                        <i class="fas fa-trash"></i>
                    </button>
                </span>
            </div>
        `;
    });
    
    container.innerHTML = html;
}

/**
 * 获取参数摘要
 */
function getParamsSummary(step) {
    const params = step.parameters || {};
    const keys = Object.keys(params);
    
    if (keys.length === 0) return '-';
    
    const summaries = keys.slice(0, 2).map(key => {
        const value = params[key];
        return `${key}=${value}`;
    });
    
    let summary = summaries.join(', ');
    if (keys.length > 2) {
        summary += ` ...(${keys.length - 2} more)`;
    }
    
    return summary;
}

/**
 * 绑定事件
 */
function bindTSMasterEvents() {
    // 配置变更事件
    document.addEventListener('change', function(e) {
        if (e.target.id?.startsWith('tsm-')) {
            updateTSMasterConfig();
        }
    });
}

/**
 * 更新配置
 */
function updateTSMasterConfig() {
    tsmasterConfig = {
        use_rpc: document.getElementById('tsm-use-rpc')?.checked ?? true,
        rpc_app_name: document.getElementById('tsm-rpc-app-name')?.value || 'TSMasterTest',
        fallback_to_traditional: document.getElementById('tsm-fallback')?.checked ?? true,
        start_timeout: parseInt(document.getElementById('tsm-start-timeout')?.value || 30),
        stop_timeout: parseInt(document.getElementById('tsm-stop-timeout')?.value || 10),
        project_path: document.getElementById('tsm-project-path')?.value || '',
        auto_start_simulation: document.getElementById('tsm-auto-start')?.checked ?? true,
        auto_stop_simulation: document.getElementById('tsm-auto-stop')?.checked ?? true
    };
}

/**
 * 测试 RPC 连接
 */
async function testRPCConnection() {
    updateTSMasterConfig();
    
    const btn = event.target.closest('button');
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> 测试中...';
    btn.disabled = true;
    
    try {
        const response = await fetch('/api/functional-tests/tsmaster/test-rpc', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ rpc_app_name: tsmasterConfig.rpc_app_name })
        });
        
        const result = await response.json();
        
        if (result.success && result.data.connected) {
            showMessage(`RPC 连接成功！模式: ${result.data.mode}`, 'success');
        } else {
            showMessage(`连接失败: ${result.data.message}`, 'error');
        }
    } catch (error) {
        showMessage('测试连接异常: ' + error.message, 'error');
    } finally {
        btn.innerHTML = originalText;
        btn.disabled = false;
    }
}

/**
 * 显示添加步骤对话框
 */
function showAddStepDialog() {
    showStepDialog(null, -1);
}

/**
 * 显示步骤编辑对话框
 */
function showStepDialog(step, index) {
    const isEdit = index >= 0;
    const dialogTitle = isEdit ? '编辑测试步骤' : '添加测试步骤';
    
    // 生成步骤类型选项
    let typeOptions = '';
    for (const [type, info] of Object.entries(TestStepTypes)) {
        const selected = step?.type === type ? 'selected' : '';
        typeOptions += `<option value="${type}" ${selected}>${info.label}</option>`;
    }
    
    // 生成参数表单
    const paramsHtml = generateParamsForm(step?.type, step?.parameters);
    
    const dialogHtml = `
        <div class="modal-overlay" id="step-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h3>${dialogTitle}</h3>
                    <button class="btn-close" onclick="closeStepDialog()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <label>步骤名称:</label>
                        <input type="text" id="step-name" value="${step?.name || ''}" placeholder="输入步骤名称">
                    </div>
                    <div class="form-group">
                        <label>步骤类型:</label>
                        <select id="step-type" onchange="onStepTypeChange()">
                            <option value="">请选择类型</option>
                            ${typeOptions}
                        </select>
                    </div>
                    <div class="form-group">
                        <label class="checkbox-label">
                            <input type="checkbox" id="step-enabled" ${step?.enabled !== false ? 'checked' : ''}>
                            启用此步骤
                        </label>
                    </div>
                    <div id="step-params-container">
                        ${paramsHtml}
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label>超时时间 (ms):</label>
                            <input type="number" id="step-timeout" value="${step?.timeout || 5000}" min="100" max="60000">
                        </div>
                        <div class="form-group">
                            <label>失败处理:</label>
                            <select id="step-on-failure">
                                <option value="continue" ${step?.on_failure === 'continue' ? 'selected' : ''}>继续执行</option>
                                <option value="stop" ${step?.on_failure === 'stop' ? 'selected' : ''}>停止执行</option>
                                <option value="abort" ${step?.on_failure === 'abort' ? 'selected' : ''}>中止测试</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-secondary" onclick="closeStepDialog()">取消</button>
                    <button class="btn btn-primary" onclick="saveStep(${index})">确定</button>
                </div>
            </div>
        </div>
    `;
    
    // 添加对话框到页面
    const existingDialog = document.getElementById('step-dialog');
    if (existingDialog) {
        existingDialog.remove();
    }
    
    document.body.insertAdjacentHTML('beforeend', dialogHtml);
}

/**
 * 生成参数表单
 */
function generateParamsForm(stepType, parameters = {}) {
    if (!stepType || !stepTemplates[stepType]) {
        return '<p class="params-hint">请选择步骤类型以配置参数</p>';
    }
    
    const template = stepTemplates[stepType];
    const paramsDef = template.parameters || {};
    
    let html = '<div class="params-section"><h4>参数配置</h4>';
    
    for (const [paramName, paramDef] of Object.entries(paramsDef)) {
        const value = parameters[paramName] ?? paramDef.default ?? '';
        const required = paramDef.required ? '<span class="required">*</span>' : '';
        
        html += `<div class="form-group">`;
        html += `<label>${paramDef.label}${required}:</label>`;
        
        if (paramDef.type === 'boolean') {
            html += `
                <select id="param-${paramName}">
                    <option value="true" ${value === true || value === 'true' ? 'selected' : ''}>是</option>
                    <option value="false" ${value === false || value === 'false' ? 'selected' : ''}>否</option>
                </select>
            `;
        } else if (paramDef.type === 'array') {
            html += `<input type="text" id="param-${paramName}" value="${Array.isArray(value) ? value.join(',') : value}" placeholder="逗号分隔的值">`;
        } else {
            html += `<input type="${paramDef.type === 'number' || paramDef.type === 'integer' ? 'number' : 'text'}" 
                           id="param-${paramName}" value="${value}" 
                           placeholder="${paramDef.label}">`;
        }
        
        html += `</div>`;
    }
    
    html += '</div>';
    return html;
}

/**
 * 步骤类型变更处理
 */
function onStepTypeChange() {
    const stepType = document.getElementById('step-type').value;
    const container = document.getElementById('step-params-container');
    container.innerHTML = generateParamsForm(stepType, {});
}

/**
 * 关闭步骤对话框
 */
function closeStepDialog() {
    const dialog = document.getElementById('step-dialog');
    if (dialog) {
        dialog.remove();
    }
}

/**
 * 保存步骤
 */
function saveStep(index) {
    const name = document.getElementById('step-name').value.trim();
    const type = document.getElementById('step-type').value;
    const enabled = document.getElementById('step-enabled').checked;
    const timeout = parseInt(document.getElementById('step-timeout').value) || 5000;
    const onFailure = document.getElementById('step-on-failure').value;
    
    if (!name) {
        showMessage('请输入步骤名称', 'warning');
        return;
    }
    
    if (!type) {
        showMessage('请选择步骤类型', 'warning');
        return;
    }
    
    // 收集参数
    const parameters = {};
    const template = stepTemplates[type];
    if (template && template.parameters) {
        for (const paramName of Object.keys(template.parameters)) {
            const input = document.getElementById(`param-${paramName}`);
            if (input) {
                let value = input.value;
                const paramDef = template.parameters[paramName];
                
                // 类型转换
                if (paramDef.type === 'number' || paramDef.type === 'integer') {
                    value = parseFloat(value) || 0;
                } else if (paramDef.type === 'boolean') {
                    value = value === 'true';
                } else if (paramDef.type === 'array') {
                    value = value.split(',').map(v => v.trim()).filter(v => v);
                }
                
                parameters[paramName] = value;
            }
        }
    }
    
    const step = {
        id: index >= 0 ? tsmasterSteps[index].id : `step_${Date.now()}`,
        name,
        type,
        enabled,
        parameters,
        timeout,
        on_failure: onFailure
    };
    
    if (index >= 0) {
        tsmasterSteps[index] = step;
    } else {
        tsmasterSteps.push(step);
    }
    
    closeStepDialog();
    renderTSMasterSteps();
    showMessage(index >= 0 ? '步骤已更新' : '步骤已添加', 'success');
}

/**
 * 编辑步骤
 */
function editStep(index) {
    showStepDialog(tsmasterSteps[index], index);
}

/**
 * 删除步骤
 */
function deleteStep(index) {
    if (confirm('确定要删除此步骤吗？')) {
        tsmasterSteps.splice(index, 1);
        renderTSMasterSteps();
        showMessage('步骤已删除', 'success');
    }
}

/**
 * 切换步骤启用状态
 */
function toggleStepEnabled(index) {
    tsmasterSteps[index].enabled = !tsmasterSteps[index].enabled;
    renderTSMasterSteps();
}

/**
 * 上移步骤
 */
function moveStepUp(index) {
    if (index > 0) {
        [tsmasterSteps[index], tsmasterSteps[index - 1]] = [tsmasterSteps[index - 1], tsmasterSteps[index]];
        renderTSMasterSteps();
    }
}

/**
 * 下移步骤
 */
function moveStepDown(index) {
    if (index < tsmasterSteps.length - 1) {
        [tsmasterSteps[index], tsmasterSteps[index + 1]] = [tsmasterSteps[index + 1], tsmasterSteps[index]];
        renderTSMasterSteps();
    }
}

/**
 * 执行测试步骤
 */
async function executeTSMasterSteps() {
    if (tsmasterSteps.length === 0) {
        showMessage('请先添加测试步骤', 'warning');
        return;
    }
    
    if (isExecuting) {
        showMessage('测试正在执行中', 'warning');
        return;
    }
    
    updateTSMasterConfig();
    isExecuting = true;
    
    const btn = document.getElementById('btn-execute-steps');
    if (btn) {
        btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> 执行中...';
        btn.disabled = true;
    }
    
    // 显示执行日志区域
    const logContainer = document.getElementById('tsmaster-execution-log');
    if (logContainer) {
        logContainer.style.display = 'block';
        logContainer.innerHTML = '<div class="execution-header"><i class="fas fa-play-circle"></i> 测试执行中...</div>';
    }
    
    try {
        console.log('发送测试步骤:', tsmasterSteps);
        
        const response = await fetch('/api/functional-tests/tsmaster/execute-steps', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                config: tsmasterConfig,
                steps: tsmasterSteps
            })
        });
        
        const result = await response.json();
        console.log('执行结果:', result);
        
        if (result.success) {
            displayExecutionResult(result.data);
        } else {
            showMessage('执行失败: ' + result.message, 'error');
        }
    } catch (error) {
        console.error('执行异常:', error);
        showMessage('执行异常: ' + error.message, 'error');
    } finally {
        isExecuting = false;
        if (btn) {
            btn.innerHTML = '<i class="fas fa-play"></i> 执行全部';
            btn.disabled = false;
        }
    }
}

/**
 * 显示执行结果
 */
function displayExecutionResult(data) {
    console.log('displayExecutionResult - 数据:', data);
    
    const logContainer = document.getElementById('tsmaster-execution-log');
    if (!logContainer) {
        console.error('未找到执行日志容器');
        return;
    }
    
    const summary = data.summary || {};
    const results = data.results || [];
    
    console.log('执行结果统计:', summary);
    console.log('详细结果:', results);
    
    const statusClass = data.status === 'completed' ? 'success' : 
                       data.status === 'aborted' ? 'warning' : 'error';
    
    let html = `
        <div class="execution-header ${statusClass}">
            <i class="fas fa-${data.status === 'completed' ? 'check-circle' : 'exclamation-circle'}"></i>
            执行完成 - 总计: ${summary.total || 0}, 
            通过: <span class="text-success">${summary.passed || 0}</span>, 
            失败: <span class="text-error">${summary.failed || 0}</span>
            <span class="execution-duration">耗时: ${data.total_duration?.toFixed(2) || 0}s</span>
        </div>
        <div class="execution-results">
    `;
    
    results.forEach((result, index) => {
        const statusIcon = result.status === 'passed' ? 'check' : 
                          result.status === 'failed' ? 'times' : 'exclamation';
        const statusClass = result.status;
        
        html += `
            <div class="result-item ${statusClass}">
                <span class="result-index">${index + 1}</span>
                <span class="result-status"><i class="fas fa-${statusIcon}"></i></span>
                <span class="result-name">${result.name}</span>
                <span class="result-message">${result.message}</span>
                <span class="result-duration">${result.duration?.toFixed(2) || 0}s</span>
            </div>
        `;
    });
    
    html += '</div>';
    logContainer.innerHTML = html;
}

/**
 * 保存配置到本地存储
 */
function saveTSMasterConfig() {
    updateTSMasterConfig();
    localStorage.setItem('tsmaster_config', JSON.stringify(tsmasterConfig));
    localStorage.setItem('tsmaster_steps', JSON.stringify(tsmasterSteps));
    showMessage('配置已保存', 'success');
}

/**
 * 从本地存储加载配置
 */
function loadTSMasterConfig() {
    const savedConfig = localStorage.getItem('tsmaster_config');
    const savedSteps = localStorage.getItem('tsmaster_steps');
    
    console.log('loadTSMasterConfig - savedSteps:', savedSteps ? '有保存的步骤' : '无保存的步骤');
    
    if (savedConfig) {
        try {
            tsmasterConfig = { ...tsmasterConfig, ...JSON.parse(savedConfig) };
            console.log('已加载保存的配置');
        } catch (e) {
            console.error('加载配置失败:', e);
        }
    }
    
    if (savedSteps) {
        try {
            tsmasterSteps = JSON.parse(savedSteps);
            console.log('已加载保存的步骤，数量:', tsmasterSteps.length);
        } catch (e) {
            console.error('加载步骤失败:', e);
        }
    } else {
        // 如果没有保存的步骤，使用默认步骤
        tsmasterSteps = JSON.parse(JSON.stringify(defaultTestSteps));
        console.log('已加载默认测试步骤，数量:', tsmasterSteps.length);
        // 自动保存默认步骤到 localStorage
        localStorage.setItem('tsmaster_steps', JSON.stringify(tsmasterSteps));
        console.log('默认步骤已自动保存到本地存储');
    }
}

/**
 * 重置为默认测试步骤
 */
function resetToDefaultSteps() {
    if (confirm('确定要重置为默认测试步骤吗？当前步骤将被覆盖。')) {
        tsmasterSteps = JSON.parse(JSON.stringify(defaultTestSteps));
        renderTSMasterSteps();
        showMessage('已重置为默认测试步骤', 'success');
    }
}

/**
 * 清空所有测试步骤
 */
function clearAllSteps() {
    if (confirm('确定要清空所有测试步骤吗？')) {
        tsmasterSteps = [];
        renderTSMasterSteps();
        showMessage('已清空所有测试步骤', 'success');
    }
}

/**
 * 显示消息提示
 */
function showMessage(message, type = 'info') {
    // 使用全局的 showMessage 函数（如果存在）
    if (typeof window.showMessage === 'function') {
        window.showMessage(message, type);
        return;
    }
    
    // 简单的 alert 后备
    alert(message);
}

// 页面加载时初始化
document.addEventListener('DOMContentLoaded', function() {
    // 检查是否在功能测试页面
    if (document.getElementById('tsmaster-steps-editor')) {
        console.log('检测到 TSMaster 测试步骤编辑器');
        // 先加载配置（包括默认步骤）
        loadTSMasterConfig();
        console.log('配置加载完成，当前步骤数:', tsmasterSteps.length);
        // 然后初始化编辑器
        initTSMasterStepsEditor();
        // 确保步骤列表被渲染
        setTimeout(function() {
            renderTSMasterSteps();
            console.log('步骤列表渲染完成');
        }, 100);
    }
});

// 导出函数供外部使用
window.TSMasterStepsEditor = {
    init: initTSMasterStepsEditor,
    addStep: showAddStepDialog,
    execute: executeTSMasterSteps,
    saveConfig: saveTSMasterConfig,
    loadConfig: loadTSMasterConfig
};
