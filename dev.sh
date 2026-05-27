#!/bin/bash
# Dev workflow: compile -> then run tests
# Usage: bash dev.sh [--skip-compile] [--skip-tests]

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
SKIP_COMPILE=false
SKIP_TESTS=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --skip-compile) SKIP_COMPILE=true; shift ;;
    --skip-tests) SKIP_TESTS=true; shift ;;
    *) echo "Unknown arg: $1"; exit 1 ;;
  esac
done

# Step 1: Compile
if [[ "$SKIP_COMPILE" == false ]]; then
  echo "--- Step 1/2: Compile ---"
  py "$PROJECT_DIR/Tools/UnityMcp/compile_cli.py"
  EXIT_CODE=$?
  if [[ $EXIT_CODE -ne 0 ]]; then
    echo ""
    echo "[FAIL] Compilation failed, skipping tests."
    exit $EXIT_CODE
  fi
else
  echo "--- Step 1/2: Compile (skipped) ---"
fi

# Step 2: Tests
if [[ "$SKIP_TESTS" == false ]]; then
  echo ""
  echo "--- Step 2/2: Tests ---"
  bash "$PROJECT_DIR/run_editmode_tests.sh"
  EXIT_CODE=$?
  if [[ $EXIT_CODE -ne 0 ]]; then
    echo ""
    echo "[FAIL] Tests failed."
    exit $EXIT_CODE
  fi
else
  echo "--- Step 2/2: Tests (skipped) ---"
fi

echo ""
echo "[OK] All passed."
