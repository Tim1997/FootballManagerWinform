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
    public partial class PitchesView : UserControl
    {
        protected Firebase.Database.FirebaseClient _firebaseDatabase;
        protected Firebase.Auth.FirebaseAuthProvider _firebaseAuthProvider;
        private string _currentPitcheLocalId;

        public PitchesView()
        {
            InitializeComponent();

            Init();
            LoadPitches();
        }

        void Init()
        {
            _firebaseDatabase = new Firebase.Database.FirebaseClient(Constants.BaseConstant._realTimeData);
            _firebaseAuthProvider = new Firebase.Auth.FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(Constants.BaseConstant._auth));

            cbStatus.DataSource = new string[] { "using", "is empty" };
            cbType.DataSource = new string[] { "normal", "vip", "supper vip" };

            cbStatus.SelectedIndex = -1;
            cbType.SelectedIndex = -1;

            dgPitches.ColumnCount = 3;
            dgPitches.ColumnHeadersVisible = true;
            dgPitches.Columns[0].Name = "Id";
            dgPitches.Columns[1].Name = "Status";
            dgPitches.Columns[2].Name = "Type";
        }

        async void LoadPitches()
        {
            var lpitche = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Pitches").OnceAsync<Pitch>());
            this.dgPitches.Rows.Clear();

            foreach (var item in lpitche)
            {
                this.dgPitches.Rows.Add(new string[]
                { item.Object.Id, item.Object.Status, item.Object.Type });
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var selectStatus = cbStatus.SelectedItem.ToString();
            var selectType = cbType.SelectedItem.ToString();

            if (string.IsNullOrEmpty(selectStatus) || string.IsNullOrEmpty(selectType))
            {
                MessageBox.Show("Value can't be empty", "Information");
                return;
            }

            var pitch = new Pitch
            {
                Id = Guid.NewGuid().ToString(),
                Status = selectStatus,
                Type = selectType,
            };

            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Pitches").PostAsync(pitch);

            this.dgPitches.Rows.Add(new string[]
                { pitch.Id, pitch.Status, pitch.Type });
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_currentPitcheLocalId == null) return;
            var selectStatus = cbStatus.SelectedItem.ToString();
            var selectType = cbType.SelectedItem.ToString();

            if (string.IsNullOrEmpty(selectStatus) || string.IsNullOrEmpty(selectType))
            {
                MessageBox.Show("Value can't be empty", "Information");
                return;
            }

            var pitch = new Pitch
            {
                Id = _currentPitcheLocalId,
                Status = selectStatus,
                Type = selectType,
            };

            var fpitch = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Pitches").OnceAsync<Pitch>()).FirstOrDefault(x => x.Object.Id == pitch.Id);
            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Pitches").Child(fpitch.Key).PutAsync(pitch);

            foreach (DataGridViewRow row in dgPitches.SelectedRows)
            {
                var localid = row.Cells[0].Value.ToString();
                if (localid == _currentPitcheLocalId)
                {
                    row.Cells[1].Value = pitch.Status;
                    row.Cells[2].Value = pitch.Type;
                    break;
                }
            }
        }

        private async void btnRemove_Click(object sender, EventArgs e)
        {
            if (_currentPitcheLocalId == null) return;
            var fpitch = (await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Pitches").OnceAsync<Pitch>()).FirstOrDefault(x => x.Object.Id == _currentPitcheLocalId);
            await _firebaseDatabase.Child(SigninPage._currentLocalId).Child("Pitches").Child(fpitch.Key).DeleteAsync();

            foreach (DataGridViewRow row in dgPitches.SelectedRows)
            {
                var localid = row.Cells[0].Value.ToString();
                if (localid == _currentPitcheLocalId)
                {
                    dgPitches.Rows.Remove(row);
                    break;
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var csv = new StringBuilder();
                var head = string.Format("{0},{1},{2}", "ID", "Status", "Type");
                csv.AppendLine(head);

                foreach (DataGridViewRow row in dgPitches.Rows)
                {
                    var id = row.Cells[0].Value.ToString();
                    var status = row.Cells[1].Value.ToString();
                    var type = row.Cells[2].Value.ToString();

                    var newLine = string.Format("{0},{1},{2}", id, status, type);
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

        private void dgPitches_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgPitches.SelectedRows[0];
            _currentPitcheLocalId = row.Cells[0].Value.ToString();
            cbStatus.SelectedItem = row.Cells[1].Value.ToString();
            cbType.SelectedItem = row.Cells[2].Value.ToString();
        }
    }
}
