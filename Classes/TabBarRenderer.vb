' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007-2010 Matt Robinson
'
' This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
' Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
' option) any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
' implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
' License for more details.
'
' You should have received a copy of the GNU General Public License along with this program; if not, write
' to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms.VisualStyles
Imports System.Runtime.InteropServices

Friend Class TabBarRenderer
    Inherits ToolStripSystemRenderer
    Implements IDisposable

    <DllImport("gdi32.dll", SetLastError:=True)> _
    Public Shared Function CreateCompatibleDC(ByVal hdc As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll", SetLastError:=True)> _
    Private Shared Function SelectObject(ByVal hdc As IntPtr, ByVal hgdiobj As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll", SetLastError:=True)> _
    Private Shared Function CreateDIBSection(ByVal hdc As IntPtr, <[In]()> ByRef lpbmi As BITMAPINFO, ByVal usage As UInteger, ByRef ppvBits As IntPtr, ByVal hSection As IntPtr, ByVal offset As UInteger) As IntPtr
    End Function

    <DllImport("gdi32.dll", SetLastError:=True)> _
    Private Shared Function DeleteObject(ByVal ho As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("gdi32.dll", SetLastError:=True)> _
    Private Shared Function DeleteDC(ByVal hdc As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("dwmapi.dll", SetLastError:=True)> _
    Private Shared Function DwmIsCompositionEnabled(<MarshalAs(UnmanagedType.Bool)> ByRef pfEnabled As Boolean) As Integer
    End Function

    <DllImport("UxTheme.dll", SetLastError:=True)> _
    Private Shared Function DrawThemeTextEx(ByVal hTheme As IntPtr, ByVal hdc As IntPtr, ByVal iPartId As Integer, ByVal iStateId As Integer, <MarshalAs(UnmanagedType.LPWStr)> ByVal pszText As String, ByVal iCharCount As Integer, ByVal dwFlags As UInteger, ByRef pRect As RECT, <[In]()> ByRef pOptions As DTTOPTS) As Integer
    End Function

    <DllImport("msimg32.dll", SetLastError:=True)> _
    Private Shared Function AlphaBlend(ByVal hdcDest As IntPtr, ByVal xoriginDest As Integer, ByVal yoriginDest As Integer, ByVal wDest As Integer, ByVal hDest As Integer, ByVal hdcSrc As IntPtr, ByVal xoriginSrc As Integer, ByVal yoriginSrc As Integer, ByVal wSrc As Integer, ByVal hSrc As Integer, ByVal ftn As BLENDFUNCTION) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure BLENDFUNCTION
        Public BlendOp As Byte
        Public BlendFlags As Byte
        Public SourceConstantAlpha As Byte
        Public AlphaFormat As Byte
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure BITMAPINFO
        Public biSize As UInteger
        Public biWidth As Integer
        Public biHeight As Integer
        Public biPlanes As UShort
        Public biBitCount As UShort
        Public biCompression As UInteger
        Public biSizeImage As UInteger
        Public biXPelsPerMeter As Integer
        Public biYPelsPerMeter As Integer
        Public biClrUsed As UInteger
        Public biClrImportant As UInteger
        Public rgbBlue As Byte
        Public rgbGreen As Byte
        Public rgbRed As Byte
        Public rgbReserved As Byte
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure DTTOPTS
        Public dwSize As UInteger
        Public dwFlags As UInteger
        Public crText As UInteger
        Public crBorder As UInteger
        Public crShadow As UInteger
        Public iTextShadowType As Integer
        Public ptShadowOffset As Point
        Public iBorderSize As Integer
        Public iFontPropId As Integer
        Public iColorPropId As Integer
        Public iStateId As Integer
        <MarshalAs(UnmanagedType.Bool)> _
        Public fApplyOverlay As Boolean
        Public iGlowSize As Integer
        Public pfnDrawTextCallback As Integer
        Public lParam As Integer
    End Structure

    Private Const BI_RGB As Integer = 0

    Private Const DTT_COMPOSITED As Integer = 8192
    Private Const DTT_GLOWSIZE As Integer = 2048
    Private Const DTT_TEXTCOLOR As Integer = 1

    Private Const AC_SRC_OVER As Integer = 0
    Private Const AC_SRC_ALPHA As Integer = 1

    Private Const tabSeparation As Integer = 3

    Private isDisposed As Boolean

    Private inactiveTabBkg As Brush
    Private hoverTabBkg As Brush
    Private tabBorder As Pen = New Pen(SystemColors.ControlDark)

    Protected Overrides Sub OnRenderToolStripBackground(ByVal e As System.Windows.Forms.ToolStripRenderEventArgs)
        Dim onGlass As Boolean = False

        If OsUtils.WinVistaOrLater Then
            If DwmIsCompositionEnabled(onGlass) <> 0 Then
                Throw New Win32Exception
            End If
        End If

        If onGlass Then
            ' Set the background colour to transparent to make it glass
            e.Graphics.Clear(Color.Transparent)
        Else
            MyBase.OnRenderToolStripBackground(e)
        End If
    End Sub

    Protected Overrides Sub OnRenderButtonBackground(ByVal e As System.Windows.Forms.ToolStripItemRenderEventArgs)
        If e.Item.DisplayStyle = ToolStripItemDisplayStyle.Image Then
            ' Do not paint a background for icon only buttons
            Return
        End If

        If inactiveTabBkg Is Nothing Then
            inactiveTabBkg = New LinearGradientBrush(New Point(0, 0), New Point(0, e.Item.Height), SystemColors.Control, SystemColors.ControlDark)
            hoverTabBkg = New LinearGradientBrush(New Point(0, 0), New Point(0, e.Item.Height), SystemColors.ControlLight, SystemColors.Control)
        End If

        Using activeTabBkg As New LinearGradientBrush(New Point(0, 0), New Point(0, e.Item.Height), SystemColors.ControlLight, GetActiveTabBtmCol(e.ToolStrip, e.Item))
            Dim button As ToolStripButton = CType(e.Item, ToolStripButton)
            Dim colour As Brush = inactiveTabBkg

            If button.Checked Then
                colour = activeTabBkg

                ' Invalidate between the buttons and the bottom of the toolstrip so that it gets repainted
                e.ToolStrip.Invalidate(New Rectangle(0, e.Item.Bounds.Bottom, e.ToolStrip.Bounds.Width, e.ToolStrip.Bounds.Height - e.Item.Bounds.Bottom))
            ElseIf e.Item.Selected Then
                colour = hoverTabBkg
            End If

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality

            Dim width As Integer = e.Item.Width - tabSeparation
            Dim height As Integer = e.Item.Height

            Const curveSize As Integer = 10

            Using tab As New GraphicsPath
                tab.AddLine(0, height, 0, curveSize)
                tab.AddArc(0, 0, curveSize, curveSize, 180, 90)
                tab.AddLine(curveSize, 0, width - curveSize, 0)
                tab.AddArc(width - curveSize, 0, curveSize, curveSize, 270, 90)
                tab.AddLine(width, curveSize, width, height)

                e.Graphics.FillPath(colour, tab)
                e.Graphics.DrawPath(tabBorder, tab)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnRenderItemText(ByVal e As System.Windows.Forms.ToolStripItemTextRenderEventArgs)
        If OsUtils.WinVistaOrLater = False Then
            ' XP and earlier don't have glass or the APIs to draw text on it
            MyBase.OnRenderItemText(e)
            Return
        End If

        ' Drawing text on glass is a bit of a pain - text generated with GDI (e.g. standard
        ' controls) ends up being transparent as GDI doesn't understand alpha transparency.
        ' GDI+ is fine drawing text on glass but it doesn't use ClearType, so the text ends
        ' up looking out of place, ugly or both.  The proper way is using DrawThemeTextEx,
        ' which works fine, but requires a top-down DIB to draw to, rather than the bottom
        ' up ones that GDI normally uses.  Hence; create top-down DIB, draw text to it and
        ' then AlphaBlend it in to the graphics object that we are rendering to.

        ' Get the rendering HDC, and create a compatible one for drawing the text to
        Dim renderHdc As IntPtr = e.Graphics.GetHdc()
        Dim memoryHdc As IntPtr = CreateCompatibleDC(renderHdc)

        If memoryHdc = IntPtr.Zero Then ' NULL Pointer
            Throw New Win32Exception
        End If

        Dim info As BITMAPINFO
        info.biSize = CUInt(Marshal.SizeOf(GetType(BITMAPINFO)))
        info.biWidth = e.TextRectangle.Width
        info.biHeight = -e.TextRectangle.Height ' Negative = top-down
        info.biPlanes = 1
        info.biBitCount = 32
        info.biCompression = BI_RGB

        ' Create the top-down DIB
        Dim dib As IntPtr = CreateDIBSection(renderHdc, info, 0, IntPtr.Zero, IntPtr.Zero, 0)

        If dib = IntPtr.Zero Then ' NULL Pointer
            Throw New Win32Exception
        End If

        ' Select the created DIB into our memory DC for use
        If SelectObject(memoryHdc, dib) = IntPtr.Zero Then ' NULL Pointer
            Throw New Win32Exception
        End If

        ' Create a font we can use with GetThemeTextEx
        Dim hFont As IntPtr = e.TextFont.ToHfont()

        ' And select it into the DC as well
        If SelectObject(memoryHdc, hFont) = IntPtr.Zero Then ' NULL Pointer
            Throw New Win32Exception
        End If

        ' Fetch a VisualStyleRenderer suitable for toolbar text
        Dim renderer As New VisualStyleRenderer(VisualStyleElement.ToolBar.Button.Normal)

        ' Set up a RECT for the area to draw the text in
        Dim textRect As RECT
        textRect.left = 0
        textRect.top = 0
        textRect.right = e.TextRectangle.Width
        textRect.bottom = e.TextRectangle.Height

        ' Options for GetThemeTextEx
        Dim opts As DTTOPTS
        opts.dwSize = CUInt(Marshal.SizeOf(opts))
        opts.dwFlags = DTT_COMPOSITED Or DTT_TEXTCOLOR ' Draw alpha blended text of the colour specified
        opts.crText = CUInt(ColorTranslator.ToWin32(e.TextColor))

        ' Paint the text
        If DrawThemeTextEx(renderer.Handle, memoryHdc, 0, 0, e.Text, -1, CUInt(e.TextFormat), textRect, opts) <> 0 Then
            Throw New Win32Exception
        End If

        ' Set up the AlphaBlend copy
        Dim blendFunc As BLENDFUNCTION
        blendFunc.BlendOp = AC_SRC_OVER
        blendFunc.SourceConstantAlpha = 255 ' Per-pixel alpha only
        blendFunc.AlphaFormat = AC_SRC_ALPHA

        ' Blend the painted text into the render DC
        If AlphaBlend(renderHdc, e.TextRectangle.Left, e.TextRectangle.Top, e.TextRectangle.Width, e.TextRectangle.Height, memoryHdc, 0, 0, e.TextRectangle.Width, e.TextRectangle.Height, blendFunc) = False Then
            Throw New Win32Exception
        End If

        ' Clean up the GDI objects

        If DeleteObject(hFont) = False Then
            Throw New Win32Exception
        End If

        If DeleteObject(dib) = False Then
            Throw New Win32Exception
        End If

        If DeleteDC(memoryHdc) = False Then
            Throw New Win32Exception
        End If

        e.Graphics.ReleaseHdc()
    End Sub

    Protected Overrides Sub OnRenderSeparator(ByVal e As System.Windows.Forms.ToolStripSeparatorRenderEventArgs)
        ' Not painted as a visible separator
        Return
    End Sub

    Protected Overrides Sub OnRenderToolStripBorder(ByVal e As System.Windows.Forms.ToolStripRenderEventArgs)
        Dim checked As ToolStripButton = Nothing

        ' Find the currently checked ToolStripButton
        For Each item As ToolStripItem In e.ToolStrip.Items
            Dim buttonItem As ToolStripButton = TryCast(item, ToolStripButton)

            If buttonItem IsNot Nothing AndAlso buttonItem.Checked Then
                checked = buttonItem
                Exit For
            End If
        Next

        If checked IsNot Nothing Then
            ' Extend the bottom of the tab over the client area border, joining the tab onto the main client area

            Using toolbarBkg As New SolidBrush(GetActiveTabBtmCol(e.ToolStrip, checked))
                e.Graphics.FillRectangle(toolbarBkg, New Rectangle(checked.Bounds.Left, checked.Bounds.Bottom, checked.Bounds.Width - tabSeparation, e.ToolStrip.Bounds.Bottom - checked.Bounds.Bottom))
            End Using

            e.Graphics.DrawLine(tabBorder, checked.Bounds.Left, checked.Bounds.Bottom, checked.Bounds.Left, e.AffectedBounds.Bottom)
            e.Graphics.DrawLine(tabBorder, checked.Bounds.Right - tabSeparation, checked.Bounds.Bottom, checked.Bounds.Right - tabSeparation, e.AffectedBounds.Bottom)
        End If
    End Sub

    Private Function GetActiveTabBtmCol(ByVal strip As ToolStrip, ByVal active As ToolStripItem) As Color
        Dim toolbarColour As Color = SystemColors.Control

        If VisualStyleRenderer.IsSupported Then
            ' Visual styles are enabled, so draw the correct background behind the toolbars

            Dim background As New Bitmap(strip.Width, strip.Height)
            Dim graphics As Graphics = graphics.FromImage(background)

            Try
                Dim rebar As New VisualStyleRenderer("Rebar", 0, 0)
                rebar.DrawBackground(graphics, New Rectangle(0, 0, strip.Width, strip.Height))
                toolbarColour = background.GetPixel(active.Bounds.Left + CInt(active.Width / 2), 0)
            Catch argumentExp As ArgumentException
                ' The 'Rebar' background image style did not exist
            End Try
        End If

        Return toolbarColour
    End Function

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.isDisposed Then
            If disposing Then
                If inactiveTabBkg IsNot Nothing Then
                    inactiveTabBkg.Dispose()
                    hoverTabBkg.Dispose()
                End If

                tabBorder.Dispose()
            End If
        End If

        Me.isDisposed = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub
End Class
