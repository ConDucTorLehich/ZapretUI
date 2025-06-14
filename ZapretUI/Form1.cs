using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ZapretUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            notifyIcon1.Visible = false;

            string dirPath = @"C:\Users\fedko\OneDrive\������� ����\172b";
            //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DirectoryInfo d = new DirectoryInfo(dirPath);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("g*.bat"); //Getting Text files
            comboBox1.DataSource = Files;
            comboBox1.DisplayMember = "Name";

            foreach (var item in Files)
            {
                string fileName = item.FullName;
                string text = File.ReadAllText(fileName);
                text = text.Replace("/min", "/B");
                text = text.Replace("\ncall service", "\n::call service");      //disabling autoUpd
                File.WriteAllText(fileName, text);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            labelStatus.Text = labelStatus.Text + "                  Starting...";
            await startScript();
            buttonStart.Enabled = false;
            // MessageBox.Show("ABOAB satrted");
        }

        private async Task startScript()
        {
            string script, scriptPath;
            script = comboBox1.Text;
            scriptPath = @"C:\Users\fedko\OneDrive\������� ����\172b\" + script;
            ProcessStartInfo zapretProcessInfo = new ProcessStartInfo(scriptPath)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };

            Process.Start(zapretProcessInfo);
            //Thread.Sleep(3000);

            // Process.Start(@"C:\Users\fedko\OneDrive\������� ����\172b\" + script);

            await CrossOrTick();
            labelStatus.Text = "Status: Running!";
            notifyIcon1.Icon = new Icon(GetType(), "runningICO");
        }

        /* �������� �������� ����������*/
        public static Task<bool> CheckForInternetConnection(int timeoutMs, string url)
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.KeepAlive = false;
                    request.Timeout = timeoutMs;
                    using (var response = (HttpWebResponse)request.GetResponse())
                        return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task CrossOrTick()
        {
            // �������� ����������� ��������
            bool statusYouTube, statusDiscord;
            string urlYouTube = "https://www.youtube.com/";
            string urlDiscord = "https://dis.gd";
            statusYouTube = await CheckForInternetConnection(5000, urlYouTube);
            statusDiscord = await CheckForInternetConnection(5000, urlDiscord);

            // ��������� ������� � ���������
            if (statusYouTube) { galkaYoutube.Visible = true; crossYoutube.Visible = false; } else { crossYoutube.Visible = true; galkaYoutube.Visible = false; }
            if (statusDiscord) { galkaDiscord.Visible = true; crossDiscord.Visible = false; } else { crossDiscord.Visible = true; galkaDiscord.Visible = false; }
            //if (statusDiscord&&statusYouTube) { return true; } else { return false; }
        }

        private async void buttonUpd_Click(object sender, EventArgs e)
        {
            bool runnig = false;
            if (labelStatus.Text == "Status: Running!") { runnig = true; }
            labelStatus.Text = labelStatus.Text + "                  Updating...";
            await CrossOrTick();
            if (runnig == true)
            {
                labelStatus.Text = "Status: Running!";
            }
            else
            {
                labelStatus.Text = "Status: Stopped";
            }
        }

        public async Task<string> ScriptStop()
        {
            //int i = 0;
            string command = "chcp 437 && taskkill /f /t /im winws.exe && " +
                     "net stop \"WinDivert\" && sc delete \"WinDivert\" " +
                     "&& net stop \"WinDivert14\" && sc delete \"WinDivert14\"";
            ProcessStartInfo stopScriptInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @"/C " + command,
                Verb = "runas",
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                //RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,

            };
            notifyIcon1.Icon = new Icon(GetType(), "stoppedICO");
            // MessageBox.Show("ABOAB " + stopScriptInfo.Arguments);

            using (Process process = new Process { StartInfo = stopScriptInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output;
            }
        }

        private async void buttonStop_Click(object sender, EventArgs e)
        {
            string output = await ScriptStop();
            labelStatus.Text += "                  Stopping...";
            galkaDiscord.Visible = false;
            galkaYoutube.Visible = false;
            crossDiscord.Visible = false;
            crossYoutube.Visible = false;
            //await CrossOrTick();
            labelStatus.Text = "Status: Stopped";
            buttonStart.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("�� ����������� ������� ����������?\n(��� ������� � ������ ����� ���������)", "��� �� ������?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                e.Cancel = true;
            }
            else
                e.Cancel = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ScriptStop();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // ��������� ���� ����, � ���� ��� ���� ��������, ������ �������        
            if (WindowState == FormWindowState.Minimized)
            {
                // ������ ���� ���� �� ������
                this.ShowInTaskbar = false;
                // ������ ���� ������ � ���� ��������
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            showMainForm();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showMainForm();
        }

        private void showMainForm()
        {
            // ������ ���� ������ �������
            notifyIcon1.Visible = false;
            // ���������� ����������� ���� � ������
            this.ShowInTaskbar = true;
            //������������� ����
            WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (labelStatus.Text != "Status: Stopped")
            {
                buttonStart.Enabled = false;
                buttonStart.Visible = false;
                buttonReboot.Enabled = true;
                buttonReboot.Visible = true;
                //MessageBox.Show("123");
            }

        }

        private async void buttonReboot_Click(object sender, EventArgs e)
        {
            labelStatus.Text += "                 Rebooting...";
            string output = await ScriptStop();
            await startScript();
            // await CrossOrTick();
            buttonStart.Enabled = false;
            buttonStart.Visible = true;
            buttonReboot.Enabled = false;
            buttonReboot.Visible = false;
        }
    }
}

