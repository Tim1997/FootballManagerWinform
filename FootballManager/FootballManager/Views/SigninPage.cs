using Firebase.Auth;
using Firebase.Database;
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
    public partial class SigninPage : Form
    {
        protected FirebaseClient _firebaseDatabase;
        protected FirebaseAuthProvider _firebaseAuthProvider;
        public static string _currentLocalId;

        public SigninPage()
        {
            InitializeComponent();
            Init();
        }

        void Init()
        {
            _firebaseDatabase = new FirebaseClient(Constants.BaseConstant._realTimeData);
            _firebaseAuthProvider = new FirebaseAuthProvider(new FirebaseConfig(Constants.BaseConstant._auth));

            tbEmail.Text = "timbkhn@gmail.com";
            tbPassword.Text = "123456";
        }

        private async void btnSignin_Click(object sender, EventArgs e)
        {
            btnSignin.Enabled = false;
            var email = tbEmail.Text;
            var pass = tbPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Value can't be empty", "Error");
                btnSignin.Enabled = true;
                return;
            }

            try
            {
                var auth = await _firebaseAuthProvider.SignInWithEmailAndPasswordAsync(email, pass);
                var content = await auth.GetFreshAuthAsync();
                if (!content.User.IsEmailVerified)
                {
                    var json = JsonConvert.SerializeObject(new BaseResponse { Error = new Error { Message = "Email is not verified" } });
                    throw new FirebaseAuthException(null, null, json, null);
                }

                var user = (await _firebaseDatabase.Child("Users")
                                    .OnceAsync<UserModel>()).FirstOrDefault(x => x.Object.LocalId == content.User.LocalId);
                if (user != null)
                {
                    _currentLocalId = user.Object.LocalId;

                    var main = new Main();
                    main.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("User does not exist", "Information");
                }    
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
                btnSignin.Enabled = true;
            }
        }

        private void lSignup_Click(object sender, EventArgs e)
        {
            var signup = new SignupPage();
            signup.Show();
            this.Hide();
        }
    }
}
