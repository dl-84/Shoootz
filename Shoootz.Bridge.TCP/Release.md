# Release

Releases werden automatisch via GitHub Actions gebaut, wenn ein Tag mit diesem Präfix gepusht wird.

```bash
git tag bridge-v1.0.0
git push origin bridge-v1.0.0
```

Der Workflow baut alle Plattformen parallel auf nativen Runnern und erstellt automatisch ein GitHub Release mit folgenden Artefakten:

| Plattform | Datei | Runner |
|---|---|---|
| Windows | `Shoootz.Bridge.TCP.exe` | `windows-latest` |
| macOS (Apple Silicon) | `Shoootz.Bridge.TCP-macos-arm64.zip` | `macos-latest` |
| macOS (Intel) | `Shoootz.Bridge.TCP-macos-x64.zip` | `macos-latest` |
| Linux | `Shoootz.Bridge.TCP.AppImage` | `ubuntu-latest` |

Die Version wird aus dem Git-Tag ausgelesen (`bridge-v1.0.0` → `1.0.0`).  
Zum Ändern der Version: `<Version>` in `Shoootz.Bridge.TCP/Shoootz.Bridge.TCP.csproj` anpassen.
