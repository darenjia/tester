# Mission Control UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Mission Control UI — a Kanban board with live status strip, real-time card updates, slide-in detail panel, and Focus Mode — replacing the current dashboard/table split for operators and test engineers.

**Architecture:** Frontend-only redesign. New `mission-control.html` template + companion CSS/JS files. `base.html` updated with sidebar rail + status strip. No backend changes. WebSocket events consumed from existing `main_production.py` SocketIO instance.

**Tech Stack:** Flask templates, vanilla JS (no framework), CSS animations, Flask-SocketIO for real-time.

---

## File Map

**New files:**
- `web/static/css/mission-control.css` — Kanban layout, card styles, animations, focus mode
- `web/static/js/mission-control.js` — Board rendering, WebSocket/polling, slide-in panel, quick analysis
- `web/templates/mission_control.html` — Kanban board HTML (extends base.html)

**Modified files:**
- `web/templates/base.html` — Sidebar rail mode, live status strip markup, focus mode CSS
- `web/static/css/style.css` — Add `--sidebar-collapsed-width: 60px` variable, sidebar rail transitions
- `web/server.py` — Redirect `/dashboard` to `mission_control.html`

---

## Task 1: CSS — Kanban Layout, Card Styles & Animations

**Files:**
- Create: `web/static/css/mission-control.css`
- Modify: `web/static/css/style.css`

- [ ] **Step 0: Add SocketIO client script to base.html**

In `web/templates/base.html`, find the `</body>` tag (near line 312) and add the SocketIO client before it:

```html
<!-- SocketIO client for real-time updates -->
<script src="https://cdn.socket.io/4.7.2/socket.io.min.js"></script>
<script>
    // Expose socketio instance globally for mission-control.js
    window.socketio = io();
</script>
```

- [ ] **Step 1: Add CSS variables to style.css**

Add after the existing `:root` block in `style.css`:

```css
/* Sidebar rail mode */
:root {
    /* ... existing variables already defined ... */
    --sidebar-collapsed-width: 60px;
    --sidebar-expanded-width: 220px;
    --status-strip-height: 48px;
}

/* Sidebar rail (collapsed) */
.sidebar.rail {
    width: var(--sidebar-collapsed-width);
    transition: width 0.25s ease;
}

.sidebar.rail .sidebar-header h1 span,
.sidebar.rail .nav-item span,
.sidebar.rail .sidebar-footer {
    display: none;
}

.sidebar.rail .nav-item {
    justify-content: center;
    padding: 14px;
}

.sidebar.rail .nav-item i {
    margin-right: 0;
}

.sidebar.rail .sidebar-header {
    padding: 16px 12px;
}

/* Sidebar hover expand */
.sidebar.rail:hover {
    width: var(--sidebar-expanded-width);
}

.sidebar.rail:hover .sidebar-header h1 span,
.sidebar.rail:hover .nav-item span,
.sidebar.rail:hover .sidebar-footer {
    display: block;
}

.sidebar.rail:hover .nav-item {
    justify-content: flex-start;
    padding: 12px 20px;
}

.sidebar.rail:hover .nav-item i {
    margin-right: 12px;
}

/* Focus mode: collapse sidebar fully */
body.focus-mode .sidebar {
    width: 0;
    overflow: hidden;
}

body.focus-mode .sidebar:hover {
    width: var(--sidebar-collapsed-width);
}
```

- [ ] **Step 2: Create mission-control.css**

Create `web/static/css/mission-control.css` with:

```css
/* ============================================
   Mission Control — Kanban Board
   ============================================ */

/* Status Strip */
.status-strip {
    height: var(--status-strip-height);
    background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
    color: white;
    display: flex;
    align-items: center;
    padding: 0 20px;
    gap: 20px;
    flex-shrink: 0;
    overflow-x: auto;
}

.status-strip .health-pill {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 4px 12px;
    border-radius: 999px;
    font-size: 13px;
    font-weight: 500;
    white-space: nowrap;
}

.status-strip .health-pill.ready { background: rgba(16, 185, 129, 0.2); color: #34d399; }
.status-strip .health-pill.warning { background: rgba(245, 158, 11, 0.2); color: #fbbf24; }
.status-strip .health-pill.blocked { background: rgba(239, 68, 68, 0.2); color: #f87171; }

.status-strip .health-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: currentColor;
}

.status-strip .running-task {
    display: flex;
    align-items: center;
    gap: 10px;
    flex: 1;
    min-width: 0;
}

.status-strip .running-task-name {
    font-size: 13px;
    font-weight: 500;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.status-strip .strip-progress {
    width: 120px;
    height: 6px;
    background: rgba(255,255,255,0.15);
    border-radius: 3px;
    overflow: hidden;
    flex-shrink: 0;
}

.status-strip .strip-progress-fill {
    height: 100%;
    background: #3b82f6;
    border-radius: 3px;
    transition: width 0.5s ease;
}

.status-strip .strip-counter {
    font-size: 12px;
    color: rgba(255,255,255,0.7);
    white-space: nowrap;
}

.status-strip .strip-counter span {
    color: white;
    font-weight: 600;
}

.status-strip .ws-indicator {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 12px;
    color: rgba(255,255,255,0.6);
    white-space: nowrap;
}

.status-strip .ws-indicator .dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
}

.status-strip .ws-indicator.connected .dot { background: #10b981; }
.status-strip .ws-indicator.disconnected .dot { background: #ef4444; }

.status-strip .focus-toggle {
    padding: 4px 10px;
    border-radius: 6px;
    background: rgba(255,255,255,0.1);
    border: none;
    color: white;
    font-size: 12px;
    cursor: pointer;
    white-space: nowrap;
    transition: background 0.2s;
}

.status-strip .focus-toggle:hover {
    background: rgba(255,255,255,0.2);
}

/* Kanban Board */
.kanban-board {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;
    height: calc(100vh - var(--header-height) - var(--status-strip-height) - 48px);
    min-height: 400px;
}

body.focus-mode .kanban-board {
    grid-template-columns: repeat(2, 1fr);
}

/* Kanban Column */
.kanban-column {
    display: flex;
    flex-direction: column;
    background: var(--bg-color);
    border-radius: 10px;
    overflow: hidden;
}

.kanban-column-header {
    padding: 12px 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-size: 13px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    border-bottom: 2px solid transparent;
}

.kanban-column-header .column-title {
    display: flex;
    align-items: center;
    gap: 8px;
}

.kanban-column-header .column-count {
    background: rgba(0,0,0,0.08);
    padding: 2px 8px;
    border-radius: 999px;
    font-size: 11px;
}

.kanban-column[data-status="pending"] .kanban-column-header { color: #6b7280; border-color: #e5e7eb; }
.kanban-column[data-status="running"] .kanban-column-header { color: #2563eb; border-color: #2563eb; }
.kanban-column[data-status="completed"] .kanban-column-header { color: #10b981; border-color: #10b981; }
.kanban-column[data-status="failed"] .kanban-column-header { color: #ef4444; border-color: #ef4444; }

.kanban-column-body {
    flex: 1;
    overflow-y: auto;
    padding: 12px;
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.kanban-column-empty {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: var(--text-secondary);
    font-size: 13px;
    text-align: center;
    padding: 20px;
    gap: 8px;
}

.kanban-column-empty i {
    font-size: 24px;
    opacity: 0.4;
}

/* Kanban Card */
.kanban-card {
    background: white;
    border-radius: 8px;
    padding: 14px;
    box-shadow: 0 1px 3px rgba(0,0,0,0.08);
    border-left: 3px solid transparent;
    cursor: pointer;
    transition: transform 0.15s, box-shadow 0.15s;
    animation: cardSlideIn 0.3s ease-out;
    position: relative;
}

@keyframes cardSlideIn {
    from { opacity: 0; transform: translateY(-8px); }
    to { opacity: 1; transform: translateY(0); }
}

.kanban-card:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(0,0,0,0.12);
}

.kanban-card[data-status="running"] {
    border-left-color: #2563eb;
    box-shadow: 0 2px 8px rgba(37, 99, 235, 0.15);
}

.kanban-card[data-status="completed"] {
    border-left-color: #10b981;
    background: linear-gradient(135deg, #f0fdf4 0%, white 100%);
}

.kanban-card[data-status="failed"] {
    border-left-color: #ef4444;
    background: linear-gradient(135deg, #fef2f2 0%, white 100%);
    animation: cardSlideIn 0.3s ease-out, failedPulse 0.5s ease-out 0.3s;
}

@keyframes failedPulse {
    0% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.4); }
    70% { box-shadow: 0 0 0 6px rgba(239, 68, 68, 0); }
    100% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
}

.kanban-card[data-status="pending"] {
    border-left-color: #9ca3af;
}

.card-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 8px;
}

.card-title {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-primary);
    word-break: break-all;
}

.card-status-badge {
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 11px;
    font-weight: 600;
    white-space: nowrap;
    flex-shrink: 0;
    margin-left: 8px;
}

.card-status-badge.running { background: #2563eb; color: white; }
.card-status-badge.completed { background: #10b981; color: white; }
.card-status-badge.failed { background: #ef4444; color: white; }
.card-status-badge.pending { background: #9ca3af; color: white; }

.card-meta {
    font-size: 12px;
    color: var(--text-secondary);
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
}

.card-meta-item {
    display: flex;
    align-items: center;
    gap: 4px;
}

.card-progress {
    margin-top: 10px;
}

.card-progress-bar {
    height: 5px;
    background: #e5e7eb;
    border-radius: 3px;
    overflow: hidden;
}

.card-progress-fill {
    height: 100%;
    background: #2563eb;
    border-radius: 3px;
    transition: width 0.5s ease;
}

.card-progress-fill.completed { background: #10b981; }
.card-progress-fill.failed { background: #ef4444; }

.card-progress-text {
    display: flex;
    justify-content: space-between;
    margin-top: 4px;
    font-size: 11px;
    color: var(--text-secondary);
}

/* Card result tally (running) */
.card-tally {
    margin-top: 8px;
    font-size: 12px;
    display: flex;
    gap: 8px;
}

.card-tally .passed { color: #10b981; font-weight: 600; }
.card-tally .failed { color: #ef4444; font-weight: 600; }

/* Failed card error preview */
.card-error-preview {
    margin-top: 8px;
    font-size: 12px;
    color: #ef4444;
    background: #fef2f2;
    padding: 6px 10px;
    border-radius: 4px;
    border-left: 2px solid #ef4444;
}

.card-quick-analysis {
    margin-top: 8px;
}

.card-quick-analysis-btn {
    width: 100%;
    padding: 6px;
    border: 1px solid #e5e7eb;
    border-radius: 4px;
    background: white;
    font-size: 12px;
    cursor: pointer;
    color: var(--text-primary);
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    transition: background 0.2s;
}

.card-quick-analysis-btn:hover {
    background: #f8f9fa;
}

.card-quick-analysis-content {
    display: none;
    margin-top: 8px;
    padding: 8px;
    background: #f8f9fa;
    border-radius: 4px;
    font-size: 12px;
    line-height: 1.5;
    color: var(--text-secondary);
}

.card-quick-analysis-content.open {
    display: block;
}

/* ============================================
   Slide-in Detail Panel
   ============================================ */

.detail-panel-overlay {
    display: none;
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    background: rgba(0,0,0,0.3);
    z-index: 500;
    opacity: 0;
    transition: opacity 0.25s ease;
}

.detail-panel-overlay.open {
    display: block;
    opacity: 1;
}

.detail-slide-panel {
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    width: 640px;
    max-width: 90vw;
    background: white;
    box-shadow: -4px 0 20px rgba(0,0,0,0.15);
    z-index: 501;
    transform: translateX(100%);
    transition: transform 0.3s ease;
    display: flex;
    flex-direction: column;
}

.detail-slide-panel.open {
    transform: translateX(0);
}

.panel-header {
    padding: 16px 20px;
    border-bottom: 1px solid var(--border-color);
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-shrink: 0;
}

.panel-header h3 {
    font-size: 16px;
    font-weight: 600;
}

.panel-close-btn {
    width: 32px;
    height: 32px;
    border-radius: 6px;
    border: none;
    background: var(--bg-color);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 18px;
    color: var(--text-secondary);
    transition: background 0.2s;
}

.panel-close-btn:hover {
    background: #e5e7eb;
}

.panel-tabs {
    display: flex;
    border-bottom: 1px solid var(--border-color);
    flex-shrink: 0;
}

.panel-tab {
    padding: 12px 20px;
    border: none;
    background: transparent;
    cursor: pointer;
    font-size: 14px;
    color: var(--text-secondary);
    border-bottom: 2px solid transparent;
    transition: all 0.2s;
}

.panel-tab:hover {
    color: var(--text-primary);
    background: var(--bg-color);
}

.panel-tab.active {
    color: var(--primary-color);
    border-bottom-color: var(--primary-color);
    font-weight: 500;
}

.panel-body {
    flex: 1;
    overflow-y: auto;
    padding: 20px;
}

.panel-content {
    display: none;
}

.panel-content.active {
    display: block;
}

/* Panel tab content styles */
.panel-hero {
    padding: 16px;
    border-radius: 8px;
    background: linear-gradient(135deg, #1e3a5f 0%, #2563eb 60%, #60a5fa 100%);
    color: white;
    margin-bottom: 16px;
}

.panel-hero h4 {
    margin: 0 0 6px;
    font-size: 16px;
}

.panel-hero p {
    margin: 0;
    font-size: 12px;
    opacity: 0.85;
}

.panel-hero .pass-rate {
    font-size: 32px;
    font-weight: 700;
}

.panel-info-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 12px;
    margin-bottom: 16px;
}

.panel-info-item {
    background: var(--bg-color);
    padding: 10px 12px;
    border-radius: 6px;
}

.panel-info-label {
    font-size: 11px;
    color: var(--text-secondary);
    margin-bottom: 2px;
}

.panel-info-value {
    font-size: 14px;
    font-weight: 500;
}

/* Panel results summary */
.panel-summary-grid {
    display: grid;
    grid-template-columns: repeat(5, 1fr);
    gap: 10px;
    margin-bottom: 16px;
}

.panel-summary-card {
    background: var(--bg-color);
    padding: 12px 8px;
    border-radius: 6px;
    text-align: center;
}

.panel-summary-value {
    font-size: 22px;
    font-weight: 700;
}

.panel-summary-label {
    font-size: 11px;
    color: var(--text-secondary);
    margin-top: 2px;
}

.panel-summary-card.passed .panel-summary-value { color: #10b981; }
.panel-summary-card.failed .panel-summary-value { color: #ef4444; }
.panel-summary-card.blocked .panel-summary-value { color: #f59e0b; }

/* Panel log list */
.panel-log-list {
    font-family: 'Consolas', monospace;
    font-size: 12px;
    background: #1e293b;
    border-radius: 8px;
    padding: 12px;
    max-height: 400px;
    overflow-y: auto;
}

.panel-log-item {
    padding: 4px 0;
    display: flex;
    gap: 10px;
    color: #e2e8f0;
}

.panel-log-time { color: #64748b; white-space: nowrap; }
.panel-log-level { min-width: 60px; font-weight: 600; }
.panel-log-level.INFO { color: #38bdf8; }
.panel-log-level.WARNING { color: #fbbf24; }
.panel-log-level.ERROR { color: #f87171; }
.panel-log-message { flex: 1; word-break: break-all; }
```

- [ ] **Step 3: Verify CSS loads correctly**

Run: Check that the CSS file is valid by loading the app and confirming no 404 errors in browser devtools.

---

## Task 2: JS — Mission Control Logic

**Files:**
- Create: `web/static/js/mission-control.js`

- [ ] **Step 1: Write mission-control.js skeleton**

Create `web/static/js/mission-control.js` with:

```javascript
/**
 * Mission Control Board
 * Real-time Kanban board with WebSocket/polling fallback
 */

(function() {
    'use strict';

    // State
    let tasks = { pending: [], running: [], completed: [], failed: [] };
    let wsConnected = false;
    let socket = null;
    let pollInterval = null;
    let currentPanelTaskId = null;

    // DOM refs
    const board = document.getElementById('kanban-board');
    const strip = document.getElementById('status-strip');

    // ============================================
    // Initialization
    // ============================================

    function init() {
        loadTasks();
        initWebSocket();
        startPolling();
        bindEvents();
    }

    // ============================================
    // WebSocket
    // ============================================

    function initWebSocket() {
        if (!window.socketio) return;
        socket = window.socketio;

        socket.on('connect', () => {
            wsConnected = true;
            updateWsIndicator(true);
        });

        socket.on('disconnect', () => {
            wsConnected = false;
            updateWsIndicator(false);
        });

        // Listen for task events
        socket.on('task_response', handleTaskEvent);
        socket.on('cancel_response', handleTaskEvent);
        socket.on('error', handleTaskError);

        // Listen for running task updates
        socket.on('task_progress', handleTaskProgress);
    }

    function updateWsIndicator(connected) {
        const el = document.getElementById('ws-dot');
        const strip = document.getElementById('ws-status');
        if (!el) return;
        el.className = 'dot ' + (connected ? 'connected' : 'disconnected');
        if (strip) strip.textContent = connected ? '● Connected' : '○ Disconnected';
    }

    // ============================================
    // Polling fallback
    // ============================================

    function startPolling() {
        if (pollInterval) return;
        pollInterval = setInterval(loadTasks, 3000);
    }

    function stopPolling() {
        if (pollInterval) {
            clearInterval(pollInterval);
            pollInterval = null;
        }
    }

    // ============================================
    // Data loading
    // ============================================

    async function loadTasks() {
        try {
            const [tasksRes, statusRes] = await Promise.all([
                fetch('/api/tasks?page=1&per_page=100'),
                fetch('/api/status')
            ]);
            const tasksData = await tasksRes.json();
            const statusData = await statusRes.json();

            if (tasksData.success) {
                groupTasksByStatus(tasksData.data.tasks);
            }

            if (statusData.success) {
                updateStatusStrip(statusData.data);
            }
        } catch (e) {
            console.warn('Polling failed:', e);
        }
    }

    function groupTasksByStatus(taskList) {
        const newTasks = { pending: [], running: [], completed: [], failed: [] };
        for (const t of taskList || []) {
            const s = t.status;
            if (s === 'pending') newTasks.pending.push(t);
            else if (s === 'running') newTasks.running.push(t);
            else if (s === 'completed') newTasks.completed.push(t);
            else if (s === 'failed' || s === 'timeout' || s === 'cancelled') newTasks.failed.push(t);
            else newTasks.pending.push(t);
        }
        // Limit completed/failed to 50
        newTasks.completed = newTasks.completed.slice(0, 50);
        newTasks.failed = newTasks.failed.slice(0, 50);
        tasks = newTasks;
        renderBoard();
    }

    // ============================================
    // Status Strip
    // ============================================

    function updateStatusStrip(status) {
        if (!status) return;

        // Health pill
        const preflight = status.runtime_operations?.preflight?.status;
        const diagnose = status.runtime_operations?.diagnose?.status;
        const pill = document.getElementById('health-pill');
        if (pill) {
            pill.className = 'health-pill ' + healthClass(preflight, diagnose);
            pill.innerHTML = `<span class="health-dot"></span>${healthLabel(preflight, diagnose)}`;
        }

        // Running task strip
        const running = tasks.running[0];
        const stripName = document.getElementById('strip-task-name');
        const stripProgress = document.getElementById('strip-progress-fill');
        const stripTime = document.getElementById('strip-elapsed');

        if (running) {
            if (stripName) stripName.textContent = running.name || running.id || 'Running task';
            if (stripProgress) {
                const pct = running.progress || 0;
                stripProgress.style.width = pct + '%';
            }
            if (stripTime) stripTime.textContent = formatElapsed(running.execution?.started_at);
        } else {
            if (stripName) stripName.textContent = 'No task running';
            if (stripProgress) stripProgress.style.width = '0%';
            if (stripTime) stripTime.textContent = '';
        }

        // Counters
        const counters = document.getElementById('strip-counters');
        if (counters) {
            counters.innerHTML =
                `<span>${tasks.running.length} running</span> · ` +
                `<span>${tasks.completed.length} done</span> · ` +
                `<span>${tasks.failed.length} failed</span> · ` +
                `<span>${tasks.pending.length} pending</span>`;
        }
    }

    function healthClass(preflight, diagnose) {
        const blocked = ['blocked', 'failed', 'offline'];
        const warn = ['warning'];
        if (blocked.includes(preflight) || blocked.includes(diagnose)) return 'blocked';
        if (warn.includes(preflight) || warn.includes(diagnose)) return 'warning';
        return 'ready';
    }

    function healthLabel(preflight, diagnose) {
        const blocked = ['blocked', 'failed', 'offline'];
        const warn = ['warning'];
        if (blocked.includes(preflight) || blocked.includes(diagnose)) return 'System Blocked';
        if (warn.includes(preflight) || warn.includes(diagnose)) return 'System Warning';
        return 'System Ready';
    }

    function formatElapsed(startedAt) {
        if (!startedAt) return '';
        const elapsed = (Date.now() - new Date(startedAt).getTime()) / 1000;
        const m = Math.floor(elapsed / 60);
        const s = Math.floor(elapsed % 60);
        return `${m}:${String(s).padStart(2, '0')} elapsed`;
    }

    // ============================================
    // Board Rendering
    // ============================================

    function renderBoard() {
        if (!board) return;
        renderColumn('pending', tasks.pending);
        renderColumn('running', tasks.running);
        renderColumn('completed', tasks.completed);
        renderColumn('failed', tasks.failed);
        updateColumnCounts();
    }

    function renderColumn(status, items) {
        const col = board.querySelector(`[data-column="${status}"] .kanban-column-body`);
        if (!col) return;

        if (items.length === 0) {
            col.innerHTML = getEmptyStateHTML(status);
            return;
        }

        // Only re-render changed cards to avoid flicker
        const existingIds = new Set(
            Array.from(col.querySelectorAll('.kanban-card')).map(c => c.dataset.taskId)
        );
        const newIds = new Set(items.map(t => t.id));

        // Remove cards no longer in this column
        existingIds.forEach(id => {
            if (!newIds.has(id)) {
                const el = col.querySelector(`[data-task-id="${id}"]`);
                if (el) el.remove();
            }
        });

        // Add or update cards
        items.forEach((task, index) => {
            let card = col.querySelector(`[data-task-id="${task.id}"]`);
            if (!card) {
                card = createCard(task);
                col.appendChild(card);
            } else {
                updateCard(card, task);
            }
        });
    }

    function createCard(task) {
        const div = document.createElement('div');
        div.className = 'kanban-card';
        div.dataset.taskId = task.id;
        div.dataset.status = task.status;
        div.addEventListener('click', () => openDetailPanel(task.id));
        refreshCardContent(div, task);
        return div;
    }

    function updateCard(card, task) {
        card.dataset.status = task.status;
        refreshCardContent(card, task);
    }

    function refreshCardContent(card, task) {
        const status = task.status;
        card.dataset.status = status;

        // Progress for running tasks
        const progress = task.progress || 0;
        const elapsed = formatElapsed(task.execution?.started_at);
        const tool = task.task_type || task.category || '';
        const passed = task.result_summary?.passed || 0;
        const total = task.result_summary?.total || 0;
        const passRate = task.result_summary?.pass_rate || '0%';
        const errorMsg = task.error_message || '';

        const isRunning = status === 'running';
        const isFailed = status === 'failed' || status === 'timeout';
        const isCompleted = status === 'completed';

        card.innerHTML = `
            <div class="card-header">
                <span class="card-title">${escapeHtml(task.name || task.id || 'Unknown')}</span>
                <span class="card-status-badge ${status}">${statusLabel(status)}</span>
            </div>
            <div class="card-meta">
                <span class="card-meta-item"><i class="fas fa-cog"></i>${escapeHtml(tool)}</span>
                ${elapsed ? `<span class="card-meta-item"><i class="fas fa-clock"></i>${elapsed}</span>` : ''}
            </div>
            ${isRunning ? `
                <div class="card-progress">
                    <div class="card-progress-bar">
                        <div class="card-progress-fill" style="width:${progress}%"></div>
                    </div>
                    <div class="card-progress-text">
                        <span>Progress</span><span>${progress}%</span>
                    </div>
                </div>
                ${total > 0 ? `<div class="card-tally"><span class="passed">✓ ${passed}/${total}</span></div>` : ''}
            ` : ''}
            ${isCompleted ? `
                <div class="card-progress">
                    <div class="card-progress-bar">
                        <div class="card-progress-fill completed" style="width:100%"></div>
                    </div>
                </div>
                <div class="card-tally">
                    <span class="passed">✓ ${passed}/${total} · ${passRate}</span>
                </div>
            ` : ''}
            ${isFailed && errorMsg ? `
                <div class="card-error-preview">${escapeHtml(errorMsg.substring(0, 60))}${errorMsg.length > 60 ? '…' : ''}</div>
                <div class="card-quick-analysis">
                    <button class="card-quick-analysis-btn" onclick="event.stopPropagation(); toggleQuickAnalysis(this)">
                        <i class="fas fa-search-plus"></i> Quick Analysis
                    </button>
                    <div class="card-quick-analysis-content">${escapeHtml(errorMsg)}</div>
                </div>
            ` : ''}
        `;
    }

    function updateColumnCounts() {
        ['pending', 'running', 'completed', 'failed'].forEach(status => {
            const el = document.querySelector(`[data-column="${status}"] .column-count`);
            if (el) el.textContent = tasks[status].length;
        });
    }

    function getEmptyStateHTML(status) {
        const messages = {
            pending: { icon: 'fa-inbox', text: 'No tasks in queue' },
            running: { icon: 'fa-play', text: 'No tasks running' },
            completed: { icon: 'fa-check-circle', text: 'No completed tasks' },
            failed: { icon: 'fa-check', text: 'No failed tasks — all clear!' }
        };
        const m = messages[status] || { icon: 'fa-circle', text: '' };
        return `
            <div class="kanban-column-empty">
                <i class="fas ${m.icon}"></i>
                <span>${m.text}</span>
            </div>
        `;
    }

    // ============================================
    // Detail Slide Panel
    // ============================================

    async function openDetailPanel(taskId) {
        currentPanelTaskId = taskId;
        const overlay = document.getElementById('detail-overlay');
        const panel = document.getElementById('detail-panel');
        if (!overlay || !panel) return;

        overlay.classList.add('open');
        panel.classList.add('open');

        // Load task detail
        try {
            const res = await fetch(`/api/tasks/${taskId}`);
            const result = await res.json();
            if (result.success) {
                renderPanelContent(result.data);
            }
        } catch (e) {
            console.error('Failed to load task detail:', e);
        }
    }

    function closeDetailPanel() {
        const overlay = document.getElementById('detail-overlay');
        const panel = document.getElementById('detail-panel');
        if (overlay) overlay.classList.remove('open');
        if (panel) panel.classList.remove('open');
        currentPanelTaskId = null;
    }

    function renderPanelContent(task) {
        // Overrideable: full implementation provided in mission_control.html
        // This minimal version is overridden by the HTML template's script block
    }

    // ============================================
    // Quick Analysis Toggle
    // ============================================

    window.toggleQuickAnalysis = function(btn) {
        const content = btn.nextElementSibling;
        if (content) content.classList.toggle('open');
    };

    // ============================================
    // Focus Mode
    // ============================================

    window.toggleFocusMode = function() {
        document.body.classList.toggle('focus-mode');
        const btn = document.getElementById('focus-toggle-btn');
        if (btn) {
            btn.textContent = document.body.classList.contains('focus-mode')
                ? 'Exit Focus'
                : 'Focus Mode';
        }
    };

    // ============================================
    // Event Binding
    // ============================================

    function bindEvents() {
        // Escape to close panel
        document.addEventListener('keydown', e => {
            if (e.key === 'Escape') closeDetailPanel();
        });

        // Overlay click to close
        const overlay = document.getElementById('detail-overlay');
        if (overlay) {
            overlay.addEventListener('click', closeDetailPanel);
        }

        // Panel close button
        const closeBtn = document.getElementById('panel-close-btn');
        if (closeBtn) {
            closeBtn.addEventListener('click', closeDetailPanel);
        }

        // Tab switching
        document.querySelectorAll('.panel-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                document.querySelectorAll('.panel-tab').forEach(t => t.classList.remove('active'));
                document.querySelectorAll('.panel-content').forEach(c => c.classList.remove('active'));
                tab.classList.add('active');
                const content = document.getElementById('panel-' + tab.dataset.tab);
                if (content) content.classList.add('active');
            });
        });
    }

    // ============================================
    // WebSocket Event Handlers
    // ============================================

    function handleTaskEvent(data) {
        loadTasks(); // Simple: reload all on any task event
    }

    function handleTaskError(data) {
        loadTasks();
    }

    function handleTaskProgress(data) {
        // Update specific running card without full re-render
        const card = document.querySelector(`[data-task-id="${data.taskId}"]`);
        if (card && data.progress !== undefined) {
            const fill = card.querySelector('.card-progress-fill');
            if (fill) fill.style.width = data.progress + '%';
            const tally = card.querySelector('.card-tally');
            if (tally && data.passed !== undefined && data.total !== undefined) {
                tally.innerHTML = `<span class="passed">✓ ${data.passed}/${data.total}</span>`;
            }
        }
    }

    // ============================================
    // Utilities
    // ============================================

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function statusLabel(status) {
        const labels = {
            pending: 'Pending',
            running: 'Running',
            completed: 'Passed',
            failed: 'Failed',
            timeout: 'Timeout',
            cancelled: 'Cancelled'
        };
        return labels[status] || status;
    }

    // ============================================
    // Boot
    // ============================================

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
```

- [ ] **Step 2: Verify JS has no syntax errors**

Run: `node --check web/static/js/mission-control.js` (or open in browser devtools)

---

## Task 3: HTML — Mission Control Template

**Files:**
- Create: `web/templates/mission_control.html`

- [ ] **Step 1: Write mission_control.html**

Create `web/templates/mission_control.html`:

```html
{% extends "base.html" %}

{% block title %}Mission Control - 测试执行器{% endblock %}
{% block page_title %}Mission Control{% endblock %}

{% block extra_css %}
<link rel="stylesheet" href="{{ url_for('static', filename='css/mission-control.css') }}">
{% endblock %}

{% block content %}
<!-- Status Strip -->
<div class="status-strip" id="status-strip">
    <span class="health-pill ready" id="health-pill">
        <span class="health-dot"></span> System Ready
    </span>

    <div class="running-task" id="running-task">
        <span class="running-task-name" id="strip-task-name">No task running</span>
        <div class="strip-progress">
            <div class="strip-progress-fill" id="strip-progress-fill" style="width:0%"></div>
        </div>
        <span class="strip-counter" id="strip-elapsed"></span>
    </div>

    <div class="strip-counter" id="strip-counters">
        <span>0 running</span> · <span>0 done</span> · <span>0 failed</span> · <span>0 pending</span>
    </div>

    <div class="ws-indicator connected" id="ws-status">
        <span class="dot connected" id="ws-dot"></span>
        <span id="ws-status-text">● Connected</span>
    </div>

    <button class="focus-toggle" id="focus-toggle-btn" onclick="toggleFocusMode()">
        <i class="fas fa-expand"></i> Focus Mode
    </button>
</div>

<!-- Kanban Board -->
<div class="kanban-board" id="kanban-board">
    <!-- Pending -->
    <div class="kanban-column" data-column="pending" data-status="pending">
        <div class="kanban-column-header">
            <span class="column-title">
                <i class="fas fa-clock"></i> Pending
            </span>
            <span class="column-count">0</span>
        </div>
        <div class="kanban-column-body">
            <div class="kanban-column-empty">
                <i class="fas fa-inbox"></i>
                <span>No tasks in queue — system is idle</span>
            </div>
        </div>
    </div>

    <!-- Running -->
    <div class="kanban-column" data-column="running" data-status="running">
        <div class="kanban-column-header">
            <span class="column-title">
                <i class="fas fa-spinner fa-spin"></i> Running
            </span>
            <span class="column-count">0</span>
        </div>
        <div class="kanban-column-body">
            <div class="kanban-column-empty">
                <i class="fas fa-play"></i>
                <span>No tasks currently running</span>
            </div>
        </div>
    </div>

    <!-- Completed -->
    <div class="kanban-column" data-column="completed" data-status="completed">
        <div class="kanban-column-header">
            <span class="column-title">
                <i class="fas fa-check-circle"></i> Completed
            </span>
            <span class="column-count">0</span>
        </div>
        <div class="kanban-column-body">
            <div class="kanban-column-empty">
                <i class="fas fa-check-circle"></i>
                <span>No completed tasks yet</span>
            </div>
        </div>
    </div>

    <!-- Failed -->
    <div class="kanban-column" data-column="failed" data-status="failed">
        <div class="kanban-column-header">
            <span class="column-title">
                <i class="fas fa-exclamation-triangle"></i> Failed
            </span>
            <span class="column-count">0</span>
        </div>
        <div class="kanban-column-body">
            <div class="kanban-column-empty">
                <i class="fas fa-check"></i>
                <span>No failed tasks — all clear!</span>
            </div>
        </div>
    </div>
</div>

<!-- Slide-in Detail Panel -->
<div class="detail-panel-overlay" id="detail-overlay"></div>
<div class="detail-slide-panel" id="detail-panel">
    <div class="panel-header">
        <h3>Task Detail</h3>
        <button class="panel-close-btn" id="panel-close-btn">
            <i class="fas fa-times"></i>
        </button>
    </div>

    <div class="panel-tabs">
        <button class="panel-tab active" data-tab="basic">Basic Info</button>
        <button class="panel-tab" data-tab="execution">Execution</button>
        <button class="panel-tab" data-tab="results">Results</button>
        <button class="panel-tab" data-tab="logs">Logs</button>
        <button class="panel-tab" data-tab="diagnosis">Diagnosis</button>
    </div>

    <div class="panel-body">
        <!-- Basic Info -->
        <div class="panel-content active" id="panel-basic"></div>
        <!-- Execution -->
        <div class="panel-content" id="panel-execution"></div>
        <!-- Results -->
        <div class="panel-content" id="panel-results"></div>
        <!-- Logs -->
        <div class="panel-content" id="panel-logs"></div>
        <!-- Diagnosis -->
        <div class="panel-content" id="panel-diagnosis"></div>
    </div>
</div>
{% endblock %}

{% block extra_js %}
<script src="{{ url_for('static', filename='js/mission-control.js') }}"></script>
<script>
// Panel content renderers (called from mission-control.js)
window.renderPanelBasic = function(task) {
    const el = document.getElementById('panel-basic');
    el.innerHTML = `
        <div class="panel-hero">
            <h4>${escapeHtml(task.name || task.id)}</h4>
            <p>${escapeHtml(task.id)} · ${escapeHtml((task.category || task.task_type || '-').toString().toUpperCase())}</p>
            <div class="pass-rate">${task.result_summary?.pass_rate || '0%'}</div>
        </div>
        <div class="panel-info-grid">
            <div class="panel-info-item">
                <div class="panel-info-label">Status</div>
                <div class="panel-info-value">${getStatusLabel(task.status)}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Tool</div>
                <div class="panel-info-value">${task.task_type || '-'}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Created</div>
                <div class="panel-info-value">${formatTime(task.created_at)}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Priority</div>
                <div class="panel-info-value">${getPriorityLabel(task.priority)}</div>
            </div>
            ${task.error_message ? `
            <div class="panel-info-item" style="grid-column:span 2">
                <div class="panel-info-label">Error</div>
                <div class="panel-info-value" style="color:#ef4444">${escapeHtml(task.error_message)}</div>
            </div>` : ''}
        </div>
    `;
};

window.renderPanelExecution = function(task) {
    const el = document.getElementById('panel-execution');
    el.innerHTML = `
        <div class="panel-info-grid">
            <div class="panel-info-item">
                <div class="panel-info-label">Started</div>
                <div class="panel-info-value">${formatTime(task.execution?.started_at)}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Completed</div>
                <div class="panel-info-value">${formatTime(task.execution?.completed_at)}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Duration</div>
                <div class="panel-info-value">${formatDuration(task.execution?.duration)}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Retry</div>
                <div class="panel-info-value">${task.execution?.retry_count || 0} / ${task.execution?.max_retries || 3}</div>
            </div>
        </div>
    `;
};

window.renderPanelResults = function(task) {
    const el = document.getElementById('panel-results');
    const summary = task.result_summary || {};
    const passRate = summary.pass_rate || '0%';
    el.innerHTML = `
        <div class="panel-summary-grid">
            <div class="panel-summary-card">
                <div class="panel-summary-value">${summary.total || 0}</div>
                <div class="panel-summary-label">Total</div>
            </div>
            <div class="panel-summary-card passed">
                <div class="panel-summary-value">${summary.passed || 0}</div>
                <div class="panel-summary-label">Passed</div>
            </div>
            <div class="panel-summary-card failed">
                <div class="panel-summary-value">${summary.failed || 0}</div>
                <div class="panel-summary-label">Failed</div>
            </div>
            <div class="panel-summary-card blocked">
                <div class="panel-summary-value">${summary.blocked || 0}</div>
                <div class="panel-summary-label">Blocked</div>
            </div>
            <div class="panel-summary-card">
                <div class="panel-summary-value">${passRate}</div>
                <div class="panel-summary-label">Pass Rate</div>
            </div>
        </div>
        ${task.test_results && task.test_results.length > 0 ? `
        <table style="width:100%;border-collapse:collapse;font-size:13px;">
            <thead>
                <tr style="background:#f8f9fa;text-align:left;">
                    <th style="padding:8px;border-bottom:1px solid #e5e7eb;">#</th>
                    <th style="padding:8px;border-bottom:1px solid #e5e7eb;">Name</th>
                    <th style="padding:8px;border-bottom:1px solid #e5e7eb;">Result</th>
                    <th style="padding:8px;border-bottom:1px solid #e5e7eb;">Error</th>
                </tr>
            </thead>
            <tbody>
                ${task.test_results.map((r,i) => `
                <tr>
                    <td style="padding:8px;border-bottom:1px solid #e5e7eb;color:#6b7280">${i+1}</td>
                    <td style="padding:8px;border-bottom:1px solid #e5e7eb;">${escapeHtml(r.name || '-')}</td>
                    <td style="padding:8px;border-bottom:1px solid #e5e7eb;">
                        <span style="padding:2px 8px;border-radius:3px;font-size:11px;font-weight:600;${r.passed ? 'background:#d1fae5;color:#065f46;' : 'background:#fee2e2;color:#991b1b;'}">
                            ${r.passed ? 'PASS' : 'FAIL'}
                        </span>
                    </td>
                    <td style="padding:8px;border-bottom:1px solid #e5e7eb;color:#ef4444;font-size:12px;">${escapeHtml(r.error || '-')}</td>
                </tr>`).join('')}
            </tbody>
        </table>` : '<p style="color:#6b7280;text-align:center;padding:20px;">No test results available</p>'}
    `;
};

window.renderPanelLogs = function(task) {
    const el = document.getElementById('panel-logs');
    const logs = task.recent_logs || [];
    el.innerHTML = logs.length > 0 ? `
        <div style="margin-bottom:12px;font-size:12px;color:#6b7280;">
            Showing ${logs.length} of ${task.log_stats?.total || logs.length} logs
        </div>
        <div class="panel-log-list">
            ${logs.map(l => `
            <div class="panel-log-item">
                <span class="panel-log-time">${formatLogTime(l.timestamp)}</span>
                <span class="panel-log-level ${l.level}">[${l.level}]</span>
                <span class="panel-log-message">${escapeHtml(l.message)}</span>
            </div>`).join('')}
        </div>` : '<p style="color:#6b7280;text-align:center;padding:20px;">No logs available</p>';
};

window.renderPanelDiagnosis = function(task) {
    const el = document.getElementById('panel-diagnosis');
    el.innerHTML = `
        <div class="panel-info-grid">
            <div class="panel-info-item">
                <div class="panel-info-label">Can Retry</div>
                <div class="panel-info-value">${task.can_retry ? 'Yes' : 'No'}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Log Count</div>
                <div class="panel-info-value">${task.log_stats?.total || 0}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Creator</div>
                <div class="panel-info-value">${task.created_by || '-'}</div>
            </div>
            <div class="panel-info-item">
                <div class="panel-info-label">Category</div>
                <div class="panel-info-value">${task.category || '-'}</div>
            </div>
        </div>
        ${task.params && Object.keys(task.params).length > 0 ? `
        <h4 style="font-size:13px;margin:16px 0 8px;">Parameters</h4>
        <pre class="json-block" style="font-size:12px;background:#f8f9fa;padding:12px;border-radius:6px;overflow:auto;max-height:200px;">${escapeHtml(JSON.stringify(task.params, null, 2))}</pre>` : ''}
        ${task.metadata && Object.keys(task.metadata).length > 0 ? `
        <h4 style="font-size:13px;margin:16px 0 8px;">Metadata</h4>
        <pre class="json-block" style="font-size:12px;background:#f8f9fa;padding:12px;border-radius:6px;overflow:auto;max-height:200px;">${escapeHtml(JSON.stringify(task.metadata, null, 2))}</pre>` : ''}
    `;
};

// Override renderPanelContent in mission-control.js
window.renderPanelContent = function(task) {
    window.renderPanelBasic(task);
    window.renderPanelExecution(task);
    window.renderPanelResults(task);
    window.renderPanelLogs(task);
    window.renderPanelDiagnosis(task);
    // Switch to basic tab
    document.querySelectorAll('.panel-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.panel-content').forEach(c => c.classList.remove('active'));
    document.querySelector('.panel-tab[data-tab="basic"]').classList.add('active');
    document.getElementById('panel-basic').classList.add('active');
};

// Helper functions
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function getStatusLabel(status) {
    const labels = { pending:'Pending', running:'Running', completed:'Completed', failed:'Failed', timeout:'Timeout', cancelled:'Cancelled' };
    return labels[status] || status;
}

function getPriorityLabel(priority) {
    return ['Low', 'Normal', 'High', 'Urgent'][priority] || 'Normal';
}

function formatTime(isoTime) {
    if (!isoTime) return '-';
    return new Date(isoTime).toLocaleString('zh-CN');
}

function formatDuration(seconds) {
    if (!seconds) return '-';
    if (seconds < 60) return Math.round(seconds) + 's';
    if (seconds < 3600) return Math.round(seconds / 60) + 'm';
    return (seconds / 3600).toFixed(1) + 'h';
}

function formatLogTime(isoTime) {
    if (!isoTime) return '-';
    const d = new Date(isoTime);
    return d.toLocaleTimeString('zh-CN', {hour:'2-digit',minute:'2-digit',second:'2-digit',hour12:false}) +
        '.' + String(d.getMilliseconds()).padStart(3,'0');
}
</script>
{% endblock %}
```

- [ ] **Step 2: Test the template renders**

Run the app and verify `/mission-control` loads without errors in browser console.

---

## Task 4: Modify base.html — Sidebar Rail + Status Strip

**Files:**
- Modify: `web/templates/base.html`

- [ ] **Step 1: Add sidebar rail class and status strip hook to base.html**

In `web/templates/base.html`, after the existing CSS block and before `{% block extra_css %}{% endblock %}`, add:

```html
<style>
/* Sidebar rail mode — applied via JS when on mission-control page */
body.mission-control .sidebar {
    width: var(--sidebar-collapsed-width, 60px);
    overflow: hidden;
    transition: width 0.25s ease;
}
body.mission-control .sidebar:hover {
    width: var(--sidebar-width, 220px);
}
body.mission-control .sidebar h1 span,
body.mission-control .nav-item span,
body.mission-control .sidebar-footer {
    display: none;
}
body.mission-control .sidebar:hover .sidebar h1 span,
body.mission-control .sidebar:hover .nav-item span,
body.mission-control .sidebar:hover .sidebar-footer {
    display: block;
}
body.mission-control .nav-item {
    justify-content: center;
    padding: 14px;
}
body.mission-control .nav-item i {
    margin-right: 0;
}
body.mission-control .sidebar:hover .nav-item {
    justify-content: flex-start;
    padding: 12px 20px;
}
body.mission-control .sidebar:hover .nav-item i {
    margin-right: 12px;
}
body.mission-control .sidebar-header {
    padding: 16px 12px;
}

/* Status strip placeholder — filled by mission-control page */
#status-strip-placeholder {
    display: none;
}
</style>
```

- [ ] **Step 2: Add body class for mission control detection**

In `<body>` tag, add `mission-control` class check. Find:
```html
<body>
```
Replace with:
```html
<body class="{% if active_page == 'dashboard' %}mission-control{% endif %}">
```

- [ ] **Step 3: Add placeholder for status strip**

Find the `<header class="top-bar">` section in base.html and add a placeholder div inside it after the top-actions div:

```html
<div id="status-strip-placeholder"></div>
```

- [ ] **Step 4: Commit**

```bash
git add web/templates/base.html
git commit -m "feat(ui): add sidebar rail mode and status strip placeholder to base"
```

---

## Task 5: Modify server.py — Route Dashboard to Mission Control

**Files:**
- Modify: `web/server.py`

- [ ] **Step 1: Change dashboard route to mission_control**

In `web/server.py`, find the dashboard route:
```python
@app.route('/dashboard')
def dashboard():
    """仪表盘页面"""
    return render_template('dashboard.html')
```

Replace with:
```python
@app.route('/dashboard')
def dashboard():
    """仪表盘页面 — Mission Control"""
    return render_template('mission_control.html', active_page='dashboard')

@app.route('/mission-control')
def mission_control_page():
    """Mission Control page"""
    return render_template('mission_control.html', active_page='dashboard')
```

Also update the root route `/` to go to mission control:
```python
@app.route('/')
def index():
    """首页 - Mission Control"""
    return render_template('mission_control.html', active_page='dashboard')
```

- [ ] **Step 2: Commit**

```bash
git add web/server.py
git commit -m "feat(ui): route dashboard to mission control, add /mission-control route"
```

---

## Task 6: Integration Test

- [ ] **Step 1: Run the app and verify in browser**

```bash
python main_production.py
```

Navigate to `http://127.0.0.1:5000/` (or configured port). Verify:
- Sidebar collapses to rail on the mission control page
- Status strip shows health pill, running task strip, counters, WS indicator
- Kanban board shows 4 columns with empty states
- Creating a task via the tasks page adds it to the Pending column
- A running task shows progress bar and elapsed timer

- [ ] **Step 2: Verify focus mode**

Click "Focus Mode" button. Sidebar should fully collapse. Press Escape or click again to restore.

- [ ] **Step 3: Verify slide-in panel**

Click any task card. Panel should slide in from right. Click overlay or Escape to close.

- [ ] **Step 4: Run existing tests**

```bash
pytest -q
```

---

## Verification Commands

```bash
# Start app
python main_production.py

# Browser: http://127.0.0.1:5000/
# Check: kanban board renders, status strip shows, sidebar rail works

# Run tests
pytest -q
```
