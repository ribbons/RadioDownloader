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
  
  !define MUI_ICON "..\Graphics\Icon.ico"
  !define MUI_UNICON "..\Graphics\GreyIcon.ico"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\VB and VBA Program Settings\Radio Downloader" "InstallLoc"

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
  File "d:\SandSprite\IEDevKit\IEDevKit.dll"
  File "..\Radio Background.exe"
  File "..\Radio Downloader.exe"
  
  SetOutPath "$INSTDIR\Components"
  File "..\Components\mplayer.exe"
  File "..\Components\lame.exe"
  
  ; Register Background App by running it
  ExecWait '"$INSTDIR\Radio Background.exe"'
  
  ;Store installation folder
  WriteRegStr HKCU "Software\Modern UI Test" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  Delete /REBOOTOK "$INSTDIR\Components\mplayer.exe"
  Delete /REBOOTOK "$INSTDIR\Components\lame.exe"
  RMDir /REBOOTOK "$INSTDIR\Components"

  Delete /REBOOTOK "$INSTDIR\Radio Downloader.exe"
  ExecWait 'regsvr32 /s /u "$INSTDIR\Radio Background.exe"'
  Delete /REBOOTOK "$INSTDIR\Radio Background.exe"
  Delete /REBOOTOK "$INSTDIR\IEDevKit.dll"

  Delete /REBOOTOK "$INSTDIR\Uninstall.exe"
  RMDir /REBOOTOK "$INSTDIR"

  DeleteRegKey HKCU "Software\VB and VBA Program Settings\Radio Downloader"

SectionEnd