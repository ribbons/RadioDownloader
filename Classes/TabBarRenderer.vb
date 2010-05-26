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

Imports System.Drawing.Drawing2D

Friend Class TabBarRenderer
    Inherits ToolStripSystemRenderer

    Private Const tabSeparation As Integer = 3

    Protected Overrides Sub OnRenderToolStripBackground(ByVal e As System.Windows.Forms.ToolStripRenderEventArgs)
        ' Set the background colour to transparent to make it glass
        e.Graphics.Clear(Color.Transparent)
    End Sub

    Protected Overrides Sub OnRenderButtonBackground(ByVal e As System.Windows.Forms.ToolStripItemRenderEventArgs)
        If e.Item.DisplayStyle = ToolStripItemDisplayStyle.Image Then
            ' Do not paint a background for icon only buttons
            Return
        End If

        Dim button As ToolStripButton = CType(e.Item, ToolStripButton)
        Dim colour As Brush = Brushes.Gray

        If button.Checked Then
            colour = Brushes.White

            ' Invalidate between the buttons and the bottom of the toolstrip so that it gets repainted
            e.ToolStrip.Invalidate(New Rectangle(0, e.Item.Bounds.Bottom, e.ToolStrip.Bounds.Width, e.ToolStrip.Bounds.Height - e.Item.Bounds.Bottom))
        ElseIf e.Item.Selected Then
            colour = Brushes.WhiteSmoke
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
            e.Graphics.DrawPath(Pens.Black, tab)
        End Using
    End Sub

    Protected Overrides Sub OnRenderItemText(ByVal e As System.Windows.Forms.ToolStripItemTextRenderEventArgs)
        ' Temporary workaround for transparent text
        Using path As New GraphicsPath()
            path.AddString(e.Text.Replace("&", ""), e.TextFont.FontFamily, e.TextFont.Style, e.TextFont.Size + 2, e.TextRectangle.Location, New StringFormat())
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality
            e.Graphics.FillPath(Brushes.Black, path)
        End Using
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
            e.Graphics.FillRectangle(Brushes.White, New Rectangle(checked.Bounds.Left, checked.Bounds.Bottom, checked.Bounds.Width - tabSeparation, e.ToolStrip.Bounds.Bottom - checked.Bounds.Bottom))
            e.Graphics.DrawLine(Pens.Black, checked.Bounds.Left, checked.Bounds.Bottom, checked.Bounds.Left, e.AffectedBounds.Bottom)
            e.Graphics.DrawLine(Pens.Black, checked.Bounds.Right - tabSeparation, checked.Bounds.Bottom, checked.Bounds.Right - tabSeparation, e.AffectedBounds.Bottom)
        End If
    End Sub
End Class
