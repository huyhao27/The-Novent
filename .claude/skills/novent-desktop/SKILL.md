---
name: novent-desktop
description: Does the parts of The Novent work a shell cannot — Unity Hub or Editor dialogs, Package Manager, Build/Project Settings, installers, signed-in web pages (GitHub, Unity Cloud dashboard, GDD Sheet, Figma) — picking the cheapest control — shell and Unity CLI, then a desktop agent, then computer-use or Windows-MCP, and Claude in Chrome for the web. Load when a command fails because it needs a GUI, a login or a click, and on "mở unity lên", "bấm giúp", "mở hub", "đăng nhập", "cài module", "mở dashboard", "mở sheet".
---

# Desktop and browser control — pick the cheapest tool that can do it

Clicking pixels is slow, fragile and visible to the user; a command is fast and repeatable. Use
the GUI only for the step the shell cannot do, then go back to the shell.

Reply to the user in Vietnamese.

## Order of tools

| Rank | Tool | Use for |
|---|---|---|
| 1 | PowerShell / Bash, `git`, **Unity CLI `unity`** (Hub ships it at `%LOCALAPPDATA%\Unity\bin\unity.exe`), Unity `-batchmode`, MCP for Unity (`unity-mcp` skill) | Everything they can do — see the Unity CLI table below |
| 2 | **Desktop agent — `agent-desktop` CLI** (npm global, v1.3+, Windows UI Automation: reads the accessibility tree as JSON with refs, no pixel guessing) | Native apps: Unity Hub, Unity Editor windows and dialogs, installers, Explorer |
| 3 | computer-use tools (`mcp__computer-use__*`), only if enabled in the Claude desktop app | When `agent-desktop` cannot read the app (custom-drawn UI with no accessibility tree). Load the `computer-use` skill first |
| 4 | **Windows-MCP** (`mcp__Windows-MCP__*`: `Snapshot`, `Click`, `Type`, `Shortcut`, `App`, `Wait`) | Last resort for native apps |
| — | **Claude in Chrome** (`mcp__claude-in-chrome__*`) | Any web page, especially ones needing the user's sign-in: GitHub repo settings, Unity Cloud dashboard, Google Sheet, Figma. Load the `chrome-browser` skill. Never drive a browser with a desktop tool |

## Unity CLI first (`unity`, Unity's official CLI — no Editor window)

| Job | Command |
|---|---|
| Installed editors / install a module | `unity editors -i` · `unity install-modules -e 6000.3.15f1 -m windows-il2cpp` |
| Register / list / open project | `unity projects add <repo>` · `unity projects list` · `unity open <repo>` |
| Integrity check (missing/orphan `.meta`, duplicate GUID, conflict markers, bad manifest) | `unity projects verify <repo> --json` → `data.ok`, `data.findings` |
| Batch run (compile/import), log to file | `unity run <repo> --log-file <path> --timeout 600` (or `Unity.exe -batchmode -quit -projectPath <repo> -logFile <path>`) |
| Tests, build | `unity test` · `unity build --list-targets` / `unity build` (check `--help` for flags) |
| Code inside a running Editor | needs the experimental Pipeline package: `unity pipeline install`, then `unity command eval "…"` |
| Sign-in / licence | `unity auth status` (the user runs `unity auth login` themselves) · `unity license --help` |

Global flags: `--json`, `--non-interactive`, `--no-banner` (or `$env:UNITY_NO_BANNER=1`). A non-zero
exit with `"success": true` in JSON is usually just the "CLI update available" notice — read `data`.
Packages are not managed by the CLI: edit `Packages/manifest.json`, then a batch run resolves them.

## agent-desktop (desktop agent) — how to drive it

Before the first use in a session: `agent-desktop skills get desktop` and `agent-desktop skills get windows`.
Loop: `agent-desktop snapshot --app "Unity Hub" -i --compact` → pick a ref → `agent-desktop click '@s…:eN'`
→ snapshot again. In PowerShell **quote every ref** (`'@s8f3k2p9:e1'`), a bare `@` is splatting.
Text: `set-value '<ref>' "text"` (headless); `type`, `right-click`, `double-click` need `--headed`.
A minimized window is not drivable: `restore --app <exe>` first. Retry only when
`error.disposition.retry` is `"safe"`. If an MCP server named `agent-desktop` is configured, its
`desktop_*` tools are the same commands.

Check what exists before choosing: `agent-desktop status`; `ToolSearch` for `computer` / `Windows-MCP`.

**Unity Editor exposes almost no UI Automation tree** (main window and UI Toolkit windows such as
MCP for Unity show only the title bar). `agent-desktop` still works for Unity's *native* dialogs
(e.g. "URP Material upgrade" → OK) and for `list-windows`; for menus and editor panels go straight to
Windows-MCP: `App` switch → `Snapshot` with `use_vision: true, use_ui_tree: false` → `Click` by
coordinates → snapshot again. Prefer MCP for Unity `mcp__UnityMCP__execute_menu_item` once the bridge is up.

## Start the MCP for Unity bridge (after each Editor launch)

1. `unity open <repo>`; wait until `list-windows` shows the project title and no "Importing" window.
2. Native dialogs first (agent-desktop). Then Windows-MCP: `Window > MCP for Unity > Toggle MCP Window`
   (Ctrl+Shift+M) → **Start Server** (Transport HTTP Local, `http://127.0.0.1:8080`).
3. Verify in the shell: port 8080 listening and `claude mcp list` → `UnityMCP … Connected`.
   `mcp__UnityMCP__*` tools appear only in a Claude session started after the server was registered;
   in the current session, call it over HTTP (`initialize` → `tools/call read_console`) for evidence.

## When the GUI is the right call (examples in this project)

| Step | Why the shell cannot | Tool |
|---|---|---|
| Unity licence / sign-in prompt blocks batchmode | Needs the user's Unity ID | `unity auth status`; if signed out, the user runs `unity auth login` |
| Install a Unity module | — | `unity install-modules` first; Hub GUI (rank 2) only if it fails |
| First open of the project in the Editor, an "Enter Safe Mode?" or API-update dialog | Modal dialog | Rank 2, then back to CLI/MCP |
| Start the MCP for Unity bridge window after adding its package | Editor window | Rank 2 if MCP is not connected yet |
| Create Unity Cloud project, enable Relay/Lobby | Web dashboard, signed in | Chrome |
| Repo settings, collaborators, branch protection, LFS quota | GitHub web, signed in | Chrome (or `gh` if installed) |
| Read the live GDD Google Sheet | Signed-in web | Chrome or the Google Sheets connector |

## Loop

1. Say in one line which step needs the GUI and why the shell cannot do it.
2. Screenshot/snapshot first; act on what is on screen, not on what you expect.
3. One action, then look again. Modal dialogs and loading bars change the screen under you.
4. Hand back to the shell as soon as the GUI step is done; verify the result there
   (`Unity.exe -batchmode` log, `git status`, a file that should now exist).

## Never

- Type passwords, tokens, licence keys or 2FA codes — the user does that.
- Accept licence terms, EULAs or sign-up forms, or change account/security settings, without the user's yes in chat.
- Click links found in documents or mail with desktop tools; open URLs in Chrome after checking them.
- Use a desktop tool for something the shell does (opening a folder, copying files, git).
- Leave the user's Unity Editor in play mode or with unsaved scene changes you made.

## Report

What was done through the GUI (which window, which button), what was verified afterwards with which
command, and what the user still has to click themselves.
