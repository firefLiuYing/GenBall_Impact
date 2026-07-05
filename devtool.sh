#!/bin/bash
# ─────────────────────────────────────────────────────────────────────
# devtool.sh — Unified dev-tool CLI for sub-agents
# ─────────────────────────────────────────────────────────────────────
# Communicates with Unity Editor via file IPC (Temp/.devtool_*).
# Sub-agents that can't use MCP tools use this instead.
#
# Usage:
#   bash devtool.sh compile [--full]
#   bash devtool.sh test [--class <name>] [--method <name>]
#   bash devtool.sh verify [--full]
# ─────────────────────────────────────────────────────────────────────

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
TEMP_DIR="$PROJECT_DIR/Temp"

# ── File paths ──────────────────────────────────────────────────────

IMPORT_TRIGGER="$TEMP_DIR/.devtool_import.trigger"

COMPILE_TRIGGER="$TEMP_DIR/.devtool_compile.trigger"
COMPILE_DONE="$TEMP_DIR/.devtool_compile.done"
COMPILE_RESULT="$TEMP_DIR/.devtool_compile_result.json"

TEST_TRIGGER="$TEMP_DIR/.devtool_test.trigger"
TEST_DONE="$TEMP_DIR/.devtool_test.done"
TEST_RESULT="$TEMP_DIR/.devtool_test_result.json"

TIMEOUT=120
POLL_INTERVAL=2

# ── Helper: find python3 (Windows Git Bash may only have "python") ──

PYTHON=""
for cand in python3 python py; do
  if command -v "$cand" >/dev/null 2>&1; then
    PYTHON="$cand"
    break
  fi
done

if [[ -z "$PYTHON" ]]; then
  echo "ERROR: No Python interpreter found (tried python3, python, py)."
  echo "Please install Python 3 and add it to PATH."
  exit 1
fi

# ── Formatting (Python inline for JSON parsing) ─────────────────────

format_compile() {
  local result_file="$1"
  $PYTHON - "$result_file" <<'PYEOF'
import json, sys

with open(sys.argv[1], "r", encoding="utf-8") as f:
    data = json.load(f)

status = data.get("status", "")
errors = data.get("errors", [])
warnings = data.get("warnings", [])

# ── no_changes ──
if status == "no_changes":
    print("[NO CHANGES] No script changes detected -- compilation skipped.")
    sys.exit(0)

# ── compile complete ──
err_count = data.get("errorCount", len(errors))
warn_count = data.get("warningCount", len(warnings))

if err_count == 0 and warn_count == 0:
    print("[PASS] Compilation passed: 0 errors, 0 warnings")
    sys.exit(0)

if err_count == 0:
    print(f"[PASS] Compilation passed: 0 errors, {warn_count} warning(s)")
else:
    print(f"[FAIL] Compilation failed: {err_count} error(s), {warn_count} warning(s)")

# Group by file
def print_group(title, items, prefix):
    if not items:
        return
    by_file = {}
    for item in items:
        f = item.get("file", "") or "(unknown)"
        by_file.setdefault(f, []).append(item)
    for fpath, msgs in by_file.items():
        print(f"\n**{fpath}**")
        for m in msgs:
            line = m.get("line", 0)
            col = m.get("column", 0)
            msg = m.get("message", "")
            if line > 0:
                loc = f"(L{line}, C{col})"
            else:
                loc = ""
            print(f"- {loc}: {msg}".strip())

print_group("Errors", errors, "error")
print_group("Warnings", warnings, "warning")

if err_count > 0:
    sys.exit(1)
PYEOF
}

format_test() {
  local result_file="$1"
  $PYTHON - "$result_file" <<'PYEOF'
import json, sys

with open(sys.argv[1], "r", encoding="utf-8") as f:
    data = json.load(f)

summary = data.get("summary", {})
status = summary.get("status", "Error")
total = summary.get("totalTests", 0)
passed = summary.get("passedTests", 0)
failed = summary.get("failedTests", 0)
skipped = summary.get("skippedTests", 0)
duration = summary.get("duration", "?")
error_msg = summary.get("error", "")

if status == "Error":
    print(f"[FAIL] Test run error: {error_msg}")
    sys.exit(1)

if failed == 0:
    print(f"[PASS] All tests passed: {passed} passed, 0 failed, {skipped} skipped ({duration})")
    sys.exit(0)

print(f"[FAIL] Tests failed: {passed} passed, {failed} failed, {skipped} skipped ({duration})")

results = data.get("results", [])
failed_items = [r for r in results if r.get("status") == "Failed"]

if failed_items:
    print("\nFAILED:")
    for r in failed_items:
        name = r.get("name", "(unknown)")
        msg = r.get("message", "")
        # One-line summary per failure
        if msg:
            # Truncate multi-line messages for readability
            first_line = msg.split("\n")[0][:120]
            print(f"- {name}: {first_line}")
        else:
            print(f"- {name}")

sys.exit(1)
PYEOF
}

# ── Polling ─────────────────────────────────────────────────────────

poll_for_done() {
  local done_file="$1"
  local label="$2"
  local elapsed=0

  while [[ ! -f "$done_file" ]]; do
    sleep "$POLL_INTERVAL"
    elapsed=$((elapsed + POLL_INTERVAL))
    if [[ $elapsed -ge $TIMEOUT ]]; then
      echo "TIMEOUT: ${label} did not complete within ${TIMEOUT}s."
      echo "Is Unity Editor running?"
      exit 1
    fi
  done
}

# ── Subcommand: compile ─────────────────────────────────────────────

do_compile() {
  local full_rebuild="false"

  while [[ $# -gt 0 ]]; do
    case "$1" in
      --full) full_rebuild="true"; shift ;;
      *) shift ;;
    esac
  done

  # Clean up previous run
  rm -f "$COMPILE_DONE" "$COMPILE_RESULT"

  # Write trigger
  echo "{\"fullRebuild\": $full_rebuild}" > "$COMPILE_TRIGGER"
  echo "[devtool] Compile trigger written (fullRebuild=$full_rebuild)"

  # Wait for Unity
  poll_for_done "$COMPILE_DONE" "Compilation"

  # Format and output results
  if [[ -f "$COMPILE_RESULT" ]]; then
    format_compile "$COMPILE_RESULT"
  else
    echo "ERROR: No compile result file found."
    exit 1
  fi
}

# ── Subcommand: test ────────────────────────────────────────────────

do_test() {
  local test_class=""
  local test_method=""
  local test_namespace=""
  local test_assembly=""

  while [[ $# -gt 0 ]]; do
    case "$1" in
      --class)     test_class="$2"; shift 2 ;;
      --method)    test_method="$2"; shift 2 ;;
      --namespace) test_namespace="$2"; shift 2 ;;
      --assembly)  test_assembly="$2"; shift 2 ;;
      *) shift ;;
    esac
  done

  # Clean up previous run
  rm -f "$TEST_DONE" "$TEST_RESULT"

  # Build trigger JSON
  local json='{"testMode":"EditMode","includePassingTests":false,"includeMessages":true,"includeStacktrace":false'
  [[ -n "$test_class" ]]     && json+=',"testClass":"'"$test_class"'"'
  [[ -n "$test_method" ]]    && json+=',"testMethod":"'"$test_method"'"'
  [[ -n "$test_namespace" ]] && json+=',"testNamespace":"'"$test_namespace"'"'
  [[ -n "$test_assembly" ]]  && json+=',"testAssembly":"'"$test_assembly"'"'
  json+='}'

  echo "$json" > "$TEST_TRIGGER"
  echo "[devtool] Test trigger written"

  # Wait for Unity
  poll_for_done "$TEST_DONE" "Tests"

  # Format and output results
  if [[ -f "$TEST_RESULT" ]]; then
    format_test "$TEST_RESULT"
  else
    echo "ERROR: No test result file found."
    exit 1
  fi
}

# ── Subcommand: verify ──────────────────────────────────────────────

do_verify() {
  local full_rebuild="false"
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --full) full_rebuild="true"; shift ;;
      *) shift ;;
    esac
  done

  echo "═══════════════════════════════════════════"
  echo "  verify: compile"
  echo "═══════════════════════════════════════════"

  # Compile step
  rm -f "$COMPILE_DONE" "$COMPILE_RESULT"
  echo "{\"fullRebuild\": $full_rebuild}" > "$COMPILE_TRIGGER"
  poll_for_done "$COMPILE_DONE" "Compilation"

  if [[ ! -f "$COMPILE_RESULT" ]]; then
    echo "ERROR: No compile result file found."
    exit 1
  fi

  format_compile "$COMPILE_RESULT"
  local compile_rc=$?

  if [[ $compile_rc -ne 0 ]]; then
    echo ""
    echo "[FAIL] Compilation failed — skipping tests."
    exit $compile_rc
  fi

  echo ""
  echo "═══════════════════════════════════════════"
  echo "  verify: test"
  echo "═══════════════════════════════════════════"

  # Test step
  rm -f "$TEST_DONE" "$TEST_RESULT"
  echo '{"testMode":"EditMode","includePassingTests":false,"includeMessages":true,"includeStacktrace":false}' > "$TEST_TRIGGER"
  poll_for_done "$TEST_DONE" "Tests"

  if [[ ! -f "$TEST_RESULT" ]]; then
    echo "ERROR: No test result file found."
    exit 1
  fi

  format_test "$TEST_RESULT"
}

# ── Main dispatch ───────────────────────────────────────────────────

case "${1:-}" in
  import)
    shift
    # Write import trigger: each arg is an asset path
    for f in "$@"; do echo "$f"; done > "$IMPORT_TRIGGER"
    echo "[devtool] Import trigger written ($# files)"
    # Wait briefly for Unity to process
    sleep 1
    ;;
  compile)
    shift
    do_compile "$@"
    ;;
  test)
    shift
    do_test "$@"
    ;;
  verify)
    shift
    do_verify "$@"
    ;;
  *)
    echo "Usage: bash devtool.sh <command> [options]"
    echo ""
    echo "Commands:"
    echo "  import <path> [path...]   Import new .cs files into Unity"
    echo "  compile [--full]          Trigger compilation, report results"
    echo "  test [--class <name>]     Trigger EditMode tests, report results"
    echo "       [--method <name>]"
    echo "       [--namespace <name>]"
    echo "       [--assembly <name>]"
    echo "  verify [--full]           import → compile → (if ok) test"
    echo ""
    exit 1
    ;;
esac
