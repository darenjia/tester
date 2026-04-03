# Mission Control UI Design

**Date:** 2026-04-03
**Status:** Approved
**Author:** claude

---

## Goal

Optimize the web UI experience for operators (task monitoring throughout the day) and test engineers (task creation and result analysis). Fix: cluttered dashboard, hard-to-read task status, too many clicks to see progress, no real-time feedback, and tedious result analysis.

---

## Layout & Information Architecture

### Sidebar
- Shrinks to icon-only rail (~60px), expands on hover to show labels
- Navigation to Settings, Logs, Docs, etc. moves to a toolbar menu (⋯) at bottom of sidebar
- Current active page indicated by filled icon background

### Top Bar → Live Status Strip
Replaces the current top bar with a single live strip:

```
[🟢 System Ready]  [Running: Task#X ████░░░░ 45% · 00:45 elapsed]  [3 running · 12 completed today · 2 failed]  [WS: ● Connected]
```

- **Health pill:** green/yellow/red based on runtime diagnostics
- **Running task:** live progress bar + elapsed time (auto-updates)
- **Day summary:** running count, completed today, failed today
- **Connection indicator:** WebSocket live state, or HTTP polling mode

### Main Content Area
Single "Mission Control" view — a Kanban board. No separate dashboard + tasks split.

---

## Kanban Board

### Four Columns

| Column | Content | Update behavior |
|--------|---------|-----------------|
| **Pending** | Queued tasks, newest first | New tasks slide in from top |
| **Running** | Active tasks with live progress bar + elapsed time + live result tally | WebSocket or 3s polling |
| **Completed** | Passed tasks (last 50) with pass rate | Slides left on completion |
| **Failed** | Failed/timeout tasks (last 50) | Red highlight + shake animation on arrival |

### Running Task Card
```
┌──────────────────────────────┐
│ Task#X  [████████░░] 67%     │
│ CANoe · 00:45 elapsed        │
│ ✓ 12/15 passed               │
└──────────────────────────────┘
```

- Progress bar fills smoothly
- Elapsed timer ticks every second
- Live result tally updates as test cases complete

### Completed Task Card
```
┌──────────────────────────────┐
│ Task#X      ✓ PASSED        │
│ CANoe · 02:13 total          │
│ ✓ 15/15 · 100% pass rate    │
└──────────────────────────────┘
```

- Green-tinted background
- Click opens slide-in panel

### Failed Task Card
```
┌──────────────────────────────┐
│ Task#X      ✗ FAILED       │
│ CANoe · 01:23 total          │
│ ✓ 5/10 · 50% pass rate     │
│ Error: Config file missing   │  ← First line of error inline
│ [Quick Analysis ▼]          │  ← Expandable 2-3 line summary
└──────────────────────────────┘
```

- Red background tint
- First error line visible on card
- "Quick Analysis" expands inline error context (not full panel)

---

## Interactions

### Card Click → Slide-in Detail Panel
- Panel slides in from **right** (not a blocking modal)
- Width: ~600px, full content area height minus header
- Tabs: Basic Info | Execution | Results | Logs | Diagnosis
- Click outside or press Escape to close
- Does NOT navigate away from the board

### Live Result Tally (Running Tasks)
- Updates every ~2s via WebSocket/polling
- Shows passed/total as strategy reports intermediate results

### Focus Mode (Operator Console)
- Toggle button in status strip: "Focus Mode"
- Switches to: larger cards, only Running + Failed columns visible
- Sidebar fully collapses
- Fullscreen feel
- Escape or toggle restores normal view

### Empty States
- Each column shows placeholder when empty:
  - Pending: "No tasks in queue — system is idle"
  - Running: "No tasks currently running"
  - Completed: "No completed tasks yet"
  - Failed: "No failed tasks — all clear!"

---

## Real-Time Updates

### WebSocket (Primary)
- `TASK_EVENT` messages update cards in place
- New task → slides into Pending
- Task started → moves to Running with progress
- Task completed → slides to Completed or Failed column
- Running task intermediate results → update tally on card

### Polling Fallback
- If WebSocket is disabled, poll `/api/status` and `/api/tasks` every 3s
- Same card animations triggered by state changes

### Visual Transitions
- New cards: `opacity 0→1 + translateY(-10px→0)`, 300ms ease-out
- Completed move: `translateX` slide animation, 400ms
- Failed arrival: normal slide + brief red pulse on card border
- Progress bar: CSS transition `width 0.3s ease`

---

## Status Strip Detail

```
[🟢 System Ready]  [Running: Task#X ████░░░░ 45% · 00:45]  [3 running · 12 done · 2 failed · 0 pending]  [WS: ●]
```

### Health Pill States
- 🟢 **Ready:** All systems operational
- 🟡 **Warning:** Some services degraded or failed reports pending
- 🔴 **Blocked:** Critical issue preventing task execution

### Summary Counters
- Running: live count from task queue
- Completed today: resets at midnight
- Failed today: resets at midnight
- Pending: queued count

---

## Visual Design

### Color & Status
| State | Visual treatment |
|-------|----------------|
| Running | Blue header accent, elevated shadow |
| Completed | Green-tinted card background |
| Failed | Red-tinted card background, red border pulse |
| Pending | Neutral gray |

### Status Badges
- Use **filled** backgrounds (not outline) for scannability
- PASSED: Green background, white text
- FAILED: Red background, white text
- RUNNING: Blue background, white text
- PENDING: Gray background, dark text

### Cards
- White background, rounded corners (8px)
- Border-left accent color matching status
- Hover: slight lift (shadow increase)
- Running cards cast stronger shadow to stand out

---

## Scope

### Build
- New sidebar rail (icon-only, hover expand)
- Live status strip in top bar
- Kanban board (`/mission-control`, redirected from `/dashboard`)
- Kanban card component with live progress
- Slide-in detail panel (replaces modal)
- WebSocket real-time updates + polling fallback
- Focus mode toggle
- Quick analysis inline expansion for failed cards

### Do NOT Change
- Settings, Logs, Case Mapping, Report Status pages
- API endpoints (frontend only)
- Backend task execution logic

---

## Files to Modify

| File | Change |
|------|--------|
| `web/templates/base.html` | Sidebar rail, live status strip in top bar |
| `web/static/css/style.css` | Kanban layout, card styles, animations, focus mode |
| `web/templates/mission_control.html` | New Kanban board page |
| `web/static/js/mission_control.js` | New — WebSocket, polling, card rendering, slide-in panel |
| `web/server.py` | Route `/dashboard` → `mission_control.html` |

---

## Success Criteria

1. Operator can see all task states at a glance from one screen
2. Task progress is visible without clicking anything
3. Failed tasks are immediately obvious (red, not small badge)
4. Quick analysis of failures takes ≤2 clicks
5. Real-time updates feel smooth (no jarring page refreshes)
6. Focus mode provides distraction-free monitoring for operators
