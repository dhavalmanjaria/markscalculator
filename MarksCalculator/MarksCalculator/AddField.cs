﻿using System;
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
    public partial class AddField : Form
    {

        SqlConnection conn;
        DataSet ds = new DataSet();
        SqlDataAdapter subjectfieldsAdapter, subjectsAdapter;
        String connStr = System.Configuration.ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
        public bool areFieldsUpdated;


        public AddField()
        {
            InitializeComponent();
            areFieldsUpdated = false;
        }

        private void AddField_Load(object sender, EventArgs e)
        {
            conn = new SqlConnection(connStr);
            ds = new DataSet();
            SqlDataAdapter classesAdapter = new SqlDataAdapter("SELECT * FROM classes", conn);
            subjectsAdapter = new SqlDataAdapter("SELECT * FROM subjects", conn);

            try
            {
                classesAdapter.Fill(ds, "classes");
                subjectsAdapter.Fill(ds, "subjects");
            }
            catch (Exception ex)
            {
                new ExceptionDialog(ex.Message, ex.ToString()).ShowDialog();
            }
            

            // Fill comboBox
            foreach (DataRow row in ds.Tables["classes"].Rows)
            {
                cmbClasses.Items.Add(row["classname"]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public int getCurrentSubjectId()
        {
            int subjectid = Convert.ToInt32(
                    ds.Tables["subjects"]
                        .Select("subjectname = '" + cmbSubjects.SelectedItem.ToString() + "'")
                        .First()["subjectid"]);

            return subjectid;
        }
        private void cmbClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            // First clear the current items
            cmbSubjects.Text = "";
            cmbSubjects.Items.Clear();

            foreach (DataRow row in ds.Tables["subjects"].Select("classname = '" + cmbClasses.SelectedItem.ToString() + "'"))
            {
                cmbSubjects.Items.Add(row["subjectname"]);
            }
        }

        private void cmbSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

       
        private void txtMaxMarks_TextChanged(object sender, EventArgs e)
        {
            int maxMarks = 0;
            if (!Int32.TryParse(txtMaxMarks.Text, out maxMarks))
                txtMaxMarks.Text = "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            subjectfieldsAdapter = new SqlDataAdapter("SELECT * FROM subjectfields", conn);
            try
            {
                subjectfieldsAdapter.Fill(ds, "subjectfields");
            }
            catch (Exception ex)
            {
                new ExceptionDialog(ex.Message, ex.ToString()).ShowDialog();
            }
            
            DataRow newRow = ds.Tables["subjectfields"].NewRow();
            newRow["fieldname"] = txtFieldname.Text;
            newRow["subjectid"] = getCurrentSubjectId();
            newRow["maxMarks"] = Int32.Parse(txtMaxMarks.Text);

            ds.Tables["subjectfields"].Rows.Add(newRow);

            SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(subjectfieldsAdapter);
            subjectfieldsAdapter.InsertCommand = cmdBuilder.GetInsertCommand(true);

            try
            {
                subjectfieldsAdapter.Update(ds, "subjectfields");
            }
            catch (Exception ex)
            {
                new ExceptionDialog(ex.Message, ex.ToString()).ShowDialog();
            }
            
            

            MessageBox.Show("New field added to " + cmbSubjects.Text + " in course " + cmbClasses.Text);
            areFieldsUpdated = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            cmbClasses.Text = "";
            cmbSubjects.Text = "";
            txtFieldname.Text = "";
            txtMaxMarks.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            txtFieldname.Text = "";
        }

        
    }
}
