; ---------- Preprocessor defines ----------
#define AppName "Eximp"
#define AppVersion "1.0.0"
#define Publisher "YourCompany"
#define ExeName "Eximp.exe"

; Option A (recommended): ABSOLUTE publish path. Replace this with your real publish path.
; Example: "C:\Users\USER\source\repos\Eximp\bin\Release\net8.0-windows\win-x64\publish"
#define PublishDir "C:\Users\USER\source\repos\Eximp\bin\Release\net8.0-windows\win-x64\publish"

; Option B (relative): if this .iss file sits right inside the project folder:
; #define PublishDir "bin\Release\net8.0-windows\win-x64\publish"

; ---------- Setup block ----------
[Setup]
AppId={{bfeec562-9db5-4bef-b6c9-359fe73d4ac0}}    ; KEEP THIS CONSTANT for upgrades
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\{#AppName}                 ; Program Files\Eximp
DefaultGroupName={#AppName}                        ; Start Menu folder
OutputDir=dist                                     ; installer will be written here
OutputBaseFilename={#AppName}_Setup_{#AppVersion}
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin 
; per-machine install → requires admin rights
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern
DisableDirPage=no

; ---------- Files to install ----------
[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

; ---------- Shortcuts ----------
[Icons]
Name: "{autoprograms}\{#AppName}\{#AppName}"; Filename: "{app}\{#ExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#ExeName}"; Tasks: desktopicon

; ---------- Optional tasks ----------
[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"

; ---------- Run after install ----------
[Run]
Filename: "{app}\{#ExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

; ---------- Create per-user data folder (your app should write here, not to Program Files) ----------
[Dirs]
Name: "{userappdata}\Eximp"
