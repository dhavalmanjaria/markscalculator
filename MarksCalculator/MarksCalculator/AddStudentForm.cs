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
    public partial class AddStudentForm : Form
    {
        DataSet ds;
        SqlConnection conn;
        String connStr = System.Configuration.ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
        
        // This variable is used to see if the current in the textboxes is saved in the db or not.
        Boolean isSaved = false;

        public AddStudentForm()
        {
            InitializeComponent();
        }
        
        private void AddStudentForm_Load(object sender, EventArgs e)
        {
            conn = new SqlConnection(connStr);
            ds = new DataSet();
            SqlDataAdapter classesAdapter = new SqlDataAdapter("SELECT * FROM classes", conn);
            classesAdapter.Fill(ds, "classes");

            // Fill comboBox
            foreach (DataRow row in ds.Tables["classes"].Rows)
            {
                cmbClasses.Items.Add(row["classname"]);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            isSaved = false;

            SqlDataAdapter studentsAdapter = new SqlDataAdapter("SELECT * FROM students", conn);

            studentsAdapter.Fill(ds, "students");

            DataRow row = ds.Tables["students"].NewRow();
            row["studentname"] = txtName.Text;
            row["classname"] = cmbClasses.SelectedItem.ToString();
            ds.Tables["students"].Rows.Add(row);

            SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(studentsAdapter);

            studentsAdapter.InsertCommand = cmdBuilder.GetInsertCommand(true);
            studentsAdapter.Update(ds, "students");

            MessageBox.Show("Student saved successfully.");

            isSaved = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!isSaved && txtName.Text != "" && cmbClasses.Text != "")
            {
                DialogResult result = MessageBox.Show("Do you want to save the current student?", "Save Student", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    btnSave_Click(sender, e);
                }
            }
            else
            {
                btnClear_Click(sender, e);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtName.Text = "";
            cmbClasses.Text = "";
        }
    }
}
