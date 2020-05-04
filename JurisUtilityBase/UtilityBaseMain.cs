using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Globalization;
using Gizmox.Controls;
using JDataEngine;
using JurisAuthenticator;
using JurisUtilityBase.Properties;
using System.Data.OleDb;

namespace JurisUtilityBase
{
    public partial class UtilityBaseMain : Form
    {
        #region Private  members

        private JurisUtility _jurisUtility;

        private JurisUtility oldBooksCon;

        #endregion

        #region Public properties

        public string CompanyCode { get; set; }

        public string JurisDbName { get; set; }

        public string JBillsDbName { get; set; }

        public string OrigDB { get; set; }

        #endregion

        #region Constructor

        public UtilityBaseMain()
        {
            InitializeComponent();
            _jurisUtility = new JurisUtility();
            oldBooksCon = new JurisUtility();
        }

        #endregion

        #region Public methods

        public void LoadCompanies()
        {
            //load licenses for new, split DB
            var companies = _jurisUtility.Companies.Cast<object>().Cast<Instance>().ToList();
//            listBoxCompanies.SelectedIndexChanged -= listBoxCompanies_SelectedIndexChanged;
            listBoxToCompany.ValueMember = "Code";
            listBoxToCompany.DisplayMember = "Key";
            listBoxToCompany.DataSource = companies;
//            listBoxCompanies.SelectedIndexChanged += listBoxCompanies_SelectedIndexChanged;
            var defaultCompany = companies.FirstOrDefault(c => c.Default == Instance.JurisDefaultCompany.jdcJuris);
            if (companies.Count > 0)
            {
                listBoxToCompany.SelectedItem = defaultCompany ?? companies[0];
            }

            //load licenses for old books
            var company = oldBooksCon.Companies.Cast<object>().Cast<Instance>().ToList();
            //            listBoxCompanies.SelectedIndexChanged -= listBoxCompanies_SelectedIndexChanged;
            listBoxFromCompany.ValueMember = "Code";
            listBoxFromCompany.DisplayMember = "Key";
            listBoxFromCompany.DataSource = company;
            //            listBoxCompanies.SelectedIndexChanged += listBoxCompanies_SelectedIndexChanged;
            var defCompany = company.FirstOrDefault(c => c.Default == Instance.JurisDefaultCompany.jdcJuris);
            if (company.Count > 0)
            {
                listBoxFromCompany.SelectedItem = defCompany ?? company[0];
            }



        }

        #endregion

        #region MainForm events

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void listBoxCompanies_SelectedIndexChanged(object sender, EventArgs e)
        {

            //TO DB
            if (_jurisUtility.DbOpen)
            {
                _jurisUtility.CloseDatabase();
            }
            CompanyCode = "Company" + listBoxToCompany.SelectedValue;
            _jurisUtility.SetInstance(CompanyCode);
            JurisDbName = _jurisUtility.Company.DatabaseName;
            JBillsDbName = "JBills" + _jurisUtility.Company.Code;
            _jurisUtility.OpenDatabase();
            if (_jurisUtility.DbOpen)
            {
                ///GetFieldLengths();
            }


        }



        #endregion

        #region Private methods

        private void DoDaFix()
        {

            MessageBox.Show(OrigDB);
            string sql = "EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'";
            _jurisUtility.ExecuteNonQueryCommand(0, sql);

            /*
            DataTable d2 = dataGridView1.DataSource as DataTable;
            int i = 1;

            if (d2.Rows.Count == 0)
            { MessageBox.Show("No vouchers to import.  Select a different file or close the application."); }
            else
            {
                foreach (DataRow dr in d2.Rows)
                {
                    string vtype = dr["Type"].ToString();
                    string VoucherDate = dr["VoucherDate"].ToString();
                    string VendorCode = dr["VendorCode"].ToString();
                    string PONbr = dr["PONbr"].ToString();
                    string InvoiceNbr = dr["InvoiceNbr"].ToString();
                    string DueDate = dr["DueDate"].ToString();
                    string InvoiceDate = dr["InvoiceDate"].ToString();
                    string DiscountDate = dr["DiscountDate"].ToString();
                    string InvoiceAmt = dr["InvoiceAmt"].ToString();
                    string NonDiscAmt = dr["NonDiscAmt"].ToString();
                    string VchReference = dr["VchReference"].ToString();
                    string SeparateCheck = dr["SeparateCheck"].ToString();
                    string APAcct = dr["APAcct"].ToString();
                    string GLDistAcct = dr["GLDistAcct"].ToString();
                    string GLAmt = dr["GLAmt"].ToString();
                    string TrustBank = dr["TrustBank"].ToString();
                    string ExpClient = dr["ExpClient"].ToString();
                    string ExpMatter = dr["ExpMatter"].ToString();
                    string ExpCode = dr["ExpCode"].ToString();
                    string ExpTaskCode = dr["ExpTaskCode"].ToString();
                    string ExpUnits = dr["ExpUnits"].ToString();
                    string ExpAmount = dr["ExpAmount"].ToString();
                    string ExpNarrative = dr["ExpNarrative"].ToString();
                    string ExpBillNote = dr["ExpBillNote"].ToString();

                    string s2 = "Insert into #Vch " +
                    "Values(" + i + ",'" + vtype + "',convert(datetime,'" + VoucherDate + "',101),'" + VendorCode + "','" + PONbr + "','" + InvoiceNbr + "',convert(datetime,'" + InvoiceDate + "',101),convert(datetime,'" + DueDate + "',101),convert(datetime,'" + DiscountDate + "',101), " +
                    "cast(isnull('" + InvoiceAmt + "','0') as decimal(12,2)), cast(isnull('" + NonDiscAmt + "','0') as decimal(12,2)), '" + VchReference + "','" + SeparateCheck + "','" + APAcct + "','" + GLDistAcct + "', cast(isnull('" + GLAmt + "','0') as money), " +
                    "'" + TrustBank + "','" + ExpClient + "','" + ExpMatter + "','" + ExpCode + "','" + ExpTaskCode + "',cast(isnull('" + ExpUnits + "','0') as decimal(12,2)),cast(isnull('" + ExpAmount + "','0') as decimal(12,2)),'" + ExpNarrative + "','" + ExpBillNote + "')";
                    _jurisUtility.ExecuteNonQueryCommand(0, s2);

                    i = i + 1;
                }
            }
            */

            UpdateStatus("All MBF07 fields updated.", 1, 1);

            MessageBox.Show("The process is complete", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        private bool VerifyFirmName()
        {
            //    Dim SQL     As String
            //    Dim rsDB    As ADODB.Recordset
            //
            //    SQL = "SELECT CASE WHEN SpTxtValue LIKE '%firm name%' THEN 'Y' ELSE 'N' END AS Firm FROM SysParam WHERE SpName = 'FirmName'"
            //    Cmd.CommandText = SQL
            //    Set rsDB = Cmd.Execute
            //
            //    If rsDB!Firm = "Y" Then
            return true;
            //    Else
            //        VerifyFirmName = False
            //    End If

        }

        private bool FieldExistsInRS(DataSet ds, string fieldName)
        {

            foreach (DataColumn column in ds.Tables[0].Columns)
            {
                if (column.ColumnName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }


        private static bool IsDate(String date)
        {
            try
            {
                DateTime dt = DateTime.Parse(date);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsNumeric(object Expression)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum; 
        }

        private void WriteLog(string comment)
        {
            var sql =
                string.Format("Insert Into UtilityLog(ULTimeStamp,ULWkStaUser,ULComment) Values('{0}','{1}', '{2}')",
                    DateTime.Now, GetComputerAndUser(), comment);
            _jurisUtility.ExecuteNonQueryCommand(0, sql);
        }

        private string GetComputerAndUser()
        {
            var computerName = Environment.MachineName;
            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var userName = (windowsIdentity != null) ? windowsIdentity.Name : "Unknown";
            return computerName + "/" + userName;
        }

        /// <summary>
        /// Update status bar (text to display and step number of total completed)
        /// </summary>
        /// <param name="status">status text to display</param>
        /// <param name="step">steps completed</param>
        /// <param name="steps">total steps to be done</param>
        private void UpdateStatus(string status, long step, long steps)
        {
            labelCurrentStatus.Text = status;

            if (steps == 0)
            {
                progressBar.Value = 0;
                labelPercentComplete.Text = string.Empty;
            }
            else
            {
                double pctLong = Math.Round(((double)step/steps)*100.0);
                int percentage = (int)Math.Round(pctLong, 0);
                if ((percentage < 0) || (percentage > 100))
                {
                    progressBar.Value = 0;
                    labelPercentComplete.Text = string.Empty;
                }
                else
                {
                    progressBar.Value = percentage;
                    labelPercentComplete.Text = string.Format("{0} percent complete", percentage);
                }
            }
        }

        private void DeleteLog()
        {
            string AppDir = Path.GetDirectoryName(Application.ExecutablePath);
            string filePathName = Path.Combine(AppDir, "VoucherImportLog.txt");
            if (File.Exists(filePathName + ".ark5"))
            {
                File.Delete(filePathName + ".ark5");
            }
            if (File.Exists(filePathName + ".ark4"))
            {
                File.Copy(filePathName + ".ark4", filePathName + ".ark5");
                File.Delete(filePathName + ".ark4");
            }
            if (File.Exists(filePathName + ".ark3"))
            {
                File.Copy(filePathName + ".ark3", filePathName + ".ark4");
                File.Delete(filePathName + ".ark3");
            }
            if (File.Exists(filePathName + ".ark2"))
            {
                File.Copy(filePathName + ".ark2", filePathName + ".ark3");
                File.Delete(filePathName + ".ark2");
            }
            if (File.Exists(filePathName + ".ark1"))
            {
                File.Copy(filePathName + ".ark1", filePathName + ".ark2");
                File.Delete(filePathName + ".ark1");
            }
            if (File.Exists(filePathName ))
            {
                File.Copy(filePathName, filePathName + ".ark1");
                File.Delete(filePathName);
            }

        }

            

        private void LogFile(string LogLine)
        {
            string AppDir = Path.GetDirectoryName(Application.ExecutablePath);
            string filePathName = Path.Combine(AppDir, "VoucherImportLog.txt");
            using (StreamWriter sw = File.AppendText(filePathName))
            {
                sw.WriteLine(LogLine);
            }	
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            DoDaFix();
        }

        private void buttonReport_Click(object sender, EventArgs e)
        {

            System.Environment.Exit(0);
          
        }

        private void listBoxFromCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrigDB = "";
            //FROM DB
            if (oldBooksCon.DbOpen)
            {
                oldBooksCon.CloseDatabase();
            }
            CompanyCode = "Company" + listBoxFromCompany.SelectedValue;
            oldBooksCon.SetInstance(CompanyCode);
            OrigDB = oldBooksCon.Company.DatabaseName;
            JBillsDbName = "JBills" + oldBooksCon.Company.Code;
            oldBooksCon.OpenDatabase();
            if (oldBooksCon.DbOpen)
            {
                //oldBooksCon.CloseDatabase();
            }
        }

        private void buttonMoveToSelection_Click(object sender, EventArgs e)
        {

        }
    }
}
