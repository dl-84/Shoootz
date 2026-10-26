# Release

Releases werden automatisch via GitHub Actions gebaut wenn ein Tag gepusht wird. Jede App im Repo hat ihr eigenes Tag-Präfix und ihren eigenen Workflow, damit sich die Releases nicht gegenseitig auslösen.

## Shoootz

```bash
git tag shoootz-v1.0.0
git push origin shoootz-v1.0.0
```

Der Workflow baut alle Plattformen parallel auf nativen Runnern und erstellt automatisch ein GitHub Release mit folgenden Artefakten:

| Plattform | Datei | Runner |
|---|---|---|
| Windows | `Shoootz.exe` | `windows-latest` |
| macOS (Apple Silicon) | `Shoootz-macos-arm64.zip` | `macos-latest` |
| macOS (Intel) | `Shoootz-macos-x64.zip` | `macos-latest` |
| Linux | `Shoootz.AppImage` | `ubuntu-latest` |

Die Version wird aus dem Git-Tag ausgelesen (`shoootz-v1.0.0` → `1.0.0`).  
Zum Ändern der Version: `<Version>` in `Shoootz/Shoootz.csproj` anpassen.

## Shoootz.Bridge.TCP

```bash
git tag bridge-v1.0.0
git push origin bridge-v1.0.0
```

Läuft nur für Windows, da einziger Einsatzort der OpticScoreServer-PC ist:

| Plattform | Datei | Runner |
|---|---|---|
| Windows | `Shoootz.Bridge.TCP.exe` | `windows-latest` |

Die Version wird aus dem Git-Tag ausgelesen (`bridge-v1.0.0` → `1.0.0`).  
Zum Ändern der Version: `<Version>` in `Shoootz.Bridge.TCP/Shoootz.Bridge.TCP.csproj` anpassen.
