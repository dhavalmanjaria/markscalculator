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

            this.dataGridView1.DataSource = subjectsDataTable;
            this.dataGridView1.Refresh();
        }



       
    }
}
