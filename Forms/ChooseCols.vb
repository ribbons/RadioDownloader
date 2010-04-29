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

Imports System.Collections.Generic
Imports System.Globalization

Public Class ChooseCols
    Private columnOrder As List(Of Integer)
    Private columnNames As Dictionary(Of Integer, String)

    Public Property Columns() As String
        Get
            Dim stringCols(columnOrder.Count - 1) As String

            For column As Integer = 0 To columnOrder.Count - 1
                stringCols(column) = columnOrder(column).ToString(CultureInfo.InvariantCulture)
            Next

            Return Join(stringCols, ",")
        End Get
        Set(ByVal cols As String)
            columnOrder = New List(Of Integer)

            If cols <> String.Empty Then
                Dim stringCols() As String = Split(cols, ",")

                For Each column As String In stringCols
                    columnOrder.Add(CInt(column))
                Next
            End If
        End Set
    End Property

    Public Sub StoreNameList(ByVal columnNames As Dictionary(Of Integer, String))
        Me.columnNames = columnNames
    End Sub

    Private Sub ChooseCols_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If columnOrder Is Nothing Then
            Throw New InvalidOperationException("Column order is not set")
        ElseIf columnNames Is Nothing Then
            Throw New InvalidOperationException("Column names are not set")
        End If

        Me.Font = SystemFonts.MessageBoxFont

        ' Add the current columns to the top of the list, in order
        For Each column As Integer In columnOrder
            Dim addCol As New ListViewItem(columnNames(column))
            addCol.Name = column.ToString(CultureInfo.InvariantCulture)
            addCol.Checked = True

            ColumnsList.Items.Add(addCol)
        Next

        ' Add the rest of the columns to the list in their defined order
        For Each column As Integer In columnNames.Keys
            If columnOrder.Contains(column) = False Then
                Dim addCol As New ListViewItem(columnNames(column))
                addCol.Name = column.ToString(CultureInfo.InvariantCulture)
                addCol.Checked = False

                ColumnsList.Items.Add(addCol)
            End If
        Next
    End Sub

    Private Sub Okay_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Okay.Click
        columnOrder.Clear()

        For Each item As ListViewItem In ColumnsList.Items
            If item.Checked Then
                columnOrder.Add(Integer.Parse(item.Name, CultureInfo.InvariantCulture))
            End If
        Next
    End Sub

    Private Sub ColumnsList_ItemChecked(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckedEventArgs) Handles ColumnsList.ItemChecked
        Call UpdateButtonState()
    End Sub

    Private Sub ColumnsList_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ColumnsList.SelectedIndexChanged
        Call UpdateButtonState()
    End Sub

    Private Sub UpdateButtonState()
        If ColumnsList.SelectedItems.Count = 0 Then
            MoveUp.Enabled = False
            MoveDown.Enabled = False
            ShowButton.Enabled = False
            HideButton.Enabled = False
        Else
            If ColumnsList.SelectedItems(0).Index = 0 Then
                MoveUp.Enabled = False
            Else
                MoveUp.Enabled = True
            End If

            If ColumnsList.SelectedItems(0).Index = ColumnsList.Items.Count - 1 Then
                MoveDown.Enabled = False
            Else
                MoveDown.Enabled = True
            End If

            If ColumnsList.SelectedItems(0).Checked Then
                ShowButton.Enabled = False
                HideButton.Enabled = True
            Else
                ShowButton.Enabled = True
                HideButton.Enabled = False
            End If
        End If
    End Sub

    Private Sub ShowButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ShowButton.Click
        ColumnsList.SelectedItems(0).Checked = True
    End Sub

    Private Sub HideButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HideButton.Click
        ColumnsList.SelectedItems(0).Checked = False
    End Sub

    Private Sub MoveUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MoveUp.Click
        Dim moveItem As ListViewItem = ColumnsList.SelectedItems(0)
        Dim origIndex As Integer = ColumnsList.SelectedItems(0).Index

        ColumnsList.Items.Remove(moveItem)
        ColumnsList.Items.Insert(origIndex - 1, moveItem)
    End Sub

    Private Sub MoveDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MoveDown.Click
        Dim moveItem As ListViewItem = ColumnsList.SelectedItems(0)
        Dim origIndex As Integer = ColumnsList.SelectedItems(0).Index

        ColumnsList.Items.Remove(moveItem)
        ColumnsList.Items.Insert(origIndex + 1, moveItem)
    End Sub
End Class