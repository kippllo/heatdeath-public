REM help: https://stackoverflow.com/questions/34116576/how-to-make-a-batch-file-run-a-program-and-then-set-priority-higher

cd "C:\Users\kippl\Projects\Rouge Art Online\Backend v0.26\bin\Release\netcoreapp2.2\win-x64"

::Start /ABOVENORMAL BackendServer.exe
Start /HIGH BackendServer.exe