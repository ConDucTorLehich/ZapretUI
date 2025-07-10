using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


namespace ZapretUI
{
    public partial class Form1 : Form
    {
        bool forceExit = false;
        string localVersionUI = "0.9.9";
        string localVersionZapret = "1.8.2";
        public Form1()
        {

            InitializeComponent();


            string dirWorkPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);//@"C:\Users\fedko\OneDrive\������� ����\172b";
            DirectoryInfo d = new DirectoryInfo(dirWorkPath);//Assuming Test is your Folder

            //Проверка установленных скриптов запрета
            DirectoryInfo[] directories = d.GetDirectories("zapret-discord-youtube-*");
            if (directories.Length == 0)
            {
                string zipName = dirWorkPath + "/zapret-discord-youtube-" + localVersionZapret + ".zip";
                string downloadLink = "https://github.com/Flowseal/zapret-discord-youtube/releases/download/" + localVersionZapret + "/zapret-discord-youtube-" + localVersionZapret + ".zip";
                DownloadFile(downloadLink, zipName);
                MessageBox.Show("FlowSeal Zapret успешно скачан!");
                ZipFile.ExtractToDirectory(zipName, dirWorkPath + "/zapret-discord-youtube-" + localVersionZapret);
                MessageBox.Show("Запускаемся!");
                File.Delete(zipName);
            }

            string dirZapret = dirWorkPath + "/zapret-discord-youtube-" + localVersionZapret;
            DirectoryInfo zapretDirInfo = new DirectoryInfo(dirZapret);
            FileInfo[] Files = zapretDirInfo.GetFiles("g*.bat"); //Getting Text files


            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width,
                                      workingArea.Bottom - Size.Height);
            notifyIcon1.Visible = false;

            comboBox1.DataSource = Files;
            comboBox1.DisplayMember = "Name";

            foreach (var item in Files)
            {
                string fileName = item.FullName;
                string text = File.ReadAllText(fileName);
                text = text.Replace("/min", "/B");
                text = text.Replace("\ncall service.bat check_updates", "\n::call service.bat check_updates");      //disabling autoUpd from flowseal scripts
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
        }

        private async Task startScript()
        {
            string script, scriptPath;
            script = comboBox1.Text;

            scriptPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/zapret-discord-youtube-" + localVersionZapret + "/" + script;
            MessageBox.Show(scriptPath);
            ProcessStartInfo zapretProcessInfo = new ProcessStartInfo(scriptPath)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
            };

            Process start = new Process();
            start.StartInfo = zapretProcessInfo;
            start.Start();

            await CrossOrTick();
            labelStatus.Text = "Status: Running!";
        }


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
            bool statusYouTube, statusDiscord;
            string urlYouTube = "https://www.youtube.com/";
            string urlDiscord = "https://dis.gd";
            statusYouTube = await CheckForInternetConnection(5000, urlYouTube);
            statusDiscord = await CheckForInternetConnection(5000, urlDiscord);

            if (statusYouTube) { galkaYoutube.Visible = true; crossYoutube.Visible = false; } else { crossYoutube.Visible = true; galkaYoutube.Visible = false; }
            if (statusDiscord) { galkaDiscord.Visible = true; crossDiscord.Visible = false; } else { crossDiscord.Visible = true; galkaDiscord.Visible = false; }

        }

        private async void buttonUpd_Click(object sender, EventArgs e)
        {
            bool runnig = false;
            if (labelStatus.Text == "Status: Running!") { runnig = true; }
            labelStatus.Text = labelStatus.Text + "                          Updating...";
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
            if (!forceExit)
            {
                if (MessageBox.Show("Вы собираетесь закрыть приложение?\n(Все скрипты и обходы будут завершены)", "РКН не сосать?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                    e.Cancel = false;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!forceExit) ScriptStop();

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // проверяем наше окно, и если оно было свернуто, делаем событие        
            if (WindowState == FormWindowState.Minimized)
            {
                // прячем наше окно из панели
                this.ShowInTaskbar = false;
                // делаем нашу иконку в трее активной
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
            // делаем нашу иконку скрытой
            notifyIcon1.Visible = false;
            // возвращаем отображение окна в панели
            this.ShowInTaskbar = true;
            //разворачиваем окно
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
            }

        }

        private async void buttonReboot_Click(object sender, EventArgs e)
        {
            labelStatus.Text += "                 Rebooting...";
            string output = await ScriptStop();
            await startScript();

            buttonStart.Enabled = false;
            buttonStart.Visible = true;
            buttonReboot.Enabled = false;
            buttonReboot.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string GIT_VersionZapret;
            using (var wc = new System.Net.WebClient())
                GIT_VersionZapret = wc.DownloadString("https://raw.githubusercontent.com/Flowseal/zapret-discord-youtube/main/.service/version.txt");

            MessageBox.Show("Git_Zapret:" + GIT_VersionZapret);
        }


        public void DownloadFile(string url, string destinationPath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, destinationPath);
            }
        }
    }
}

