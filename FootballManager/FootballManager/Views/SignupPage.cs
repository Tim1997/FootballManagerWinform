using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using FootballManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FootballManager.Views
{
    public partial class SignupPage : Form
    {
        protected FirebaseClient _firebaseDatabase;
        protected FirebaseAuthProvider _firebaseAuthProvider;
        public static UserModel _userModel;

        public SignupPage()
        {
            InitializeComponent();
            Init();
        }

        void Init()
        {
            _firebaseDatabase = new FirebaseClient(Constants.BaseConstant._realTimeData);
            _firebaseAuthProvider = new FirebaseAuthProvider(new FirebaseConfig(Constants.BaseConstant._auth));
        }

        private async void btnSignup_Click(object sender, EventArgs e)
        {
            var email = tbEmail.Text;
            var pass = tbPassword.Text;
            btnSignup.Enabled = false;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Value can't be empty", "Error");
                btnSignup.Enabled = true;

                return;
            }

            try
            {
                var auth = await _firebaseAuthProvider.CreateUserWithEmailAndPasswordAsync(email, pass, "", true);
                var content = await auth.GetFreshAuthAsync();

                var model = new UserModel
                {
                    Email = email,
                    FirstName = pass,
                    LocalId = content.User.LocalId,
                    IsEmailVerified = content.User.IsEmailVerified,
                };

                await _firebaseDatabase.Child("Users")
                        .PostAsync(model);

                MessageBox.Show("Create account success!\nPlease vertify your email", "Information");
                var signin = new SigninPage();
                signin.Show();

                this.Close();
            }
            catch (FirebaseAuthException ex)
            {
                if (ex.ResponseData == "N/A")
                    MessageBox.Show("Internet connection error", "Error");

                else
                {
                    var response = JsonConvert.DeserializeObject<BaseResponse>(ex.ResponseData);
                    MessageBox.Show(response.Error.Message, "Error");
                }
            }
            finally
            {
                btnSignup.Enabled = true;

            }
        }
    }
}
