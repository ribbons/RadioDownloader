Option Strict On
Option Explicit On

' Code in this class is based on code from http://www.codeproject.com/cs/miscctrl/ListViewEmbeddedControls.asp

Public Class ExtListview : Inherits ListView
    ' ListView messages
    Private Const LVM_FIRST As Integer = &H1000
    Private Const LVM_GETCOLUMNORDERARRAY As Integer = (LVM_FIRST + 59)

    ' Windows Messages
    Private Const WM_PAINT As Integer = &HF

    Private Structure EmbeddedControl
        Dim ctrControl As Control
        Dim intColumn As Integer
        Dim intRow As Integer
        Dim dstDock As DockStyle
        Dim lstItem As ListViewItem
    End Structure

    Private embeddedControls As New ArrayList()

    Protected Function GetColumnOrder() As Integer()
        Dim intOrder(Me.Columns.Count) As Integer

        For intLoop As Integer = 0 To Me.Columns.Count - 1
            intOrder(intLoop) = Me.Columns(intLoop).DisplayIndex
        Next

        Return intOrder
    End Function

    Protected Function GetSubItemBounds(ByVal lstListItem As ListViewItem, ByVal intSubItem As Integer) As Rectangle
        Dim subItemRect As Rectangle = Rectangle.Empty

        If lstListItem Is Nothing Then
            Throw New ArgumentNullException("Item")
        End If

        Dim intOrder() As Integer = GetColumnOrder()

        If intOrder Is Nothing Then
            ' No Columns
            Return subItemRect
        End If

        If intSubItem >= intOrder.Length Then
            Throw New IndexOutOfRangeException("SubItem " + intSubItem.ToString + " out of range")
        End If

        ' Retrieve the bounds of the entire ListViewItem (all subitems)
        Dim lviBounds As Rectangle = lstListItem.GetBounds(ItemBoundsPortion.Entire)

        Dim subItemX As Integer = lviBounds.Left

        ' Calculate the X position of the SubItem.
        ' Because the columns can be reordered we have to use Columns[order[i]] instead of Columns[i] !

        Dim colHdr As ColumnHeader
        Dim intLoop As Integer

        For intLoop = 0 To intOrder.Length - 1
            colHdr = Me.Columns(intOrder(intLoop))

            If colHdr.Index = intSubItem Then
                Exit For
            End If

            subItemX += colHdr.Width
        Next

        subItemRect = New Rectangle(subItemX, lviBounds.Top, Me.Columns(intOrder(intLoop)).Width, lviBounds.Height)

        Return subItemRect
    End Function

    ' Add a control to the ListView
    Public Sub AddEmbeddedControl(ByVal ctlControl As Control, ByVal intCol As Integer, ByVal intRow As Integer)
        Call AddEmbeddedControl(ctlControl, intCol, intRow, DockStyle.Fill)
    End Sub

    ' Add a control to the ListView
    Public Sub AddEmbeddedControl(ByVal ctlControl As Control, ByVal intCol As Integer, ByVal intRow As Integer, ByVal dstDock As DockStyle)
        If ctlControl Is Nothing Then
            Throw New ArgumentNullException()
        End If

        If intCol >= Columns.Count Or intRow >= Items.Count Then
            Throw New ArgumentOutOfRangeException()
        End If

        Dim emcControl As EmbeddedControl

        emcControl.ctrControl = ctlControl
        emcControl.intColumn = intCol
        emcControl.intRow = intRow
        emcControl.dstDock = dstDock
        emcControl.lstItem = Items(intRow)

        embeddedControls.Add(emcControl)

        ' Add a Click event handler to select the ListView row when an embedded control is clicked
        'AddHandler ctlControl.Click, embeddedControl_Click()

        'ctlControl.Click += New EventHandler()

        Me.Controls.Add(ctlControl)
    End Sub

    ' Remove a control from the ListView
    Public Sub RemoveEmbeddedControl(ByVal ctlControl As Control)
        If ctlControl Is Nothing Then
            Throw New ArgumentNullException()
        End If

        For intLoop As Integer = 0 To embeddedControls.Count - 1
            Dim emcControl As EmbeddedControl = CType(embeddedControls(intLoop), EmbeddedControl)

            If emcControl.ctrControl.Equals(ctlControl) Then
                'ctlControl.Click -= New EventHandler(embeddedControl_Click)
                Me.Controls.Remove(ctlControl)
                embeddedControls.RemoveAt(intLoop)
                Exit Sub
            End If
        Next

        Throw New Exception("Control not found!")
    End Sub

    ' Retrieve the control embedded at a given location
    Public Function GetEmbeddedControl(ByVal intCol As Integer, ByVal intRow As Integer) As Control
        For Each emcControl As EmbeddedControl In embeddedControls
            If emcControl.intRow = intRow And emcControl.intColumn = intCol Then
                Return emcControl.ctrControl
            End If
        Next

        Return Nothing
    End Function

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case WM_PAINT
                If View <> View.Details Then
                    Exit Select
                End If

                ' Calculate the position of all embedded controls
                For Each emcControl As EmbeddedControl In embeddedControls
                    Dim rect As Rectangle = Me.GetSubItemBounds(emcControl.lstItem, emcControl.intColumn)

                    If ((Me.HeaderStyle <> ColumnHeaderStyle.None) And (rect.Top < Me.Font.Height)) Then
                        ' Control overlaps ColumnHeader
                        emcControl.ctrControl.Visible = False
                        Continue For
                    Else
                        emcControl.ctrControl.Visible = True
                    End If

                    Select Case emcControl.dstDock
                        Case DockStyle.Fill
                        Case DockStyle.Top
                            rect.Height = emcControl.ctrControl.Height
                        Case DockStyle.Left
                            rect.Width = emcControl.ctrControl.Width
                        Case DockStyle.Bottom
                            rect.Offset(0, rect.Height - emcControl.ctrControl.Height)
                            rect.Height = emcControl.ctrControl.Height
                        Case DockStyle.Right
                            rect.Offset(rect.Width - emcControl.ctrControl.Width, 0)
                            rect.Width = emcControl.ctrControl.Width
                        Case DockStyle.None
                            rect.Size = emcControl.ctrControl.Size
                    End Select

                    ' Set embedded control's bounds
                    emcControl.ctrControl.Bounds = rect
                Next
        End Select

        Call MyBase.WndProc(m)
    End Sub


    Private Sub embeddedControl_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' When a control is clicked the ListViewItem holding it is selected
        For Each emcControl As EmbeddedControl In embeddedControls
            If emcControl.ctrControl.Equals(CType(sender, Control)) Then
                Me.SelectedItems.Clear()
                emcControl.lstItem.Selected = True
            End If
        Next
    End Sub
End Class
