(function() {
    'use strict';

    // ==========================================================================
    // State Variables
    // ==========================================================================

    var tasks = {
        pending: [],
        running: [],
        completed: [],
        failed: []
    };

    var wsConnected = false;
    var socket = null;
    var pollInterval = null;
    var currentPanelTaskId = null;

    // ==========================================================================
    // DOM References
    // ==========================================================================

    var board = null;
    var strip = null;

    // ==========================================================================
    // Initialization
    // ==========================================================================

    function init() {
        board = document.getElementById('kanban-board');
        strip = document.getElementById('status-strip');

        loadTasks();
        initWebSocket();
        startPolling();
        bindEvents();
    }

    document.addEventListener('DOMContentLoaded', init);

    // ==========================================================================
    // WebSocket Section
    // ==========================================================================

    function initWebSocket() {
        socket = window.socketio;

        if (!socket) {
            console.warn('SocketIO instance not found on window.socketio');
            return;
        }

        // Note: SocketIO handles reconnection automatically with exponential backoff.
        // The connect/disconnect events will fire accordingly when the connection is restored.

        socket.on('connect', function() {
            wsConnected = true;
            updateWsIndicator(true);
        });

        socket.on('disconnect', function() {
            wsConnected = false;
            updateWsIndicator(false);
        });

        socket.on('task_response', function(data) {
            handleTaskEvent(data);
        });

        socket.on('cancel_response', function(data) {
            handleTaskEvent(data);
        });

        socket.on('error', function(data) {
            handleTaskError(data);
        });

        socket.on('task_progress', function(data) {
            handleTaskProgress(data);
        });
    }

    function updateWsIndicator(connected) {
        var dot = document.getElementById('ws-dot');
        var statusText = document.getElementById('ws-status-text');

        if (dot) {
            if (connected) {
                dot.classList.remove('disconnected');
            } else {
                dot.classList.add('disconnected');
            }
        }

        if (statusText) {
            statusText.textContent = connected ? 'Live' : 'Disconnected';
        }
    }

    // ==========================================================================
    // Polling Fallback
    // ==========================================================================

    function startPolling() {
        if (pollInterval) {
            return;
        }
        pollInterval = setInterval(loadTasks, 3000);
    }

    function stopPolling() {
        if (pollInterval) {
            clearInterval(pollInterval);
            pollInterval = null;
        }
    }

    // ==========================================================================
    // Data Loading
    // ==========================================================================

    function loadTasks() {
        Promise.all([
            fetch('/api/tasks?page=1&per_page=100'),
            fetch('/api/status')
        ])
        .then(function(responses) {
            return Promise.all(responses.map(function(res) {
                return res.json();
            }));
        })
        .then(function(results) {
            var taskResult = results[0];
            var statusResult = results[1];

            if (taskResult && taskResult.success && taskResult.data) {
                groupTasksByStatus(taskResult.data.tasks || []);
            }

            if (statusResult && statusResult.success && statusResult.data) {
                updateStatusStrip(statusResult.data);
            }
        })
        .catch(function(err) {
            console.warn('Failed to load tasks:', err);
        });
    }

    function groupTasksByStatus(taskList) {
        var pending = [];
        var running = [];
        var completed = [];
        var failed = [];

        for (var i = 0; i < taskList.length; i++) {
            var task = taskList[i];
            var status = task.status;

            if (status === 'pending' || status === 'queued') {
                pending.push(task);
            } else if (status === 'running' || status === 'starting') {
                running.push(task);
            } else if (status === 'completed' || status === 'passed') {
                completed.push(task);
            } else if (status === 'failed' || status === 'error' || status === 'timeout') {
                failed.push(task);
            } else {
                // Default to pending for unknown statuses
                pending.push(task);
            }
        }

        // Limit completed and failed to 50 items each
        tasks.completed = completed.slice(0, 50);
        tasks.failed = failed.slice(0, 50);
        tasks.pending = pending;
        tasks.running = running;

        renderBoard();
    }

    // ==========================================================================
    // Status Strip Updates
    // ==========================================================================

    function updateStatusStrip(status) {
        var healthPill = document.getElementById('health-pill');
        var stripTaskName = document.getElementById('strip-task-name');
        var stripProgressFill = document.getElementById('strip-progress-fill');
        var stripElapsed = document.getElementById('strip-elapsed');
        var stripCounters = document.getElementById('strip-counters');

        // Update health pill
        if (healthPill) {
            var preflightStatus = null;
            var diagnoseStatus = null;

            if (status && status.runtime_operations) {
                if (status.runtime_operations.preflight) {
                    preflightStatus = status.runtime_operations.preflight.status;
                }
                if (status.runtime_operations.diagnose) {
                    diagnoseStatus = status.runtime_operations.diagnose.status;
                }
            }

            var healthClassName = healthClass(preflightStatus, diagnoseStatus);
            var healthLabelText = healthLabel(preflightStatus, diagnoseStatus);

            healthPill.className = 'health-pill ' + healthClassName;

            var dot = healthPill.querySelector('.dot');
            if (dot) {
                dot.className = 'dot';
                if (healthClassName === 'blocked') {
                    dot.classList.add('error');
                } else if (healthClassName === 'warning') {
                    dot.classList.add('warning');
                }
            }

            var labelSpan = healthPill.querySelector('.label');
            if (labelSpan) {
                labelSpan.textContent = healthLabelText;
            }
        }

        // Update running task strip
        var runningTask = tasks.running && tasks.running.length > 0 ? tasks.running[0] : null;

        if (runningTask && stripTaskName) {
            stripTaskName.textContent = runningTask.task_name || runningTask.name || 'Running task';
        }

        if (runningTask && stripProgressFill) {
            var progress = 0;
            if (runningTask.progress !== undefined) {
                progress = runningTask.progress;
            } else if (runningTask.total && runningTask.total > 0) {
                progress = (runningTask.passed / runningTask.total) * 100;
            }
            stripProgressFill.style.width = progress + '%';
        }

        if (runningTask && stripElapsed && runningTask.started_at) {
            stripElapsed.textContent = formatElapsed(runningTask.started_at);
        }

        // Update counters - use textContent for dynamic values to prevent XSS
        if (stripCounters) {
            var runningCount = tasks.running ? tasks.running.length : 0;
            var completedCount = tasks.completed ? tasks.completed.length : 0;
            var failedCount = tasks.failed ? tasks.failed.length : 0;
            var pendingCount = tasks.pending ? tasks.pending.length : 0;

            stripCounters.innerHTML =
                '<div class="counter running"><span class="count"></span> Running</div>' +
                '<div class="counter completed"><span class="count"></span> Passed</div>' +
                '<div class="counter failed"><span class="count"></span> Failed</div>' +
                '<div class="counter pending"><span class="count"></span> Pending</div>';

            var countSpans = stripCounters.querySelectorAll('.count');
            if (countSpans[0]) countSpans[0].textContent = runningCount;
            if (countSpans[1]) countSpans[1].textContent = completedCount;
            if (countSpans[2]) countSpans[2].textContent = failedCount;
            if (countSpans[3]) countSpans[3].textContent = pendingCount;
        }
    }

    function healthClass(preflight, diagnose) {
        if (preflight === 'blocked' || diagnose === 'blocked') {
            return 'blocked';
        }
        if (preflight === 'warning' || diagnose === 'warning') {
            return 'warning';
        }
        return 'ready';
    }

    function healthLabel(preflight, diagnose) {
        if (preflight === 'blocked' || diagnose === 'blocked') {
            return 'System Blocked';
        }
        if (preflight === 'warning' || diagnose === 'warning') {
            return 'System Warning';
        }
        return 'System Ready';
    }

    function formatElapsed(startedAt) {
        if (!startedAt) {
            return '0:00 elapsed';
        }

        var startTime = new Date(startedAt).getTime();
        var now = Date.now();
        var elapsed = Math.max(0, now - startTime);

        var totalSeconds = Math.floor(elapsed / 1000);
        var minutes = Math.floor(totalSeconds / 60);
        var seconds = totalSeconds % 60;

        return minutes + ':' + (seconds < 10 ? '0' : '') + seconds + ' elapsed';
    }

    // ==========================================================================
    // Board Rendering
    // ==========================================================================

    function renderBoard() {
        renderColumn('pending', tasks.pending);
        renderColumn('running', tasks.running);
        renderColumn('completed', tasks.completed);
        renderColumn('failed', tasks.failed);
        updateColumnCounts();
    }

    function renderColumn(status, items) {
        var columnBody = document.querySelector('.kanban-column-body[data-column="' + status + '"]');

        if (!columnBody) {
            return;
        }

        // Clear existing content
        columnBody.innerHTML = '';

        if (items.length === 0) {
            columnBody.innerHTML = getEmptyStateHTML(status);
            return;
        }

        // Get existing cards for diffing
        var existingCards = columnBody.querySelectorAll('.kanban-card');
        var existingIds = [];

        for (var i = 0; i < existingCards.length; i++) {
            existingIds.push(existingCards[i].dataset.taskId);
        }

        var newIds = [];
        for (var j = 0; j < items.length; j++) {
            newIds.push(items[j].id || items[j].task_id);
        }

        // Remove cards not in new list
        for (var k = 0; k < existingCards.length; k++) {
            var cardId = existingCards[k].dataset.taskId;
            if (newIds.indexOf(cardId) === -1) {
                existingCards[k].parentNode.removeChild(existingCards[k]);
            }
        }

        // Add/update cards
        for (var l = 0; l < items.length; l++) {
            var task = items[l];
            var taskId = task.id || task.task_id;
            var existingCard = columnBody.querySelector('[data-task-id="' + taskId + '"]');

            if (existingCard) {
                updateCard(existingCard, task);
            } else {
                var newCard = createCard(task);
                columnBody.appendChild(newCard);
            }
        }
    }

    function createCard(task) {
        var div = document.createElement('div');
        div.className = 'kanban-card';
        // Note: taskId and status are set by refreshCardContent which is called below
        div.dataset.status = task.status;

        div.addEventListener('click', function() {
            openDetailPanel(task.id || task.task_id);
        });

        refreshCardContent(div, task);
        return div;
    }

    function updateCard(card, task) {
        refreshCardContent(card, task);
    }

    function refreshCardContent(card, task) {
        var status = task.status;
        var taskId = task.id || task.task_id;

        card.dataset.taskId = taskId;
        card.dataset.status = status;

        // Update card class based on status
        card.className = 'kanban-card';
        if (status === 'running' || status === 'starting') {
            card.classList.add('running');
        } else if (status === 'completed' || status === 'passed') {
            card.classList.add('completed');
        } else if (status === 'failed' || status === 'error' || status === 'timeout') {
            card.classList.add('failed');
        } else if (status === 'pending' || status === 'queued') {
            card.classList.add('pending');
        }

        var taskName = task.task_name || task.name || 'Untitled Task';
        var caseNo = task.case_no || '';
        var toolType = task.tool_type || '';

        var errorPreview = '';
        if ((status === 'failed' || status === 'error' || status === 'timeout') && task.error_message) {
            var errorMsg = task.error_message;
            if (errorMsg.length > 60) {
                errorMsg = errorMsg.substring(0, 60) + '...';
            }
            errorPreview =
                '<div class="card-error-preview">' +
                    '<div class="error-title"><i class="icon-error"></i>Error</div>' +
                    '<div class="error-message">' + escapeHtml(errorMsg) + '</div>' +
                '</div>';
        }

        var progressBar = '';
        var tallyHtml = '';
        var quickAnalysis = '';

        if (status === 'running' || status === 'starting') {
            var progress = task.progress !== undefined ? task.progress : 0;
            var passed = task.passed || 0;
            var total = task.total || 0;

            progressBar =
                '<div class="card-progress">' +
                    '<div class="progress-bar">' +
                        '<div class="progress-fill" style="width:' + progress + '%"></div>' +
                    '</div>' +
                    '<div class="progress-text">' +
                        '<span>' + progress.toFixed(0) + '%</span>' +
                    '</div>' +
                '</div>';

            tallyHtml =
                '<div class="card-tally">' +
                    '<div class="tally-item passed"><i class="icon-check"></i> ' + passed + ' passed</div>' +
                    '<div class="tally-item failed"><i class="icon-x"></i> ' + (task.failed || 0) + ' failed</div>' +
                    '<div class="tally-item skipped"><i class="icon-skip"></i> ' + (task.skipped || 0) + ' skipped</div>' +
                '</div>';
        } else if (status === 'completed' || status === 'passed') {
            var passRate = task.pass_rate !== undefined ? task.pass_rate : 0;
            var totalPassed = task.passed || 0;
            var totalFailed = task.failed || 0;
            var totalSkipped = task.skipped || 0;

            progressBar =
                '<div class="card-progress">' +
                    '<div class="progress-bar">' +
                        '<div class="progress-fill completed" style="width:100%"></div>' +
                    '</div>' +
                    '<div class="progress-text">' +
                        '<span>' + passRate.toFixed(1) + '% pass rate</span>' +
                    '</div>' +
                '</div>';

            tallyHtml =
                '<div class="card-tally">' +
                    '<div class="tally-item passed"><i class="icon-check"></i> ' + totalPassed + ' passed</div>' +
                    '<div class="tally-item failed"><i class="icon-x"></i> ' + totalFailed + ' failed</div>' +
                    '<div class="tally-item skipped"><i class="icon-skip"></i> ' + totalSkipped + ' skipped</div>' +
                '</div>';
        } else if (status === 'failed' || status === 'error' || status === 'timeout') {
            var passRate = task.pass_rate !== undefined ? task.pass_rate : 0;
            var totalPassed = task.passed || 0;
            var totalFailed = task.failed || 0;
            var totalSkipped = task.skipped || 0;

            tallyHtml =
                '<div class="card-tally">' +
                    '<div class="tally-item passed"><i class="icon-check"></i> ' + totalPassed + ' passed</div>' +
                    '<div class="tally-item failed"><i class="icon-x"></i> ' + totalFailed + ' failed</div>' +
                    '<div class="tally-item skipped"><i class="icon-skip"></i> ' + totalSkipped + ' skipped</div>' +
                '</div>';

            quickAnalysis =
                '<div class="card-quick-analysis">' +
                    '<button class="card-quick-analysis-btn" type="button">' +
                        '<i class="fas fa-search-plus"></i> Quick Analysis' +
                    '</button>' +
                    '<div class="card-quick-analysis-content">' + escapeHtml(task.error_message || '') + '</div>' +
                '</div>';
        }

        card.innerHTML =
            '<div class="card-header">' +
                '<div class="card-title-row">' +
                    '<div class="card-title">' + escapeHtml(taskName) + '</div>' +
                    '<div class="card-subtitle">' + escapeHtml(caseNo) + '</div>' +
                '</div>' +
                '<span class="card-status-badge ' + status + '">' + statusLabel(status) + '</span>' +
            '</div>' +
            '<div class="card-meta">' +
                '<div class="meta-item">' +
                    '<i class="icon-clock"></i>' +
                    '<span>' + (task.created_at ? formatElapsed(task.created_at) : 'N/A') + '</span>' +
                '</div>' +
                '<span class="tool-badge">' + escapeHtml(toolType) + '</span>' +
            '</div>' +
            progressBar +
            tallyHtml +
            errorPreview +
            quickAnalysis;
    }

    function updateColumnCounts() {
        var columns = ['pending', 'running', 'completed', 'failed'];

        for (var i = 0; i < columns.length; i++) {
            var status = columns[i];
            var countSpan = document.querySelector('.kanban-column-header[data-column="' + status + '"] .column-count');
            if (countSpan) {
                var count = tasks[status] ? tasks[status].length : 0;
                countSpan.textContent = count;
            }
        }
    }

    function getEmptyStateHTML(status) {
        var emptyStates = {
            pending: {
                icon: 'icon-clock',
                title: 'No Pending Tasks',
                text: 'Tasks waiting to be executed will appear here'
            },
            running: {
                icon: 'icon-play',
                title: 'No Running Tasks',
                text: 'Currently executing tasks will appear here'
            },
            completed: {
                icon: 'icon-check',
                title: 'No Completed Tasks',
                text: 'Successfully completed tasks will appear here'
            },
            failed: {
                icon: 'icon-x',
                title: 'No Failed Tasks',
                text: 'Failed tasks will appear here for quick analysis'
            }
        };

        var state = emptyStates[status] || {
            icon: 'icon-list',
            title: 'No Tasks',
            text: 'No tasks in this category'
        };

        return '<div class="kanban-column-empty">' +
            '<div class="empty-icon"><i class="' + state.icon + '"></i></div>' +
            '<div class="empty-text">' +
                '<strong>' + state.title + '</strong>' +
                state.text +
            '</div>' +
        '</div>';
    }

    // ==========================================================================
    // Detail Slide Panel
    // ==========================================================================

    function openDetailPanel(taskId) {
        var overlay = document.getElementById('detail-overlay');
        var panel = document.getElementById('detail-slide-panel');

        if (!overlay || !panel) {
            console.warn('Detail panel elements not found');
            return;
        }

        overlay.classList.add('active');
        panel.classList.add('active');
        currentPanelTaskId = taskId;

        fetch('/api/tasks/' + taskId)
            .then(function(res) {
                if (!res.ok) {
                    console.error('Failed to load task detail: HTTP ' + res.status);
                    return;
                }
                return res.json();
            })
            .then(function(result) {
                // Validate response structure
                if (!result || !result.success || !result.data) {
                    console.error('Invalid task detail response:', result);
                    return;
                }
                renderPanelContent(result.data);
            })
            .catch(function(err) {
                console.warn('Failed to fetch task details:', err);
            });
    }

    function closeDetailPanel() {
        var overlay = document.getElementById('detail-overlay');
        var panel = document.getElementById('detail-slide-panel');

        if (overlay) {
            overlay.classList.remove('active');
        }
        if (panel) {
            panel.classList.remove('active');
        }
        currentPanelTaskId = null;
    }

    function renderPanelContent(task) {
        if (typeof window.renderPanelBasic === 'function') {
            window.renderPanelBasic(task);
        }
        if (typeof window.renderPanelExecution === 'function') {
            window.renderPanelExecution(task);
        }
        if (typeof window.renderPanelResults === 'function') {
            window.renderPanelResults(task);
        }
        if (typeof window.renderPanelLogs === 'function') {
            window.renderPanelLogs(task);
        }
    }

    // ==========================================================================
    // Quick Analysis Toggle
    // ==========================================================================

    window.toggleQuickAnalysis = function(btn) {
        if (!btn) return;

        var content = btn.nextElementSibling;
        if (!content) return;

        if (content.classList.contains('quick-analysis-content')) {
            content.classList.toggle('open');
        }
    };

    // ==========================================================================
    // Focus Mode
    // ==========================================================================

    window.toggleFocusMode = function() {
        var body = document.body;
        var btn = document.getElementById('focus-mode-btn');

        body.classList.toggle('focus-mode');

        if (btn) {
            if (body.classList.contains('focus-mode')) {
                btn.innerHTML = '<i class="icon-fullscreen-exit"></i> Exit Focus';
            } else {
                btn.innerHTML = '<i class="icon-fullscreen"></i> Focus Mode';
            }
        }
    };

    // ==========================================================================
    // Event Binding
    // ==========================================================================

    function bindEvents() {
        // Escape key to close panel
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                closeDetailPanel();
            }
        });

        // Click on overlay to close panel
        var overlay = document.getElementById('detail-overlay');
        if (overlay) {
            overlay.addEventListener('click', function() {
                closeDetailPanel();
            });
        }

        // Click on panel close button
        var closeBtn = document.getElementById('panel-close-btn');
        if (closeBtn) {
            closeBtn.addEventListener('click', function() {
                closeDetailPanel();
            });
        }

        // Panel tab clicks
        var panelTabs = document.querySelectorAll('.panel-tab');
        for (var i = 0; i < panelTabs.length; i++) {
            panelTabs[i].addEventListener('click', function() {
                var tabName = this.dataset.tab;

                // Update active tab
                panelTabs.forEach(function(tab) {
                    tab.classList.remove('active');
                });
                this.classList.add('active');

                // Update active content
                var panelContents = document.querySelectorAll('.panel-content');
                panelContents.forEach(function(content) {
                    content.classList.remove('active');
                });

                var activeContent = document.getElementById('panel-' + tabName);
                if (activeContent) {
                    activeContent.classList.add('active');
                }
            });
        }

        // Event delegation for quick analysis button on kanban cards
        // Using delegation because cards are re-rendered via innerHTML
        if (board) {
            board.addEventListener('click', function(e) {
                var btn = e.target.closest('.card-quick-analysis-btn');
                if (btn) {
                    e.stopPropagation();
                    toggleQuickAnalysis(btn);
                }
            });
        }
    }

    // ==========================================================================
    // WebSocket Event Handlers
    // ==========================================================================

    function handleTaskEvent(data) {
        // Note: This re-fetches ALL data and does a full re-render for every event.
        // For a single-user dashboard this is acceptable, but in production
        // with many users or high event frequency, this could be optimized
        // to only update the affected task via the WebSocket payload.
        loadTasks();
    }

    function handleTaskError(data) {
        loadTasks();
    }

    function handleTaskProgress(data) {
        // Note: This function assumes specific DOM structure (CSS class names).
        // Acceptable for now since CSS class names are stable and unlikely to change frequently.
        // If DOM structure changes, this function will silently fail to update.
        var taskId = data.task_id || data.id;
        if (!taskId) return;

        var card = document.querySelector('.kanban-card[data-task-id="' + taskId + '"]');
        if (!card) return;

        // Update progress bar and tally without full re-render
        var progressFill = card.querySelector('.card-progress .progress-fill');
        var progressText = card.querySelector('.card-progress .progress-text');

        if (progressFill && data.progress !== undefined) {
            progressFill.style.width = data.progress + '%';
        }

        if (progressText && data.progress !== undefined) {
            progressText.innerHTML = '<span>' + data.progress.toFixed(0) + '%</span>';
        }

        // Update tally
        var passedItem = card.querySelector('.tally-item.passed');
        var failedItem = card.querySelector('.tally-item.failed');
        var skippedItem = card.querySelector('.tally-item.skipped');

        if (passedItem && data.passed !== undefined) {
            passedItem.innerHTML = '<i class="icon-check"></i> ' + data.passed + ' passed';
        }
        if (failedItem && data.failed !== undefined) {
            failedItem.innerHTML = '<i class="icon-x"></i> ' + data.failed + ' failed';
        }
        if (skippedItem && data.skipped !== undefined) {
            skippedItem.innerHTML = '<i class="icon-skip"></i> ' + data.skipped + ' skipped';
        }
    }

    // ==========================================================================
    // Utilities
    // ==========================================================================

    function escapeHtml(text) {
        if (!text) return '';

        var div = document.createElement('div');
        div.appendChild(document.createTextNode(text));
        return div.innerHTML;
    }

    function statusLabel(status) {
        var labels = {
            'pending': 'Pending',
            'queued': 'Queued',
            'running': 'Running',
            'starting': 'Starting',
            'completed': 'Passed',
            'passed': 'Passed',
            'failed': 'Failed',
            'error': 'Error',
            'timeout': 'Timeout',
            'cancelled': 'Cancelled',
            'canceled': 'Cancelled'
        };

        return labels[status] || status.charAt(0).toUpperCase() + status.slice(1);
    }

})();
