Imports System.Data.OleDb

Module UserSession
    Private _currentUserRole As String = ""
    Private _currentUserName As String = ""

    ''' <summary>
    ''' Sets the authenticated user's role from a trusted source (the database).
    ''' Called once at login; the role is then immutable until ClearSession().
    ''' </summary>
    Public Sub SetUserRole(ByVal role As String, ByVal userName As String)
        _currentUserRole = If(role, "")
        _currentUserName = If(userName, "")
    End Sub

    Public Function GetCurrentRole() As String
        Return _currentUserRole
    End Function

    Public Function GetCurrentUserName() As String
        Return _currentUserName
    End Function

    Public Function IsAdmin() As Boolean
        Return String.Equals(_currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Checks whether the current session holds Admin privileges.
    ''' Returns True if allowed, False (with a message box) if denied.
    ''' </summary>
    Public Function RequireAdmin() As Boolean
        If Not IsAdmin() Then
            MessageBox.Show("Access denied. This operation requires Admin privileges.", "Authorization Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Verifies the current user's role against the database.
    ''' Returns True if the stored role matches the DB record.
    ''' </summary>
    Public Function VerifyRoleFromDatabase() As Boolean
        If String.IsNullOrEmpty(_currentUserName) Then
            Return False
        End If
        Try
            Using dbCon As New OleDbConnection(cs)
                dbCon.Open()
                Dim cmd As New OleDbCommand("SELECT UserType FROM Login WHERE UserName=@uname", dbCon)
                cmd.Parameters.AddWithValue("@uname", _currentUserName)
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot Nothing Then
                    Return String.Equals(result.ToString(), _currentUserRole, StringComparison.OrdinalIgnoreCase)
                End If
            End Using
        Catch
            ' If verification fails, deny by default
        End Try
        Return False
    End Function

    Public Sub ClearSession()
        _currentUserRole = ""
        _currentUserName = ""
    End Sub
End Module
