Imports System.Data.OleDb
Imports System.IO

Public Class frmMainMenu
    Dim strHelpPath As String = System.IO.Path.Combine(Application.StartupPath, "HMS.chm")
    Private Const STR_SelectRoomRoomNoGuestGuestIDGuestNameFormatDateI As String = "Select Room.RoomNo,Guest.GuestID,GuestName,Format(DateIN,'dd/MMM/yyyy'),Format(DateOut,'dd/MMM/yyyy') from Guest,Room,Checkin_Room where Guest.GuestID=Checkin_room.GuestID and Room.RoomNo=Checkin_Room.RoomNo and Status='Checked In' Order by DateIN Desc"
    Private Const STR_SelectRoomNoAsRoomNoGuestGuestIDAsGuestIDGuestNa As String = "Select RoomNo as [Room No],Guest.GuestID as [Guest ID],GuestName as [Guest Name],Format(DateIN,'dd/MMM/yyyy') as [Date IN],Format(DateOut,'dd/MMM/yyyy') as [Date Out] from Guest,Temp_Reservation where Guest.GuestID=Temp_Reservation.GuestID and Status='Reserved' order by DateIN"






    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        frmAboutBox1.Show()
    End Sub


    Private Sub NotePadToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            System.Diagnostics.Process.Start("Notepad.exe")
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Sub yonatanMainLoad()
        Try
            con = New OleDbConnection(cs)
            con.Open()
            cmd = New OleDbCommand("SELECT Reservation_HallandGarden.ID as [Reservation ID], Guest.GuestID as [Guest ID],GuestName as [Guest Name],(address) as [Address],(city) as [City],(ContactNo) as [Contact No],IDType as [ID Type],IDNumber as [ID Number],DateFrom_Hall as [Hall Date From], DateTo_Hall as [Hall Date To], Days_Hall as [Hall Days], Rate_Hall as [Hall Rate],TotalCharges_Hall as [Hall Total Charges],DateFrom_Garden as [Garden Date From], DateTo_Garden as [Garden Date To,], Days_Garden as [Garden Days], Rate_Garden as [Garden Rate],TotalCharges_Garden as [Garden Total Charges], OtherCharges as [Other Charges], SubTotal as [Sub Total], ServiceTaxPer as [Service Tax %],ServiceTaxAmount as [Service Tax Amount], LuxuryTaxPer as [Luxury Tax %], LuxuryTaxAmount as [Luxury Tax Amount],DiscountPer as [Discount %], Discount, GrandTotal, TotalPaid as [Total Paid], Balance,CurrencySet.ID as [Currency ID],CS_Currency as [Currency], Reservation_HallandGarden.Notes from Guest,Reservation_HallandGarden,CurrencySet where Guest.GuestID=Reservation_HallandGarden.GuestID and CurrencySet.ID=Reservation_HallandGarden.CurrencyID   order by GuestName", con)
            Dim myDA As OleDbDataAdapter = New OleDbDataAdapter(cmd)
            Dim myDataSet As DataSet = New DataSet()
            myDA.Fill(myDataSet, "Guest")
            myDA.Fill(myDataSet, "Reservation_HallandGarden")
            hallgarden.DataSource = myDataSet.Tables("Guest").DefaultView
            hallgarden.DataSource = myDataSet.Tables("Reservation_HallandGarden").DefaultView
            con.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub main_form_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Refresh()
        HelpProvider1.HelpNamespace = strHelpPath
        Button2.PerformClick()
        Timer2.Start()
        Timer2.Interval = 1000
        ToolStripStatusLabel4.Text = Now
        lblUser.Text = frmLogin.UserName.Text
        UserSession.SetUserRole(frmLogin.UserType.Text, frmLogin.UserName.Text)
        lblUserType.Text = UserSession.GetCurrentRole()
        yonatanMainLoad()

    End Sub


    Private Sub SystemInfoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        frmSystemInfo.Show()
    End Sub

    Private Sub CalculatorToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            System.Diagnostics.Process.Start("Calc.exe")
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub EmployeeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EmployeeToolStripMenuItem.Click

        frmEmployeesRecord.Show()
    End Sub



    Private Sub EmployeeAttendanceToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EmployeeAttendanceToolStripMenuItem.Click

        frmEmployeeAttendanceRecord.Show()

    End Sub

    Private Sub AttendanceToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AttendanceToolStripMenuItem.Click
        frmAttendance.Show()
        frmAttendance.Reset()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Timer1.Start()
        ToolStripStatusLabel4.Text = Now
    End Sub

    Private Sub OverTimeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OverTimeToolStripMenuItem.Click
        frmSearchByOverTime.Show()
    End Sub


    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        frmEmployeeAttendanceRecord.Show()
    End Sub


    Private Sub AdvanceToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AdvanceToolStripMenuItem.Click

        frmAdvance.Show()
        frmAdvance.Reset()
    End Sub

    Private Sub PaymentToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PaymentToolStripMenuItem.Click
        frmEmployeePayment.Show()
        frmEmployeePayment.Reset()
    End Sub

    Private Sub RegistrationToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RegistrationToolStripMenuItem.Click
        frmEmployee_registration.Show()
        frmEmployee_registration.Reset()
    End Sub


    Private Sub AdvanceEntryToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AdvanceEntryToolStripMenuItem.Click
        frmAdvanceEntryRecord.Show()
    End Sub

    Private Sub EmployeePaymentToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EmployeePaymentToolStripMenuItem.Click

        frmEmployeePaymentRecord1.Show()
    End Sub

    Private Sub DeductionEntryToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeductionEntryToolStripMenuItem.Click
        frmDeductionEntryRecord.Show()
    End Sub


    Private Sub EmployeesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EmployeesToolStripMenuItem.Click
        frmEmployeeReport.Show()
    End Sub

    Private Sub AdvancePaymentToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AdvancePaymentToolStripMenuItem.Click

        frmAdvanceEntryReport.Show()
    End Sub

    Private Sub AttendanceToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AttendanceToolStripMenuItem2.Click

        frmAttendance.Show()
        frmAttendance.Reset()
    End Sub

    Private Sub EmployeeToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EmployeeToolStripMenuItem1.Click

        frmEmployee_registration.Show()
        frmEmployee_registration.Reset()
    End Sub

    Private Sub AdvanceEntryToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AdvanceEntryToolStripMenuItem1.Click
        Me.Hide()
        frmAdvance.Show()
        frmAdvance.Reset()
    End Sub

    Private Sub PaymentToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PaymentToolStripMenuItem1.Click

        frmEmployeePayment.Show()
        frmEmployeePayment.Reset()
    End Sub

    Private Sub OvertimeToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OvertimeToolStripMenuItem1.Click

        frmOverTimeReport.Show()
    End Sub

    Private Sub EmployeePaymentToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EmployeePaymentToolStripMenuItem1.Click

        frmEmployeePaymentReport.Show()
    End Sub

    Private Sub LogoutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LogoutToolStripMenuItem.Click
        Me.Hide()
        frmServer.Hide()
        frmClient.Hide()
        frmAboutBox1.Hide()
        frmHotelInfo.Hide()
        frmCurrency.Hide()
        frmFood.Hide()
        frmExtraBed.Hide()
        frmLiquor.Hide()
        frmRoom.Hide()
        frmGuest.Hide()
        frmOrder.Hide()
        frmRegistration.Hide()
        frmLoginDetails.Hide()
        frmCheckIn.Hide()
        frmCheckOut.Hide()
        frmReservation.Hide()
        frmPurchaseInventory.Hide()
        frmTransaction.Hide()
        frmEmployee_registration.Hide()
        frmAttendance.Hide()
        frmAdvance.Hide()
        frmEmployeePayment.Hide()
        frmEmployeesRecord.Hide()
        frmEmployeeAttendanceRecord.Hide()
        frmSearchByOverTime.Hide()
        frmEmployeePaymentRecord1.Hide()
        frmAdvanceEntryRecord.Hide()
        frmDeductionEntryRecord.Hide()
        frmCheckedInRecord.Hide()
        frmCheckedInRecord2.Hide()
        frmCheckedInRecord4.Hide()
        frmCheckedOutRecord.Hide()
        frmPurchasedInventoryRecord.Hide()
        frmTransactionRecord.Hide()
        frmEmployeeReport.Hide()
        frmAdvanceEntryReport.Hide()
        frmEmployeePaymentReport.Hide()
        frmAttendanceReport.Hide()
        frmOverTimeReport.Hide()
        frmSalarySlipsReport.Hide()
        frmReservationRecord1.Hide()
        frmAboutBox1.Hide()
        frmLiquorRecord.Hide()
        frmfoodRecord.Hide()
        frmHall.Hide()
        frmGarden.Hide()
        frmStock.Hide()
        frmCheckINReport.Hide()
        frmReservationRecord3.Hide()
        frmReservationReport.Hide()
        frmCheckOutReport.Hide()
        frmGuestsReport.Hide()
        frmOrdersRecord.Hide()
        frmOrdersRecord1.Hide()
        frmBeer.Hide()
        frmRoomsAvailability.Hide()
        UserSession.ClearSession()
        frmLogin.UserName.Text = ""
        frmLogin.Password.Text = ""
        frmLogin.UserType.SelectedIndex = -1
        frmLogin.Show()
        frmLogin.UserType.Focus()
    End Sub

    Private Sub AttendanceToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AttendanceToolStripMenuItem1.Click

        frmAttendanceReport.Show()
    End Sub

    Private Sub SalarySlipsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SalarySlipsToolStripMenuItem.Click

        frmSalarySlipsReport.Show()
    End Sub

    Private Sub RoomToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RoomToolStripMenuItem.Click
        frmRoom.Show()
    End Sub


    Private Sub CheckInToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckInToolStripMenuItem.Click
        frmCheckIn.Show()
    End Sub

    Private Sub CheckOutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckOutToolStripMenuItem.Click
        frmCheckOut.Show()
    End Sub


    Private Sub CheckInToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckInToolStripMenuItem1.Click
        frmCheckIn.Show()
    End Sub

    Private Sub CheckOutToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckOutToolStripMenuItem1.Click
        frmCheckOut.Show()
    End Sub

    Private Sub ReservationToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReservationToolStripMenuItem.Click
        Me.Hide()

        frmReservation.Show()


    End Sub

    Private Sub ReservationToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReservationToolStripMenuItem1.Click
        frmReservation.Show()
    End Sub


    Private Sub RegistrationToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RegistrationToolStripMenuItem1.Click
        frmRegistration.Show()
    End Sub



    Private Sub OthersTransToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OthersTransToolStripMenuItem.Click
        frmTransaction.Show()
    End Sub

    Private Sub DishToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DishToolStripMenuItem.Click
        Me.Hide()
        frmFood.Show()
    End Sub

    Private Sub GuestToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GuestToolStripMenuItem.Click
        frmGuest.Show()
    End Sub

    Private Sub LoginDetailsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LoginDetailsToolStripMenuItem.Click
        frmLoginDetails.ShowDialog()
    End Sub

    Private Sub OthersTransactionToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OthersTransactionToolStripMenuItem.Click
        frmTransactionRecord.Show()
    End Sub

    Private Sub PurchasedInventoryToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PurchasedInventoryToolStripMenuItem1.Click
        frmPurchasedInventoryRecord.Show()
    End Sub

    Private Sub LiquorToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles LiquorToolStripMenuItem.Click
        Me.Hide()
        frmLiquor.Show()
    End Sub

    Private Sub ExtraToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ExtraToolStripMenuItem.Click
        Me.Hide()
        frmExtraBed.ShowDialog()
    End Sub

    Private Sub CheckInToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckInToolStripMenuItem2.Click

        frmCheckedInRecord.Show()
    End Sub

    Private Sub CheckOutToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckOutToolStripMenuItem2.Click
        Me.Hide()
        frmCheckedOutRecord.Show()
    End Sub

    Private Sub ReservationToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReservationToolStripMenuItem2.Click
        Me.Hide()
        frmReservationRecord1.Show()
    End Sub

    Private Sub OrderToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OrderToolStripMenuItem.Click
        frmOrder.lblUserType.Text = UserSession.GetCurrentRole()
        frmOrder.Show()
    End Sub

    Private Sub LiquorToolStripMenuItem1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles LiquorToolStripMenuItem1.Click
        Me.Hide()
        frmLiquor_Master.Show()
    End Sub

    Private Sub HallToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HallToolStripMenuItem.Click
        Me.Hide()
        frmHall.Show()
    End Sub

    Private Sub GardenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GardenToolStripMenuItem.Click
        Me.Hide()
        frmGarden.Show()
    End Sub

    Private Sub StockToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StockToolStripMenuItem1.Click
        frmStock.lblUserType.Text = UserSession.GetCurrentRole()
        If Not UserSession.IsAdmin() Then
            frmStock.btnSave.Enabled = False
            frmStock.btnSave1.Enabled = False
        End If
        frmStock.Show()
    End Sub

    Private Sub StockToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StockToolStripMenuItem.Click
        frmStock.lblUserType.Text = UserSession.GetCurrentRole()
        If Not UserSession.IsAdmin() Then
            frmStock.btnSave.Enabled = False
            frmStock.btnSave1.Enabled = False
        End If
        Me.Hide()
        frmStock.Show()
    End Sub

    Private Sub OrderToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OrderToolStripMenuItem1.Click
        frmOrder.lblUserType.Text = UserSession.GetCurrentRole()
        frmOrder.Show()
    End Sub

    Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick
        Button1.PerformClick()
        Button2.PerformClick()
    End Sub

    Private Sub ReservationToolStripMenuItem3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReservationToolStripMenuItem3.Click
        Me.Hide()
        frmReservationRecord3.Show()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Try
            con = New OleDbConnection(cs)
            con.Open()
            Dim sql As String = STR_SelectRoomRoomNoGuestGuestIDGuestNameFormatDateI
            cmd = New OleDbCommand(sql, con)
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            DataGridView1.Rows.Clear()
            While (rdr.Read() = True)
                DataGridView1.Rows.Add(rdr(0), rdr(1), rdr(2), rdr(3), rdr(4))
            End While
            con.Close()
            con = New OleDbConnection(cs)
            con.Open()
            cmd = New OleDbCommand(STR_SelectRoomNoAsRoomNoGuestGuestIDAsGuestIDGuestNa, con)
            Dim myDA As OleDbDataAdapter = New OleDbDataAdapter(cmd)
            Dim myDataSet As DataSet = New DataSet()
            myDA.Fill(myDataSet, "Guest")
            myDA.Fill(myDataSet, "Temp_Reservation")
            DataGridView2.DataSource = myDataSet.Tables("Guest").DefaultView
            DataGridView2.DataSource = myDataSet.Tables("Temp_Reservation").DefaultView
            con.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub OrdersToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OrdersToolStripMenuItem.Click
        Me.Hide()
        frmOrdersRecord1.Show()
    End Sub


    Private Sub PurchaseToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PurchaseToolStripMenuItem.Click
        Me.Hide()
        frmPurchaseInventory.Show()
    End Sub

    Private Sub BeerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BeerToolStripMenuItem.Click
        Me.Hide()
        frmBeer.Show()
    End Sub

    Private Sub GuestToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GuestToolStripMenuItem1.Click
        Me.Hide()
        frmGuest.Show()
    End Sub

    Private Sub frmMainMenu_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        End
    End Sub

    Private Sub RoomsAvailabilityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RoomsAvailabilityToolStripMenuItem.Click
        Me.Hide()
        frmRoomsAvailability.Show()
    End Sub

    Private Sub RestaurantOrderToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RestaurantOrderToolStripMenuItem.Click
        Me.Hide()
        frmRestaurant.Show()
    End Sub

    Private Sub BackupToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BackupToolStripMenuItem.Click
        If Not UserSession.RequireAdmin() Then Exit Sub
        Try
            Cursor = Cursors.WaitCursor
            Timer3.Enabled = True
            If (Not System.IO.Directory.Exists("C:\DBBackup")) Then
                System.IO.Directory.CreateDirectory("C:\DBBackup")
            End If
            Dim destdir As String = "C:\DBBackup\HMS_DB " & DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") & ".accdb"
            Dim dlg As New SaveFileDialog
            con.Close()
            File.Copy(Application.StartupPath & "\HMS_DB.accdb", destdir, True)
            MessageBox.Show("Successfully performed", "Database Backup", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RestoreToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RestoreToolStripMenuItem.Click
        If Not UserSession.RequireAdmin() Then Exit Sub
        Try
            Cursor = Cursors.WaitCursor
            Timer3.Enabled = True
            Dim dlg As New OpenFileDialog
            dlg.DefaultExt = "*.accdb"
            dlg.Filter = "ACCESS DB|*.accdb|All File|*"
            If dlg.ShowDialog = Windows.Forms.DialogResult.OK Then
                con.Close()
                File.Copy(dlg.FileName, Application.StartupPath & "\HMS_DB.accdb", True)
                main_form_Load(Nothing, Nothing)
                MessageBox.Show("Successfully performed", "Database Restore", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub AdditionalTaxToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AdditionalTaxToolStripMenuItem.Click
        Me.Hide()
        frmAdditionalTaxesRecords.Show()
    End Sub

    Private Sub CurrencyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CurrencyToolStripMenuItem.Click
        Me.Hide()
        frmCurrency.Show()
    End Sub

    Private Sub HotelInfoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HotelInfoToolStripMenuItem.Click
        Me.Hide()
        frmHotelInfo.Show()
    End Sub

    Private Sub ChatServerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChatServerToolStripMenuItem.Click
        frmServer.Show()
    End Sub

    Private Sub ChatClientToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChatClientToolStripMenuItem.Click

        frmClient.Show()
    End Sub

    Private Sub DataGridView2_CellContentClick(sender As System.Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView2.CellContentClick

    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub

    Private Sub CahngePasswordToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CahngePasswordToolStripMenuItem.Click
        frmChangePassword.Show()

    End Sub

    Private Sub ScheduleToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScheduleToolStripMenuItem.Click

        FrmSchedule.Show()

    End Sub

    Private Sub TaxInformationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TaxInformationToolStripMenuItem.Click
        frmtaxinformation.Show()

    End Sub


    Private Sub HelpToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles HelpToolStripMenuItem2.Click

        Help.ShowHelp(Me, HelpProvider1.HelpNamespace, HelpNavigator.TableOfContents)
    End Sub


    Private Sub Timer3_Tick(sender As System.Object, e As System.EventArgs) Handles Timer3.Tick
        Cursor = Cursors.Default
        Timer3.Enabled = False
    End Sub

End Class