using FootballManager.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FootballManager
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var customers = new CustomersView();
            pContent.Controls.Clear();
            pContent.Controls.Add(customers);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var pitches = new PitchesView();
            pContent.Controls.Clear();
            pContent.Controls.Add(pitches);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var yardrental = new YardRentalView();
            pContent.Controls.Clear();
            pContent.Controls.Add(yardrental);
        }
    }
}
