#!/usr/bin/env python3
"""SessionStart hook: put a short memory brief in front of every new, cleared or compacted session.

Without it a fresh session either skips memory (and repeats a settled mistake) or reads every
memory file (and pays for it). The brief is the memory index, the current branch, and the top
entry of this developer's own session log — enough to know where work stopped and which file
to open next. Everything else stays on disk until a task needs it.

Test:  echo {"source":"startup"} | python -I .claude/hooks/session_start.py
"""
import json
import os
import re
import subprocess
import sys
import tempfile

ROOT = os.environ.get("CLAUDE_PROJECT_DIR") or os.getcwd()
MEMORY = os.path.join(ROOT, ".claude", "memory")
MAX_LOG_LINES = 10


def git(*args):
    try:
        out = subprocess.run(["git", "-C", ROOT, *args], capture_output=True, text=True,
                             encoding="utf-8", timeout=5)
        return out.stdout.strip()
    except (OSError, subprocess.SubprocessError):
        return ""


def member_slug():
    name = git("config", "user.name") or os.environ.get("USERNAME") or os.environ.get("USER") or "me"
    return re.sub(r"[^a-z0-9-]+", "-", name.lower()).strip("-") or "me"


def read(path):
    try:
        with open(path, encoding="utf-8") as f:
            return f.read()
    except OSError:
        return ""


def top_entry(text):
    lines, started = [], False
    for line in text.splitlines():
        if line.startswith("## "):
            if started:
                break
            started = True
        if started:
            lines.append(line)
    return "\n".join(lines[:MAX_LOG_LINES])


def reset_router(data):
    """After /clear or compaction the loaded skills are gone, so the router may name them again."""
    if data.get("source") not in ("clear", "compact"):
        return
    sid = re.sub(r"[^\w-]", "_", data.get("session_id") or "nosession")
    try:
        os.remove(os.path.join(tempfile.gettempdir(), "claude-skill-router-novent", sid + ".json"))
    except OSError:
        pass


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    try:
        data = json.loads(sys.stdin.buffer.read().decode("utf-8") or "{}")
    except ValueError:
        data = {}
    reset_router(data)
    index = read(os.path.join(MEMORY, "INDEX.md")).strip()
    if not index:
        return
    slug = member_slug()
    log_rel = f".claude/memory/log/{slug}.md"
    last = top_entry(read(os.path.join(ROOT, log_rel)))
    branch = git("rev-parse", "--abbrev-ref", "HEAD") or "?"
    dirty = len([l for l in git("status", "--porcelain").splitlines() if l.strip()])

    parts = [
        "[novent-memory brief]",
        f"Branch: {branch} · uncommitted files: {dirty} · your session log: {log_rel}",
        index,
        "Your last session:\n" + last if last else f"No session log yet — create {log_rel} when this session ends.",
        "Open a memory file only when the task matches its row. Rules for code live in "
        ".claude/rules/ and load by path; procedures live in the novent-* skills.",
    ]
    print(json.dumps({"hookSpecificOutput": {"hookEventName": "SessionStart",
                                             "additionalContext": "\n\n".join(parts)}},
                     ensure_ascii=False))


if __name__ == "__main__":
    main()
