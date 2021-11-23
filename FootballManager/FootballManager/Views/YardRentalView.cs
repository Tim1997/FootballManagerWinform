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
    public partial class YardRentalView : UserControl
    {
        protected Firebase.Database.FirebaseClient _firebaseDatabase;
        protected Firebase.Auth.FirebaseAuthProvider _firebaseAuthProvider;
        protected List<string> CustomerIds;
        protected List<string> PitchesIds;
        private string _currentCustomerLocalId;

        public YardRentalView()
        {
            InitializeComponent();

            Init();
            LoadCustomers();
        }

        void Init()
        {
            _firebaseDatabase = new Firebase.Database.FirebaseClient(Constants.BaseConstant._realTimeData);
            _firebaseAuthProvider = new Firebase.Auth.FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(Constants.BaseConstant._auth));

            CustomerIds = new List<string>(0);
            PitchesIds = new List<string>();

            dgYardRental.ColumnCount = 5;
            dgYardRental.ColumnHeadersVisible = true;
            dgYardRental.Columns[0].Name = "Id";
            dgYardRental.Columns[1].Name = "CustomerId";
            dgYardRental.Columns[2].Name = "PitchId";
            dgYardRental.Columns[3].Name = "Name";
            dgYardRental.Columns[4].Name = "Time";

        }

        async void LoadCustomers()
        {
            var lcustomer = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Customers").OnceAsync<Customer>());
            foreach (var item in lcustomer)
            {
                CustomerIds.Add(item.Object.LocalId);
            }

            var lpitch = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Pitches").OnceAsync<Pitch>());
            foreach (var item in lpitch)
            {
                PitchesIds.Add(item.Object.Id);
            }

            cbCustomers.DataSource = CustomerIds;
            cbPitches.DataSource = PitchesIds;

            cbCustomers.SelectedIndex = -1;
            cbPitches.SelectedIndex = -1;

            var lyardrental = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("YardRentals").OnceAsync<YardRental>());
            this.dgYardRental.Rows.Clear();

            foreach (var item in lyardrental)
            {
                this.dgYardRental.Rows.Add(new string[]
                { item.Object.CustomerId, item.Object.PitchId, item.Object.Name,
                    item.Object.Time.ToString() });
            }
        }

        /// <summary>
        /// add
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button1_Click(object sender, EventArgs e)
        {
            var name = tbName.Text;
            var customerid = cbCustomers.SelectedItem.ToString();
            var pitchid = cbPitches.SelectedItem.ToString();
            var time = dtpDatetime.Value.ToString("dd/MM/yyyy HH:mm:ss");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(customerid)
                || string.IsNullOrEmpty(time) || string.IsNullOrEmpty(pitchid))
            {
                MessageBox.Show("Value can't be empty", "Information");
                return;
            }

            var yardrental = new YardRental
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CustomerId = customerid,
                PitchId = pitchid,
                Time = time,
            };

            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("YardRentals").PostAsync(yardrental);

            this.dgYardRental.Rows.Add(new string[]
                {yardrental.Id, yardrental.CustomerId, yardrental.PitchId, yardrental.Name,
                    yardrental.Time.ToString() });
        }

        /// <summary>
        /// remove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button3_Click(object sender, EventArgs e)
        {
            if (_currentCustomerLocalId == null) return;
            var fcyardrental = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("YardRentals").OnceAsync<YardRental>()).FirstOrDefault(x => x.Object.Id == _currentCustomerLocalId);
            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("YardRentals").Child(fcyardrental.Key).DeleteAsync();

            foreach (DataGridViewRow row in dgYardRental.SelectedRows)
            {
                var localid = row.Cells[0].Value.ToString();
                if (localid == _currentCustomerLocalId)
                {
                    dgYardRental.Rows.Remove(row);
                    break;
                }
            }
        }

        private void dgYardRental_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgYardRental.SelectedRows[0];
            _currentCustomerLocalId = row.Cells[0].Value.ToString();
            cbCustomers.SelectedItem = row.Cells[1].Value.ToString();
            cbPitches.SelectedItem = row.Cells[2].Value.ToString();
            tbName.Text = row.Cells[3].Value.ToString();
            dtpDatetime.Value = DateTime.ParseExact(row.Cells[4].Value.ToString(), "dd/MM/yyyy HH:mm:ss",
                                      System.Globalization.CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button2_Click(object sender, EventArgs e)
        {
            if (_currentCustomerLocalId == null) return;
            var name = tbName.Text;
            var customerid = cbCustomers.SelectedItem.ToString();
            var pitchid = cbPitches.SelectedItem.ToString();
            var time = dtpDatetime.Value.ToString("dd/MM/yyyy HH:mm:ss");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(customerid)
                || string.IsNullOrEmpty(time) || string.IsNullOrEmpty(pitchid))
            {
                MessageBox.Show("Value can't be empty", "Information");
                return;
            }

            var yardrental = new YardRental
            {
                Id = _currentCustomerLocalId,
                Name = name,
                CustomerId = customerid,
                PitchId = pitchid,
                Time = time,
            };

            var fcustomer = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("YardRentals").OnceAsync<YardRental>()).FirstOrDefault(x => x.Object.Id == yardrental.Id);
            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("YardRentals").Child(fcustomer.Key).PutAsync(yardrental);

            foreach (DataGridViewRow row in dgYardRental.SelectedRows)
            {
                var localid = row.Cells[0].Value.ToString();
                if (localid == _currentCustomerLocalId)
                {
                    row.Cells[1].Value = yardrental.CustomerId;
                    row.Cells[2].Value = yardrental.PitchId;
                    row.Cells[3].Value = yardrental.Name;
                    row.Cells[4].Value = yardrental.Time;
                    break;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var csv = new StringBuilder();
                var head = string.Format("{0},{1},{2},{3},{4}", "ID", "CustomerId", "PitcheId", "Name", "Time");
                csv.AppendLine(head);

                foreach (DataGridViewRow row in dgYardRental.Rows)
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
