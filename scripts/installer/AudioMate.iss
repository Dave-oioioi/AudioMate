; AudioMate Inno Setup skeleton.
; Build output should use AudioMate-Setup-v<version>.exe.

#define MyAppName "AudioMate"
#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif
#define MyAppPublisher "AudioMate"
#define MyAppExeName "AudioMate.App.exe"

[Setup]
AppId={{F7A4C3A8-4184-4D4F-861B-19F747E83B46}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppVerName={#MyAppName} {#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=AudioMate-Setup-v{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\..\src\AudioMate.App\Assets\AudioMate.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
CloseApplicationsFilter={#MyAppExeName}
RestartApplications=no
AlwaysRestart=no
RestartIfNeededByRun=no
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}

[Files]
; Publish first:
; dotnet publish src\AudioMate.App\AudioMate.App.csproj -c Release -r win-x64 --self-contained false
Source: "..\..\artifacts\publish\AudioMate-{#MyAppVersion}-win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "..\codex-aural\submit-audiomate-narration.ps1"; DestDir: "{app}\codex-aural"; Flags: ignoreversion

[Icons]
Name: "{group}\AudioMate"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\AudioMate"; Filename: "{app}\{#MyAppExeName}"
Name: "{autostartup}\AudioMate"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Start AudioMate"; Flags: nowait postinstall skipifsilent

[Code]
function KillAudioMate(): Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{cmd}'), '/C taskkill /IM "{#MyAppExeName}" /F /T >nul 2>nul', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := True;
end;

function InitializeSetup(): Boolean;
begin
  KillAudioMate();
  Result := True;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  KillAudioMate();
  Result := '';
end;
