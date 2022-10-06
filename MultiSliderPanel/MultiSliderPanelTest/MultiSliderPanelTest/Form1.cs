using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MultiSliderPanelTest.Properties;

namespace MultiSliderPanelTest {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            msPanel.Dock = DockStyle.Fill;
            msPanel.Main = mainPanel;
            msPanel.AddSlider(panel1, true);
            msPanel.AddSlider(panel2, false);
            msPanel.AddSlider(panel3, true);
            msPanel.AddSlider(panel4, true);
            int count = 1;
            for (int i = 0; i < 100; ++i) {
                int r = dgvData.Rows.Add(1);
                var row = dgvData.Rows[r];
                foreach (var col in dgvData.Columns.Cast<DataGridViewColumn>()) {
                    row.Cells[col.Index].Value = "main " + count++;
                }
                count -= dgvData.ColumnCount;
                r = dgvSubData.Rows.Add(1);
                row = dgvSubData.Rows[r];
                foreach (var col in dgvSubData.Columns.Cast<DataGridViewColumn>()) {
                    row.Cells[col.Index].Value = "sub " + count++;
                }
            }
            var box = new Button {
                                     Anchor = AnchorStyles.Right | AnchorStyles.Top,
                                     BackColor = Color.Transparent,
                                     BackgroundImage = Resources.redX,
                                     BackgroundImageLayout = ImageLayout.Stretch,
                                     FlatStyle = FlatStyle.Flat,
                                     ForeColor = Color.Transparent,
                                     Location = new Point(Width - 20, 0),
                                     Name = "btnCloseBox",
                                     Size = new Size(20, 20),
                                     UseVisualStyleBackColor = false,
                                     Visible = true
                                 };
            box.MouseEnter += (o, args) => ((Button)o).BackgroundImage = Resources.redX_hilite;
            box.MouseLeave += (o, args) => ((Button)o).BackgroundImage = Resources.redX;
            box.Click += (sender, args) => Close();
            msPanel.Controls.Add(box);
            box.BringToFront();
            box = new Button {
                                 Anchor = AnchorStyles.Right | AnchorStyles.Top,
                                 BackColor = Color.Transparent,
                                 BackgroundImage = Resources.minimize,
                                 BackgroundImageLayout = ImageLayout.Stretch,
                                 FlatStyle = FlatStyle.Flat,
                                 ForeColor = Color.Transparent,
                                 Location = new Point(Width - 40, 0),
                                 Name = "btnCloseBox",
                                 Size = new Size(20, 20),
                                 UseVisualStyleBackColor = false,
                                 Visible = true
                             };
            box.MouseEnter += (o, args) => ((Button)o).BackgroundImage = Resources.minimize_hilite;
            box.MouseLeave += (o, args) => ((Button)o).BackgroundImage = Resources.minimize;
            box.Click += (sender, args) => WindowState = FormWindowState.Minimized;
            msPanel.Controls.Add(box);
            box.BringToFront();
        }
        private void back_Click(object sender, EventArgs e) {
            msPanel.SlideBack();
        }
        private void button1_Click(object sender, EventArgs e) {
            msPanel.SlideTo(panel1);
        }
        private void button2_Click(object sender, EventArgs e) {
            msPanel.SlideTo(panel2);
        }
        private void button4_Click(object sender, EventArgs e) {
            msPanel.SlideTo(panel4);
        }
        private void button9_Click(object sender, EventArgs e) {
            msPanel.SlideTo(panel3);
        }
        private void chkPanel1NoBitmap_CheckedChanged(object sender, EventArgs e) {
            msPanel.SetNoBitmapDrawing(panel1, chkPanel1NoBitmap.Checked);
        }
        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            new Thread(() => {
                           Thread.Sleep(3000);
                           msPanel.SlideMain(true);
                       }).Start();
        }
    }
}