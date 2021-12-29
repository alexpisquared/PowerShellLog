ReadMe #f80

Uncommenting will cause the Exception:

  Description: A .NET Core application failed.
  Application: PowerShellLog.abc.exe
  Path: C:\C\ef\PowerShellLog.Package\bin\x86\Release\AppX\PowerShellLog.abc\PowerShellLog.abc.exe
  Message: Error:
    An assembly specified in the application dependencies manifest (PowerShellLog.abc.deps.json) was not found:
      package: 'Microsoft.EntityFrameworkCore.Design', version: '3.1.0'
      path: 'lib/netstandard2.0/Microsoft.EntityFrameworkCore.Design.dll'


#2021-12-29
Prototyping User Secrets proper store through the Connected Services feature.
1. Just added the required stuff following https://www.youtube.com/watch?v=7OBMhoKieqk
