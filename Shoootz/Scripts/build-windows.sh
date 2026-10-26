# ONLY FOR TESTS

#!/bin/bash
set -e

RID="win-x64"
ROOT="$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
BIN="$ROOT/bin"

echo "Publishing ($RID)..."
dotnet publish "$ROOT/Shoootz/Shoootz.csproj" -r "$RID" -c Release -o "$BIN"

echo "Done: $BIN/Shoootz.exe"
