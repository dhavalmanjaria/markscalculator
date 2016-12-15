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
using Microsoft.SqlServer;

namespace MarksCalculator
{
    public partial class Form1 : Form
    {
        SqlConnection conn;
        DataSet ds;
        SqlDataAdapter studentFieldsAdapter, classesAdapter, subjectsAdapter;
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

            subjectsAdapter = new SqlDataAdapter("SELECT * FROM subjects", conn);
            classesAdapter = new SqlDataAdapter("SELECT * FROM classes", conn);
            
            try
            {
                studentFieldsAdapter.Fill(ds, "studentFields");
                if (ds.Tables["studentFields"] == null)
                    MessageBox.Show("student fields is null?");
                classesAdapter.Fill(ds, "classes"); 
                subjectsAdapter.Fill(ds, "subjects");
                
            }
            catch (SqlException sqlex)
            {
                ExceptionDialog ed = new ExceptionDialog(sqlex.Message, sqlex.StackTrace);
                ed.ShowDialog();
            }

            // Fill comboBox
            foreach(DataRow row in ds.Tables["classes"].Rows)
            {
                cmbClasses.Items.Add(row["classname"]);
            }

            // Intialize comboboxes to prevent NREs
            cmbClasses.SelectedIndex = 0;
            cmbSubjects.SelectedIndex = 0;
        }

        private void cmbClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            // First clear the current items
            cmbSubjects.Text = "";
            cmbSubjects.Items.Clear();

            foreach(DataRow row in ds.Tables["subjects"].Select("classname = '" + cmbClasses.SelectedItem.ToString() + "'"))
            {
                cmbSubjects.Items.Add(row["subjectname"]);
            }

            refreshDataGridView();
            this.dataGridView1.Columns.Remove("calculated Marks");
        }

        private void cmbSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set non-field columns as read only
            int fieldColumnOffset = getFieldColumnOffsetInPivot();
            for (int i = 0; i < fieldColumnOffset; i++)
            {
                this.dataGridView1.Columns[i].ReadOnly = true;
            }

            refreshDataGridView();
        }

        public DataTable getPivotedDataTable()
        {
            DataTable pivotedTable = new DataTable();

            // First we get the subjectid
            int subjectId = getCurrentSubjectId();

            Debug.WriteLine("getPivotedDataTable() subjectid = " + subjectId);

            // Then we get the fieldids and names for that subject
            DataTable fieldIdAndNameTable = getFieldIdAndNameTable(subjectId);
            
            // So now we create a column string 
            String query = getPivotQuery(fieldIdAndNameTable, subjectId);

            // Should really use a DataReader but 
            // we're using an adapter because we still need a DataTable for the dgv
            SqlDataAdapter pivotedAdapter = new SqlDataAdapter(query, conn);

            DataTable pivotedDataTable = new DataTable();
            try
            {
                pivotedAdapter.Fill(pivotedDataTable);
            }
            catch (Exception ex)
            {
                new ExceptionDialog(ex.Message, ex.ToString()).ShowDialog();
            }

            // Add calculated column

            addCalculatedColumn(pivotedDataTable, fieldIdAndNameTable);

            return pivotedDataTable;
        }

        void addCalculatedColumn(DataTable pivotedDataTable, DataTable fieldIdAndNameTable)
        {
            List<int> values = new List<int>();
            List<int> maxValues = new List<int>();

            DataColumn calculatedMarks = new DataColumn("calculated marks");
            pivotedDataTable.Columns.Add(calculatedMarks);

            List<string> fieldNames = new List<string>();
            List<int> fieldIDs = new List<int>();

            // Get subjectTable for maxMarks
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT maxMarks FROM subjectFields WHERE subjectid = " + getCurrentSubjectId(), conn);
            DataTable maxMarksTable = new DataTable();
            adapter.Fill(maxMarksTable);

            foreach (DataRow row in fieldIdAndNameTable.Rows)
            {
                fieldIDs.Add(Convert.ToInt32(row["fieldid"]));
                fieldNames.Add(row["fieldname"].ToString());
            }

            foreach (DataRow row in pivotedDataTable.Rows)
            {
                values.Clear();
                maxValues.Clear();
                // Get marks of the student from pivotedDataTable
                foreach (DataColumn column in pivotedDataTable.Columns)
                {
                    // if the column is a field
                    if (fieldNames.Contains(column.ColumnName))
                    {
                        values.Add(Convert.ToInt32(row[column]));
                    }
                }

                // Get max marks
                foreach (DataRow marksRow in maxMarksTable.Rows)
                {
                    maxValues.Add(Convert.ToInt32(marksRow["maxMarks"]));
                }

                PercentageStrategy pObj = new PercentageStrategy();
                row[calculatedMarks] = pObj.getResults(values, maxValues);
            }

            return;
        }

        public DataTable getFieldIdAndNameTable(int subjectid)
        {
            /*
             * This may actually be a better idea, since we don't get duplicate values and we get fresh values each time
             * for the pivot query
             */

            SqlDataAdapter studentFieldsAdapter = new SqlDataAdapter("SELECT distinct(fieldid), fieldname FROM studentFields WHERE subjectid = " + subjectid, conn);
            DataTable fieldIdAndNameTable = new DataTable();
            try
            {
                studentFieldsAdapter.Fill(fieldIdAndNameTable);
            }
            catch(Exception ex)
            {
                new ExceptionDialog(ex.Message, ex.StackTrace).ShowDialog();
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
                fieldNamesList.Add("[" + row["fieldid"] + "] as '" + row["fieldname"] + "'");
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

            if (colsInPivot == "" || colsInSelect == "")
            {
                query = String.Format(@"select studentid, studentname from students where classname = '{0}'", cmbClasses.SelectedItem.ToString());
            }

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
            int subjectid = 0;
            try
            {
                subjectid = Convert.ToInt32(
                            ds.Tables["subjects"]
                                .Select("subjectname = '" + cmbSubjects.SelectedItem.ToString() + "'")
                                .First()["subjectid"]);
            }
            catch(NullReferenceException nre)
            {
                subjectid = 0;
            }
                
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
            foreach (DataRow row in getFieldIdAndNameTable(getCurrentSubjectId()).Rows)
            {
                int id = Convert.ToInt32(row["fieldid"]); 
                // Make sure it's distinct
                if (!fieldIds.Contains(id))
                {
                    fieldIds.Add(id);
                }
            }

            if(this.dataGridView1.CurrentCell.ColumnIndex - fieldColumnOffset >= this.dataGridView1.Columns.Count)
            {
                return;
            }
            int fieldIdToUpdate = fieldIds[this.dataGridView1.CurrentCell.ColumnIndex - fieldColumnOffset];

            Debug.WriteLine(fieldIdToUpdate);
            int studentid = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells["studentid"].Value);

            // Now that we have the studentid and fieldid, we have enough to update student marks.
            SqlDataAdapter subjectFieldsAdapter = new SqlDataAdapter("SELECT * FROM subjectfields", conn);
            try
            {
                subjectFieldsAdapter.Fill(ds, "subjectFields");
            }
            catch (Exception ex)
            {
                new ExceptionDialog(ex.Message, ex.ToString()).ShowDialog();
            }
            

            DataRow rowToUpdate = ds.Tables["subjectFields"]
                                    .Select("subjectid = " + getCurrentSubjectId() + " and fieldid = " + fieldIdToUpdate)
                                    .First();
            
            int maxMarks = Convert.ToInt32(rowToUpdate["maxMarks"]);

            if(newValue > maxMarks)
            {
                MessageBox.Show("Marks given greater than max marks ("+maxMarks+")");
                return;
            }

            String query = String.Format("UPDATE studentFields SET studentMarks = {0} where fieldid = {1} and studentid = {2}", newValue, fieldIdToUpdate, studentid);
            SqlCommand cmd = new SqlCommand(query, conn);
            Debug.WriteLine("update query: " + query);
            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                ExceptionDialog ed = new ExceptionDialog(ex.Message, ex.ToString());
                ed.ShowDialog();
            }

            this.dataGridView1.BeginInvoke(new MethodInvoker(refreshDataGridView));
            
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void studentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddStudentForm frm = new AddStudentForm();
            frm.ShowDialog();
        }

        private void fieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddField frm = new AddField();
            frm.ShowDialog();
            if (frm.areFieldsUpdated == true)
            {
                refreshDataGridView();
            }
        }

        public void refreshDataGridView()
        {
            this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = getPivotedDataTable();
            this.dataGridView1.Refresh();

            for(int i = 0; i < getFieldColumnOffsetInPivot(); i++)
            {
                this.dataGridView1.Columns[i].ReadOnly = true;
            }
            this.dataGridView1.Columns["calculated Marks"].ReadOnly = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refreshDataGridView();
        }
    }
}
    
