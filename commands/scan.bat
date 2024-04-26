@echo off
setlocal

:: Name of this batch script
set "me=%~n0"

:: Check if an instance of this script is already running
for /f "tokens=2 delims=," %%A in ('tasklist /nh /fo csv /fi "imagename eq cmd.exe"') do (
    for /f "tokens=*" %%B in ('wmic process where "processid=%%~A" get commandline /value') do (
        for /f "tokens=*" %%C in ("%%~B") do (
            set "cmdline=%%~C"
            if "!cmdline!" neq "" (
                echo "!cmdline!" | findstr /i /c:"%me%" >nul && (
                    echo Instance of this script is already running.
                    exit /b
                )
            )
        )
    )
)

REM https://www.naps2.com/doc-command-line.html
REM --progress: Show progress bar / --force: Replace existing file
REM Run scanner and then -o :Output file (output.jpg) in "/Output" folder 
START /W /B ../NAPS2.Console.exe -o ../Output/output.pdf --progress 
REM Covert the actual file to pdf
REM START /W /B ../NAPS2.Console.exe -i ../Output/output.pdf -n 0 -o ../Output/output.jpg 

EXIT