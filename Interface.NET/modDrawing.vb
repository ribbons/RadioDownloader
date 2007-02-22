Option Strict Off
Option Explicit On
Module modDrawing
	
	Private Structure SafeArrayBound
		Dim cElements As Integer
		Dim lLbound As Integer
	End Structure
	Private Structure SAFEARRAY1D ' used as DMA overlay on a DIB
		Dim cDims As Short
		Dim fFeatures As Short
		Dim cbElements As Integer
		Dim cLocks As Integer
		Dim pvData As Integer
		Dim rgSABound As SafeArrayBound
	End Structure
	Private Structure BLENDFUNCTION ' used for AlphaBlend API
		Dim BlendOp As Byte
		Dim BlendFlags As Byte
		Dim SourceConstantAlpha As Byte
		Dim AlphaFormat As Byte
	End Structure
	Private Structure BITMAP ' used to get stdPicture attributes
		Dim bmType As Integer
		Dim bmWidth As Integer
		Dim bmHeight As Integer
		Dim bmWidthBytes As Integer
		Dim bmPlanes As Short
		Dim bmBitsPixel As Short
		Dim bmBits As Integer
	End Structure
	
	Private Declare Function GetSystemMetrics Lib "user32" (ByVal nIndex As Integer) As Integer
	Private Declare Function LoadImageAsString Lib "user32"  Alias "LoadImageA"(ByVal hInst As Integer, ByVal lpsz As String, ByVal uType As Integer, ByVal cxDesired As Integer, ByVal cyDesired As Integer, ByVal fuLoad As Integer) As Integer
	Private Declare Function SendMessageLong Lib "user32"  Alias "SendMessageA"(ByVal hWnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
	Private Declare Function GetWindow Lib "user32" (ByVal hWnd As Integer, ByVal wCmd As Integer) As Integer
	
	Private Declare Function VarPtrArray Lib "msvbvm60.dll"  Alias "VarPtr"(ByRef Ptr() As Any) As Integer
	Private Declare Function AlphaBlend Lib "msimg32.dll" (ByVal hdcDest As Integer, ByVal nXOriginDest As Integer, ByVal nYOriginDest As Integer, ByVal nWidthDest As Integer, ByVal nHeightDest As Integer, ByVal hdcSrc As Integer, ByVal nXOriginSrc As Integer, ByVal nYOriginSrc As Integer, ByVal nWidthSrc As Integer, ByVal nHeightSrc As Integer, ByVal lBlendFunction As Integer) As Integer
	'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
	Private Declare Function GetGDIObject Lib "gdi32.dll"  Alias "GetObjectA"(ByVal hObject As Integer, ByVal nCount As Integer, ByRef lpObject As Any) As Integer
	
	' used for workaround of VB not exposing IStream interface
	'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
	Private Declare Function CreateStreamOnHGlobal Lib "ole32" (ByVal hGlobal As Integer, ByVal fDeleteOnRelease As Integer, ByRef ppstm As Any) As Integer
	Private Declare Function GlobalLock Lib "kernel32" (ByVal hMem As Integer) As Integer
	Private Declare Function GlobalUnlock Lib "kernel32" (ByVal hMem As Integer) As Integer
	'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
	'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
	'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
	Private Declare Function OleLoadPicture Lib "olepro32" (ByRef pStream As Any, ByVal lSize As Integer, ByVal fRunmode As Integer, ByRef riid As Any, ByRef ppvObj As Any) As Integer
	
	Private Const SM_CXICON As Short = 11
	Private Const SM_CYICON As Short = 12
	Private Const SM_CXSMICON As Short = 49
	Private Const SM_CYSMICON As Short = 50
	Private Const LR_DEFAULTCOLOR As Short = &H0s
	Private Const LR_MONOCHROME As Short = &H1s
	Private Const LR_COLOR As Short = &H2s
	Private Const LR_COPYRETURNORG As Short = &H4s
	Private Const LR_COPYDELETEORG As Short = &H8s
	Private Const LR_LOADFROMFILE As Short = &H10s
	Private Const LR_LOADTRANSPARENT As Short = &H20s
	Private Const LR_DEFAULTSIZE As Short = &H40s
	Private Const LR_VGACOLOR As Short = &H80s
	Private Const LR_LOADMAP3DCOLORS As Short = &H1000s
	Private Const LR_CREATEDIBSECTION As Short = &H2000s
	Private Const LR_COPYFROMRESOURCE As Short = &H4000s
	Private Const LR_SHARED As Integer = &H8000
	Private Const IMAGE_ICON As Short = 1
	Private Const WM_SETICON As Short = &H80s
	Private Const ICON_SMALL As Short = 0
	Private Const ICON_BIG As Short = 1
	Private Const GW_OWNER As Short = 4
	
	Public Sub SetIcon(ByVal hWnd As Integer, ByVal sIconResName As String, Optional ByVal bSetAsAppIcon As Boolean = True)
		Dim lhWndTop As Integer
		Dim lhWnd As Integer
		Dim cx As Integer
		Dim cy As Integer
		Dim hIconLarge As Integer
		Dim hIconSmall As Integer
		
		If (bSetAsAppIcon) Then
			' Find VB's hidden parent window:
			lhWnd = hWnd
			lhWndTop = lhWnd
			Do While Not (lhWnd = 0)
				lhWnd = GetWindow(lhWnd, GW_OWNER)
				If Not (lhWnd = 0) Then
					lhWndTop = lhWnd
				End If
			Loop 
		End If
		
		cx = GetSystemMetrics(SM_CXICON)
		cy = GetSystemMetrics(SM_CYICON)
		hIconLarge = LoadImageAsString(VB6.GetHInstance.ToInt32, sIconResName, IMAGE_ICON, cx, cy, LR_SHARED)
		If (bSetAsAppIcon) Then
			SendMessageLong(lhWndTop, WM_SETICON, ICON_BIG, hIconLarge)
		End If
		SendMessageLong(hWnd, WM_SETICON, ICON_BIG, hIconLarge)
		
		cx = GetSystemMetrics(SM_CXSMICON)
		cy = GetSystemMetrics(SM_CYSMICON)
		hIconSmall = LoadImageAsString(VB6.GetHInstance.ToInt32, sIconResName, IMAGE_ICON, cx, cy, LR_SHARED)
		If (bSetAsAppIcon) Then
			SendMessageLong(lhWndTop, WM_SETICON, ICON_SMALL, hIconSmall)
		End If
		SendMessageLong(hWnd, WM_SETICON, ICON_SMALL, hIconSmall)
	End Sub
	
	Public Function SystrayIcon(ByVal sIconResName As String) As Integer
		Dim lhWndTop As Integer
		Dim lhWnd As Integer
		Dim cx As Integer
		Dim cy As Integer
		Dim hIconLarge As Integer
		Dim hIconSmall As Integer
		
		cx = GetSystemMetrics(SM_CXSMICON)
		cy = GetSystemMetrics(SM_CYSMICON)
		SystrayIcon = LoadImageAsString(VB6.GetHInstance.ToInt32, sIconResName, IMAGE_ICON, cx, cy, LR_SHARED)
	End Function
	
	Public Sub DrawTransp(ByVal strID As String, ByRef picPicturebox As System.Windows.Forms.PictureBox, ByVal lngImageX As Integer, ByVal lngImageY As Integer, ByVal lngContainerX As Integer, ByVal lngContainerY As Integer)
		Dim picLoad As System.Drawing.Image
		
		' LoadResPicture does not like 32bpp images, so create bitmap from array
		Dim tArray() As Byte
		'UPGRADE_ISSUE: Global method LoadResData was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6B85A2A7-FE9F-4FBE-AA0C-CF11AC86A305"'
		tArray = VB6.LoadResData(strID, "BMP")
		picLoad = CreateStdPicFromArray(tArray)
		
		Dim tSA As SAFEARRAY1D
		Dim tBMP As BITMAP
		Dim dibBytes() As Byte
		Dim X, Y As Integer
		Dim bAbort As Boolean
		
		'UPGRADE_WARNING: Couldn't resolve default property of object tBMP. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_ISSUE: Picture property picLoad.Handle was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="CC4C7EC0-C903-48FC-ACCC-81861D12DA4A"'
		Call GetGDIObject(picLoad.Handle, Len(tBMP), tBMP)
		
		With tSA
			.cbElements = 1 ' byte elements
			.cDims = 1 ' single dim array
			.pvData = tBMP.bmBits
			.rgSABound.cElements = tBMP.bmWidth * tBMP.bmHeight * 4
		End With
		' overlay now
		'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="367764E5-F3F8-4E43-AC3E-7FE0B5E074E2"'
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		WinSubHook2.Kernel32.CopyMemory(VarPtrArray(dibBytes), VarPtr(tSA), 4)
		
		' premultiply RGBs
		For X = 3 To tSA.rgSABound.cElements - 1 Step 4
			For Y = X - 3 To X - 1
				dibBytes(Y) = ((0 + dibBytes(X)) * dibBytes(Y)) \ &HFFs
			Next 
		Next 
		
		' remove overlay
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		WinSubHook2.Kernel32.CopyMemory(VarPtrArray(dibBytes), 0, 4)
		
		'picPicturebox.Cls
		
		If picLoad Is Nothing Then Exit Sub
		
		Dim bf As BLENDFUNCTION
		Dim tDC, lBlend, tOldBmp As Integer
		Dim imgCy, imgCx, imgHandle As Integer
		
		' may be premultiplying a copy of the image for AlphaBlend's use.
		'UPGRADE_ISSUE: Picture property picLoad.Handle was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="CC4C7EC0-C903-48FC-ACCC-81861D12DA4A"'
		imgHandle = picLoad.Handle
		
		With bf ' fill in the blend function
			.AlphaFormat = WinSubHook2.eLayeredConsts.AC_SRC_ALPHA
			.BlendOp = WinSubHook2.eLayeredConsts.AC_SRC_OVER
			.SourceConstantAlpha = 255
		End With
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		WinSubHook2.Kernel32.CopyMemory(lBlend, bf, 4)
		
		' get image width/height
		'UPGRADE_ISSUE: Constant vbPixels was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="55B59875-9A95-4B71-9D6A-7C294BF7139D"'
		'UPGRADE_ISSUE: Constant vbHimetric was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="55B59875-9A95-4B71-9D6A-7C294BF7139D"'
		'UPGRADE_ISSUE: Picture property picLoad.Width was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="CC4C7EC0-C903-48FC-ACCC-81861D12DA4A"'
		'UPGRADE_ISSUE: Form method frmMain.ScaleX was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="CC4C7EC0-C903-48FC-ACCC-81861D12DA4A"'
		imgCx = frmMain.ScaleX(picLoad.Width, vbHimetric, vbPixels)
		'UPGRADE_ISSUE: Constant vbPixels was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="55B59875-9A95-4B71-9D6A-7C294BF7139D"'
		'UPGRADE_ISSUE: Constant vbHimetric was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="55B59875-9A95-4B71-9D6A-7C294BF7139D"'
		'UPGRADE_ISSUE: Picture property picLoad.Height was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="CC4C7EC0-C903-48FC-ACCC-81861D12DA4A"'
		'UPGRADE_ISSUE: Form method frmMain.ScaleY was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="CC4C7EC0-C903-48FC-ACCC-81861D12DA4A"'
		imgCy = frmMain.ScaleY(picLoad.Height, vbHimetric, vbPixels)
		' select image into a DC
		'UPGRADE_ISSUE: Form property frmMain.hDC was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="CC4C7EC0-C903-48FC-ACCC-81861D12DA4A"'
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		tDC = WinSubHook2.Gdi32.CreateCompatibleDC(frmMain.hDC)
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		tOldBmp = WinSubHook2.Gdi32.SelectObject(tDC, imgHandle)
		'UPGRADE_ISSUE: PictureBox property picPicturebox.hDC was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="CC4C7EC0-C903-48FC-ACCC-81861D12DA4A"'
		AlphaBlend(picPicturebox.hDC, lngContainerX, lngContainerY, imgCx - lngImageX, imgCy - lngImageY, tDC, lngImageX, lngImageY, imgCx - lngImageX, imgCy - lngImageY, lBlend)
		
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		WinSubHook2.Gdi32.SelectObject(tDC, tOldBmp)
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		WinSubHook2.Gdi32.DeleteDC(tDC)
	End Sub
	
	Private Function CreateStdPicFromArray(ByRef bytContent() As Byte, Optional ByRef byteOffset As Integer = 0) As System.Drawing.Image
		Dim o_lngLowerBound As Integer
		Dim o_lngByteCount As Integer
		Dim o_hMem As Integer
		Dim o_lpMem As Integer
		
		Dim aGUID(4) As Integer
		Dim tStream As stdole.IUnknown
		' IPicture GUID {7BF80980-BF32-101A-8BBB-00AA00300CAB}
		aGUID(0) = &H7BF80980
		aGUID(1) = &H101ABF32
		aGUID(2) = &HAA00BB8B
		aGUID(3) = &HAB0C3000
		
		o_lngByteCount = UBound(bytContent) - byteOffset + 1
		'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
		o_hMem = WinSubHook2.Kernel32.GlobalAlloc(&H2s, o_lngByteCount)
		If o_hMem <> 0 Then
			o_lpMem = GlobalLock(o_hMem)
			If o_lpMem <> 0 Then
				'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
				WinSubHook2.Kernel32.CopyMemory(o_lpMem, bytContent(byteOffset), o_lngByteCount)
				Call GlobalUnlock(o_hMem)
				'UPGRADE_WARNING: Couldn't resolve default property of object tStream. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				If CreateStreamOnHGlobal(o_hMem, 1, tStream) = 0 Then
					'UPGRADE_WARNING: Couldn't resolve default property of object CreateStdPicFromArray. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					'UPGRADE_ISSUE: ObjPtr function is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="367764E5-F3F8-4E43-AC3E-7FE0B5E074E2"'
					Call OleLoadPicture(ObjPtr(tStream), 0, 0, aGUID(0), CreateStdPicFromArray)
				End If
			End If
		End If
	End Function
End Module