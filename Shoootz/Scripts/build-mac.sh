# ONLY FOR TESTS

#!/bin/bash
set -e

RID="osx-arm64"
VERSION="0.0.1"
ROOT="$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
BIN="$ROOT/bin"
BUNDLE="$BIN/Shoootz.app"
LOGO="$ROOT/Shoootz/Assets/Disag/Logo.png"

echo "Publishing ($RID)..."
dotnet publish "$ROOT/Shoootz/Shoootz.csproj" -r "$RID" -c Release -o "$BIN"

echo "Creating .app bundle..."
mkdir -p "$BUNDLE/Contents/MacOS"
mkdir -p "$BUNDLE/Contents/Resources"
mv "$BIN/Shoootz" "$BUNDLE/Contents/MacOS/"

ICONSET="$BUNDLE/Contents/Resources/AppIcon.iconset"
mkdir -p "$ICONSET"
for SIZE in 16 32 64 128 256; do
  sips -z $SIZE $SIZE "$LOGO" --out "$ICONSET/icon_${SIZE}x${SIZE}.png" > /dev/null
done
for SIZE in 32 64 128 256; do
  HALF=$((SIZE / 2))
  sips -z $SIZE $SIZE "$LOGO" --out "$ICONSET/icon_${HALF}x${HALF}@2x.png" > /dev/null
done
iconutil -c icns "$ICONSET" -o "$BUNDLE/Contents/Resources/AppIcon.icns"
rm -rf "$ICONSET"

cat > "$BUNDLE/Contents/Info.plist" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key><string>Shoootz</string>
    <key>CFBundleDisplayName</key><string>Shoootz</string>
    <key>CFBundleIdentifier</key><string>de.fn.shoootz</string>
    <key>CFBundleVersion</key><string>$VERSION</string>
    <key>CFBundleShortVersionString</key><string>$VERSION</string>
    <key>CFBundleExecutable</key><string>Shoootz</string>
    <key>CFBundlePackageType</key><string>APPL</string>
    <key>CFBundleIconFile</key><string>AppIcon</string>
    <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
PLIST

echo "Signing..."
codesign --deep --force --sign - "$BUNDLE"

echo "Done: $BUNDLE"
