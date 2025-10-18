#define MyAppName    "ハイハイスクールアドベンチャー(AvaloniaUI)"
#define MyAppExeName "HHSAdvAvalonia.exe"
#define MyAppVersion GetFileVersion("..\bin\Release\net8.0\win-x64\publish\HHSAdvAvalonia.exe")

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher=WildTreeJP
DefaultDirName={commonpf64}\HHSAdvAvalonia
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputBaseFilename=hhsadvava_setup_{#MyAppVersion}
Compression=lzma
SolidCompression=no
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "..\bin\Release\net8.0\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"


[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,ハイハイスクールアドベンチャー(AvaloniaUI)}"; Flags: nowait postinstall skipifsilent
