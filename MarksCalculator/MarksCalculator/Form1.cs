using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;

namespace MarksCalculator
{
    public partial class Form1 : Form
    {
        SqlConnection conn;
        DataSet ds;
        SqlDataAdapter studentFieldsAdapter, classesAdapter, subjectsAdapter, studentFieldsProcAdapter;
        SqlCommandBuilder cmdBuilder;
        String connStr = System.Configuration.ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            conn = new SqlConnection(connStr);
            studentFieldsAdapter = new SqlDataAdapter("SELECT * FROM studentFields", conn);

            // Fill tables
            ds = new DataSet();
            studentFieldsAdapter.Fill(ds, "studentFields");

            subjectsAdapter = new SqlDataAdapter("SELECT * FROM subjects", conn);
            subjectsAdapter.Fill(ds, "subjects");

            classesAdapter = new SqlDataAdapter("SELECT * FROM classes", conn);
            classesAdapter.Fill(ds, "classes");

            // Fill comboBox
            foreach(DataRow row in ds.Tables["classes"].Rows)
            {
                cmbClasses.Items.Add(row["classname"]);
            }
        }

        private void cmbClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            // First clear the current items
            cmbSubjects.Text = "";
            cmbSubjects.Items.Clear();

            // This is required to clear the dgv
            this.dataGridView1.DataSource = null;
            this.dataGridView1.Refresh();

            foreach(DataRow row in ds.Tables["subjects"].Select("classname = '" + cmbClasses.SelectedItem.ToString() + "'"))
            {
                cmbSubjects.Items.Add(row["subjectname"]);
            }

            // This uses a stored procedure because it executes a join
            SqlCommand getStudentFieldsProcCmd = new SqlCommand("getStudentFields", conn);
            getStudentFieldsProcCmd.CommandType = CommandType.StoredProcedure;
            getStudentFieldsProcCmd.Parameters.AddWithValue("@classname", cmbClasses.SelectedItem.ToString());
            getStudentFieldsProcCmd.Connection = conn;

            studentFieldsProcAdapter = new SqlDataAdapter(getStudentFieldsProcCmd);
            if(ds.Tables["studentFieldsWithNames"] != null)
            {
                ds.Tables["studentFieldsWithNames"].Clear();
                studentFieldsProcAdapter.Fill(ds, "studentFieldsWithNames");
            }
            else
            {
                studentFieldsProcAdapter.Fill(ds, "studentFieldsWithNames");
            }
            
            this.dataGridView1.DataSource = ds.Tables["studentFieldsWithNames"];
            this.dataGridView1.Refresh();
        }

        private void cmbSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.dataGridView1.DataSource = null;
            this.dataGridView1.Refresh();

            DataTable subjectsDataTable = new DataTable();
            subjectsDataTable = ds.Tables["studentFieldsWithNames"].Select("subjectname = '" + cmbSubjects.SelectedItem.ToString() + "'").CopyToDataTable();

            this.dataGridView1.DataSource = getPivotedDataTable();
            this.dataGridView1.Refresh();

            // Set non-field columns as read only
            int fieldColumnOffset = getFieldColumnOffsetInPivot();
            for (int i = 0; i < fieldColumnOffset; i++)
            {
                this.dataGridView1.Columns[i].ReadOnly = true;
            }
        }

        public DataTable getPivotedDataTable()
        {
            DataTable pivotedTable = new DataTable();

            // First we get the subjectid

            int subjectId = getCurrentSubjectId();

            Debug.WriteLine(subjectId);

            // Then we get the fieldids and names for that subject
            DataTable fieldIdAndNameTable = getFieldIdAndNameTable(subjectId);
            
            // So now we create a column string 
            String query = getPivotQuery(fieldIdAndNameTable, subjectId);

            SqlDataAdapter pivotedAdapter = new SqlDataAdapter(query, conn);

            if (ds.Tables["pivotedStudentFields"] != null)
            {
                ds.Tables.Remove("pivotedStudentFields");
                pivotedAdapter.Fill(ds, "pivotedStudentFields");
            }
            else
            {
                pivotedAdapter.Fill(ds, "pivotedStudentFields");
            }


            return ds.Tables["pivotedStudentFields"];
        }

        public DataTable getFieldIdAndNameTable(int subjectid)
        {
            DataRow[] rowCollection = ds.Tables["studentFields"]
                                    .Select("subjectid = " + subjectid);

            DataTable fieldIdAndNameTable = new DataTable();
            fieldIdAndNameTable.Columns.Add("fieldid");
            fieldIdAndNameTable.Columns.Add("fieldname");

            foreach(DataRow row in rowCollection)
            {
                fieldIdAndNameTable.Rows.Add(row["fieldid"], row["fieldname"]);
            }

            return fieldIdAndNameTable;
        }

        public string getPivotQuery(DataTable fieldIdAndNameTable, int subjectid)
        {
            // for use in the pivot function
            List<string> fieldIdList = new List<string>();
            foreach (DataRow row in fieldIdAndNameTable.Rows)
            {
                fieldIdList.Add("[" + row["fieldid"] + "]");
            }
            String colsInPivot = String.Join(",", fieldIdList.Distinct());

            // for use in the select part
            List<string> fieldNamesList = new List<string>();
            foreach (DataRow row in fieldIdAndNameTable.Rows)
            {
                fieldNamesList.Add("[" + row["fieldid"] + "] as " + row["fieldname"]);
            }
            String colsInSelect = String.Join(",", fieldNamesList.Distinct());

            // Now we build the pivoted query
            String query = String.Format(@"select studentid, studentname, {0} from
	                        (
		                        select students.studentid, students.studentname, fieldid, studentmarks
		                        from studentFields join subjects
			                        on studentFields.subjectid = subjects.subjectid
		                        join students
			                        on studentFields.studentid = students.studentid
		                        where studentFields.subjectid = '{1}'
	                        ) as tab
	                        pivot
	                        (
		                        max(studentmarks) for fieldid in ({2})
	                        ) as pvt", colsInSelect, subjectid , colsInPivot.ToString());

            Debug.WriteLine(query);
            
            return query;
        }

        /**
         * This function gets the offset for where the fields start in the pivot table
         * This is so that we can know what columns to update when changes are made
         */
        public int getFieldColumnOffsetInPivot()
        {
            // Get subjectid
            int subjectid = getCurrentSubjectId();

            DataTable fieldNameTable = getFieldIdAndNameTable(subjectid);

            DataTable pivotTable = getPivotedDataTable();
            
            List<string> fields = new List<string>();
            foreach(DataRow row in fieldNameTable.Rows)
            {
                fields.Add(row["fieldname"].ToString());
            }

            int offset = 0;
            foreach(DataColumn col in pivotTable.Columns)
            {
                if (fields.Contains(col.ColumnName))
                {
                    return offset;
                } 
                else
                {
                    offset++;
                }
                
            }
            return -1;
        }

        public int getCurrentSubjectId()
        {
            int subjectid = Convert.ToInt32(
                    ds.Tables["subjects"]
                        .Select("subjectname = '" + cmbSubjects.SelectedItem.ToString() + "'")
                        .First()["subjectid"]);

            return subjectid;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // First get a column index
            String colName = this.dataGridView1
                            .Columns[this.dataGridView1.CurrentCell.ColumnIndex]
                            .ToString();

            int newValue = Convert.ToInt32(dataGridView1.CurrentCell.Value);

            int fieldColumnOffset = getFieldColumnOffsetInPivot();

            // Taking the fieldColumn offset, we map the column in the pivot table to it's relevant field
            List<int> fieldIds = new List<int>();
            foreach(DataRow row in getFieldIdAndNameTable(getCurrentSubjectId()).Rows)
            {
                int id = Convert.ToInt32(row["fieldid"]); 
                // Make sure it's distinct
                if (!fieldIds.Contains(id))
                {
                    fieldIds.Add(id);
                }
            }

            int fieldIdToUpdate = fieldIds[this.dataGridView1.CurrentCell.ColumnIndex - fieldColumnOffset];

            Debug.WriteLine(fieldIdToUpdate);
            int studentid = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells["studentid"].Value);

            // Now that we have the studentid and fieldid, we have enough to update student marks.
            DataRow rowToUpdate = ds.Tables["studentFields"]
                                    .Select("studentid = " + studentid + " and fieldid = " + fieldIdToUpdate)
                                    .First();

            int maxMarks = Convert.ToInt32(rowToUpdate["maxMarks"]);

            if(newValue > maxMarks)
            {
                MessageBox.Show("Marks given greater than max marks");
                return;
            }

            String query = String.Format("UPDATE studentFields SET studentMarks = {0} where fieldid = {1} and studentid = {2}", newValue, fieldIdToUpdate, studentid);
            SqlCommand cmd = new SqlCommand(query, conn);
            Debug.WriteLine("update query: " + query);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
    
