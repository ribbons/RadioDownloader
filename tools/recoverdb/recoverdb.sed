; Copyright © 2014 Matt Robinson
;
; SPDX-License-Identifier: GPL-3.0-or-later

[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=
DisplayLicense=
FinishMessage=
TargetName=recoverdb.exe
FriendlyName=Recover Radio Downloader Database
AppLaunched=cmd.exe /C recoverdb.bat
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
SourceFiles=SourceFiles
[Strings]
PostInstallCmd=
AdminQuietInstCmd=
UserQuietInstCmd=
[SourceFiles]
SourceFiles0=.
SourceFiles1=..
[SourceFiles0]
recoverdb.bat=
[SourceFiles1]
sqlite3.exe=
