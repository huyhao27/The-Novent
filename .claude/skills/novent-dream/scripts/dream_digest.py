#!/usr/bin/env python3
"""Condense one project's Claude Code transcripts into a digest the novent-dream skill can read.

Raw transcripts are megabytes of JSONL (API shapes, tool output, file snapshots). Reading them
directly would burn the context the dream pass needs for thinking, so this keeps only what can
carry a lesson: what the user asked, where they corrected or interrupted Claude, which tool
calls failed, which skills loaded, which files changed, and how each turn ended. Every line
carries an anchor `<session8>:L<line>` so a proposal can quote its evidence and the user can
check it against the transcript.

Secrets are redacted here, on this machine, before anything reaches the model.

  python dream_digest.py                         # project = current folder, since last dream
  python dream_digest.py --since 2026-11-10 --project D:/NHH/Work/SandBus --out digest.md
"""
import argparse
import datetime as dt
import glob
import json
import os
import re
import sys

PROJECTS_DIR = os.path.expanduser("~/.claude/projects")
DREAM_LOG = os.path.join(".claude", "memory", "dream-log.md")

REDACT = [
    (re.compile(r"sk-[A-Za-z0-9_-]{16,}"), "<key>"),
    (re.compile(r"gh[pousr]_[A-Za-z0-9]{20,}"), "<token>"),
    (re.compile(r"AKIA[0-9A-Z]{16}"), "<aws-key>"),
    (re.compile(r"AIza[0-9A-Za-z_-]{30,}"), "<google-key>"),
    (re.compile(r"eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}"), "<jwt>"),
    (re.compile(r"(?i)(password|passwd|pwd|secret|token|api[_-]?key)\s*[:=]\s*\S+"), r"\1=<redacted>"),
    (re.compile(r"[\w.+-]+@[\w-]+\.[\w.-]+"), "<email>"),
    (re.compile(r"\b[A-Fa-f0-9]{40,}\b"), "<hex>"),
]

# Vietnamese and English ways a user says "that was wrong". A hit is a candidate, not a verdict:
# the dream pass reads the quote and decides.
CORRECTION = re.compile(
    r"\b(không phải|ko phải|k phải|sai rồi|sai |nhầm|chưa đúng|không đúng|đừng|dừng lại|"
    r"làm lại|sửa lại|vẫn lỗi|vẫn bị|lại bị|tôi đã nói|đã bảo|bảo rồi|why did|that's wrong|"
    r"not what i|revert|hoàn tác|undo)", re.I)
INTERRUPT = "[Request interrupted"
REMINDER = re.compile(r"<system-reminder>.*?</system-reminder>", re.S)
TAGGED = re.compile(r"<(command-name|command-message|command-args|local-command-stdout)>.*?</\1>", re.S)
SKILLS_LINE = re.compile(r"^Skills:.*$", re.M)
# App-generated turns arrive as user messages in older transcripts that lack `origin`.
MACHINE = re.compile(r"<(task-notification|side-sessions-event|ci-monitor-event|scheduled-task)\b|"
                     r"\[SYSTEM NOTIFICATION|\[Subagent hand-back|Another Claude session sent a message", re.I)


def redact(text):
    for pattern, repl in REDACT:
        text = pattern.sub(repl, text)
    return text


def clip(text, n):
    text = " ".join(text.split())
    return text if len(text) <= n else text[: n - 1] + "…"


def project_folder(path):
    # Claude Code names the folder after the absolute path with every non-alphanumeric char → "-".
    return re.sub(r"[^A-Za-z0-9]", "-", os.path.abspath(path))


def last_dream_date(project_path):
    log = os.path.join(project_path, DREAM_LOG)
    if not os.path.exists(log):
        return None
    dates = re.findall(r"^## (\d{4}-\d{2}-\d{2})", open(log, encoding="utf-8").read(), re.M)
    return max(dates) if dates else None


def text_of(content):
    if isinstance(content, str):
        return content
    return " ".join(c.get("text", "") for c in content or [] if isinstance(c, dict) and c.get("type") == "text")


def parse(path, since):
    sid = os.path.basename(path)[:8]
    turns, cur, title = [], None, ""
    with open(path, encoding="utf-8") as fh:
        for n, raw in enumerate(fh, 1):
            try:
                e = json.loads(raw)
            except ValueError:
                continue
            kind = e.get("type")
            if kind == "custom-title":
                title = e.get("customTitle", "")
            if kind not in ("user", "assistant") or e.get("isSidechain"):
                continue
            if e.get("timestamp", "")[:10] < since:
                continue
            content = (e.get("message") or {}).get("content")

            if kind == "user":
                if isinstance(content, list) and any(isinstance(c, dict) and c.get("type") == "tool_result" for c in content):
                    for c in content:
                        if isinstance(c, dict) and c.get("type") == "tool_result" and c.get("is_error") and cur:
                            body = c.get("content")
                            body = body if isinstance(body, str) else text_of(body)
                            cur["errors"].append(f"{sid}:L{n} {clip(redact(body), 220)}")
                    continue
                if e.get("isMeta") or (e.get("origin") or {}).get("kind", "human") != "human":
                    continue
                raw_text = text_of(content)
                text = TAGGED.sub(" ", REMINDER.sub(" ", raw_text)).strip()
                slash = re.findall(r"<command-name>/?([\w:.-]+)</command-name>", raw_text)
                if (not text and not slash) or MACHINE.search(raw_text[:600]):
                    continue
                if INTERRUPT in text and cur:
                    cur["flags"].append(f"{sid}:L{n} user interrupted")
                    continue
                prompt = ("/" + slash[0] + " " if slash else "") + text
                cur = dict(anchor=f"{sid}:L{n}", time=e["timestamp"][:16].replace("T", " "),
                           prompt=clip(redact(prompt), 600), skills=list(slash), edits=[], errors=[],
                           flags=[], tools=0, end="")
                if CORRECTION.search(text):
                    cur["flags"].append(f"{sid}:L{n} possible correction")
                turns.append(cur)

            elif kind == "assistant" and cur and isinstance(content, list):
                for c in content:
                    if not isinstance(c, dict):
                        continue
                    if c.get("type") == "text" and c.get("text", "").strip():
                        cur["end"] = c["text"]
                    elif c.get("type") == "tool_use":
                        cur["tools"] += 1
                        name, inp = c.get("name", ""), c.get("input") or {}
                        if name == "Skill":
                            cur["skills"].append(inp.get("skill", "?"))
                        elif name in ("Edit", "Write", "MultiEdit", "NotebookEdit"):
                            p = inp.get("file_path", "")
                            if p and p not in cur["edits"]:
                                cur["edits"].append(p)
    for t in turns:
        claimed = SKILLS_LINE.findall(t["end"])
        t["claimed"] = claimed[-1] if claimed else ""
        t["end"] = clip(redact(SKILLS_LINE.sub("", t["end"])), 400)
    return sid, title, turns


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--project", default=os.getcwd(), help="project folder (default: cwd)")
    ap.add_argument("--since", help="YYYY-MM-DD inclusive (default: day after last dream, else today)")
    ap.add_argument("--out", help="write the digest here instead of stdout")
    args = ap.parse_args()
    sys.stdout.reconfigure(encoding="utf-8")

    since = args.since
    if not since:
        last = last_dream_date(args.project)
        since = (dt.date.fromisoformat(last) + dt.timedelta(days=1)).isoformat() if last else dt.date.today().isoformat()

    folder = project_folder(args.project)
    # Worktree sessions of the same project live in sibling folders that share the prefix.
    paths = sorted(p for p in glob.glob(os.path.join(PROJECTS_DIR, "*", "*.jsonl"))
                   if os.path.basename(os.path.dirname(p)).lower().startswith(folder.lower()))
    out = [f"# Dream digest — {args.project} — since {since}", ""]
    total = 0
    for path in paths:
        if dt.date.fromtimestamp(os.path.getmtime(path)).isoformat() < since:
            continue
        sid, title, turns = parse(path, since)
        if not turns:
            continue
        total += len(turns)
        out.append(f"## Session {sid} — {title or '(untitled)'} — {len(turns)} turns")
        for t in turns:
            out.append(f"### {t['anchor']} · {t['time']} · tools={t['tools']}")
            out.append(f"- prompt: {t['prompt']}")
            if t["skills"]:
                out.append(f"- skills loaded: {', '.join(t['skills'])}")
            if t["claimed"]:
                out.append(f"- skills claimed: {t['claimed']}")
            if t["edits"]:
                out.append(f"- edited: {', '.join(os.path.basename(p) for p in t['edits'][:12])}")
            for f in t["flags"]:
                out.append(f"- FLAG {f}")
            for err in t["errors"][:5]:
                out.append(f"- tool error {err}")
            if t["end"]:
                out.append(f"- ended with: {t['end']}")
        out.append("")
    if not total:
        out.append("No turns found for that project and date range.")
    text = "\n".join(out)
    if args.out:
        open(args.out, "w", encoding="utf-8").write(text)
        print(f"{total} turns from {len(paths)} transcript files → {args.out}")
    else:
        print(text)


if __name__ == "__main__":
    main()
