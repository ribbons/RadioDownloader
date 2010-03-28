@echo off
title Creating Zipped Source

set tempfolder="%temp%\Radio Downloader"

rem Delete the temp folder if already exists
if exist %tempfolder% rmdir /S /Q %tempfolder%
if ERRORLEVEL 1 goto failed

rem Copy the versioned files across to the temp folder
svn export . %tempfolder%
if ERRORLEVEL 1 goto failed

rem make sure that 7za.exe is on the PATH.
7za a -tzip "%userprofile%\Desktop\Radio Downloader Source.zip" %tempfolder%
if ERRORLEVEL 1 goto failed

rmdir /S /Q %tempfolder%
if ERRORLEVEL 1 goto failed

goto :EOF

:failed

echo.
echo Source zipping failed - review above output for more details

pause
