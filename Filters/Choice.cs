using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Choice : Form
    {
        private bool _answer;

        public Choice()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _answer = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _answer = false;

            MathMorphols.creatMask(false, null, 0);
            Close();
        }

        public bool Answer
        {
            get => _answer;
        }
    }
}
