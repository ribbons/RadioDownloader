;NSIS Modern User Interface
;Basic Example Script
;Written by Joost Verburg

;--------------------------------
;Include Modern UI

  !include "MUI.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Radio Downloader"
  OutFile "Radio Downloader.exe"
  SetCompressor /SOLID lzma
  ;SetCompress off
  InstallDir "$PROGRAMFILES\Radio Downloader"

  !define MUI_ICON "..\Graphics\icon\Icon.ico"
  !define MUI_UNICON "..\Graphics\icon\Icon.ico"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Radio Downloader" ""

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\Modern UI\License.txt"
  
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

  SetOutPath "$INSTDIR"
  File "..\bin\Radio Downloader.exe"
  File "..\bin\System.Data.SQLite.DLL"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\Radio Downloader" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  Delete /REBOOTOK "$INSTDIR\Radio Downloader.exe"
  Delete /REBOOTOK "$INSTDIR\System.Data.SQLite.DLL"

  Delete /REBOOTOK "$INSTDIR\Uninstall.exe"
  RMDir /REBOOTOK "$INSTDIR"

  DeleteRegKey HKCU "Software\Radio Downloader"

SectionEnd