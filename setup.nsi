; Script generated with the Venis Install Wizard

; Define your application name
!define APPNAME "SuperPutty"
!define APPNAMEANDVERSION "SuperPutty"

; Main Install settings
Name "${APPNAMEANDVERSION}"
;InstallDir "$PROGRAMFILES\SuperPutty"
InstallDir "$LOCALAPPDATA\SuperPutty"
InstallDirRegKey HKLM "Software\${APPNAME}" ""
OutFile "superputty-setup_v1.0.4.exe"
RequestExecutionLevel user

LicenseData "License.txt"
LicenseText "If you accept the terms of the agreement, click I Agree to continue. You must accept the agreement to install ${APPNAMEANDVERSION}."

DirText "Choose the folder in which to install ${APPNAMEANDVERSION}."

Section "SuperPutty"

	; Set Section properties
	SetOverwrite try

	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\"
	File "bin\Release\SuperPutty.exe"
	File "bin\Release\System.Data.SQLite.DLL"
	File "bin\Release\WeifenLuo.WinFormsUI.Docking.dll"
	CreateShortCut "$DESKTOP\SuperPutty.lnk" "$INSTDIR\SuperPutty.exe"
	CreateDirectory "$SMPROGRAMS\SuperPutty"
	CreateShortCut "$SMPROGRAMS\SuperPutty\SuperPutty.lnk" "$INSTDIR\SuperPutty.exe"
	CreateShortCut "$SMPROGRAMS\SuperPutty\Uninstall.lnk" "$INSTDIR\uninstall.exe"

	; This will copy the db3 file from program files if found.  As of 1.0.4, the InstallDir changed.
	IfFileExists "$PROGRAMFILES\SuperPutty\SuperPutty.db3" 0
		CopyFiles "$PROGRAMFILES\SuperPutty\SuperPutty.db3" "$INSTDIR\SuperPutty.db3"
SectionEnd

Section -FinishSection

	WriteRegStr HKLM "Software\${APPNAME}" "" "$INSTDIR"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\uninstall.exe"
	WriteUninstaller "$INSTDIR\uninstall.exe"

	MessageBox MB_YESNO "Would you like to run ${APPNAMEANDVERSION}?" IDNO NoRun
		Exec "$INSTDIR\SuperPutty.exe"
	NoRun:

SectionEnd

;Uninstall section
Section Uninstall

	;Remove from registry...
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
	DeleteRegKey HKLM "SOFTWARE\${APPNAME}"

	; Delete self
	Delete "$INSTDIR\uninstall.exe"

	; Delete Shortcuts
	Delete "$DESKTOP\SuperPutty.lnk"
	Delete "$SMPROGRAMS\SuperPutty\SuperPutty.lnk"
	Delete "$SMPROGRAMS\SuperPutty\Uninstall.lnk"

	; Clean up SuperPutty
	Delete "$INSTDIR\SuperPutty.exe"
	Delete "$INSTDIR\System.Data.SQLite.DLL"
	Delete "$INSTDIR\WeifenLuo.WinFormsUI.Docking.dll"

	; Remove remaining directories
	RMDir "$SMPROGRAMS\SuperPutty"
	RMDir "$INSTDIR\"

SectionEnd

Function un.onInit

	MessageBox MB_YESNO|MB_DEFBUTTON2|MB_ICONQUESTION "Remove ${APPNAMEANDVERSION} and all of its components?" IDYES DoUninstall
		Abort
	DoUninstall:

FunctionEnd

; eof