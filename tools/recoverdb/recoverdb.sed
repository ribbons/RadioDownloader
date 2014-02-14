; This file is part of Radio Downloader.
; Copyright © 2007-2014 Matt Robinson
;
; This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
; Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
; option) any later version.
;
; This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
; implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
; License for more details.
;
; You should have received a copy of the GNU General Public License along with this program.  If not, see
; <http://www.gnu.org/licenses/>.

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
