@echo off
title Creating Zipped Source

set tempfolder="%temp%\Radio Downloader"

rem create the temp folder if it doesn't already exist
if not exist %tempfolder% mkdir %tempfolder%

rem copy all of the files across that are needed, but try and miss out the files that can be easily re-created.
robocopy .\ %tempfolder% /MIR /XD .svn obj /XF "Radio Downloader.exe" "Radio Downloader.exe.config" "Radio Downloader.pdb" "Radio Downloader.vshost.exe" "Radio Downloader.vshost.exe.config" "Radio Downloader.xml" System.Data.SQLite.DLL System.Data.SQLite.xml "Radio Downloader.suo" "Radio Donloader.msi" "Radio Downloader.wixobj" "Radio Downloader.log"

rem make sure that 7za.exe is on the PATH.
7za a "%userprofile%\Desktop\Radio Downloader Source.7z" %tempfolder%

rmdir /S /Q %tempfolder%