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
using System.Drawing.Printing;

namespace IT_Service_Management_System
{
   

    public partial class Form1 : Form
    {
        DatabaseHelper db = new DatabaseHelper();
        private int selectedRequestId = -1;
        private PrintDocument printDocument = new PrintDocument();
        private PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();

        public Form1()
        {
            InitializeComponent();
            printDocument.PrintPage += new PrintPageEventHandler(printDocument_PrintPage);
            CheckPermissions();
        }

        private void CheckPermissions()
        {
            // إخفاء ميزات الإدارة عن المستخدمين العاديين
            if (Session.Role != "Admin")
            {
                btnChangeStatus.Visible = false;
                button1.Visible = false; // زر الحذف
                this.Text += " (User Mode)";
            }
            else
            {
                this.Text += " (Admin Mode)";
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.ShowDialog();
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Bitmap bm = new Bitmap(this.dgvRequests.Width, this.dgvRequests.Height);
            dgvRequests.DrawToBitmap(bm, new Rectangle(0, 0, this.dgvRequests.Width, this.dgvRequests.Height));
            e.Graphics.DrawImage(bm, 0, 0);
        }


        private void Form1_Load(object sender, EventArgs e)
            {
                LoadData();

                // إضافة عناصر ComboBox
              
            }

            private void LoadData()
        {
            try
            {
                string query = "SELECT * FROM ServiceRequests";
                dgvRequests.DataSource = db.ExecuteQuery(query);
                UpdateDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateDashboard()
        {
            // تحديث إحصائيات سريعة في العنوان أو شريط الحالة
            int openCount = Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM ServiceRequests WHERE Status='Open'"));
            label1.Text = $"IT Management | Open Requests: {openCount}";
        }

    
       private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserName.Text) || cmbIssueType.SelectedIndex == -1 || cmbPriority.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query = @"INSERT INTO ServiceRequests (UserName, IssueType, Priority, Status, CreatedBy) 
                                 VALUES (@UserName, @IssueType, @Priority, 'Open', @CreatedBy)";
                SqlParameter[] parameters = {
                    new SqlParameter("@UserName", txtUserName.Text),
                    new SqlParameter("@IssueType", cmbIssueType.Text),
                    new SqlParameter("@Priority", cmbPriority.Text),
                    new SqlParameter("@CreatedBy", Session.UserID)
                };
                db.ExecuteNonQuery(query, parameters);
                db.LogAction(Session.UserID, $"Added new request for {txtUserName.Text}");
                
                LoadData();
                ClearFields();
                MessageBox.Show("Request added successfully! ✅", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void ClearFields()
        {
            txtUserName.Text = "";
            cmbIssueType.SelectedIndex = -1;
            cmbPriority.SelectedIndex = -1;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void dgvRequests_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // تأكد أن الصف صحيح
            {
                DataGridViewRow row = dgvRequests.Rows[e.RowIndex];
                selectedRequestId = Convert.ToInt32(row.Cells["RequestID"].Value); // مهم جداً
                txtUserName.Text = row.Cells["UserName"].Value.ToString();
                cmbIssueType.Text = row.Cells["IssueType"].Value.ToString();
                cmbPriority.Text = row.Cells["Priority"].Value.ToString();
            }
        }


        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedRequestId == -1)
            {
                MessageBox.Show("Select a request first.");
                return;
            }

            try
            {
                string query = @"UPDATE ServiceRequests SET UserName=@UserName, IssueType=@IssueType, Priority=@Priority 
                                 WHERE RequestID=@RequestID";
                SqlParameter[] parameters = {
                    new SqlParameter("@UserName", txtUserName.Text),
                    new SqlParameter("@IssueType", cmbIssueType.Text),
                    new SqlParameter("@Priority", cmbPriority.Text),
                    new SqlParameter("@RequestID", selectedRequestId)
                };
                db.ExecuteNonQuery(query, parameters);
                db.LogAction(Session.UserID, $"Updated request ID: {selectedRequestId}");

                MessageBox.Show("Request updated successfully! 🔄");
                LoadData();
                ClearFields();
                selectedRequestId = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }


        private void dgvRequests_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
            {
                if (dgvRequests.Rows[e.RowIndex].Cells["Status"].Value != null)
                {
                    string status = dgvRequests.Rows[e.RowIndex].Cells["Status"].Value.ToString();
                    if (status == "Open")
                        dgvRequests.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                    else if (status == "Closed")
                        dgvRequests.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                }
            }

      
            private void btnChangeStatus_Click(object sender, EventArgs e)
        {
            if (selectedRequestId == -1)
            {
                MessageBox.Show("Select a request first.");
                return;
            }

            try
            {
                string currentStatus = dgvRequests.CurrentRow.Cells["Status"].Value.ToString();
                string newStatus = (currentStatus == "Open") ? "Closed" : "Open";

                string query = "UPDATE ServiceRequests SET Status=@Status WHERE RequestID=@RequestID";
                SqlParameter[] parameters = {
                    new SqlParameter("@Status", newStatus),
                    new SqlParameter("@RequestID", selectedRequestId)
                };
                db.ExecuteNonQuery(query, parameters);
                db.LogAction(Session.UserID, $"Changed status of request {selectedRequestId} to {newStatus}");

                LoadData();
                MessageBox.Show($"Status changed to {newStatus}! 🚥");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       
            private void txtSearch_TextChanged(object sender, EventArgs e)
            {
                try
                {
                    DataTable dt = dgvRequests.DataSource as DataTable;
                    if (dt != null)
                    {
                        dt.DefaultView.RowFilter =
                            $"UserName LIKE '%{txtSearch.Text}%' OR IssueType LIKE '%{txtSearch.Text}%'";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
    }
    
    



