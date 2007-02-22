Option Strict Off
Option Explicit On
Friend Class cSubclass
	'==================================================================================================
	'cSubclass - module-less, IDE safe, machine code subclassing thunk
	'
	'Paul_Caton@hotmail.com
	'Copyright free, use and abuse as you see fit.
	'
	'v1.00 20030107 First cut..........................................................................
	'v1.01 20031118 Allow control over callback gating
	'               Use global memory for the machine code buffer
	'               Reform the assembler...............................................................
	'v1.02 20040118 Use EbMode for breakpoint/stop detection rather than callback gating
	'               Further reform the assembler for greater speed and smaller size
	'               Made InIDE public..................................................................
	'
	'==================================================================================================
	
	Private Const PATCH_05 As Integer = 93 'Table B (before) entry count
	Private Const PATCH_09 As Integer = 137 'Table A (after) entry count
	
	Private nMsgCntB As Integer 'Before msg table entry count
	Private nMsgCntA As Integer 'After msg table entry count
	Private aMsgTblB() As WinSubHook2.eMsg 'Before msg table array
	Private aMsgTblA() As WinSubHook2.eMsg 'After msg table array
	Private hWndSub As Integer 'Handle of the window being subclassed
	Private nAddrSubclass As Integer 'The address of our WndProc
	Private nAddrOriginal As Integer 'The address of the existing WndProc
	
	'============================================
	'Class creation/destruction
	'============================================
	
	'Build the subclass thunk into allocated memory
	'UPGRADE_NOTE: Class_Initialize was upgraded to Class_Initialize_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
	Private Sub Class_Initialize_Renamed()
		Const PATCH_01 As Integer = 18 'Code buffer offset to the location of the relative address to EbMode
		Const PATCH_03 As Integer = 78 'Relative address of SetWindowsLong
		Const PATCH_07 As Integer = 121 'Relative address of CallWindowProc
		Const FUNC_EBM As String = "EbMode" 'VBA's EbMode function allows the machine code thunk to know if the IDE has stopped or is on a breakpoint
		Const FUNC_SWL As String = "SetWindowLongA" 'SetWindowLong allows the cSubclasser machine code thunk to unsubclass the subclasser itself if it detects via the EbMode function that the IDE has stopped
		Const FUNC_CWP As String = "CallWindowProcA" 'We use CallWindowProc to call the original WndProc
		Const MOD_VBA5 As String = "vba5" 'Location of the EbMode function if running VB5
		Const MOD_VBA6 As String = "vba6" 'Location of the EbMode function if running VB6
		Const MOD_USER As String = "user32" 'Location of the SetWindowLong & CallWindowProc functions
		Dim i As Integer 'Loop index
		Dim nLen As Integer 'String lengths
		Dim sHex As String 'Hex code string
		Dim sCode As String 'Binary code string
		
		'Store the hex pair machine code representation in sHex
		sHex = "5589E583C4F85731C08945FC8945F8EB0EE8xxxxx01x83F802742185C07424E830000000837DF800750AE838000000E84D0000005F8B45FCC9C21000E826000000EBF168xxxxx02x6AFCFF7508E8xxxxx03xEBE031D24ABFxxxxx04xB9xxxxx05xE82D000000C3FF7514FF7510FF750CFF750868xxxxx06xE8xxxxx07x8945FCC331D2BFxxxxx08xB9xxxxx09xE801000000C3E32F09C978078B450CF2AF75248D4514508D4510508D450C508D4508508D45FC508D45F85052B8xxxxx0Ax508B00FF501CC3"
		nLen = Len(sHex) 'Length of hex pair string
		
		'Convert the string from hex pairs to bytes and store in the ASCII string opcode buffer
		For i = 1 To nLen Step 2 'For each pair of hex characters
			'UPGRADE_ISSUE: ChrB$ function is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="367764E5-F3F8-4E43-AC3E-7FE0B5E074E2"'
			sCode = sCode & ChrB$(Val("&H" & Mid(sHex, i, 2))) 'Convert a pair of hex characters to a byte and append to the ASCII string
		Next i 'Next pair
		
		'UPGRADE_ISSUE: LenB function is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="367764E5-F3F8-4E43-AC3E-7FE0B5E074E2"'
		nLen = LenB(sCode) 'Get the machine code length
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		nAddrSubclass = WinSubHook2.Kernel32.GlobalAlloc(0, nLen) 'Allocate fixed memory for machine code buffer
		
		'Copy the code to allocated memory
		'UPGRADE_ISSUE: StrPtr function is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="367764E5-F3F8-4E43-AC3E-7FE0B5E074E2"'
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		Call WinSubHook2.CopyMemory(nAddrSubclass, StrPtr(sCode), nLen)
		
		If InIDE Then
			'Patch the jmp (EB0E) with two nop's (90) enabling the IDE breakpoint/stop checking code
			'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
			Call WinSubHook2.CopyMemory(nAddrSubclass + 15, &H9090s, 2)
			
			i = AddrFunc(MOD_VBA6, FUNC_EBM) 'Get the address of EbMode in vba6.dll
			If i = 0 Then 'Found?
				i = AddrFunc(MOD_VBA5, FUNC_EBM) 'VB5 perhaps, try vba5.dll
			End If
			
			System.Diagnostics.Debug.Assert(i, "") 'Ensure the EbMode function was found
			Call PatchRel(PATCH_01, i) 'Patch the relative address to the EbMode api function
		End If
		
		Call PatchRel(PATCH_03, AddrFunc(MOD_USER, FUNC_SWL)) 'Address of the SetWindowLong api function
		Call PatchVal(PATCH_05, 0) 'Initial before table entry count
		Call PatchRel(PATCH_07, AddrFunc(MOD_USER, FUNC_CWP)) 'Address of the CallWindowProc api function
		Call PatchVal(PATCH_09, 0) 'Initial after table entry count
	End Sub
	Public Sub New()
		MyBase.New()
		Class_Initialize_Renamed()
	End Sub
	
	'UnSubclass and release the allocated memory
	'UPGRADE_NOTE: Class_Terminate was upgraded to Class_Terminate_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
	Private Sub Class_Terminate_Renamed()
		Call Me.UnSubclass() 'UnSubclass if the Subclass thunk is active
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		Call WinSubHook2.Kernel32.GlobalFree(nAddrSubclass) 'Release the allocated memory
	End Sub
	Protected Overrides Sub Finalize()
		Class_Terminate_Renamed()
		MyBase.Finalize()
	End Sub
	
	'============================================
	'Public interface
	'============================================
	
	'Add the message to the callback table
	'UPGRADE_NOTE: When was upgraded to When_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
	Public Sub AddMsg(ByVal uMsg As WinSubHook2.eMsg, ByVal When_Renamed As WinSubHook2.eMsgWhen)
		If When_Renamed And WinSubHook2.eMsgWhen.MSG_BEFORE Then 'If Before
			'Add the message, pass the before table and before table message count variables ByRef
			Call AddMsgSub(uMsg, aMsgTblB, nMsgCntB, WinSubHook2.eMsgWhen.MSG_BEFORE)
		End If
		
		If When_Renamed And WinSubHook2.eMsgWhen.MSG_AFTER Then 'If After
			'Add the message, pass the after table and after table message count variables ByRef
			Call AddMsgSub(uMsg, aMsgTblA, nMsgCntA, WinSubHook2.eMsgWhen.MSG_AFTER)
		End If
	End Sub
	
	'Arbitarily call the original WndProc
	Public Function CallOrigWndProc(ByVal uMsg As WinSubHook2.eMsg, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
		If hWndSub <> 0 Then
			'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
			CallOrigWndProc = WinSubHook2.CallWindowProc(nAddrOriginal, hWndSub, uMsg, wParam, lParam) 'Call the original WndProc
		End If
	End Function
	
	'Delete the message from the msg table
	'UPGRADE_NOTE: When was upgraded to When_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
	Public Sub DelMsg(ByVal uMsg As WinSubHook2.eMsg, ByVal When_Renamed As WinSubHook2.eMsgWhen)
		
		If When_Renamed And WinSubHook2.eMsgWhen.MSG_BEFORE Then 'If before
			'Delete the message, pass the Before table and before message count variables ByRef
			Call DelMsgSub(uMsg, aMsgTblB, nMsgCntB, WinSubHook2.eMsgWhen.MSG_BEFORE)
		End If
		
		If When_Renamed And WinSubHook2.eMsgWhen.MSG_AFTER Then 'If After
			'Delete the message, pass the After table and after message count variables ByRef
			Call DelMsgSub(uMsg, aMsgTblA, nMsgCntA, WinSubHook2.eMsgWhen.MSG_AFTER)
		End If
	End Sub
	
	'Set the window subclass
	Public Function Subclass(ByVal hWnd As Integer, ByVal Owner As WinSubHook2.iSubclass) As Boolean
		Const PATCH_02 As Integer = 68 'Address of the previous WndProc
		Const PATCH_06 As Integer = 116 'Address of the previous WndProc
		Const PATCH_0A As Integer = 186 'Address of the owner object
		
		If hWndSub = 0 Then
			'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
			System.Diagnostics.Debug.Assert(WinSubHook2.User32.IsWindow(hWnd), "") 'Invalid window handle
			hWndSub = hWnd 'Store the window handle
			
			'Get the original window proc
			'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
			nAddrOriginal = WinSubHook2.GetWindowLong(hWnd, WinSubHook2.GWL_WNDPROC)
			Call PatchVal(PATCH_02, nAddrOriginal) 'Original WndProc address for CallWindowProc, call the original WndProc
			Call PatchVal(PATCH_06, nAddrOriginal) 'Original WndProc address for SetWindowLong, unsubclass on IDE stop
			'UPGRADE_ISSUE: ObjPtr function is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="367764E5-F3F8-4E43-AC3E-7FE0B5E074E2"'
			Call PatchVal(PATCH_0A, ObjPtr(Owner)) 'Owner object address for iSubclass_Proc
			
			'Set our WndProc in place of the original
			'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
			nAddrOriginal = WinSubHook2.SetWindowLong(hWnd, WinSubHook2.GWL_WNDPROC, nAddrSubclass)
			If nAddrOriginal <> 0 Then
				Subclass = True 'Success
			End If
		End If
		
		System.Diagnostics.Debug.Assert(Subclass, "")
	End Function
	
	'Stop subclassing the window
	Public Function UnSubclass() As Boolean
		If hWndSub <> 0 Then
			Call PatchVal(PATCH_05, 0) 'Patch the Table B entry count to ensure no further iSubclass_Proc callbacks
			Call PatchVal(PATCH_09, 0) 'Patch the Table A entry count to ensure no further iSubclass_Proc callbacks
			
			'Restore the original WndProc
			'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
			Call WinSubHook2.SetWindowLong(hWndSub, WinSubHook2.GWL_WNDPROC, nAddrOriginal)
			
			hWndSub = 0 'Indicate the subclasser is inactive
			nMsgCntB = 0 'Message before count equals zero
			nMsgCntA = 0 'Message after count equals zero
			UnSubclass = True 'Success
		End If
		
		System.Diagnostics.Debug.Assert(UnSubclass, "")
	End Function
	
	'============================================
	'Private interface
	'============================================
	
	'Return the address of the passed function in the passed dll
	Private Function AddrFunc(ByVal sDLL As String, ByVal sProc As String) As Integer
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		AddrFunc = WinSubHook2.GetProcAddress(WinSubHook2.GetModuleHandle(sDLL), sProc)
		
		'You may want to comment out the following line if you're using vb5 else the EbMode
		'GetProcAddress will stop here everytime because we look in vba6.dll first
		System.Diagnostics.Debug.Assert(AddrFunc, "")
	End Function
	
	'Worker sub for AddMsg
	'UPGRADE_NOTE: When was upgraded to When_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
	Private Sub AddMsgSub(ByVal uMsg As WinSubHook2.eMsg, ByRef aMsgTbl() As WinSubHook2.eMsg, ByRef nMsgCnt As Integer, ByVal When_Renamed As WinSubHook2.eMsgWhen)
		Const PATCH_04 As Integer = 88 'Table B (before) address
		Const PATCH_08 As Integer = 132 'Table A (after) address
		Dim nEntry As Integer
		Dim nOff1 As Integer
		Dim nOff2 As Integer
		
		If uMsg = WinSubHook2.eMsg.ALL_MESSAGES Then 'If ALL_MESSAGES
			nMsgCnt = -1 'Indicates that all messages shall callback
		Else 'Else a specific message number
			For nEntry = 1 To nMsgCnt 'For each existing entry. NB will skip if nMsgCnt = 0
				Select Case aMsgTbl(nEntry) 'Select on the message number stored in this table entry
					Case -1 'This msg table slot is a deleted entry
						aMsgTbl(nEntry) = uMsg 'Re-use this entry
						Exit Sub 'Bail
					Case uMsg 'The msg is already in the table!
						Exit Sub 'Bail
				End Select
			Next nEntry 'Next entry
			
			'Make space for the new entry
			'UPGRADE_WARNING: Lower bound of array aMsgTbl was changed from 1 to 0. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
			ReDim Preserve aMsgTbl(nEntry) 'Increase the size of the table. NB nEntry = nMsgCnt + 1
			nMsgCnt = nEntry 'Bump the entry count
			aMsgTbl(nEntry) = uMsg 'Store the message number in the table
		End If
		
		If When_Renamed = WinSubHook2.eMsgWhen.MSG_BEFORE Then 'If before
			nOff1 = PATCH_04 'Offset to the Before table address
			nOff2 = PATCH_05 'Offset to the Before table entry count
		Else 'Else after
			nOff1 = PATCH_08 'Offset to the After table address
			nOff2 = PATCH_09 'Offset to the After table entry count
		End If
		
		'Patch the appropriate table entries
		Call PatchVal(nOff1, AddrMsgTbl(aMsgTbl)) 'Patch the appropriate table address. We need do this because there's no guarantee that the table existed at SubClass time, the table only gets created if a message number is added.
		Call PatchVal(nOff2, nMsgCnt) 'Patch the appropriate table entry count
	End Sub
	
	'Return the address of the low bound of the passed table array
	Private Function AddrMsgTbl(ByRef aMsgTbl() As WinSubHook2.eMsg) As Integer
		On Error Resume Next 'The table may not be dimensioned yet so we need protection
		'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="367764E5-F3F8-4E43-AC3E-7FE0B5E074E2"'
		AddrMsgTbl = VarPtr(aMsgTbl(1)) 'Get the address of the first element of the passed message table
		On Error GoTo 0 'Switch off error protection
	End Function
	
	'Worker sub for DelMsg
	'UPGRADE_NOTE: When was upgraded to When_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
	Private Sub DelMsgSub(ByVal uMsg As WinSubHook2.eMsg, ByRef aMsgTbl() As WinSubHook2.eMsg, ByRef nMsgCnt As Integer, ByVal When_Renamed As WinSubHook2.eMsgWhen)
		Dim nEntry As Integer
		
		If uMsg = WinSubHook2.eMsg.ALL_MESSAGES Then 'If deleting all messages (specific or ALL_MESSAGES)
			nMsgCnt = 0 'Message count is now zero
			If When_Renamed = WinSubHook2.eMsgWhen.MSG_BEFORE Then 'If before
				nEntry = PATCH_05 'Patch the before table message count location
			Else 'Else after
				nEntry = PATCH_09 'Patch the after table message count location
			End If
			Call PatchVal(nEntry, 0) 'Patch the table message count
		Else 'Else deleteting a specific message
			For nEntry = 1 To nMsgCnt 'For each table entry
				If aMsgTbl(nEntry) = uMsg Then 'If this entry is the message we wish to delete
					aMsgTbl(nEntry) = -1 'Mark the table slot as available
					Exit For 'Bail
				End If
			Next nEntry 'Next entry
		End If
	End Sub
	
	'Patch the machine code buffer offset with the relative address to the target address
	Private Sub PatchRel(ByVal nOffset As Integer, ByVal nTargetAddr As Integer)
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		Call WinSubHook2.CopyMemory((nAddrSubclass + nOffset), nTargetAddr - nAddrSubclass - nOffset - 4, 4)
	End Sub
	
	'Patch the machine code buffer offset with the passed value
	Private Sub PatchVal(ByVal nOffset As Integer, ByVal nValue As Integer)
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		Call WinSubHook2.CopyMemory((nAddrSubclass + nOffset), nValue, 4)
	End Sub
End Class