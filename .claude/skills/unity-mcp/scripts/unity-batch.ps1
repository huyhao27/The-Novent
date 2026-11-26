<#
Compile or run tests for a Unity project in batchmode, with agent-friendly output.

  powershell -File scripts/unity-batch.ps1 -Mode compile
  powershell -File scripts/unity-batch.ps1 -Mode editmode [-Filter My.Tests]
  powershell -File scripts/unity-batch.ps1 -Mode playmode

Output is a few key: value lines (result, errors, log, next) instead of the raw log, so an
agent reads 10 lines rather than 5000. Exit codes: 0 ok, 1 compile/test failure,
3 project already open in an Editor (use MCP instead), 4 Unity not found, 5 timeout.

Only one batch Unity runs at a time on this machine (named mutex), so parallel crewmates
queue instead of exhausting RAM.
#>
param(
    [string]$Project = ".",
    [ValidateSet("compile", "editmode", "playmode")][string]$Mode = "compile",
    [string]$Filter = "",
    [string]$Unity = $env:UNITY_EXE,
    [int]$TimeoutMin = 60,
    [int]$QueueTimeoutMin = 90
)

$ErrorActionPreference = "Stop"
$Project = (Resolve-Path $Project).Path

function Out-Result($result, $lines, $code) {
    Write-Output "result: $result"
    foreach ($l in $lines) { Write-Output $l }
    exit $code
}

$versionFile = Join-Path $Project "ProjectSettings/ProjectVersion.txt"
if (-not (Test-Path $versionFile)) { Out-Result "error" @("reason: not a Unity project: $Project") 4 }
$version = ((Get-Content $versionFile | Select-String "m_EditorVersion:") -split ":\s*")[1].Trim()

if (-not $Unity) {
    $hubList = Join-Path $env:APPDATA "UnityHub/editors-v2.json"
    if (Test-Path $hubList) {
        $entry = (Get-Content $hubList -Raw | ConvertFrom-Json).data | Where-Object { $_.version -eq $version } | Select-Object -First 1
        if ($entry) { $Unity = @($entry.location)[0] }
    }
    if (-not $Unity) {
        $candidate = "C:/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
        if (Test-Path $candidate) { $Unity = $candidate }
    }
}
if (-not $Unity -or -not (Test-Path $Unity)) {
    Out-Result "error" @("reason: Unity $version not found", "next: set UNITY_EXE to the Unity.exe path") 4
}

# An open Editor holds Temp/UnityLockfile exclusively.
$lock = Join-Path $Project "Temp/UnityLockfile"
if (Test-Path $lock) {
    try { $fs = [IO.File]::Open($lock, 'Open', 'ReadWrite', 'None'); $fs.Close() }
    catch { Out-Result "editor-open" @("reason: an Editor has this project open", "next: use MCP (refresh_unity + read_console) or close that Editor") 3 }
}

$logDir = Join-Path $Project "Logs"
New-Item -ItemType Directory -Force $logDir | Out-Null
$log = Join-Path $logDir "batch-$Mode.log"
$results = Join-Path $logDir "test-results-$Mode.xml"
Remove-Item $log, $results -ErrorAction SilentlyContinue

$uargs = @("-batchmode", "-nographics", "-projectPath", "`"$Project`"", "-logFile", "`"$log`"")
if ($Mode -eq "compile") {
    $uargs += "-quit"
} else {
    # No -quit with -runTests: the test runner exits by itself.
    $platform = if ($Mode -eq "editmode") { "EditMode" } else { "PlayMode" }
    $uargs += @("-runTests", "-testPlatform", $platform, "-testResults", "`"$results`"")
    if ($Filter) { $uargs += @("-testFilter", $Filter) }
}

$mutex = New-Object System.Threading.Mutex($false, "Global\UnityAiKitBatch")
$start = Get-Date
if (-not $mutex.WaitOne([TimeSpan]::FromMinutes($QueueTimeoutMin))) {
    Out-Result "timeout" @("reason: waited $QueueTimeoutMin min for another batch Unity to finish") 5
}
try {
    $proc = Start-Process -FilePath $Unity -ArgumentList $uargs -PassThru -WindowStyle Hidden
    if (-not $proc.WaitForExit($TimeoutMin * 60 * 1000)) {
        Stop-Process -Id $proc.Id -Force
        Out-Result "timeout" @("reason: Unity ran longer than $TimeoutMin min (first import of a new worktree can be slow)", "log: $log") 5
    }
    $exit = $proc.ExitCode
} finally {
    $mutex.ReleaseMutex()
}
$secs = [int]((Get-Date) - $start).TotalSeconds

$errors = @()
if (Test-Path $log) {
    $errors = Select-String -Path $log -Pattern "error CS\d+" | ForEach-Object { $_.Line.Trim() } | Sort-Object -Unique
}

if ($Mode -eq "compile") {
    if ($errors.Count -gt 0 -or $exit -ne 0) {
        $lines = @("errors: $($errors.Count)", "exit: $exit", "seconds: $secs") + ($errors | Select-Object -First 10 | ForEach-Object { "  $_" }) + @("log: $log", "next: fix the first error, rerun -Mode compile")
        Out-Result "fail" $lines 1
    }
    Out-Result "ok" @("errors: 0", "seconds: $secs", "log: $log") 0
}

if (-not (Test-Path $results)) {
    $lines = @("reason: no test results written (compile error or runner crash)", "exit: $exit", "errors: $($errors.Count)") + ($errors | Select-Object -First 10 | ForEach-Object { "  $_" }) + @("log: $log")
    Out-Result "fail" $lines 1
}
[xml]$xml = Get-Content $results
$run = $xml.'test-run'
$failedNames = $xml.SelectNodes("//test-case[@result='Failed']") | Select-Object -First 10 | ForEach-Object { "  FAILED $($_.fullname): $(($_.failure.message.InnerText -split "`n")[0])" }
$lines = @("total: $($run.total)", "passed: $($run.passed)", "failed: $($run.failed)", "skipped: $($run.skipped)", "seconds: $secs") + $failedNames + @("results: $results")
if ([int]$run.failed -gt 0) { Out-Result "fail" $lines 1 }
Out-Result "ok" $lines 0
