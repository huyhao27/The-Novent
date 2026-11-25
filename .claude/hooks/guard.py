#!/usr/bin/env python3
"""PreToolUse hook: keep Claude's file edits inside the parts of the repo that text edits cannot break.

Unity owns scenes, prefabs, assets and .meta files: a hand edit to their YAML compiles, imports
and then silently drops objects or breaks GUID references for every teammate on the next pull.
Package and third-party folders are overwritten on the next import. ProjectSettings changes
affect every scene, so they are allowed only after the user says yes. New C# text is also
checked for APIs that are obsolete or throw in this project (FORBIDDEN_CS).

Decisions: deny -> the edit does not happen and Claude reads the reason; ask -> the user decides.

Test:  echo {"tool_name":"Edit","tool_input":{"file_path":"Assets/Scenes/A.unity"}} | python -I .claude/hooks/guard.py
"""
import json
import re
import sys

EDIT_TOOLS = {"Edit", "Write", "MultiEdit", "NotebookEdit"}

DENY = [
    (re.compile(r"\.(unity|prefab|asset|meta|mat|anim|controller|overrideController|physicMaterial|"
                r"lighting|mask|playable|signal|spriteatlas|spriteatlasv2|terrainlayer)$", re.I),
     "Unity serialized asset: change it through Unity (MCP for Unity tools such as "
     "mcp__UnityMCP__manage_gameobject / manage_prefabs / manage_asset / manage_scriptable_object, "
     "or exact click steps for the user). Hand-edited YAML breaks GUID/fileID references."),
    (re.compile(r"(^|/)(Library|Temp|Logs|obj|UserSettings)/", re.I),
     "Generated folder: Unity rebuilds it; edits are lost and never reach teammates."),
    (re.compile(r"(^|/)Packages/(?!manifest\.json$|packages-lock\.json$)", re.I),
     "Package folder: overwritten on the next resolve. Wrap or copy into Assets/_Project instead."),
    (re.compile(r"(^|/)Assets/(Plugins|ThirdParty)/", re.I),
     "Third-party code: overwritten on re-import. Copy into Assets/_Project and change the copy."),
    (re.compile(r"(^|/)Assets/NoventAssets/.*\.(cs|shader|hlsl)$", re.I),
     "Asset submodule: code does not belong there. Put scripts in Assets/_Project."),
]
ASK = [
    (re.compile(r"(^|/)ProjectSettings/", re.I),
     "ProjectSettings affects every scene and every teammate. Confirm this change is intended."),
    (re.compile(r"(^|/)Packages/manifest\.json$", re.I),
     "Adding or removing a package changes the project for the whole team. Confirm it was agreed."),
]


# APIs that are obsolete or wrong in this project (Unity 6.3, Input System only, NGO 2.x).
# Caught before the edit lands, with the replacement, instead of after a compile warning.
FORBIDDEN_CS = [
    (re.compile(r"\bFindObjectOfType\s*[<(]|\bFindObjectsOfType\s*[<(]"),
     "FindObjectOfType/FindObjectsOfType are obsolete: use Services.Get<T>() or a serialized "
     "reference; if a search is unavoidable, FindAnyObjectByType / FindObjectsByType(FindObjectsSortMode.None)."),
    (re.compile(r"\bGetInstanceID\s*\("),
     "GetInstanceID is removed in later Unity 6 releases: use GetEntityId(), and never save it."),
    (re.compile(r"\bBinaryFormatter\b"),
     "BinaryFormatter is unsafe and unsupported: save through SaveService (JSON)."),
    (re.compile(r"\bInput\.(GetKey|GetKeyDown|GetKeyUp|GetAxis|GetAxisRaw|GetButton|GetButtonDown|"
                r"GetButtonUp|GetMouseButton|GetMouseButtonDown|GetMouseButtonUp|mousePosition|"
                r"touchCount|GetTouch|anyKey)\b"),
     "UnityEngine.Input throws in this project (Input System only): use the Input System actions."),
    (re.compile(r"\bRequireOwnership\s*="),
     "RequireOwnership is replaced by InvokePermission (RpcInvokePermission) in NGO 2.x."),
]


def new_text(tool_input):
    if "content" in tool_input:
        return tool_input.get("content") or ""
    if "new_string" in tool_input:
        return tool_input.get("new_string") or ""
    return "\n".join(e.get("new_string", "") for e in tool_input.get("edits") or [])


def decide(path, tool_input):
    norm = path.replace("\\", "/")
    for pattern, reason in DENY:
        if pattern.search(norm):
            return "deny", reason
    for pattern, reason in ASK:
        if pattern.search(norm):
            return "ask", reason
    if norm.endswith(".cs"):
        text = new_text(tool_input)
        for pattern, reason in FORBIDDEN_CS:
            if pattern.search(text):
                return "deny", reason
    return None, None


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    try:
        data = json.loads(sys.stdin.buffer.read().decode("utf-8") or "{}")
    except ValueError:
        return
    if data.get("tool_name") not in EDIT_TOOLS:
        return
    tool_input = data.get("tool_input") or {}
    path = tool_input.get("file_path") or tool_input.get("notebook_path") or ""
    decision, reason = decide(path, tool_input)
    if not decision:
        return
    print(json.dumps({"hookSpecificOutput": {
        "hookEventName": "PreToolUse",
        "permissionDecision": decision,
        "permissionDecisionReason": "[novent-guard] " + reason,
    }}, ensure_ascii=False))


if __name__ == "__main__":
    main()
