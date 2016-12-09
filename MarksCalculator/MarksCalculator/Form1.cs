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

            // Populate dgv
            this.dataGridView1.DataSource = ds.Tables["studentFields"];

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

            int selectedSubjectId;
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
        }

        public DataTable getPivotedDataTable()
        {
            DataTable pivotedTable = new DataTable();

            // First we get the subjectid
            
            int subjectId = Convert.ToInt32(
                ds.Tables["subjects"]
                    .Select("subjectname = '" + cmbSubjects.SelectedItem.ToString() + "'")
                    .First()["subjectid"]);

            Debug.WriteLine(subjectId);

            // Then we get the fieldids and names for that subject
            DataTable fieldIdAndNameTable = getFieldIdAndNameTable(subjectId);
            
            // So now we create a column string 
            
            // for use in the pivot function
            List<string> fieldIdList = new List<string>();
            foreach(DataRow row in fieldIdAndNameTable.Rows)
            {
                fieldIdList.Add("[" + row["fieldid"] + "]");
            }
            String colsInPivot = String.Join(",", fieldIdList.Distinct());
            
            // for use in the select part
            



            // Now we build the pivoted query, i.e. populate the [40x],[...] part

            String query = String.Format(@"select studentid, studentname, {0} from
	                        (
		                        select students.studentid, students.studentname, fieldid, studentmarks, maxmarks 
		                        from studentFields join subjects
			                        on studentFields.subjectid = subjects.subjectid
		                        join students
			                        on studentFields.studentid = students.studentid
		                        where studentFields.subjectid = '{1}'
	                        ) as tab
	                        pivot
	                        (
		                        max(studentmarks) for fieldid in ({0})
	                        ) as pvt", colsInPivot.ToString(), subjectId);


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

            // Finally, get the table

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


        public List<string> getColIdsList(int subjectid)
        {
            DataRow[] rowCollection = ds.Tables["studentFields"]
                                    .Select("subjectid = " + subjectid);

            List<string> fieldIdList = new List<string>();
            foreach (DataRow row in rowCollection)
            {
                // Make sure we only add distinct values
                string fieldid = row["fieldid"].ToString();
                if (!fieldIdList.Contains(fieldid))

                    fieldIdList.Add(fieldid);
            }

            return fieldIdList;
        }

        public List<string> getColNamesList(int subjectid)
        {
            DataRow[] rowCollection = ds.Tables["studentFields"]
                                    .Select("subjectid = " + subjectid);

            List<string> fieldNameList = new List<string>();
            foreach (DataRow row in rowCollection)
            {
                // Make sure we only add distinct values
                string fieldname = row["fieldname"].ToString();
                if (!fieldNameList.Contains(fieldname))
                    fieldNameList.Add(fieldname);
            }
            return fieldNameList;
        }
       
    }
}
