# Release

Releases werden automatisch via GitHub Actions gebaut, wenn ein Tag mit diesem Präfix gepusht wird.

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
