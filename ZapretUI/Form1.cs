using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZapretUI.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ZapretUI
{
    public partial class Form1 : Form
    {
        private bool _forceExit = true;
        private const string LocalVersionUI = "0.1.32";
        private string _localVersionZapret;
        private const string ZapretBaseName = "zapret-discord-youtube-";
        private const string GitHubZapretUrl = "https://github.com/Flowseal/zapret-discord-youtube";
        private const string GitHubUIUrl = "https://github.com/ConDucTorLehich/ZapretUI";

        //AutoUpdate 
        static System.Threading.Timer timer;
        long interval = 300000; //300 секунд
        static object synclock = new object();

        public Form1()
        {
            InitializeComponent();
            _localVersionZapret = GetInstalledVersion();

            string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (_localVersionZapret == "error")
            {
                if (MessageBox.Show(
                    "Отсутствуют файлы Zapret, нажмите \"Да\" и они будут скачаны.\nНажмите \"Нет\", и по желанию сделайте это сами",
                    "Загрузка Zapret",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    _forceExit = false;
                    Environment.Exit(1);
                }
                else
                {
                    string lastVersion = GetLastVersion(1);
                    LoadLastZapret(workDir, lastVersion);
                    _localVersionZapret = GetInstalledVersion();
                }
            }

            string zapretDirPath = Path.Combine(workDir, $"{ZapretBaseName}{_localVersionZapret}");
            var zapretDirInfo = new DirectoryInfo(zapretDirPath);

            CheckAndClearUpdates(new DirectoryInfo(workDir));

            // Initialize scripts combo box
            InitializeScriptsComboBox(zapretDirInfo);

            // Position window in bottom-right corner
            PositionWindow();

            // Modify scripts
            ModifyScripts(zapretDirInfo);

            InitAutoUpdate();

            SetLabelVersion();

        }

        private void InitializeScriptsComboBox(DirectoryInfo zapretDirInfo)
        {
            if (Settings.Default.FirstStartScriptSave == false)
            {
                FileInfo[] scriptFiles = zapretDirInfo.GetFiles("g*.bat");
                comboBox1.DataSource = scriptFiles;
                comboBox1.DisplayMember = "Name";

                System.Collections.Specialized.StringCollection coll = new System.Collections.Specialized.StringCollection();
                foreach (var item in comboBox1.Items)
                    coll.Add(item.ToString());
                //coll.AddRange(comboBox1.Items.Cast<FileInfo>().ToArray());
                Properties.Settings.Default.scriptCombo = coll;
                Settings.Default.FirstStartScriptSave = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                System.Collections.Specialized.StringCollection coll = Properties.Settings.Default.scriptCombo;
                foreach (var item in coll)
                    comboBox1.Items.Add(item);

                comboBox1.SelectedItem = Properties.Settings.Default.lastChosen;
            }
            // Properties.Settings.Default.cb1 = comboBox1.DataSource.ToString();
        }

        private void PositionWindow()
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(
                workingArea.Right - Size.Width,
                workingArea.Bottom - Size.Height);
            notifyIcon1.Visible = false;
        }

        private void ModifyScripts(DirectoryInfo zapretDirInfo)
        {
            foreach (var file in zapretDirInfo.GetFiles("g*.bat"))
            {
                string text = File.ReadAllText(file.FullName);
                text = text
                    .Replace("/min", "/B")
                    .Replace("\ncall service.bat check_updates", "\n::call service.bat check_updates");
                File.WriteAllText(file.FullName, text);
            }
        }

        private void CheckAndClearUpdates(DirectoryInfo zapretDirInfo)
        {
            bool updated = false;
            // Use single loop for both file types
            foreach (var file in zapretDirInfo.GetFiles())
            {
                if (file.Name == "ZapretUIUpdate.exe" || file.Name == "ZapretUIUpdate.bat")
                {
                    file.Delete();
                    updated = true;
                }
            }
            if (updated) MessageBox.Show("Обновление интерфейса прошло успешно!");
        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            UpdateStatus("Starting...");
            SetButtonsEnabled(false);
            await StartScript();
            SetButtonsEnabled(true);
            buttonStart.Enabled = false;
        }

        private async Task StartScript()
        {
            string scriptPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"{ZapretBaseName}{_localVersionZapret}",
                comboBox1.Text);

            var startInfo = new ProcessStartInfo(scriptPath)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                Thread.Sleep(2000);
                await CrossOrTick();
                UpdateStatus("Running!");
                Properties.Settings.Default.lastChosen = comboBox1.SelectedItem.ToString();
                Properties.Settings.Default.Save();
            }
        }

        public async Task CrossOrTick()
        {
            const string youtubeUrl = "https://www.youtube.com/";
            const string discordUrl = "https://dis.gd";
            const int timeoutMs = 3000;

            try
            {
                using (var cts = new CancellationTokenSource(timeoutMs))
                using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeoutMs) })
                {

                    var youtubeTask = CheckConnectionAsync(httpClient, youtubeUrl, cts.Token);
                    var discordTask = CheckConnectionAsync(httpClient, discordUrl, cts.Token);

                    await Task.WhenAll(youtubeTask, discordTask);

                    UpdateConnectionStatus(youtubeTask.Result, discordTask.Result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке соединения: {ex.Message}");
                UpdateConnectionStatus(false, false);
            }
        }

        private async Task<bool> CheckConnectionAsync(HttpClient client, string url, CancellationToken ct)
        {
            try
            {
                var response = await client.GetAsync(url, ct);
                return response.IsSuccessStatusCode;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"Проверка {url} отменена по таймауту");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке {url}: {ex.Message}");
                return false;
            }
        }

        private void UpdateConnectionStatus(bool youtubeStatus, bool discordStatus)
        {
            // Используем Invoke для безопасного обновления UI из другого потока
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateConnectionStatus(youtubeStatus, discordStatus)));
                return;
            }

            galkaYoutube.Visible = youtubeStatus;
            crossYoutube.Visible = !youtubeStatus;
            galkaDiscord.Visible = discordStatus;
            crossDiscord.Visible = !discordStatus;
        }

        private void UpdateStatus(string status)
        {
            labelStatus.Text = $"Status: {status}";
        }

        private async void buttonUpd_Click(object sender, EventArgs e)
        {
            bool wasRunning = labelStatus.Text == "Status: Running!";

            SetButtonsEnabled(false);
            UpdateStatus("Updating...");

            await CrossOrTick();

            UpdateStatus(wasRunning ? "Running!" : "Stopped");
            SetButtonsEnabled(true);
        }

        private void SetButtonsEnabled(bool enabled)
        {
            buttonStart.Enabled = enabled;
            buttonCheckUpd.Enabled = enabled;
            buttonUpd.Enabled = enabled;
            buttonStop.Enabled = enabled;
            comboBox1.Enabled = enabled;
        }

        public async Task<string> StopScript()
        {
            const string command = "chcp 437 && taskkill /f /t /im winws.exe && " +
                                 "net stop \"WinDivert\" && sc delete \"WinDivert\" " +
                                 "&& net stop \"WinDivert14\" && sc delete \"WinDivert14\"";

            var stopScriptInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @"/C " + command,
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = new Process { StartInfo = stopScriptInfo })
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit());
                return output;
            }
        }

        private async void buttonStop_Click(object sender, EventArgs e)
        {
            await StopScript();
            UpdateStatus("Stopping...");

            // Reset all connection indicators
            galkaDiscord.Visible = galkaYoutube.Visible = false;
            crossDiscord.Visible = crossYoutube.Visible = false;

            UpdateStatus("Stopped");
            buttonStart.Enabled = true;
            buttonStart.Visible = true;
            buttonReboot.Visible = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                if (_forceExit && MessageBox.Show(
                "Вы собираетесь закрыть приложение?\n(Все скрипты и обходы будут завершены)",
                "РКН не сосать?",
                MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private async void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_forceExit)
            {
                await StopScript();

            }

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) => ShowMainForm();
        private void toolStripMenuItem1_Click(object sender, EventArgs e) => ShowMainForm();

        private void ShowMainForm()
        {
            notifyIcon1.Visible = false;
            this.ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e) => Close();

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
            SetButtonsEnabled(false);
            UpdateStatus("Rebooting...");

            await StopScript();
            await StartScript();

            SetButtonsEnabled(true);
            buttonStart.Enabled = false;
            buttonStart.Visible = true;
            buttonReboot.Enabled = false;
            buttonReboot.Visible = false;
        }

        private void buttonCheckUpd_Click(object sender, EventArgs e)
        {
            string gitZapretVersion = GetLastVersion(1);
            string gitUIVersion = GetLastVersion(2);

            if (gitZapretVersion != _localVersionZapret)
            {
                if (MessageBox.Show("Вышел патч на скрипты Zapret\nОбновиться?", "Update...", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    StopScript();
                    Properties.Settings.Default.FirstStartScriptSave = false;
                    Properties.Settings.Default.Save();
                    string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    Directory.Delete(Path.Combine(workDir, $"{ZapretBaseName}{_localVersionZapret}"), true);

                    LoadLastZapret(workDir, gitZapretVersion);
                    _forceExit = false;
                    Application.Restart();
                }
            }
            else if (gitUIVersion != LocalVersionUI)
            {
                if (MessageBox.Show("Вышел патч на GUI\nОбновиться?", "Update...", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    StopScript();
                    File.Move("ZapretUI.exe", "ZapretUIUpdate.exe");
                    UpdateSelf(gitUIVersion);
                }
            }
            else
            {
                MessageBox.Show($"Обновы нет.\nGit_Zapret: {gitZapretVersion}\nGit_UI: {gitUIVersion}");
            }
        }

        public void InitAutoUpdate()
        {
            timer = new System.Threading.Timer(new TimerCallback(UpdateCheckingAuto), null, 0, interval);
        }

        private void UpdateCheckingAuto(object obj)
        {
            lock (synclock)
            {
                string gitZapretVersion = GetLastVersion(1);
                string gitUIVersion = GetLastVersion(2);
                if (gitZapretVersion != _localVersionZapret || gitUIVersion != LocalVersionUI)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        updLabel.Visible = true;
                        updArrowLabel.Visible = true;
                    });
                }

            }
        }

        public string GetInstalledVersion()
        {
            const string versionPrefix = "set \"LOCAL_VERSION=";

            try
            {
                string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var directories = new DirectoryInfo(workDir).GetDirectories($"{ZapretBaseName}*");

                if (directories.Length == 0)
                    return "error";

                string serviceBatPath = Path.Combine(directories[0].FullName, "service.bat");
                string text = File.ReadAllText(serviceBatPath);

                int startIndex = text.IndexOf(versionPrefix) + versionPrefix.Length;
                int endIndex = text.IndexOf('"', startIndex);

                return text.Substring(startIndex, endIndex - startIndex).Trim();
            }
            catch
            {
                return "error";
            }
        }

        private string GetLastVersion(int type)
        {
            using (var wc = new WebClient())
            {
                switch (type)
                {
                    case 1: return wc.DownloadString($"{GitHubZapretUrl}/raw/main/.service/version.txt");
                    case 2: return wc.DownloadString($"{GitHubUIUrl}/raw/master/ZapretUI/versionUI.txt");
                    default: return "error";
                }
            }
        }

        public void DownloadFile(string url, string destinationPath)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, destinationPath);
            }
        }

        public void LoadLastZapret(string workDir, string version)
        {
            string zipName = Path.Combine(workDir, $"{ZapretBaseName}{version}.zip");
            string downloadLink = $"{GitHubZapretUrl}/releases/download/{version}/{ZapretBaseName}{version}.zip";

            DownloadFile(downloadLink, zipName);
            ZipFile.ExtractToDirectory(zipName, Path.Combine(workDir, $"{ZapretBaseName}{version}"), Encoding.GetEncoding(866));

            MessageBox.Show("Скрипты Zapret успешно скачаны!");
            File.Delete(zipName);
        }

        private void SetLabelVersion()
        {
            labelVersion.Text = $"Zapret: {_localVersionZapret}\nApp GUI: {LocalVersionUI}";
        }

        public void UpdateSelf(string newVersion)
        {
            string workDir = Assembly.GetExecutingAssembly().Location;
            string selfWithoutExt = Path.Combine(
                Path.GetDirectoryName(workDir),
                Path.GetFileNameWithoutExtension(workDir));

            string downloadUrl = $"{GitHubUIUrl}/releases/download/{newVersion}/ZapretUI.exe";
            DownloadFile(downloadUrl, workDir);

            // Create update batch file
            File.WriteAllText($"{selfWithoutExt}Update.bat",
                "@ECHO OFF\n" +
                "TIMEOUT /t 2 /nobreak > NUL\n" +
                $"DEL \"{selfWithoutExt}Update.exe\" & START \"\" /B \"{workDir}\"");

            var startInfo = new ProcessStartInfo($"{selfWithoutExt}Update.bat")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(workDir)
            };

            Process.Start(startInfo);
            Environment.Exit(0);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы желаете запустить Discord?", "Discord Logo Clicked", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string[] allFoundFiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\..\\Local\\Discord\\", "Discord.exe", SearchOption.AllDirectories);
                Process.Start(allFoundFiles[0]);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы желаете запустить YouTube в браузере по умолчанию?", "YouTube Logo Clicked", MessageBoxButtons.YesNo) == DialogResult.Yes)
                Process.Start("http://youtube.com");
        }
    }
}