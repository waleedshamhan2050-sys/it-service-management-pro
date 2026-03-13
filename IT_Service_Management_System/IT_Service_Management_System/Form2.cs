using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace IT_Service_Management_System
{
    public partial class LoginForm : Form
    {
        // ضع مسار قاعدة البيانات عندك
        readonly string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                                             AttachDbFilename=C:\Users\walee\source\repos\IT_Service_Management_System\IT_Service_Management_System\IT_Service_DB.mdf;
                                             Integrated Security=True;Connect Timeout=30";

        public LoginForm()
        {
            InitializeComponent();  // <- هذا يجب أن يكون أول شيء

            // ربط الأحداث بعد InitializeComponent فقط
            btnLogin.Click += btnLogin_Click;
            this.Load += LoginForm_Load;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // أي إعدادات عند تحميل الفورم
            txtUsername.Focus(); // التركيز على خانة المستخدم
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter username and password.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseHelper db = new DatabaseHelper();
                string query = "SELECT UserID, UserName, Password, Role FROM Users WHERE UserName=@u";
                SqlParameter[] parameters = { new SqlParameter("@u", txtUsername.Text) };
                DataTable dt = db.ExecuteQuery(query, parameters);

                if (dt.Rows.Count == 1)
                {
                    string storedHash = dt.Rows[0]["Password"].ToString();
                    if (SecurityHelper.VerifyPassword(txtPassword.Text, storedHash))
                    {
                        Session.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                        Session.UserName = dt.Rows[0]["UserName"].ToString();
                        Session.Role = dt.Rows[0]["Role"].ToString();

                        db.LogAction(Session.UserID, "User logged in");
                        
                        MessageBox.Show($"Welcome {Session.UserName}! ✅", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Form1 mainForm = new Form1();
                        mainForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Invalid password ❌", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("User not found ❌", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogin_Click_2(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
