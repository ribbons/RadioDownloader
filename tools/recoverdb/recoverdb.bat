@echo off

rem Copyright © 2014 Matt Robinson
rem
rem SPDX-License-Identifier: GPL-3.0-or-later

title Recovering Radio Downloader Database

if not exist sqlite3.exe (
	echo sqlite3.exe not found!
	goto failed
)

set dbname=%appdata%\nerdoftheherd.com\Radio Downloader\store.db

echo.
echo Shutting down Radio Downloader
echo.
"%SystemDrive%\Program Files\Radio Downloader\Radio Downloader.exe" /exit
if ERRORLEVEL 1 goto failed

echo.
echo Removing search index
echo.
del "%appdata%\nerdoftheherd.com\Radio Downloader\searchindex.db"
if ERRORLEVEL 1 goto failed

echo.
echo Backing up database
echo.
goto testname
:bumpnum
set /a backupnum=%backupnum%+1 > nul
:testname
set backupname=%dbname%.corrupt%backupnum%
if exist "%backupname%" goto bumpnum
:retrycopy
PING 1.1.1.1 -n 1 -w 1000 >NUL
move "%dbname%" "%backupname%"
if ERRORLEVEL 1 goto retrycopy

echo.
echo Dumping database contents
echo.
echo .dump | sqlite3 "%backupname%" > "%dbname%.sql"
if ERRORLEVEL 1 goto failed

echo.
echo Building new database
echo.
sqlite3 "%dbname%" < "%dbname%.sql"
if ERRORLEVEL 1 goto failed

echo.
echo Cleaning up
echo.
del "%dbname%.sql"
if ERRORLEVEL 1 goto failed

goto success

:failed

echo.
echo Failed to recover database.

goto exit

:success

echo.
echo Database appears to have recovered successfully.

:exit

pause
