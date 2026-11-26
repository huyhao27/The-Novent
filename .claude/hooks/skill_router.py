#!/usr/bin/env python3
"""UserPromptSubmit hook for The Novent: name the novent-* skills a prompt needs, once.

The model picks skills by the work it is about to do and skips them when a task looks simple,
so a one-line hint is injected before it starts. Each hint costs context, so the router:
  - ignores machine turns (task notifications, agent hand-backs, side-session events);
  - names a skill at most once per session (reset by session_start.py on /clear and compact);
  - names at most MAX_SKILLS skills, highest priority first.
Code style, co-op and asset rules load by file path from .claude/rules/, not from here.

Test:  echo {"prompt":"sửa lỗi player","session_id":"t"} | python -I .claude/hooks/skill_router.py
Add a phrase: put the user's literal words into ROUTES (novent-dream proposes them).
"""
import json
import os
import re
import sys
import tempfile

MAX_SKILLS = 4
STATE_DIR = os.path.join(tempfile.gettempdir(), "claude-skill-router-novent")

# (skills, pattern) in priority order, matched on the lowercased prompt.
ROUTES = [
    (["novent-dream"], r"\bdream|mơ đi|tổng kết cuối ngày|rút kinh nghiệm|học từ hôm nay"),
    (["novent-debug", "novent-verify"],
     r"lỗi|\bbug|crash|exception|error cs\d+|nullreference|missingreference|không chạy|ko chạy|"
     r"không hiện|không hoạt động|đứng im|không ăn|\bfailed|\bfix\b|sao nó|check error|đọc log|"
     r"desync|bị lệch|client không|bên kia không|máy kia không|người kia không"),
    (["novent-challenge"], r"sửa thẳng|sửa trong package|copy đại|viết lại hết|đập đi|làm lại từ đầu|hardcode"),
    (["novent-architecture"],
     r"kiến trúc|architecture|cấu trúc (code|thư mục|project)|module mới|asmdef|\bservice\b|event ?bus|"
     r"\bpool(ing)?\b|save ?system|hệ thống save|panel|màn hình ui|\bui mới|bootstrap|scene boot"),
    (["novent-plan"],
     r"lên plan|kế hoạch|\bplan\b|tính năng mới|làm hệ thống|hệ thống mới|refactor|chia nhỏ|chia phase|"
     r"roadmap|milestone|nhiều file"),
    (["novent-context"],
     r"co-?op|netcode|multiplayer|nhiều người|chơi chung|online|\bhost\b|\bclient\b|\brpc\b|đồng bộ|"
     r"\bsync\b|relay|lobby|đã có .* chưa|có sẵn"),
    (["novent-test"], r"\btest|kiểm thử|unit ?test|edit ?mode|play ?mode test|tdd"),
    (["novent-implement", "novent-verify"],
     r"\bthêm\b|làm đi|\bcode\b|\bviết\b|implement|triển khai|\bsửa\b|\btạo\b|\bđổi\b|\bchỉnh\b|tiếp đi|"
     r"bắt đầu đi|\bmerge\b|conflict|\.cs\b|\bscript\b|\bclass\b|\bhàm\b|method"),
    (["novent-code-style"],
     r"comment|đặt tên|naming|code cho sạch|chuẩn hoá|chuẩn hóa|code style|review code|xem lại code"),
    (["unity-mcp"],
     r"compile|play mode|bấm play|chạy thử|prefab|scene|console|screenshot|chụp màn|component|"
     r"inspector|gameobject|chạy test|run test|\bmcp\b|\bbuild\b"),
    (["novent-verify"], r"xong chưa|chạy được không|build được không|đã fix|kiểm tra lại|chắc chưa|double check|verify"),
    (["novent-desktop"],
     r"mở unity|mở hub|unity hub|bấm giúp|click giúp|xem màn hình|chụp màn hình|đăng nhập|cài module|"
     r"dashboard|github settings|mở sheet|trên web"),
    (["novent-memory"],
     r"lưu memory|ghi nhớ|note lại|nhớ giùm|cập nhật memory|cốt truyện|đổi session|tắt máy|"
     r"đọc memory|làm tới đâu|hôm trước"),
]
VAGUE = re.compile(r"^(?!.*[/\\._]\w)(?!.*[a-z][A-Z])(?=.*(?i:sửa|thêm|làm|fix|tối ưu|đổi)).{0,30}$")
MACHINE = re.compile(r"<(task-notification|side-sessions-event|ci-monitor-event|scheduled-task)\b|"
                     r"\[system notification|\[subagent hand-back|another claude session sent a message", re.I)


def state_path(session_id):
    os.makedirs(STATE_DIR, exist_ok=True)
    return os.path.join(STATE_DIR, re.sub(r"[^\w-]", "_", session_id or "nosession") + ".json")


def load_seen(path):
    try:
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    except (OSError, ValueError):
        return None


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    try:
        data = json.loads(sys.stdin.buffer.read().decode("utf-8") or "{}")
    except ValueError:
        return
    raw = (data.get("prompt") or "").strip()
    if not raw or MACHINE.search(raw[:400]):
        return
    text = raw.lower()

    path = state_path(data.get("session_id", ""))
    seen = load_seen(path)
    picked = ["novent-context"] if seen is None else []
    seen = seen or []

    if VAGUE.match(raw):
        picked.append("novent-clarify")
    for skills, pattern in ROUTES:
        if re.search(pattern, text):
            picked += skills
    picked = [s for s in dict.fromkeys(picked) if s not in seen][:MAX_SKILLS]

    with open(path, "w", encoding="utf-8") as f:
        json.dump(seen + picked, f)
    if not picked:
        return
    msg = ("[skill-router] Load with the Skill tool before acting: " + ", ".join(picked)
           + ". Skip one only if it clearly does not apply.")
    print(json.dumps({"hookSpecificOutput": {"hookEventName": "UserPromptSubmit",
                                             "additionalContext": msg}}, ensure_ascii=False))


if __name__ == "__main__":
    main()
