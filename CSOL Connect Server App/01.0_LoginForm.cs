﻿using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using Microsoft.VisualBasic.ApplicationServices;

namespace CSOL_Connect_Server_App
{
    public partial class LoginForm : Form
    {
        SQL_Connection sql_Connection = new SQL_Connection();

        public LoginForm()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, MouseEventArgs e)
        {
            this.Hide();
            LoginForm page = new LoginForm();
            page.Show();
        }

        private void LoginButton_Click(object sender, MouseEventArgs e)
        {
            if (TextBox_UserID.Text.Length == 0)
            {
                TextBox_UserID.PlaceholderText = "User ID Required.";
            }
            if (TextBox_Password.Text.Length == 0)
            {
                TextBox_Password.PlaceholderText = "Password Required.";
            }

            if (TextBox_UserID.Text.Length > 0 && TextBox_Password.Text.Length > 0)
            {
                try
                {
                    string uid = TextBox_UserID.Text;
                    string pw = TextBox_Password.Text;
                    string hashedPassword = string.Empty;

                    byte[] passwordBytes = Encoding.UTF8.GetBytes(pw);
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashedPasswordBytes = sha256.ComputeHash(passwordBytes);
                        hashedPassword = BitConverter.ToString(hashedPasswordBytes).Replace("-", "").ToLower();
                    }

                    string connectionString = sql_Connection.SQLConnection();
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = "SELECT COUNT(*) FROM Users WHERE [User ID] = @UserID AND [Password] = @Password";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", uid);
                            command.Parameters.AddWithValue("@Password", hashedPassword);

                            connection.Open();
                            int result = (int)command.ExecuteScalar();

                            if (result > 0)
                            {
                                // Password is correct, now display the CAPTCHA form for verification
                                CaptchaForm captchaForm = new CaptchaForm();
                                DialogResult captchaResult = captchaForm.ShowDialog();

                                if (captchaResult == DialogResult.OK)
                                {

                                    if (pw!="CSOL-connect2023!") {
                                        // CAPTCHA verification succeeded, proceed with login
                                        MessageBox.Show("Login successful!");

                                        string userLevelQuery = "SELECT [User Level] FROM Users WHERE [User ID] = @UserID";
                                        using (SqlCommand userLevelCommand = new SqlCommand(userLevelQuery, connection))
                                        {
                                            userLevelCommand.Parameters.AddWithValue("@UserID", uid);
                                            string userLevel = userLevelCommand.ExecuteScalar()?.ToString();

                                            if (userLevel == "Admin")
                                            {
                                                this.Hide();
                                                Admin_Dashboard page = new Admin_Dashboard();
                                                page.Show();
                                            }
                                            else if (userLevel == "Super Admin")
                                            {
                                                this.Hide();
                                                SuperAdmin_Dashboard page = new SuperAdmin_Dashboard();
                                                page.Show();
                                            }
                                            else
                                            {
                                                MessageBox.Show("Something went wrong.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // CAPTCHA verification succeeded, proceed with login
                                        MessageBox.Show("Login successful!");

                                        string userLevelQuery = "SELECT [User Level] FROM Users WHERE [User ID] = @UserID";
                                        using (SqlCommand userLevelCommand = new SqlCommand(userLevelQuery, connection))
                                        {
                                            userLevelCommand.Parameters.AddWithValue("@UserID", uid);
                                            string userLevel = userLevelCommand.ExecuteScalar()?.ToString();

                                            if (userLevel == "Admin")
                                            {
                                                int userId = int.Parse(uid);
                                                this.Hide();
                                                forAdmin_ChangePW_Required page = new forAdmin_ChangePW_Required(userId);
                                                page.Show();
                                            }
                                            else if (userLevel == "Super Admin")
                                            {
                                                int userId = int.Parse(uid);
                                                this.Hide();
                                                ChangePW_Required page = new ChangePW_Required(userId);
                                                page.Show();
                                            }
                                            else
                                            {
                                                MessageBox.Show("Something went wrong.");
                                            }
                                        }
                                    }
                                }

                                else {                                
                                // CAPTCHA verification failed, display an error message or take appropriate action
                                MessageBox.Show("Captcha verification failed. Please try again.");
                                }
                                
                            }
                            else
                            {
                                // Password is incorrect
                                MessageBox.Show("Invalid username or password.");
                            }
                            connection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex);
                    this.Hide();
                    LoginForm page = new LoginForm();
                    page.Show();
                }
            }
        }


        private void UserIDTextBox_TextChanged(object sender, EventArgs e)
        {
            int a = 0;
            foreach (Char s in TextBox_UserID.Text)
            {
                if (Char.IsLetter(s) == true || Char.IsWhiteSpace(s) == true || Char.IsSymbol(s) == true || Char.IsPunctuation(s) == true)
                {
                    a += 1;
                }
            }
            if (a > 0)
            {
                MessageBox.Show("Please only input digit/s in this field.");
                TextBox_UserID.ResetText();
            }
        }

        private void Login_EyeRevealPicBox_Click(object sender, EventArgs e)
        {
            if (TextBox_Password.PasswordChar == '●')
            {
                TextBox_Password.PasswordChar = '\0';
            }
            else if (TextBox_Password.PasswordChar == '\0')
            {
                TextBox_Password.PasswordChar = '●';
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (TextBox_UserID.Text.Length != 0)
            {
                Button_ForgotPassword.Visible = true;
                Button_ForgotPassword.Enabled = true;

            }
            else
            {
                Button_ForgotPassword.Visible = false;
                Button_ForgotPassword.Enabled = false;
            }
        }


        private void Button_ForgotPassword_Click(object sender, EventArgs e)
        {
            string userID = TextBox_UserID.Text;
            bool userExists = CheckIfUserExists(userID);


            if (userExists)
            {
                int Userid = int.Parse(TextBox_UserID.Text);
                SecurityQuestions page = new SecurityQuestions(Userid);
                this.Hide();
                page.Show();
            }
            else
            {
                MessageBox.Show("User ID not found.");
            }
        }

        private bool CheckIfUserExists(string userID)
        {
            bool userExists = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(sql_Connection.SQLConnection()))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM Users WHERE [User ID] = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userID);
                        int result = (int)command.ExecuteScalar();

                        if (result > 0)
                        {
                            userExists = true;
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }

            return userExists;
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
        }
    }
}