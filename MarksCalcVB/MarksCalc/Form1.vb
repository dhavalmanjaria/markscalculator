Imports System.Data.SqlClient
Imports System.Data
Imports System.Diagnostics

Public Class Form1

    Dim conn As SqlConnection
    Dim ds As DataSet
    Dim adapter As SqlDataAdapter
    Dim cmdBuilder As SqlCommandBuilder

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn = New SqlConnection("Data Source=DEFIANT\SQLEXPRESS;Initial Catalog=markscalc;Integrated Security=true")
        adapter = New SqlDataAdapter("SELECT * FROM studentFields", conn)
        ds = New DataSet()
        adapter.Fill(ds, "studentFields")
        DataGridView1.DataSource = ds.Tables("studentFields")

        ''' Fill combobox
        Dim classesAdapter = New SqlDataAdapter("SELECT * FROM classes", conn)
        classesAdapter.Fill(ds, "classes")

        For Each row In ds.Tables("classes").Rows
            ClassesComboBox.Items.Add(row("classname"))
        Next

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ClassesComboBox.SelectedIndexChanged
        ' First clear the current items

        SubjectComboBox.Text = ""
        SubjectComboBox.Items.Clear()
        SubjectComboBox.ResetText()

        ' This is required to clear the DGV
        Me.DataGridView1.DataSource = Nothing
        Me.DataGridView1.Refresh()

        If ds.Tables("subjects") Is Nothing Then
            Dim subjectsAdapter = New SqlDataAdapter("SELECT * from subjects", conn)
            subjectsAdapter.Fill(ds, "subjects")
        End If

        Dim selectedSubjectId As Integer
        For Each row In ds.Tables("subjects").Select("classname = '" & ClassesComboBox.SelectedItem.ToString() & "'")
            SubjectComboBox.Items.Add(row("subjectname"))
        Next

        ' This uses a stored procedure because it executes a join
        Dim studentFieldsProcAdapter = New SqlDataAdapter("execute studentFeildDetailsByClassname N'" & ClassesComboBox.SelectedItem.ToString() & "'", conn)
        ' This is also required to clear the DGV
        If Not ds.Tables("studentFieldsByClassname") Is Nothing Then
            ds.Tables.Remove("studentFieldsByClassname")
        End If
        studentFieldsProcAdapter.Fill(ds, "studentFieldsByClassname")


        DataGridView1.DataSource = ds.Tables("studentFieldsByClassname")
        DataGridView1.Refresh()
    End Sub

    Private Sub SubjectComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles SubjectComboBox.SelectedIndexChanged
        ' This is required to clear the DGV
        Me.DataGridView1.DataSource = Nothing
        Me.DataGridView1.Refresh()

        Dim subjectDataTable = ds.Tables("studentFieldsByClassname").Select("subjectname = '" & SubjectComboBox.SelectedItem.ToString() & "'").CopyToDataTable()

        ' Get subjectid
        Dim row = ds.Tables("subjects").Select("subjectname = '" & SubjectComboBox.SelectedItem.ToString() & "'").First()
        Dim subjectid = row("subjectid").ToString()

        Dim fieldTable = getFields(subjectid)

        Dim columnList As List(Of String) = New List(Of String)
        For Each row In fieldTable.Rows
            columnList.Add(row("fieldname").ToString())
        Next
        Dim cols = String.Join(",", columnList)
        Debug.WriteLine(cols)
        Dim pivotQuery = String.Format("Select studentid, studentname, subjectname")
        Me.DataGridView1.DataSource = subjectDataTable
        Me.DataGridView1.Refresh()

    End Sub

    Public Function getFields(ByVal subjectid As Integer) As DataTable
        ' First get all the fields for a subject
        Dim adapter = New SqlDataAdapter("SELECT fieldid, fieldname from subjectfields where subjectid = " & subjectid, conn)
        Dim fieldSet = New DataSet()
        adapter.Fill(fieldSet, "fields")

        Return fieldSet.Tables(0)

    End Function
End Class
