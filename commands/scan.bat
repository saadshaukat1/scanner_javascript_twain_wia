@echo off

REM https://www.naps2.com/doc-command-line.html
REM --progress: Show progress bar / --force: Replace existing file
REM Run scanner and then -o :Output file (output.jpg) in "/Output" folder 
START /W /B ../NAPS2.Console.exe -o ../Output/output.pdf --progress 
REM Covert the actual file to pdf
START /W /B ../NAPS2.Console.exe -i ../Output/output.pdf -n 0 -o ../Output/output.jpg 


EXIT
