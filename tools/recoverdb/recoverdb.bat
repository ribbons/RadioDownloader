@echo off

rem This file is part of Radio Downloader.
rem Copyright © 2007-2014 by the authors - see the AUTHORS file for details.
rem
rem This program is free software: you can redistribute it and/or modify
rem it under the terms of the GNU General Public License as published by
rem the Free Software Foundation, either version 3 of the License, or
rem (at your option) any later version.
rem
rem This program is distributed in the hope that it will be useful,
rem but WITHOUT ANY WARRANTY; without even the implied warranty of
rem MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
rem GNU General Public License for more details.
rem
rem You should have received a copy of the GNU General Public License
rem along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
