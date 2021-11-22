using Firebase.Database.Query;
using FootballManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FootballManager.Views
{
    public partial class CustomersView : UserControl
    {
        protected Firebase.Database.FirebaseClient _firebaseDatabase;
        protected Firebase.Auth.FirebaseAuthProvider _firebaseAuthProvider;
        private string _currentCustomerLocalId;

        public CustomersView()
        {
            InitializeComponent();

            Init();
            LoadCustomers();
        }

        void Init()
        {
            _firebaseDatabase = new Firebase.Database.FirebaseClient(Constants.BaseConstant._realTimeData);
            _firebaseAuthProvider = new Firebase.Auth.FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(Constants.BaseConstant._auth));

            cbSex.DataSource = new string[] { "Male", "Female" };
            cbSex.SelectedIndex = -1;

            dbCustomers.ColumnCount = 5;
            dbCustomers.ColumnHeadersVisible = true;
            dbCustomers.Columns[0].Name = "Id";
            dbCustomers.Columns[1].Name = "Name";
            dbCustomers.Columns[2].Name = "Phone";
            dbCustomers.Columns[3].Name = "Sex";
            dbCustomers.Columns[4].Name = "Address";

        }

        async void LoadCustomers()
        {
            var lcustomer = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Customers").OnceAsync<Customer>());
            this.dbCustomers.Rows.Clear();

            foreach (var item in lcustomer)
            {
                this.dbCustomers.Rows.Add(new string[] 
                { item.Object.LocalId, item.Object.DisplayName, item.Object.PhoneNumber,
                    item.Object.Sex, item.Object.Address });
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var name = tbName.Text;
            var address = tbAddress.Text;
            var phone = tbPhone.Text;
            var selectSex = cbSex.SelectedItem.ToString();

            if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address)
                || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(selectSex))
            {
                MessageBox.Show("Value can't be empty", "Information");
                return;
            }

            var customer = new Customer
            {
                DisplayName = name,
                LocalId = Guid.NewGuid().ToString(),
                Address = address,
                PhoneNumber = phone,
                Sex = selectSex,
            };

            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Customers").PostAsync(customer);

            this.dbCustomers.Rows.Add(new string[]
                { customer.LocalId, customer.DisplayName, customer.PhoneNumber,
                    customer.Sex, customer.Address });
        }

        private async void btnRemove_Click(object sender, EventArgs e)
        {
            if (_currentCustomerLocalId == null) return;
            var fcustomer = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Customers").OnceAsync<Customer>()).FirstOrDefault(x => x.Object.LocalId == _currentCustomerLocalId);
            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Customers").Child(fcustomer.Key).DeleteAsync();

            foreach (DataGridViewRow row in dbCustomers.SelectedRows)
            {
                var localid = row.Cells[0].Value.ToString();
                if(localid == _currentCustomerLocalId)
                {
                    dbCustomers.Rows.Remove(row);
                    break;
                }    
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_currentCustomerLocalId == null) return;
            var name = tbName.Text;
            var address = tbAddress.Text;
            var phone = tbPhone.Text;
            var selectSex = cbSex.SelectedItem.ToString();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address)
                || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(selectSex))
            {
                MessageBox.Show("Value can't be empty", "Information");
                return;
            }

            var customer = new Customer
            {
                DisplayName = name,
                LocalId = _currentCustomerLocalId,
                Address = address,
                PhoneNumber = phone,
                Sex = selectSex,
            };

            var fcustomer = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Customers").OnceAsync<Customer>()).FirstOrDefault(x => x.Object.LocalId == customer.LocalId);
            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Customers").Child(fcustomer.Key).PutAsync(customer);

            foreach (DataGridViewRow row in dbCustomers.SelectedRows)
            {
                var localid = row.Cells[0].Value.ToString();
                if (localid == _currentCustomerLocalId)
                {
                    row.Cells[1].Value = customer.DisplayName;
                    row.Cells[2].Value = customer.PhoneNumber;
                    row.Cells[3].Value = customer.Sex;
                    row.Cells[4].Value = customer.Address;
                    break;
                }
            }
        }

        private void dbCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = dbCustomers.SelectedRows[0];
            _currentCustomerLocalId = row.Cells[0].Value.ToString();
            tbName.Text = row.Cells[1].Value.ToString();
            tbPhone.Text = row.Cells[2].Value.ToString();
            cbSex.SelectedIndex = row.Cells[3].Value.ToString() == "Male" ? 0 : 1;
            tbAddress.Text = row.Cells[4].Value.ToString();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var csv = new StringBuilder();
                var head = string.Format("{0},{1},{2},{3},{4}", "ID", "Name", "Phone", "Sex", "Address");
                csv.AppendLine(head);

                foreach (DataGridViewRow row in dbCustomers.Rows)
                {
                    var localid = row.Cells[0].Value.ToString();
                    var name = row.Cells[1].Value.ToString();
                    var phone = row.Cells[2].Value.ToString();
                    var sex = row.Cells[3].Value.ToString();
                    var address = row.Cells[4].Value.ToString();

                    var newLine = string.Format("{0},{1},{2},{3},{4}", localid, name, phone, sex, address);
                    csv.AppendLine(newLine);
                }

                try
                {
                    File.WriteAllText(saveFileDialog1.FileName, csv.ToString());
                    MessageBox.Show("Save file success", "Information");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }
    }
}
