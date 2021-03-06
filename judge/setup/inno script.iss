; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define AppName "SDNUACM JudgeService"
#define AppVersion "0.1"
#define AppPublisher "SDNUACM, Inc."
#define AppExeName "JudgeService.exe"    
#define AppConfigName "config.json"
#define AppErrorLogName "error.log"
#define DotNetFX35SP1Name "dotnetfx35sp1.exe"
#define AppId "{{644ADD05-0827-430A-B36D-6897DEB7FD3D}"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
;AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={pf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
;LicenseFile=
InfoBeforeFile=D:\SDNUACM JudgeService\Require.txt      
InfoAfterFile=D:\SDNUACM JudgeService\Readme.txt
OutputDir=D:\SDNUACM JudgeService\setup build
OutputBaseFilename=SDNUACM JudgeService Setup
;SetupIconFile=D:\SDNUACM JudgeService\icon.png
Compression=lzma
SolidCompression=yes

[Languages]                                                        
Name: "chinese";  MessagesFile: "compiler:Languages/Chinese.isl"
;Name: "english";  MessagesFile: "compiler:Default.isl"

[Types]
Name: "default";          Description: "默认"
Name: "onlyservice";      Description: "只安装服务"
Name: "full";             Description: "全部"
Name: "custom";           Description: "定制";      Flags: iscustom

[Components]
Name: "service";          Description: "服务";                        Types: default onlyservice full custom; Flags: fixed
Name: "gcc";              Description: "GCC";                         Types: default full custom 
;Name: "dotnetfx35sp1";    Description: ".NET 3.5 Framework SP1";      Types: full custom
;Name: "dotnet35support";  Description: "运行于.NET2.0的.NET3.5支持";  Types: full custom

[Registry]
Root: HKLM; Subkey: "Software\SDNUACM\JudgeService"; ValueType: string; ValueName: "InstallLocation"; ValueData: "{app}"; Flags: uninsdeletekey

[Code]
function PrepareToInstall(var NeedsRestart: Boolean): String;                                               
var
  ResultCode: Integer;
  ServiceExePath: String;
begin
  ServiceExePath := ExpandConstant('{reg:HKLM\Software\SDNUACM\JudgeService,InstallLocation}\{#AppExeName}')
  Exec(ServiceExePath, '--uninstall', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := '';
end;

[Files]
Source: "D:\Documents\Visual Studio 2012\Projects\Judger\JudgeService\bin\Release\JudgeClient.JudgeService.exe";  DestDir: "{app}";   Flags: ignoreversion;   Components: "service";        DestName: "{#AppExeName}"                     
Source: "D:\Documents\Visual Studio 2012\Projects\Judger\JudgeService\bin\Release\Burst.dll";                     DestDir: "{app}";   Flags: ignoreversion;   Components: "service";    
Source: "D:\Documents\Visual Studio 2012\Projects\Judger\JudgeService\bin\Release\ICSharpCode.SharpZipLib.dll";   DestDir: "{app}";   Flags: ignoreversion;   Components: "service";
Source: "D:\Documents\Visual Studio 2012\Projects\Judger\JudgeService\bin\Release\NUniversalCharDet.dll";         DestDir: "{app}";   Flags: ignoreversion;   Components: "service";
Source: "D:\Documents\Visual Studio 2012\Projects\Judger\JudgeService\bin\Release\JudgeClient.Definition.dll";    DestDir: "{app}";   Flags: ignoreversion;   Components: "service";
Source: "D:\Documents\Visual Studio 2012\Projects\Judger\JudgeService\bin\Release\JudgeClient.Fetcher.dll";       DestDir: "{app}";   Flags: ignoreversion;   Components: "service";
Source: "D:\Documents\Visual Studio 2012\Projects\Judger\JudgeService\bin\Release\JudgeClient.Judger.dll";        DestDir: "{app}";   Flags: ignoreversion;   Components: "service";
Source: "D:\SDNUACM JudgeService\Readme.txt";             DestDir: "{app}";                 Flags: ignoreversion isreadme;                                    Components: "service"
Source: "D:\lib\limited_mingw\build_for_no_fstream\*";    DestDir: "{app}\compiler\gcc\";   Flags: ignoreversion recursesubdirs createallsubdirs;             Components: "gcc"      
;Source: "D:\DOWNLOAD\tools\runtime\dotnetfx35sp1.exe";    DestDir: "{app}";                 Flags: ignoreversion deleteafterinstall;                          Components: "dotnetfx35sp1";  DestName: "{#DotNetFX35SP1Name}"
;Source: "D:\Documents\Visual Studio 2012\Projects\BurstFor2.0\System.Core.dll";         DestDir: "{app}";   Flags: ignoreversion;   Components: "dotnet35support"
;Source: "D:\Documents\Visual Studio 2012\Projects\BurstFor2.0\System.Data.Linq.dll";    DestDir: "{app}";   Flags: ignoreversion;   Components: "dotnet35support"
;Source: "D:\Documents\Visual Studio 2012\Projects\BurstFor2.0\System.Xml.Linq.dll";     DestDir: "{app}";   Flags: ignoreversion;   Components: "dotnet35support"
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]    
Name: "{group}\重启服务";     Filename: "{app}\{#AppExeName}";    Parameters: "--restart"   
Name: "{group}\打开配置文件"; Filename: "{app}\{#AppConfigName}";                              
Name: "{group}\打开错误日志"; Filename: "{app}\{#AppErrorLogName}";                        
Name: "{group}\打开安装目录"; Filename: "{app}";                            
Name: "{group}\结束服务";     Filename: "{app}\{#AppExeName}";    Parameters: "--stop"  
Name: "{group}\启动服务";     Filename: "{app}\{#AppExeName}";    Parameters: "--start"  
Name: "{group}\卸载服务";     Filename: "{app}\{#AppExeName}";    Parameters: "--uninstall"    
Name: "{group}\安装服务";     Filename: "{app}\{#AppExeName}";    Parameters: "--install"     
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"

[Run]                                                                                                                                                  
Filename: "{app}\{#AppExeName}";          Description: "尝试卸载旧版本的服务";          StatusMsg: "尝试卸载旧版本的服务中...";        Parameters: "--tryuninstall"                  
;Filename: "{app}\{#DotNetFX35SP1Name}";   Description: "安装.NET 3.5 Framework SP1";    StatusMsg: "安装.NET 3.5 Framework SP1中..."                                         
Filename: "{app}\{#AppExeName}";          Description: "安装服务";                      StatusMsg: "安装服务中...";                    Parameters: "--install"                                       
Filename: "{app}\{#AppExeName}";          Description: "启动服务";                      StatusMsg: "启动服务中...";                    Parameters: "--start"

[UninstallDelete]
Type: filesandordirs; Name: "{app}\*"  

[UninstallRun]                                     
Filename: "{app}\{#AppExeName}"; Parameters: "--stop"
Filename: "{app}\{#AppExeName}"; Parameters: "--uninstall"
