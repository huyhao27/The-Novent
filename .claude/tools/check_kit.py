#!/usr/bin/env python3
"""Check The Novent's AI kit (.claude/) before committing a change to it. Standard library only.

  python .claude/tools/check_kit.py      -> prints problems, then OK / FAIL (exit 0 / 1)

Checks: skill/agent/rule frontmatter shape, description length, name = folder, body length,
bare MCP tool names, machine paths, settings.json is valid JSON, and each hook answers a sample.
"""
import glob
import json
import os
import re
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
KIT = os.path.join(ROOT, ".claude")
MAX_DESC = 700
MAX_BODY = 300
MCP_TOOLS = ("refresh_unity|read_console|run_tests|get_test_job|manage_scene|find_gameobjects|"
             "manage_gameobject|manage_components|manage_prefabs|manage_asset|manage_editor|"
             "manage_scriptable_object|manage_tools|batch_execute|execute_menu_item|manage_script")
BARE_TOOL = re.compile(r"(?<![\w_])(?<!mcp__UnityMCP__)`(" + MCP_TOOLS + r")`")
MACHINE_PATH = re.compile(r"\b[A-Z]:[\\/](?!…)|/Users/|/home/")

problems = []


def rel(path):
    return os.path.relpath(path, ROOT).replace("\\", "/")


def frontmatter(path):
    text = open(path, encoding="utf-8").read()
    match = re.match(r"---\n(.*?)\n---\n(.*)", text, re.S)
    if not match:
        problems.append(f"{rel(path)}: no frontmatter")
        return {}, text
    fields = {}
    for line in match.group(1).splitlines():
        if re.match(r"^[\w-]+:", line):
            key, _, value = line.partition(":")
            fields[key.strip()] = value.strip()
    return fields, match.group(2)


def check_skills():
    for path in glob.glob(os.path.join(KIT, "skills", "*", "SKILL.md")):
        fields, body = frontmatter(path)
        folder = os.path.basename(os.path.dirname(path))
        extra = set(fields) - {"name", "description"}
        if extra:
            problems.append(f"{rel(path)}: extra frontmatter {sorted(extra)} (keep name + description)")
        if fields.get("name") != folder:
            problems.append(f"{rel(path)}: name '{fields.get('name')}' != folder '{folder}'")
        desc = fields.get("description", "")
        if not desc or len(desc) > MAX_DESC:
            problems.append(f"{rel(path)}: description length {len(desc)} (1..{MAX_DESC})")
        if ": " in desc:
            problems.append(f"{rel(path)}: description contains ': ' (breaks YAML plain scalar)")
        if body.count("\n") > MAX_BODY:
            problems.append(f"{rel(path)}: body {body.count(chr(10))} lines > {MAX_BODY}")


def check_text_files():
    files = (glob.glob(os.path.join(KIT, "skills", "**", "*.md"), recursive=True)
             + glob.glob(os.path.join(KIT, "rules", "*.md"))
             + glob.glob(os.path.join(KIT, "agents", "*.md"))
             + [os.path.join(KIT, "CLAUDE.md")])
    for path in files:
        for number, line in enumerate(open(path, encoding="utf-8"), 1):
            if BARE_TOOL.search(line):
                problems.append(f"{rel(path)}:{number}: bare MCP tool name, use mcp__UnityMCP__<tool>")
            if MACHINE_PATH.search(line):
                problems.append(f"{rel(path)}:{number}: machine-specific path")


def check_rules_and_agents():
    for path in glob.glob(os.path.join(KIT, "rules", "*.md")):
        fields, _ = frontmatter(path)
        if "paths" not in fields:
            problems.append(f"{rel(path)}: no paths: — it would load in every session")
    for path in glob.glob(os.path.join(KIT, "agents", "*.md")):
        fields, _ = frontmatter(path)
        for key in ("name", "description"):
            if not fields.get(key):
                problems.append(f"{rel(path)}: missing {key}")


def check_settings_and_hooks():
    try:
        json.load(open(os.path.join(KIT, "settings.json"), encoding="utf-8"))
    except (OSError, ValueError) as error:
        problems.append(f".claude/settings.json: {error}")
    samples = {
        "session_start.py": {"source": "startup", "session_id": "check-kit"},
        "skill_router.py": {"prompt": "sửa lỗi player", "session_id": "check-kit"},
        "guard.py": {"tool_name": "Edit", "tool_input": {"file_path": "Assets/Scenes/A.unity"}},
    }
    for hook, sample in samples.items():
        path = os.path.join(KIT, "hooks", hook)
        result = subprocess.run([sys.executable, "-I", path], input=json.dumps(sample).encode("utf-8"),
                                capture_output=True, timeout=20, env={**os.environ, "CLAUDE_PROJECT_DIR": ROOT})
        out = result.stdout.decode("utf-8", "replace").strip()
        if result.returncode != 0:
            problems.append(f".claude/hooks/{hook}: exit {result.returncode}: {result.stderr.decode('utf-8', 'replace')[-200:]}")
            continue
        try:
            json.loads(out)
        except ValueError:
            problems.append(f".claude/hooks/{hook}: output is not JSON: {out[:120]!r}")
    marker = os.path.join(__import__("tempfile").gettempdir(), "claude-skill-router-novent", "check-kit.json")
    if os.path.exists(marker):
        os.remove(marker)


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    check_skills()
    check_text_files()
    check_rules_and_agents()
    check_settings_and_hooks()
    for problem in problems:
        print(problem)
    print("FAIL" if problems else "OK")
    sys.exit(1 if problems else 0)


if __name__ == "__main__":
    main()
