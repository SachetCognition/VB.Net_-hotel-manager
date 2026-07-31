Imports System.Data.OleDb
Imports System.Security.Cryptography
Imports System.Text
Imports Excel = Microsoft.Office.Interop.Excel
Public Class frmStock








    Public Shared Function GetUniqueKey(ByVal maxSize As Integer) As String
        Dim chars As Char() = New Char(61) {}
        chars = "123456789".ToCharArray()
        Dim data As Byte() = New Byte(0) {}
        Dim crypto As New RNGCryptoServiceProvider()
        crypto.GetNonZeroBytes(data)
        data = New Byte(maxSize - 1) {}
        crypto.GetNonZeroBytes(data)
        Dim result As New StringBuilder(maxSize)
        For Each b As Byte In data
            result.Append(chars(b Mod (chars.Length)))
        Next
        Return result.ToString()
    End Function
    Sub Reset()
        txtStockID.Text = ""
        cmbLiquorName.Text = ""
        txtNoOfBottles.Text = ""
        txtVolume.Text = ""
        txtTotalVolume.Text = ""
        txtLiquor.Text = ""
        btnDelete.Enabled = False
        btnUpdate_record.Enabled = False
        If Not UserSession.IsAdmin() Then
            btnSave.Enabled = False
        Else
            btnSave.Enabled = False
        End If
        cmbLiquorName.Focus()
        GetData()
    End Sub
    Sub Reset1()
        txtStockID1.Text = ""
        cmbBeerName.Text = ""
        txtNoOfBottles1.Text = ""
        txtBeer.Text = ""
        btnDelete1.Enabled = False
        btnUpdate1.Enabled = False
        If Not UserSession.IsAdmin() Then
            btnSave1.Enabled = False
        Else
            btnSave1.Enabled = False
        End If
        cmbBeerName.Focus()
        GetData1()
    End Sub

    Private Sub frmStock_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        GetData()
        GetData1()
        fillLiquor()
        fillBeer()
    End Sub

    Private Sub btnNewRecord_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNewRecord.Click
        Reset()
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        If Not UserSession.RequireAdmin() Then Exit Sub
        Try
            If Len(Trim(cmbLiquorName.Text)) = 0 Then
                MessageBox.Show("Please select liquor name", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                cmbLiquorName.Focus()
                Exit Sub
            End If

            If Len(Trim(txtNoOfBottles.Text)) = 0 Then
                MessageBox.Show("Please enter no. of bottles", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtNoOfBottles.Focus()
                Exit Sub
            End If
            If Len(Trim(txtVolume.Text)) = 0 Then
                MessageBox.Show("Please enter volume", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtVolume.Focus()
                Exit Sub
            End If
            con = New OleDbConnection(cs)
            con.Open()
            Dim ct As String = "select LiquorName from stock where LiquorName='" & cmbLiquorName.Text & "'"
            cmd = New OleDbCommand(ct)
            cmd.Connection = con
            rdr = cmd.ExecuteReader()
            If rdr.Read Then
                MessageBox.Show("Liquor Name already exists" & vbCrLf & "please update the stock of Liquor", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                If Not rdr Is Nothing Then
                    rdr.Close()
                End If
                Exit Sub
            End If
            txtStockID.Text = "S-" & GetUniqueKey(6)
            con = New OleDbConnection(cs)
            con.Open()
            Dim cb As String = "insert into Stock(StockID, LiquorName, NoOfBottles, Volume, TotalVolume,StockDate) VALUES ('" & txtStockID.Text & "','" & cmbLiquorName.Text & "'," & txtNoOfBottles.Text & "," & txtVolume.Text & "," & txtTotalVolume.Text & ",#" & System.DateTime.Now & "#)"
            cmd = New OleDbCommand(cb)
            cmd.Connection = con
            cmd.ExecuteReader()
            MessageBox.Show("Successfully added", "Stock", MessageBoxButtons.OK, MessageBoxIcon.Information)
            GetData()
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Sub fillLiquor()
        Try
            Dim CN As New OleDbConnection(cs)
            CN.Open()
            adp = New OleDbDataAdapter()
            adp.SelectCommand = New OleDbCommand("SELECT distinct RTRIM(LiquorName) FROM Liquor_master", CN)
            ds = New DataSet("ds")
            adp.Fill(ds)
            dtable = ds.Tables(0)
            cmbLiquorName.Items.Clear()
            For Each drow As DataRow In dtable.Rows
                cmbLiquorName.Items.Add(drow(0).ToString())
            Next

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Sub fillBeer()
        Try
            Dim CN As New OleDbConnection(cs)
            CN.Open()
            adp = New OleDbDataAdapter()
            adp.SelectCommand = New OleDbCommand("SELECT distinct RTRIM(BeerName) FROM Beer", CN)
            ds = New DataSet("ds")
            adp.Fill(ds)
            dtable = ds.Tables(0)
            cmbBeerName.Items.Clear()
            For Each drow As DataRow In dtable.Rows
                cmbBeerName.Items.Add(drow(0).ToString())
            Next

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtVolume_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtVolume.TextChanged
        TextBox1.Text = Val(txtNoOfBottles.Text) * Val(txtVolume.Text)
        txtTotalVolume.Text = Val(TextBox1.Text) + Val(TextBox2.Text)
    End Sub
    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        Try
            If MessageBox.Show("Do you really want to delete this record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = Windows.Forms.DialogResult.Yes Then
                DeleteRecord()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Sub GetData()
        Try
            con = New OleDbConnection(cs)
            con.Open()
            cmd = New OleDbCommand("SELECT * from Stock order by LiquorName", con)
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            DataGridView1.Rows.Clear()
            While (rdr.Read() = True)
                DataGridView1.Rows.Add(rdr(0), rdr(1), rdr(2), rdr(3), rdr(4), rdr(5))
            End While
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Sub GetData1()
        Try
            con = New OleDbConnection(cs)
            con.Open()
            cmd = New OleDbCommand("SELECT StockID,BeerName,NoOfBottles,StockDate from Stock_Beer,Beer where Beer.ID=Stock_Beer.BeerID order by BeerName", con)
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            DataGridView2.Rows.Clear()
            While (rdr.Read() = True)
                DataGridView2.Rows.Add(rdr(0), rdr(1), rdr(2), rdr(3))
            End While
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Sub DeleteRecord()
        If Not UserSession.RequireAdmin() Then Exit Sub
        Try
            Dim RowsAffected As Integer = 0
            con = New OleDbConnection(cs)
            con.Open()
            Dim cq As String = "delete from Stock where StockID='" & txtStockID.Text & "'"
            cmd = New OleDbCommand(cq)
            cmd.Connection = con
            RowsAffected = cmd.ExecuteNonQuery()
            If RowsAffected > 0 Then
                MessageBox.Show("Successfully deleted", "Record", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Reset()
                GetData()
            Else
                MessageBox.Show("No record found", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Reset()
            End If
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtNoOfBottles_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtNoOfBottles.KeyPress
        If (e.KeyChar < Chr(48) Or e.KeyChar > Chr(57)) And e.KeyChar <> Chr(8) Then
            e.Handled = True
        End If
    End Sub

    Private Sub txtVolume_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtVolume.KeyPress
        If (e.KeyChar < Chr(48) Or e.KeyChar > Chr(57)) And e.KeyChar <> Chr(8) Then
            e.Handled = True
        End If
    End Sub

    Private Sub DataGridView1_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles DataGridView1.RowHeaderMouseClick
        Try
            Dim dr As DataGridViewRow = DataGridView1.SelectedRows(0)
            txtStockID.Text = dr.Cells(0).Value.ToString()
            cmbLiquorName.Text = dr.Cells(1).Value.ToString()
            txtNoOfBottles.Text = dr.Cells(2).Value.ToString()
            txtVolume.Text = dr.Cells(3).Value.ToString()
            txtTotalVolume.Text = dr.Cells(4).Value.ToString()
            TextBox2.Text = dr.Cells(4).Value.ToString()
            If Not UserSession.IsAdmin() Then
                btnDelete.Enabled = False
                btnUpdate_record.Enabled = False
            Else
                btnDelete.Enabled = True
                btnUpdate_record.Enabled = True
            End If
          
            btnSave.Enabled = False
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DataGridView1_RowPostPaint(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewRowPostPaintEventArgs) Handles DataGridView1.RowPostPaint
        Dim strRowNumber As String = (e.RowIndex + 1).ToString()
        Dim size As SizeF = e.Graphics.MeasureString(strRowNumber, Me.Font)
        If DataGridView1.RowHeadersWidth < Convert.ToInt32((size.Width + 20)) Then
            DataGridView1.RowHeadersWidth = Convert.ToInt32((size.Width + 20))
        End If
        Dim b As Brush = SystemBrushes.ControlText
        e.Graphics.DrawString(strRowNumber, Me.Font, b, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + ((e.RowBounds.Height - size.Height) / 2))
    End Sub

    Private Sub btnUpdate_record_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUpdate_record.Click
        If Not UserSession.RequireAdmin() Then Exit Sub
        Try
            If Len(Trim(cmbLiquorName.Text)) = 0 Then
                MessageBox.Show("Please select liquor name", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                cmbLiquorName.Focus()
                Exit Sub
            End If

            If Len(Trim(txtNoOfBottles.Text)) = 0 Then
                MessageBox.Show("Please enter no. of bottles", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtNoOfBottles.Focus()
                Exit Sub
            End If
            If Len(Trim(txtVolume.Text)) = 0 Then
                MessageBox.Show("Please enter volume", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtVolume.Focus()
                Exit Sub
            End If
          
            con = New OleDbConnection(cs)
            con.Open()
            Dim cb As String = "update Stock set liquorName='" & cmbLiquorName.Text & "',NoOfBottles=" & txtNoOfBottles.Text & ",Volume=" & txtVolume.Text & ",TotalVolume=" & txtTotalVolume.Text & ",StockDate= #" & System.DateTime.Now & "# where StockID='" & txtStockID.Text & "'"
            cmd = New OleDbCommand(cb)
            cmd.Connection = con
            cmd.ExecuteReader()
            MessageBox.Show("Successfully updated", "Stock", MessageBoxButtons.OK, MessageBoxIcon.Information)
            GetData()
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    

    Private Sub txtLiquor_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtLiquor.TextChanged
        Try
            con = New OleDbConnection(cs)
            con.Open()
            cmd = New OleDbCommand("SELECT * from Stock where liquorName like '" & txtLiquor.Text & "%' order by LiquorName", con)
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            DataGridView1.Rows.Clear()
            While (rdr.Read() = True)
                DataGridView1.Rows.Add(rdr(0), rdr(1), rdr(2), rdr(3), rdr(4), rdr(5))
            End While
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Try
            con = New OleDbConnection(cs)
            con.Open()
            cmd = New OleDbCommand("SELECT * from Stock where TotalVolume <= 0 order by LiquorName", con)
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            DataGridView1.Rows.Clear()
            While (rdr.Read() = True)
                DataGridView1.Rows.Add(rdr(0), rdr(1), rdr(2), rdr(3), rdr(4), rdr(5))
            End While
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtNoOfBottles_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtNoOfBottles.TextChanged
        TextBox1.Text = Val(txtNoOfBottles.Text) * Val(txtVolume.Text)
        txtTotalVolume.Text = Val(TextBox1.Text) + Val(TextBox2.Text)
    End Sub

    

    Private Sub btnDelete1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete1.Click
        Try
            If MessageBox.Show("Do you really want to delete this record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = Windows.Forms.DialogResult.Yes Then
                DeleteRecord1()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Sub DeleteRecord1()
        If Not UserSession.RequireAdmin() Then Exit Sub
        Try
            Dim RowsAffected As Integer = 0
            con = New OleDbConnection(cs)
            con.Open()
            Dim cq As String = "delete from Stock_beer where StockID='" & txtStockID1.Text & "'"
            cmd = New OleDbCommand(cq)
            cmd.Connection = con
            RowsAffected = cmd.ExecuteNonQuery()
            If RowsAffected > 0 Then
                MessageBox.Show("Successfully deleted", "Record", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Reset1()
            Else
                MessageBox.Show("No record found", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Reset1()
            End If
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSave1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave1.Click
        If Not UserSession.RequireAdmin() Then Exit Sub
        Try
            If Len(Trim(cmbBeerName.Text)) = 0 Then
                MessageBox.Show("Please select beer name", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                cmbBeerName.Focus()
                Exit Sub
            End If

            If Len(Trim(txtNoOfBottles1.Text)) = 0 Then
                MessageBox.Show("Please enter no. of bottles", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtNoOfBottles1.Focus()
                Exit Sub
            End If
            con = New OleDbConnection(cs)
            con.Open()
            Dim ct As String = "select BeerName from stock_Beer,Beer where Beer.ID=Stock_beer.BeerID and BeerName='" & cmbBeerName.Text & "'"
            cmd = New OleDbCommand(ct)
            cmd.Connection = con
            rdr = cmd.ExecuteReader()
            If rdr.Read Then
                MessageBox.Show("Beer Name already exists" & vbCrLf & "please update the stock of Beer", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                If Not rdr Is Nothing Then
                    rdr.Close()
                End If
                Exit Sub
            End If
            txtStockID1.Text = "S-" & GetUniqueKey(5)
            con = New OleDbConnection(cs)
            con.Open()
            Dim cb As String = "insert into Stock_Beer(StockID, BeerID, NoOfBottles,StockDate) VALUES ('" & txtStockID1.Text & "','" & txtID.Text & "'," & txtNoOfBottles1.Text & ",#" & System.DateTime.Now & "#)"
            cmd = New OleDbCommand(cb)
            cmd.Connection = con
            cmd.ExecuteReader()
            MessageBox.Show("Successfully added", "Stock", MessageBoxButtons.OK, MessageBoxIcon.Information)
            GetData1()
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnUpdate1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUpdate1.Click
        If Not UserSession.RequireAdmin() Then Exit Sub
        Try
            If Len(Trim(cmbBeerName.Text)) = 0 Then
                MessageBox.Show("Please select Beer name", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                cmbBeerName.Focus()
                Exit Sub
            End If

            If Len(Trim(txtNoOfBottles1.Text)) = 0 Then
                MessageBox.Show("Please enter no. of bottles", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtNoOfBottles1.Focus()
                Exit Sub
            End If
            con = New OleDbConnection(cs)
            con.Open()
            Dim cb As String = "update Stock_beer set BeerID='" & txtID.Text & "',NoOfBottles=" & txtNoOfBottles1.Text & ",StockDate= #" & System.DateTime.Now & "# where StockID='" & txtStockID1.Text & "'"
            cmd = New OleDbCommand(cb)
            cmd.Connection = con
            cmd.ExecuteReader()
            MessageBox.Show("Successfully updated", "Stock", MessageBoxButtons.OK, MessageBoxIcon.Information)
            GetData1()
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnNew1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNew1.Click
        Reset1()
    End Sub

    Private Sub txtBeer_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtBeer.TextChanged
        Try
            con = New OleDbConnection(cs)
            con.Open()
            cmd = New OleDbCommand("SELECT StockID,BeerName,NoOfBottles,StockDate from Stock_Beer,Beer where Beer.ID=Stock_Beer.BeerID and BeerName like '" & txtBeer.Text & "%' order by BeerName", con)
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            DataGridView2.Rows.Clear()
            While (rdr.Read() = True)
                DataGridView2.Rows.Add(rdr(0), rdr(1), rdr(2), rdr(3))
            End While
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        Try
            con = New OleDbConnection(cs)
            con.Open()
            cmd = New OleDbCommand("SELECT StockID,BeerName,NoOfBottles,StockDate from Stock_Beer,Beer where Beer.ID=Stock_Beer.BeerID and NoOfBottles <=0 order by BeerName", con)
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            DataGridView2.Rows.Clear()
            While (rdr.Read() = True)
                DataGridView2.Rows.Add(rdr(0), rdr(1), rdr(2), rdr(3))
            End While
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtNoOfBottles1_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtNoOfBottles1.KeyPress
        If (e.KeyChar < Chr(48) Or e.KeyChar > Chr(57)) And e.KeyChar <> Chr(8) Then
            e.Handled = True
        End If
    End Sub

    Private Sub cmbBeerName_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbBeerName.SelectedIndexChanged
        Try
            con = New OleDbConnection(cs)
            con.Open()
            cmd = con.CreateCommand()
            cmd.CommandText = "SELECT ID FROM Beer WHERE BeerName= '" & cmbBeerName.Text & "'"
            rdr = cmd.ExecuteReader()
            If rdr.Read() Then
                txtID.Text = rdr.GetString(0)
            End If
            If (rdr IsNot Nothing) Then
                rdr.Close()
            End If
            If con.State = ConnectionState.Open Then
                con.Close()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Private Sub DataGridView2_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles DataGridView2.RowHeaderMouseClick
        Try
            Dim dr As DataGridViewRow = DataGridView2.SelectedRows(0)
            txtStockID1.Text = dr.Cells(0).Value.ToString()
            cmbBeerName.Text = dr.Cells(1).Value.ToString()
            txtNoOfBottles1.Text = dr.Cells(2).Value.ToString()
            If Not UserSession.IsAdmin() Then
                btnDelete1.Enabled = False
                btnUpdate1.Enabled = False
            Else
                btnDelete1.Enabled = True
                btnUpdate1.Enabled = True
            End If
           btnSave1.Enabled = False
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DataGridView2_RowPostPaint(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewRowPostPaintEventArgs) Handles DataGridView2.RowPostPaint
        Dim strRowNumber As String = (e.RowIndex + 1).ToString()
        Dim size As SizeF = e.Graphics.MeasureString(strRowNumber, Me.Font)
        If DataGridView1.RowHeadersWidth < Convert.ToInt32((size.Width + 20)) Then
            DataGridView1.RowHeadersWidth = Convert.ToInt32((size.Width + 20))
        End If
        Dim b As Brush = SystemBrushes.ControlText
        e.Graphics.DrawString(strRowNumber, Me.Font, b, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + ((e.RowBounds.Height - size.Height) / 2))

    End Sub

    Private Sub TabControl1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TabControl1.Click
        GetData()
        GetData1()
    End Sub

    Private Sub SimpleButton3_Click(sender As Object, e As EventArgs) Handles SimpleButton3.Click
        Close()
    End Sub

    Private Sub SimpleButton1_Click(sender As Object, e As EventArgs) Handles SimpleButton1.Click
        Close()
        frmMainMenu.Show()
    End Sub
End Class