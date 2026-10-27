; Inno Setup script for Shoootz.Bridge.TCP.
; Build: ISCC installer.iss /DMyAppVersion=1.0.0 /DSourceDir=C:\path\to\publish\output

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\..\bin"
#endif

#define MyAppName "Shoootz TCP Bridge"
#define MyAppExeName "Shoootz.Bridge.TCP.exe"
#define MyAppPublisher "Daniel Lindner"

[Setup]
AppId={{A3D2E6F1-4C9B-4F7E-8A3D-2E6F1B9C4D7A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DefaultDirName={autopf}\Shoootz
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
Compression=lzma
SolidCompression=yes
WizardStyle=modern
OutputDir=.
OutputBaseFilename=Shoootz.Bridge.TCP-Windows-Setup
SetupIconFile=..\Assets\Logo.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#SourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
