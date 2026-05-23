#!/bin/bash
# Trigger Unity EditMode tests via file-based IPC.
# Usage: bash run_editmode_tests.sh [--compile] [--mode EditMode|PlayMode] [--assembly <name>] [--class <name>] [--namespace <ns>] [--method <full.name>]

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
TRIGGER="$PROJECT_DIR/Temp/.run_tests.trigger"
DONE="$PROJECT_DIR/Temp/.run_tests.done"
RESULTS="$PROJECT_DIR/Temp/TestResults.txt"
TIMEOUT=120  # seconds for tests to complete

MODE="EditMode"
ASSEMBLY=""
NAMESPACE=""
CLASS=""
METHOD=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --mode) MODE="$2"; shift 2 ;;
    --assembly) ASSEMBLY="$2"; shift 2 ;;
    --namespace) NAMESPACE="$2"; shift 2 ;;
    --class) CLASS="$2"; shift 2 ;;
    --method) METHOD="$2"; shift 2 ;;
    *) echo "Unknown arg: $1"; exit 1 ;;
  esac
done

# Build JSON payload
JSON='{"testMode":"'"$MODE"'","includePassingTests":false,"includeMessages":true,"includeStacktrace":false'
[[ -n "$ASSEMBLY" ]]  && JSON+=',"testAssembly":"'"$ASSEMBLY"'"'
[[ -n "$NAMESPACE" ]] && JSON+=',"testNamespace":"'"$NAMESPACE"'"'
[[ -n "$CLASS" ]]     && JSON+=',"testClass":"'"$CLASS"'"'
[[ -n "$METHOD" ]]    && JSON+=',"testMethod":"'"$METHOD"'"'
JSON+='}'

# Clean up any previous run
rm -f "$DONE" "$RESULTS"

# Write trigger
echo "$JSON" > "$TRIGGER"
echo "Trigger written: $JSON"

# Poll for completion
elapsed=0
while [[ ! -f "$DONE" ]]; do
  sleep 2
  elapsed=$((elapsed + 2))
  if [[ $elapsed -ge $TIMEOUT ]]; then
    echo "TIMEOUT: Tests did not complete within ${TIMEOUT}s. Is Unity Editor running?"
    rm -f "$TRIGGER"
    exit 1
  fi
done

# Output results
if [[ -f "$RESULTS" ]]; then
  cat "$RESULTS"
  # Parse summary for exit code
  if grep -q '"status"[[:space:]]*:[[:space:]]*"Failed"' "$RESULTS"; then
    exit 1
  fi
else
  echo "No results file found."
  exit 1
fi
