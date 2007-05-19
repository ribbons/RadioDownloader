;Radio Downloader Installer
;Copyright 2007 www.nerdoftheherd.com

;--------------------------------
;Includes

  !include "MUI.nsh"
  !include "LogicLib.nsh" ; For DotNET.nsh
  !include "DotNET.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Radio Downloader"
  OutFile "Radio Downloader.exe"
  SetCompressor /SOLID lzma
  InstallDir "$PROGRAMFILES\Radio Downloader"

  !define MUI_ICON "..\Graphics\icon\Icon.ico"
  !define MUI_UNICON "..\Graphics\icon\Icon.ico"

  !define DOTNET_VERSION "2"

  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Radio Downloader" ""

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "..\License.txt"
  
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Radio Downloader" RadioDownloader
  !insertmacro CheckDotNET ${DOTNET_VERSION}

  ;Output the files for the components folder
  SetOutPath "$INSTDIR\Components"
  File "..\bin\Components\mplayer.exe"
  File "..\bin\Components\lame.exe"

  ;Output the files in the program files folder
  SetOutPath "$INSTDIR"
  File "..\bin\Radio Downloader.exe"
  File "..\bin\System.Data.SQLite.DLL"
  File "..\bin\store.db"
  File "..\Graphics\icon\Icon - Grey.ico"
  File "..\GPL.txt"
  ;Store installation folder
  WriteRegStr HKCU "Software\Radio Downloader" "" $INSTDIR

  ;Create Start Menu Shortcuts
  SetShellVarContext all ; Store shortcuts in all users folder.
  CreateDirectory "$SMPROGRAMS\Radio Downloader"
  CreateShortCut "$SMPROGRAMS\Radio Downloader\Radio Downloader.lnk" "$INSTDIR\Radio Downloader.exe"
  CreateShortCut "$SMPROGRAMS\Radio Downloader\Uninstall.lnk" "$INSTDIR\Uninstall.exe" "" "$INSTDIR\Icon - Grey.ico"
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  ;Add an entry to the uninstall list
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\RadioDownloader" "DisplayName" "Radio Downloader"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\RadioDownloader" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\RadioDownloader" "DisplayIcon" "$INSTDIR\Radio Downloader.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\RadioDownloader" "URLInfoAbout" "http://www.nerdoftheherd.com/tools/radiodld/"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\RadioDownloader" "Publisher" "www.nerdoftheherd.com"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\RadioDownloader" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\RadioDownloader" "NoRepair" 1
SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"
  ;Delete the Start Menu shortcuts and folder
  SetShellVarContext all
  Delete /REBOOTOK  "$SMPROGRAMS\Radio Downloader\Radio Downloader.lnk"
  Delete /REBOOTOK  "$SMPROGRAMS\Radio Downloader\Uninstall.lnk"
  RMDir /REBOOTOK "$SMPROGRAMS\Radio Downloader"

  ;Delete the components folder

  ;Delete the files in the program files folder
  Delete /REBOOTOK "$INSTDIR\Radio Downloader.exe"
  Delete /REBOOTOK "$INSTDIR\System.Data.SQLite.DLL"
  Delete /REBOOTOK "$INSTDIR\store.db"
  Delete /REBOOTOK "$INSTDIR\Icon - Grey.ico"
  Delete /REBOOTOK "$INSTDIR\GPL.txt"

  ;Delete the installer and the program files folder
  Delete /REBOOTOK "$INSTDIR\Uninstall.exe"
  RMDir /REBOOTOK "$INSTDIR"

  ;Remove the record of the install location from the registry
  DeleteRegKey HKCU "Software\Radio Downloader"
  
  ;Remove the entry on the uninstall list
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\RadioDownloader"
SectionEnd