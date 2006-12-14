Attribute VB_Name = "modDrawing"
Option Explicit

Private Type SafeArrayBound
    cElements As Long
    lLbound As Long
End Type
Private Type SAFEARRAY1D        ' used as DMA overlay on a DIB
    cDims As Integer
    fFeatures As Integer
    cbElements As Long
    cLocks As Long
    pvData As Long
    rgSABound As SafeArrayBound
End Type
Private Type BLENDFUNCTION      ' used for AlphaBlend API
    BlendOp As Byte
    BlendFlags As Byte
    SourceConstantAlpha As Byte
    AlphaFormat As Byte
End Type
Private Type BITMAP             ' used to get stdPicture attributes
    bmType As Long
    bmWidth As Long
    bmHeight As Long
    bmWidthBytes As Long
    bmPlanes As Integer
    bmBitsPixel As Integer
    bmBits As Long
End Type

Private Declare Function GetSystemMetrics Lib "user32" (ByVal nIndex As Long) As Long
Private Declare Function LoadImageAsString Lib "user32" Alias "LoadImageA" (ByVal hInst As Long, ByVal lpsz As String, ByVal uType As Long, ByVal cxDesired As Long, ByVal cyDesired As Long, ByVal fuLoad As Long) As Long
Private Declare Function SendMessageLong Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
Private Declare Function GetWindow Lib "user32" (ByVal hWnd As Long, ByVal wCmd As Long) As Long

Private Declare Function VarPtrArray Lib "msvbvm60.dll" Alias "VarPtr" (ByRef Ptr() As Any) As Long
Private Declare Function AlphaBlend Lib "msimg32.dll" (ByVal hdcDest As Long, ByVal nXOriginDest As Long, ByVal nYOriginDest As Long, ByVal nWidthDest As Long, ByVal nHeightDest As Long, ByVal hdcSrc As Long, ByVal nXOriginSrc As Long, ByVal nYOriginSrc As Long, ByVal nWidthSrc As Long, ByVal nHeightSrc As Long, ByVal lBlendFunction As Long) As Long
Private Declare Function GetGDIObject Lib "gdi32.dll" Alias "GetObjectA" (ByVal hObject As Long, ByVal nCount As Long, ByRef lpObject As Any) As Long

' used for workaround of VB not exposing IStream interface
Private Declare Function CreateStreamOnHGlobal Lib "ole32" (ByVal hGlobal As Long, ByVal fDeleteOnRelease As Long, ppstm As Any) As Long
Private Declare Function GlobalLock Lib "kernel32" (ByVal hMem As Long) As Long
Private Declare Function GlobalUnlock Lib "kernel32" (ByVal hMem As Long) As Long
Private Declare Function OleLoadPicture Lib "olepro32" (pStream As Any, ByVal lSize As Long, ByVal fRunmode As Long, riid As Any, ppvObj As Any) As Long

Private Const SM_CXICON = 11
Private Const SM_CYICON = 12
Private Const SM_CXSMICON = 49
Private Const SM_CYSMICON = 50
Private Const LR_DEFAULTCOLOR = &H0
Private Const LR_MONOCHROME = &H1
Private Const LR_COLOR = &H2
Private Const LR_COPYRETURNORG = &H4
Private Const LR_COPYDELETEORG = &H8
Private Const LR_LOADFROMFILE = &H10
Private Const LR_LOADTRANSPARENT = &H20
Private Const LR_DEFAULTSIZE = &H40
Private Const LR_VGACOLOR = &H80
Private Const LR_LOADMAP3DCOLORS = &H1000
Private Const LR_CREATEDIBSECTION = &H2000
Private Const LR_COPYFROMRESOURCE = &H4000
Private Const LR_SHARED = &H8000&
Private Const IMAGE_ICON = 1
Private Const WM_SETICON = &H80
Private Const ICON_SMALL = 0
Private Const ICON_BIG = 1
Private Const GW_OWNER = 4

Public Sub SetIcon(ByVal hWnd As Long, ByVal sIconResName As String, Optional ByVal bSetAsAppIcon As Boolean = True)
Dim lhWndTop As Long
Dim lhWnd As Long
Dim cx As Long
Dim cy As Long
Dim hIconLarge As Long
Dim hIconSmall As Long
      
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
   hIconLarge = LoadImageAsString(App.hInstance, sIconResName, IMAGE_ICON, cx, cy, LR_SHARED)
   If (bSetAsAppIcon) Then
      SendMessageLong lhWndTop, WM_SETICON, ICON_BIG, hIconLarge
   End If
   SendMessageLong hWnd, WM_SETICON, ICON_BIG, hIconLarge
   
   cx = GetSystemMetrics(SM_CXSMICON)
   cy = GetSystemMetrics(SM_CYSMICON)
   hIconSmall = LoadImageAsString(App.hInstance, sIconResName, IMAGE_ICON, cx, cy, LR_SHARED)
   If (bSetAsAppIcon) Then
      SendMessageLong lhWndTop, WM_SETICON, ICON_SMALL, hIconSmall
   End If
   SendMessageLong hWnd, WM_SETICON, ICON_SMALL, hIconSmall
End Sub

Public Function SystrayIcon(ByVal sIconResName As String) As Long
    Dim lhWndTop As Long
    Dim lhWnd As Long
    Dim cx As Long
    Dim cy As Long
    Dim hIconLarge As Long
    Dim hIconSmall As Long
      
   cx = GetSystemMetrics(SM_CXSMICON)
   cy = GetSystemMetrics(SM_CYSMICON)
   SystrayIcon = LoadImageAsString(App.hInstance, sIconResName, IMAGE_ICON, cx, cy, LR_SHARED)
End Function

Public Sub DrawTransp(ByVal strID As String, ByRef picPicturebox As PictureBox, ByVal lngImageX As Long, ByVal lngImageY As Long, ByVal lngContainerX As Long, ByVal lngContainerY As Long)
    Dim picLoad As StdPicture
    
    ' LoadResPicture does not like 32bpp images, so create bitmap from array
    Dim tArray() As Byte
    tArray() = LoadResData(strID, "BMP")
    Set picLoad = CreateStdPicFromArray(tArray)
        
    Dim tSA As SAFEARRAY1D, tBMP As BITMAP
    Dim dibBytes() As Byte
    Dim X As Long, Y As Long
    Dim bAbort As Boolean

    Call GetGDIObject(picLoad.Handle, Len(tBMP), tBMP)

    With tSA
        .cbElements = 1 ' byte elements
        .cDims = 1      ' single dim array
        .pvData = tBMP.bmBits
        .rgSABound.cElements = tBMP.bmWidth * tBMP.bmHeight * 4&
    End With
    ' overlay now
    CopyMemory ByVal VarPtrArray(dibBytes()), VarPtr(tSA), 4&

    ' premultiply RGBs
    For X = 3 To tSA.rgSABound.cElements - 1 Step 4
        For Y = X - 3 To X - 1
            dibBytes(Y) = ((0& + dibBytes(X)) * dibBytes(Y)) \ &HFF
        Next
    Next
        
    ' remove overlay
    CopyMemory ByVal VarPtrArray(dibBytes()), 0&, 4&
        
    'picPicturebox.Cls
    
    If picLoad Is Nothing Then Exit Sub
    
    Dim bf As BLENDFUNCTION, lBlend As Long, tDC As Long, tOldBmp As Long
    Dim imgCx As Long, imgCy As Long, imgHandle As Long
    
    ' may be premultiplying a copy of the image for AlphaBlend's use.
    imgHandle = picLoad.Handle
    
    With bf ' fill in the blend function
        .AlphaFormat = AC_SRC_ALPHA
        .BlendOp = AC_SRC_OVER
        .SourceConstantAlpha = 255
    End With
    CopyMemory lBlend, bf, 4&
    
    ' get image width/height
    imgCx = frmMain.ScaleX(picLoad.Width, vbHimetric, vbPixels)
    imgCy = frmMain.ScaleY(picLoad.Height, vbHimetric, vbPixels)
    ' select image into a DC
    tDC = CreateCompatibleDC(frmMain.hDC)
    tOldBmp = SelectObject(tDC, imgHandle)
    AlphaBlend picPicturebox.hDC, lngContainerX, lngContainerY, imgCx - lngImageX, imgCy - lngImageY, tDC, lngImageX, lngImageY, imgCx - lngImageX, imgCy - lngImageY, lBlend
    
    SelectObject tDC, tOldBmp
    DeleteDC tDC
End Sub

Private Function CreateStdPicFromArray(bytContent() As Byte, Optional byteOffset As Long = 0) As IPicture
    Dim o_lngLowerBound As Long
    Dim o_lngByteCount  As Long
    Dim o_hMem As Long
    Dim o_lpMem  As Long
    
    Dim aGUID(0 To 4) As Long, tStream As IUnknown
    ' IPicture GUID {7BF80980-BF32-101A-8BBB-00AA00300CAB}
    aGUID(0) = &H7BF80980
    aGUID(1) = &H101ABF32
    aGUID(2) = &HAA00BB8B
    aGUID(3) = &HAB0C3000
    
    o_lngByteCount = UBound(bytContent) - byteOffset + 1
    o_hMem = GlobalAlloc(&H2, o_lngByteCount)
    If o_hMem <> 0 Then
        o_lpMem = GlobalLock(o_hMem)
        If o_lpMem <> 0 Then
            CopyMemory ByVal o_lpMem, bytContent(byteOffset), o_lngByteCount
            Call GlobalUnlock(o_hMem)
            If CreateStreamOnHGlobal(o_hMem, 1, tStream) = 0 Then
                Call OleLoadPicture(ByVal ObjPtr(tStream), 0, 0, aGUID(0), CreateStdPicFromArray)
            End If
        End If
    End If
End Function
