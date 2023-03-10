; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "QLS Zoneck"
#define MyAppVersion "1.0.6"
#define MyAppPublisher "zonetecde"
#define MyAppURL "github.com/zonetecde/zoneck-server"
#define MyAppExeName "QLS UI.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{F8C27731-CCD5-4906-BC77-429390E92504}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=setup
OutputBaseFilename=Zoneck Server Setup
SetupIconFile=icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; 

[Files]
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\ClassLibrary.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\ClassLibrary.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\log4net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\log4net.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\Newtonsoft.Json.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\QLS UI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\QLS UI.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\QLS UI.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.Common.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.Facility.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.Facility.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.SocketBase.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.SocketBase.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.SocketEngine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.SocketEngine.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\SuperSocket.SocketService.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\zck_server.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\zoneck server\QLS UI\bin\Debug\zck_server.pdb"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

