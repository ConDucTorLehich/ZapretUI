using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZapretUI.Properties;


namespace ZapretUI
{
    /// <summary>
    /// Главная форма приложения ZapretUI - графический интерфейс для управления Zapret
    /// </summary>
    public partial class Form1 : Form
    {
        #region Константы и настройки

        private const string LocalVersionUI = "0.1.7";
        private const string ZapretBaseName = "zapret-discord-youtube-";
        private const string GitHubZapretUrl = "https://github.com/Flowseal/zapret-discord-youtube";
        private const string GitHubUIUrl = "https://github.com/ConDucTorLehich/ZapretUI";
        private const long AutoUpdateIntervalMs = 300_000; // 5 минут
        private const int ConnectionTimeoutMs = 3000;
        private const int ProcessCheckDelayMs = 2000;

        #endregion

        #region Перечисления

        /// <summary>
        /// Тип версии для проверки обновлений
        /// </summary>
        private enum VersionType { Zapret = 1, UI = 2 }

        #endregion

        #region Поля класса

        // Состояние приложения
        private string _localVersionZapret;
        private string _selectedScript;
        private bool _forceExit = true;
        private bool _isClosing = false;
        private bool _isScriptRunning = false;

        // Общие ресурсы
        private static readonly HttpClient _sharedHttpClient;
        private static readonly object _syncLock = new object();
        private static System.Threading.Timer _updateTimer;
        private readonly CancellationTokenSource _globalCts = new CancellationTokenSource();

        #endregion

        #region Статический конструктор

        /// <summary>
        /// Статический конструктор для инициализации общих ресурсов
        /// </summary>
        static Form1()
        {
            // Настраиваем общий HttpClient для всего приложения
            _sharedHttpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            {
                Timeout = TimeSpan.FromMilliseconds(ConnectionTimeoutMs),
                DefaultRequestHeaders =
                {
                    { "User-Agent", "ZapretUI/1.0" },
                    { "Accept", "text/plain, application/json" }
                }
            };
        }

        #endregion

        #region Конструктор и инициализация

        /// <summary>
        /// Конструктор главной формы
        /// </summary>
        public Form1()
        {
            // Устанавливаем DPI-осознание до инициализации компонентов
            SetHighDpiMode();

            InitializeComponent();

            // Включаем подробное логирование для отладки
            Debug.WriteLine("[Form1] Конструктор начал работу");

            // Настраиваем форму
            InitializeForm();

            // Инициализируем приложение
            InitializeApplication();

            Debug.WriteLine("[Form1] Конструктор завершил работу");
        }

        // <summary>
        // Инициализация DPI режима для поддержки высоких разрешений
        // </summary>
        private void SetHighDpiMode()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();


        /// <summary>
        /// Настройка формы
        /// </summary>
        private void InitializeForm()
        {
            // Устанавливаем иконку приложения
            try
            {
                this.Icon = Properties.Resources.AppIcon; // Добавьте иконку в Resources
            }
            catch
            {
                this.Icon = SystemIcons.Application;
            }

            // Устанавливаем минимальные размеры
            //this.MinimumSize = new Size(100, 100); //294; 556

            // Настраиваем двойную буферизацию для уменьшения мерцания
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.DoubleBuffer, true);

            // Регистрируем обработчик сообщений
            Application.ApplicationExit += Application_ApplicationExit;
        }

        /// <summary>
        /// Основная инициализация приложения
        /// </summary>
        private void InitializeApplication()
        {
            try
            {
                Debug.WriteLine("[InitializeApplication] Начало инициализации");

                // Получаем версию установленного Zapret
                _localVersionZapret = GetInstalledVersion();
                Debug.WriteLine($"[InitializeApplication] Версия Zapret: {_localVersionZapret}");

                // Если Zapret не установлен, предлагаем установку
                if (_localVersionZapret == "error")
                {
                    Debug.WriteLine("[InitializeApplication] Zapret не установлен");
                    HandleMissingZapret();
                    return;
                }

                string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Debug.WriteLine($"[InitializeApplication] Рабочая директория: {workDir}");

                string zapretDirPath = Path.Combine(workDir, $"{ZapretBaseName}{_localVersionZapret}");
                Debug.WriteLine($"[InitializeApplication] Путь к Zapret: {zapretDirPath}");

                // Проверяем существование директории
                if (!Directory.Exists(zapretDirPath))
                {
                    Debug.WriteLine($"[InitializeApplication] Директория не существует: {zapretDirPath}");
                    ShowErrorMessage($"Директория Zapret не найдена:\n{zapretDirPath}",
                        "Ошибка инициализации");
                    _forceExit = false;
                    SafeExit(1);
                    return;
                }

                var zapretDirInfo = new DirectoryInfo(zapretDirPath);
                Debug.WriteLine($"[InitializeApplication] Найдено файлов в директории: {zapretDirInfo.GetFiles().Length}");

                // Очищаем файлы обновления
                CheckAndClearUpdates(new DirectoryInfo(workDir));

                // Инициализируем ComboBox со скриптами
                Debug.WriteLine("[InitializeApplication] Начало инициализации ComboBox");
                InitializeScriptsComboBox(zapretDirInfo);
                Debug.WriteLine("[InitializeApplication] ComboBox инициализирован");

                // Позиционируем окно
                PositionWindow();

                // Модифицируем скрипты
                ModifyScripts(zapretDirInfo);

                // Инициализируем автообновление
                InitAutoUpdate();

                // Устанавливаем метку версии
                SetLabelVersion();

                // Загружаем настройки
                LoadSettings();

                // Обновляем статус
                UpdateStatus();

                // Устанавливаем автозагрузку из настроек
                SetStartupFromSettings();

                // Проверяем состояние сервиса при запуске
                CheckInitialServiceState();

                Debug.WriteLine("[InitializeApplication] Инициализация завершена успешно");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InitializeApplication] Критическая ошибка: {ex.Message}");
                Debug.WriteLine($"[InitializeApplication] StackTrace: {ex.StackTrace}");

                ShowErrorMessage($"Ошибка инициализации приложения:\n{ex.Message}\n\n" +
                               $"StackTrace:\n{ex.StackTrace}",
                               "Критическая ошибка");
                _forceExit = false;
                SafeExit(1);
            }
        }

        /// <summary>
        /// Проверка состояния сервиса при запуске
        /// </summary>
        private void CheckInitialServiceState()
        {
            try
            {
                bool isRunning = Process.GetProcessesByName("winws").Length > 0;
                _isScriptRunning = isRunning;

                SafeInvoke(() =>
                {
                    buttonStart.Enabled = !isRunning;
                    buttonStop.Enabled = isRunning;

                    if (isRunning)
                    {
                        // Если сервис уже запущен, проверяем соединение
                        _ = CheckConnectionsAsync(_globalCts.Token);
                    }
                });
            }
            catch { }
        }

        #endregion

        #region Обработка отсутствия Zapret

        /// <summary>
        /// Обработка ситуации, когда Zapret не установлен
        /// </summary>
        private void HandleMissingZapret()
        {
            var result = MessageBox.Show(this,
                "Файлы Zapret не найдены.\n\n" +
                "Нажмите 'Да' для автоматической загрузки и установки.\n" +
                "Нажмите 'Нет' для выхода и ручной установки.",
                "Zapret не установлен",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (result == DialogResult.No)
            {
                _forceExit = false;
                SafeExit(1);
                return;
            }

            // Проверяем интернет соединение
            if (!IsInternetAvailable())
            {
                ShowErrorMessage(
                    "Отсутствует интернет соединение. Загрузка невозможна.\n\n" +
                    "Подключитесь к интернету и перезапустите приложение.",
                    "Ошибка соединения");
                _forceExit = false;
                SafeExit(1);
                return;
            }

            // Запускаем процесс установки
            InstallZapret();
        }

        /// <summary>
        /// Установка Zapret
        /// </summary>
        private async void InstallZapret()
        {
            try
            {
                // Показываем индикатор загрузки
                SafeInvoke(() => this.Cursor = Cursors.WaitCursor);

                string lastVersion = GetLastVersion(VersionType.Zapret);
                if (lastVersion == "error")
                {
                    ShowErrorMessage(
                        "Не удалось получить информацию о последней версии Zapret.\n" +
                        "Проверьте соединение с GitHub.",
                        "Ошибка загрузки");
                    SafeInvoke(() => this.Cursor = Cursors.Default);
                    _forceExit = false;
                    SafeExit(1);
                    return;
                }

                string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                await LoadLastZapretAsync(workDir, lastVersion, _globalCts.Token);

                _localVersionZapret = GetInstalledVersion();

                SafeInvoke(() =>
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show(this,
                        $"Zapret версии {lastVersion} успешно установлен!",
                        "Установка завершена",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                });

                // Переинициализируем приложение после установки
                InitializeApplication();
            }
            catch (OperationCanceledException)
            {
                SafeInvoke(() =>
                {
                    this.Cursor = Cursors.Default;
                    ShowErrorMessage("Установка отменена.", "Отмена");
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() =>
                {
                    this.Cursor = Cursors.Default;
                    ShowErrorMessage($"Ошибка установки Zapret:\n{ex.Message}", "Ошибка");
                });
                _forceExit = false;
                SafeExit(1);
            }
        }

        #endregion

        #region Безопасные методы работы с файлами и данными

        /// <summary>
        /// Безопасное получение установленной версии Zapret
        /// </summary>
        private string GetInstalledVersion()
        {
            const string versionPrefix = "set \"LOCAL_VERSION=";

            try
            {
                string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(workDir)) return "error";

                // Ищем директории с Zapret
                var zapretDirectories = Directory.GetDirectories(workDir, $"{ZapretBaseName}*")
                    .ToList();

                if (zapretDirectories.Count == 0) return "error";

                // Проверяем каждую директорию на наличие service.bat
                foreach (var dir in zapretDirectories)
                {
                    string serviceBatPath = Path.Combine(dir, "service.bat");
                    if (File.Exists(serviceBatPath))
                    {
                        string text = File.ReadAllText(serviceBatPath);
                        int startIndex = text.IndexOf(versionPrefix);
                        if (startIndex == -1) continue;

                        startIndex += versionPrefix.Length;
                        int endIndex = text.IndexOf('"', startIndex);
                        if (endIndex == -1) continue;

                        string version = text.Substring(startIndex, endIndex - startIndex).Trim();

                        // Проверяем, что версия соответствует имени директории
                        string dirName = Path.GetFileName(dir);
                        if (dirName.EndsWith(version))
                        {
                            return version;
                        }
                    }
                }

                return "error";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetInstalledVersionSafe] Ошибка: {ex.Message}");
                return "error";
            }
        }

        /// <summary>
        /// Безопасная инициализация списка скриптов
        /// </summary>
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

                Settings.Default.scriptCombo = coll;
                Settings.Default.FirstStartScriptSave = true;
                Settings.Default.Save();
            }
            else
            {
                System.Collections.Specialized.StringCollection coll = Settings.Default.scriptCombo;
                foreach (var item in coll)
                    comboBox1.Items.Add(item);

                comboBox1.SelectedItem = Settings.Default.lastChosen;
            }
        }

        /// <summary>
        /// Безопасная модификация скриптов
        /// </summary>
        private void ModifyScripts(DirectoryInfo zapretDirInfo)
        {
            try
            {
                foreach (var file in zapretDirInfo.GetFiles("g*.bat"))
                {
                    try
                    {
                        string text = File.ReadAllText(file.FullName);

                        // Применяем необходимые замены
                        text = text
                            .Replace("/min", "/B")
                            .Replace("\ncall service.bat check_updates", "\n::call service.bat check_updates")
                            //.Replace("\ncall service.bat load_game_filter", "\n::call service.bat load_game_filter")
                            .Replace("\ncall service.bat status_zapret", "\n::call service.bat status_zapret");

                        // Дополнительные улучшения для стабильности
                        if (!text.Contains("chcp 65001"))
                        {
                            text = "@chcp 65001 >nul\n@echo off\n" + text;
                        }

                        File.WriteAllText(file.FullName, text, Encoding.GetEncoding(1251));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ModifyScriptsSafe] Ошибка в файле {file.Name}: {ex.Message}");
                    }
                }

                Debug.WriteLine("[ModifyScriptsSafe] Скрипты успешно модифицированы");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ModifyScriptsSafe] Общая ошибка: {ex.Message}");
            }
        }

        #endregion

        #region Методы работы с UI

        /// <summary>
        /// Позиционирование окна в правом нижнем углу экрана
        /// </summary>
        private void PositionWindow()
        {
            SafeInvoke(() =>
            {
                try
                {
                    Rectangle workingArea = Screen.GetWorkingArea(this);

                    // Учитываем размер окна
                    int x = workingArea.Right - Size.Width;
                    int y = workingArea.Bottom - Size.Height;

                    // Небольшой отступ от краев
                    //x = Math.Max(x, workingArea.Left);
                    //y = Math.Max(y, workingArea.Top);

                    this.Location = new Point(x, y);

                    // Скрываем иконку в трее
                    if (notifyIcon1 != null)
                        notifyIcon1.Visible = false;
                }
                catch { }
            });
        }

        /// <summary>
        /// Установка текста версии
        /// </summary>
        private void SetLabelVersion()
        {
            SafeInvoke(() =>
            {
                labelVersion.Text = $"Zapret: {_localVersionZapret}\nApp GUI: {LocalVersionUI}";
            });
        }

        /// <summary>
        /// Обновление статуса приложения
        /// </summary>
        /// <param name="status">Статус для отображения (null для автоматического определения)</param>
        private void UpdateStatus(string status = null)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            SafeInvoke(() =>
            {
                if (!string.IsNullOrEmpty(status))
                {
                    labelStatus.Text = $"Status: {status}";
                    return;
                }

                try
                {
                    _isScriptRunning = Process.GetProcessesByName("winws").Length > 0;
                    labelStatus.Text = _isScriptRunning ? "Status: Running!" : "Status: Stopped";

                    // Обновляем состояние кнопок
                    buttonStart.Enabled = !_isScriptRunning;
                    buttonStop.Enabled = _isScriptRunning;
                }
                catch
                {
                    labelStatus.Text = "Status: Unknown";
                }
            });
        }

        /// <summary>
        /// Включение/отключение кнопок управления
        /// </summary>
        /// <param name="enabled">Состояние кнопок</param>
        private void SetButtonsEnabled(bool enabled)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            SafeInvoke(() =>
            {
                buttonStart.Enabled = enabled && !_isScriptRunning;
                buttonCheckUpd.Enabled = enabled;
                buttonUpd.Enabled = enabled;
                buttonStop.Enabled = enabled && _isScriptRunning;
                comboBox1.Enabled = enabled;
            });
        }

        /// <summary>
        /// Безопасный вызов в UI-потоке с подробным логированием
        /// </summary>
        private void SafeInvoke(Action action, bool logErrors = true)
        {
            try
            {
                if (this.IsDisposed || !this.IsHandleCreated)
                {
                    if (logErrors) Debug.WriteLine("[SafeInvoke] Форма уничтожена или handle не создан");
                    return;
                }

                if (this.InvokeRequired)
                {
                    try
                    {
                        this.Invoke(new Action(() =>
                        {
                            try
                            {
                                if (!this.IsDisposed && this.IsHandleCreated)
                                    action();
                            }
                            catch (Exception ex)
                            {
                                if (logErrors)
                                    Debug.WriteLine($"[SafeInvoke.Invoke] Ошибка в действии: {ex.Message}");
                            }
                        }));
                    }
                    catch (ObjectDisposedException)
                    {
                        if (logErrors) Debug.WriteLine("[SafeInvoke] ObjectDisposedException");
                    }
                    catch (InvalidOperationException)
                    {
                        if (logErrors) Debug.WriteLine("[SafeInvoke] InvalidOperationException");
                    }
                }
                else
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                if (logErrors) Debug.WriteLine($"[SafeInvoke] Общая ошибка: {ex.Message}");
            }
        }

        #endregion

        #region Основные операции запуска/остановки

        /// <summary>
        /// Обработчик кнопки Start
        /// </summary>
        private async void buttonStart_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                ShowErrorMessage("Выберите скрипт для запуска", "Ошибка");
                return;
            }

            SetButtonsEnabled(false);
            UpdateStatus("Starting...");

            try
            {
                await StartScriptAsync(_globalCts.Token);
                buttonStart.Enabled = false;
                _isScriptRunning = true;
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Отменено");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка запуска скрипта:\n{ex.Message}", "Ошибка запуска");
                UpdateStatus("Ошибка");
            }
            finally
            {
                SetButtonsEnabled(true);
                UpdateStatus();
            }
        }

        /// <summary>
        /// Запуск выбранного скрипта
        /// </summary>
        private async Task StartScriptAsync(CancellationToken cancellationToken)
        {
            if (comboBox1.SelectedItem == null)
                throw new InvalidOperationException("Не выбран скрипт");

            string scriptName = comboBox1.SelectedItem.ToString();
            _selectedScript = scriptName;

            string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string scriptPath = Path.Combine(
                workDir,
                $"{ZapretBaseName}{_localVersionZapret}",
                scriptName);

            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Скрипт не найден: {scriptPath}");

            // Обновляем статус в UI
            UpdateStatus("Starting...");

            // Подготавливаем информацию для запуска процесса
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"{scriptPath}\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                Verb = "runas" // Запуск от имени администратора
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                try
                {
                    process.Start();

                    // Ждем запуска процесса
                    await Task.Delay(ProcessCheckDelayMs, cancellationToken);

                    // Проверяем, запустился ли winws.exe
                    int checkCount = 0;
                    bool processStarted = false;

                    while (checkCount < 10 && !processStarted)
                    {
                        await Task.Delay(1000, cancellationToken);
                        processStarted = Process.GetProcessesByName("winws").Length > 0;
                        checkCount++;
                    }

                    if (!processStarted)
                    {
                        throw new TimeoutException("Сервис winws не запустился в течение 5 секунд");
                    }

                    // Обновляем статус
                    UpdateStatus();

                    // Проверяем соединение
                    await CheckConnectionsAsync(cancellationToken);

                    // Сохраняем выбор
                    Settings.Default.lastChosen = scriptName;
                    Settings.Default.Save();

                    Debug.WriteLine($"[StartScriptAsync] Скрипт {scriptName} успешно запущен");
                }
                catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
                {
                    // Пользователь отменил UAC
                    throw new OperationCanceledException("Запуск отменен пользователем (UAC)");
                }
                finally
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки Check
        /// </summary>
        private async void buttonUpd_Click(object sender, EventArgs e)
        {
            SetButtonsEnabled(false);
            UpdateStatus("Checking...");

            try
            {
                await CheckConnectionsAsync(_globalCts.Token);
                UpdateStatus();
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Отменено");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[buttonUpd_Click] Ошибка: {ex.Message}");
                UpdateStatus("Ошибка");
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        /// <summary>
        /// Обработчик кнопки Stop
        /// </summary>
        private async void buttonStop_Click(object sender, EventArgs e)
        {
            UpdateStatus("Stopping...");
            SetButtonsEnabled(false);

            try
            {
                await StopServicesAsync(_globalCts.Token);

                // Сбрасываем индикаторы соединения
                SafeInvoke(() =>
                {
                    galkaDiscord.Visible = galkaYoutube.Visible = false;
                    crossDiscord.Visible = crossYoutube.Visible = false;
                });

                _isScriptRunning = false;
                UpdateStatus();
                buttonStart.Enabled = true;
                buttonStart.Visible = true;
                buttonReboot.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка остановки сервисов:\n{ex.Message}", "Ошибка остановки");
                UpdateStatus("Ошибка");
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        /// <summary>
        /// Остановка всех сервисов Zapret
        /// </summary>
        private async Task<string> StopServicesAsync(CancellationToken cancellationToken)
        {
            // Команда для остановки всех сервисов
            const string command = @"
@echo off
chcp 65001 >nul
echo Остановка Zapret...

taskkill /f /t /im winws.exe >nul 2>&1
timeout /t 1 /nobreak >nul

net stop ""WinDivert"" >nul 2>&1
sc delete ""WinDivert"" >nul 2>&1

net stop ""WinDivert14"" >nul 2>&1
sc delete ""WinDivert14"" >nul 2>&1

echo Сервисы остановлены
";

            string tempBatchFile = Path.GetTempFileName() + ".bat";

            try
            {
                // Создаем временный batch-файл
                File.WriteAllText(tempBatchFile, command);

                var stopScriptInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{tempBatchFile}\"",
                    Verb = "runas", // Права администратора
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(stopScriptInfo))
                {
                    if (process != null)
                    {
                        await Task.Run(() => process.WaitForExit(), cancellationToken);
                        return "Сервисы остановлены";
                    }

                    return "Не удалось запустить процесс остановки";
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                throw new OperationCanceledException("Остановка отменена пользователем (UAC)");
            }
            finally
            {
                // Удаляем временный файл
                if (File.Exists(tempBatchFile))
                {
                    try { File.Delete(tempBatchFile); } catch { }
                }
            }
        }

        #endregion

        #region Проверка соединений

        /// <summary>
        /// Проверка доступности YouTube и Discord
        /// </summary>
        private async Task CheckConnectionsAsync(CancellationToken cancellationToken)
        {
            const string youtubeUrl = "https://www.youtube.com/";
            const string discordUrl = "https://discord.com";

            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    _globalCts.Token))
                {
                    cts.CancelAfter(ConnectionTimeoutMs);

                    var youtubeTask = CheckConnectionAsync(youtubeUrl, cts.Token);
                    var discordTask = CheckConnectionAsync(discordUrl, cts.Token);

                    await Task.WhenAll(youtubeTask, discordTask);

                    UpdateConnectionStatus(youtubeTask.Result, discordTask.Result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckConnectionsAsync] Ошибка: {ex.Message}");
                UpdateConnectionStatus(false, false);
            }
        }

        /// <summary>
        /// Проверка доступности конкретного URL
        /// </summary>
        private async Task<bool> CheckConnectionAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                // Используем HEAD запрос для экономии трафика
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    var response = await _sharedHttpClient.SendAsync(request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);

                    return response.IsSuccessStatusCode;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"[CheckConnectionAsync] Проверка {url} отменена");
                return false;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[CheckConnectionAsync] Ошибка HTTP для {url}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckConnectionAsync] Общая ошибка для {url}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Обновление индикаторов соединения
        /// </summary>
        private void UpdateConnectionStatus(bool youtubeStatus, bool discordStatus)
        {
            SafeInvoke(() =>
            {
                try
                {
                    galkaYoutube.Visible = youtubeStatus;
                    crossYoutube.Visible = !youtubeStatus;
                    galkaDiscord.Visible = discordStatus;
                    crossDiscord.Visible = !discordStatus;
                }
                catch { }
            });
        }

        #endregion

        #region Автообновление

        /// <summary>
        /// Инициализация автообновления
        /// </summary>
        private void InitAutoUpdate()
        {
            try
            {
                // Запускаем проверку в фоне
                Task.Run(() =>
                {
                    if (IsInternetAvailable())
                    {
                        _updateTimer = new System.Threading.Timer(
                            callback: _ => SafeAutoUpdateCheck(),
                            state: null,
                            dueTime: 0,
                            period: AutoUpdateIntervalMs);
                    }
                    else
                    {
                        Debug.WriteLine("[InitAutoUpdateSafe] Автообновление отключено: нет интернета");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InitAutoUpdateSafe] Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Безопасная проверка обновлений
        /// </summary>
        private void SafeAutoUpdateCheck()
        {
            try
            {
                if (_isClosing || !IsInternetAvailable())
                    return;

                lock (_syncLock)
                {
                    // Перечитываем версию на диске
                    string currentZapretVersion = GetInstalledVersion();
                    string gitZapretVersion = GetLastVersion(VersionType.Zapret);
                    string gitUIVersion = GetLastVersion(VersionType.UI);

                    bool zapretUpdateAvailable = gitZapretVersion != "error" &&
                                                gitZapretVersion != currentZapretVersion;
                    bool uiUpdateAvailable = gitUIVersion != "error" &&
                                           gitUIVersion != LocalVersionUI;

                    if (zapretUpdateAvailable || uiUpdateAvailable)
                    {
                        SafeInvoke(() =>
                        {
                            try
                            {
                                updLabel.Visible = true;
                                updArrowLabel.Visible = true;
                            }
                            catch { }
                        });

                        Debug.WriteLine($"[AutoUpdate] Доступны обновления: Zapret={zapretUpdateAvailable}, UI={uiUpdateAvailable}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SafeAutoUpdateCheck] Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение последней версии с GitHub
        /// </summary>
        private string GetLastVersion(VersionType type)
        {
            string url = type switch
            {
                VersionType.Zapret => $"{GitHubZapretUrl}/raw/main/.service/version.txt",
                VersionType.UI => $"{GitHubUIUrl}/raw/master/ZapretUI/versionUI.txt",
                _ => throw new ArgumentException("Неизвестный тип версии")
            };

            try
            {
                // Асинхронная версия с таймаутом
                var task = _sharedHttpClient.GetStringAsync(url);
                if (task.Wait(5000)) // 5 секунд таймаут
                {
                    return task.Result.Trim();
                }

                Debug.WriteLine($"[GetLastVersionSafe] Таймаут для {type}");
                return "error";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetLastVersionSafe] Ошибка для {type}: {ex.Message}");
                return "error";
            }
        }

        /// <summary>
        /// Проверка доступности интернета
        /// </summary>
        private bool IsInternetAvailable()
        {
            try
            {
                // Быстрая проверка через несколько методов
                var checkTasks = new[]
                {
                    Task.Run(() => CheckPing("1.1.1.1")),
                    Task.Run(() => CheckPing("8.8.8.8")),
                    Task.Run(() => CheckHttp("http://cloudflare.com"))
                };

                // Ждем первую успешную проверку или таймаут
                var completedTask = Task.WhenAny(checkTasks).Result;
                return completedTask.Result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка через Ping
        /// </summary>
        private bool CheckPing(string host)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(host, 2000);
                    return reply?.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка через HTTP
        /// </summary>
        private bool CheckHttp(string url)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    var response = _sharedHttpClient.SendAsync(request,
                        new CancellationTokenSource(2000).Token).Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Ручное обновление

        /// <summary>
        /// Обработчик кнопки Updates
        /// </summary>
        private async void buttonCheckUpd_Click(object sender, EventArgs e)
        {
            SetButtonsEnabled(false);

            try
            {
                // Показываем индикатор загрузки
                SafeInvoke(() => this.Cursor = Cursors.WaitCursor);

                string gitZapretVersion = GetLastVersion(VersionType.Zapret);
                string gitUIVersion = GetLastVersion(VersionType.UI);

                SafeInvoke(() => this.Cursor = Cursors.Default);

                if (gitZapretVersion == "error" && gitUIVersion == "error")
                {
                    ShowErrorMessage(
                        "Не удалось проверить обновления.\nПроверьте интернет-соединение.",
                        "Ошибка проверки");
                    return;
                }

                bool zapretUpdateNeeded = gitZapretVersion != "error" &&
                                         gitZapretVersion != _localVersionZapret;
                bool uiUpdateNeeded = gitUIVersion != "error" &&
                                     gitUIVersion != LocalVersionUI;

                // Предлагаем обновления по порядку
                if (zapretUpdateNeeded)
                {
                    await HandleZapretUpdate(gitZapretVersion);
                }
                else if (uiUpdateNeeded)
                {
                    await HandleUiUpdate(gitUIVersion);
                }
                else
                {
                    ShowInformationMessage(
                        $"Обновлений не найдено.\n\nТекущие версии:\n• Zapret: {_localVersionZapret}\n• Интерфейс: {LocalVersionUI}",
                        "Проверка обновлений");
                }
            }
            catch (Exception ex)
            {
                SafeInvoke(() => this.Cursor = Cursors.Default);
                ShowErrorMessage($"Ошибка при проверке обновлений:\n{ex.Message}", "Ошибка");
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        /// <summary>
        /// Обработка обновления Zapret
        /// </summary>
        private async Task HandleZapretUpdate(string newVersion)
        {
            var result = MessageBox.Show(this,
                $"Доступно обновление Zapret!\n\n" +
                $"Текущая версия: {_localVersionZapret}\n" +
                $"Новая версия: {newVersion}\n\n" +
                "Обновить сейчас?",
                "Обновление Zapret",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (result != DialogResult.Yes)
                return;

            SetButtonsEnabled(false);
            UpdateStatus("Updating Zapret...");

            try
            {
                // Останавливаем сервисы перед обновлением
                await StopServicesAsync(_globalCts.Token);

                // Сбрасываем настройки скриптов
                Settings.Default.FirstStartScriptSave = false;
                Settings.Default.Save();

                // Удаляем старую версию
                string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string oldDir = Path.Combine(workDir, $"{ZapretBaseName}{_localVersionZapret}");

                if (Directory.Exists(oldDir))
                {
                    await Task.Run(() => Directory.Delete(oldDir, true));
                }

                // Загружаем новую версию
                await LoadLastZapretAsync(workDir, newVersion, _globalCts.Token);

                // Перезапускаем приложение
                _forceExit = false;
                Application.Restart();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка обновления Zapret:\n{ex.Message}", "Ошибка обновления");
                UpdateStatus("Ошибка");
                SetButtonsEnabled(true);
            }
        }

        /// <summary>
        /// Обработка обновления UI
        /// </summary>
        private async Task HandleUiUpdate(string newVersion)
        {
            var result = MessageBox.Show(this,
                $"Доступно обновление интерфейса!\n\n" +
                $"Текущая версия: {LocalVersionUI}\n" +
                $"Новая версия: {newVersion}\n\n" +
                "Обновить сейчас?",
                "Обновление интерфейса",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (result != DialogResult.Yes)
                return;

            SetButtonsEnabled(false);
            UpdateStatus("Updating UI...");

            try
            {
                await StopServicesAsync(_globalCts.Token);
                await UpdateSelfAsync(newVersion, _globalCts.Token);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка обновления интерфейса:\n{ex.Message}", "Ошибка обновления");
                UpdateStatus("Ошибка");
                SetButtonsEnabled(true);
            }
        }

        /// <summary>
        /// Загрузка и установка последней версии Zapret
        /// </summary>
        private async Task LoadLastZapretAsync(string workDir, string version, CancellationToken cancellationToken)
        {
            string zipName = Path.Combine(workDir, $"{ZapretBaseName}{version}.zip");
            string downloadLink = $"{GitHubZapretUrl}/releases/download/{version}/{ZapretBaseName}{version}.zip";

            try
            {
                UpdateStatus($"Downloading {version}...");

                // Используем WebClient для поддержки прогресса
                using (var client = new WebClient())
                {
                    var tcs = new TaskCompletionSource<bool>();

                    client.DownloadProgressChanged += (s, e) =>
                    {
                        SafeInvoke(() =>
                        {
                            UpdateStatus($"Downloading... {e.ProgressPercentage}%");
                        });
                    };

                    client.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error != null) tcs.TrySetException(e.Error);
                        else if (e.Cancelled) tcs.TrySetCanceled();
                        else tcs.TrySetResult(true);
                    };

                    client.DownloadFileAsync(new Uri(downloadLink), zipName);

                    using (cancellationToken.Register(() =>
                    {
                        client.CancelAsync();
                        tcs.TrySetCanceled();
                    }))
                    {
                        await tcs.Task;
                    }
                }

                UpdateStatus("Extracting...");

                // Извлекаем архив
                await Task.Run(() =>
                    ZipFile.ExtractToDirectory(zipName,
                        Path.Combine(workDir, $"{ZapretBaseName}{version}"),
                        Encoding.GetEncoding(866)),
                    cancellationToken);

                // Удаляем архив
                File.Delete(zipName);

                UpdateStatus("Update complete!");
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(zipName))
                {
                    try { File.Delete(zipName); } catch { }
                }
                throw;
            }
            catch (Exception ex)
            {
                if (File.Exists(zipName))
                {
                    try { File.Delete(zipName); } catch { }
                }
                throw new InvalidOperationException($"Ошибка загрузки Zapret: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Обновление самого приложения
        /// </summary>
        private async Task UpdateSelfAsync(string newVersion, CancellationToken cancellationToken)
        {
            string currentPath = Assembly.GetExecutingAssembly().Location;
            string tempPath = currentPath + ".tmp";
            string backupPath = currentPath + ".bak";
            string batchPath = Path.Combine(
                Path.GetDirectoryName(currentPath),
                "ZapretUI_Update.bat");

            string downloadUrl = $"{GitHubUIUrl}/releases/download/{newVersion}/ZapretUI.exe";

            try
            {
                UpdateStatus("Downloading new version...");

                // Скачиваем новую версию
                using (var client = new WebClient())
                {
                    var tcs = new TaskCompletionSource<bool>();

                    client.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error != null) tcs.TrySetException(e.Error);
                        else if (e.Cancelled) tcs.TrySetCanceled();
                        else tcs.TrySetResult(true);
                    };

                    client.DownloadFileAsync(new Uri(downloadUrl), tempPath);

                    using (cancellationToken.Register(() =>
                    {
                        client.CancelAsync();
                        tcs.TrySetCanceled();
                    }))
                    {
                        await tcs.Task;
                    }
                }

                // Создаем скрипт обновления
                string batchContent = $@"
@echo off
chcp 65001 >nul
echo ========================================
echo          ZapretUI Updater
echo ========================================
echo.
echo Closing ZapretUI...
timeout /t 2 /nobreak >nul

echo Creating backup...
if exist ""{backupPath}"" del ""{backupPath}"" >nul 2>&1
if exist ""{currentPath}"" copy ""{currentPath}"" ""{backupPath}"" >nul 2>&1

echo Updating application...
if exist ""{tempPath}"" (
    move /y ""{tempPath}"" ""{currentPath}"" >nul 2>&1
    if %errorlevel% equ 0 (
        echo Update successful!
        echo Starting new version...
        start """" ""{currentPath}""
    ) else (
        echo Update failed! Restoring backup...
        if exist ""{backupPath}"" copy ""{backupPath}"" ""{currentPath}"" >nul 2>&1
        start """" ""{currentPath}""
    )
) else (
    echo Update file not found!
    if exist ""{backupPath}"" start """" ""{currentPath}""
)

echo Cleaning up...
del ""{batchPath}"" >nul 2>&1
exit
";

                File.WriteAllText(batchPath, batchContent);

                // Запускаем скрипт обновления
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{batchPath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(currentPath)
                };

                Process.Start(startInfo);

                _forceExit = false;
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                // Очистка в случае ошибки
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
                try { if (File.Exists(batchPath)) File.Delete(batchPath); } catch { }

                throw new InvalidOperationException($"Ошибка самосбновления: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Синхронная версия загрузки Zapret
        /// </summary>
        private void LoadLastZapret(string workDir, string version)
        {
            LoadLastZapretAsync(workDir, version, CancellationToken.None).Wait();
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Очистка файлов обновления
        /// </summary>
        private void CheckAndClearUpdates(DirectoryInfo workDir)
        {
            try
            {
                bool updated = false;

                foreach (var file in workDir.GetFiles())
                {
                    if (file.Name.StartsWith("ZapretUIUpdate", StringComparison.OrdinalIgnoreCase) ||
                        file.Extension.Equals(".bak", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            file.Delete();
                            updated = true;
                        }
                        catch { }
                    }
                }

                if (updated)
                {
                    Debug.WriteLine("[CheckAndClearUpdates] Файлы обновления очищены");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckAndClearUpdates] Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Безопасный выход из приложения
        /// </summary>
        private void SafeExit(int exitCode)
        {
            try
            {
                _isClosing = true;
                _globalCts.Cancel();

                if (_updateTimer != null)
                {
                    _updateTimer.Dispose();
                    _updateTimer = null;
                }

                notifyIcon1?.Dispose();

                if (Application.MessageLoop)
                {
                    Application.Exit();
                }

                Environment.Exit(exitCode);
            }
            catch
            {
                Environment.Exit(exitCode);
            }
        }

        /// <summary>
        /// Отображение сообщения об ошибке
        /// </summary>
        private void ShowErrorMessage(string message, string title)
        {
            SafeInvoke(() =>
            {
                MessageBox.Show(this, message, title,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        /// <summary>
        /// Отображение информационного сообщения
        /// </summary>
        private void ShowInformationMessage(string message, string title)
        {
            SafeInvoke(() =>
            {
                MessageBox.Show(this, message, title,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        #endregion

        #region Обработчики событий формы

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isClosing) return;

            if (e.CloseReason != CloseReason.WindowsShutDown && _forceExit)
            {
                var result = MessageBox.Show(this,
                    "Вы действительно хотите закрыть приложение?\n\n" +
                    "Все запущенные скрипты Zapret будут остановлены.",
                    "Подтверждение закрытия",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            _isClosing = true;

            // Отменяем все операции
            _globalCts.Cancel();

            // Останавливаем таймер автообновления
            _updateTimer?.Dispose();
            _updateTimer = null;
        }

        private async void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_forceExit && _isScriptRunning)
            {
                try
                {
                    // Даем короткую задержку для корректного закрытия UI
                    await Task.Delay(100);

                    // Останавливаем сервисы
                    await StopServicesAsync(_globalCts.Token);
                }
                catch { }
            }

            // Освобождаем ресурсы
            _globalCts.Dispose();
            notifyIcon1?.Dispose();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                if (notifyIcon1 != null)
                    notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) => ShowMainForm();

        private void toolStripMenuItem1_Click(object sender, EventArgs e) => ShowMainForm();

        /// <summary>
        /// Показать главную форму
        /// </summary>
        private void ShowMainForm()
        {
            SafeInvoke(() =>
            {
                notifyIcon1.Visible = false;
                this.ShowInTaskbar = true;
                WindowState = FormWindowState.Normal;
                this.BringToFront();
                this.Activate();
            });
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e) => Close();

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SafeInvoke(() =>
            {
                if (_isScriptRunning)
                {
                    string currentSelection = comboBox1.SelectedItem?.ToString();

                    if (!string.IsNullOrEmpty(currentSelection) &&
                        currentSelection != _selectedScript)
                    {
                        buttonStart.Enabled = false;
                        buttonStart.Visible = false;
                        buttonReboot.Enabled = true;
                        buttonReboot.Visible = true;
                    }
                    else
                    {
                        buttonReboot.Enabled = false;
                        buttonReboot.Visible = false;
                        buttonStart.Enabled = false;
                        buttonStart.Visible = true;
                    }
                }
            });
        }

        private async void buttonReboot_Click(object sender, EventArgs e)
        {
            SetButtonsEnabled(false);
            UpdateStatus("Restarting...");

            try
            {
                await StopServicesAsync(_globalCts.Token);
                await Task.Delay(1000, _globalCts.Token); // Пауза перед перезапуском
                await StartScriptAsync(_globalCts.Token);

                SafeInvoke(() =>
                {
                    buttonStart.Enabled = false;
                    buttonStart.Visible = true;
                    buttonReboot.Enabled = false;
                    buttonReboot.Visible = false;
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка перезапуска:\n{ex.Message}", "Ошибка");
                UpdateStatus("Ошибка");
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            _isClosing = true;
            _globalCts.Cancel();
            _updateTimer?.Dispose();
        }

        #endregion

        #region Настройки

        /// <summary>
        /// Загрузка настроек
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                SafeInvoke(() =>
                {
                    checkBoxBlackPheme.Checked = Settings.Default.blackPheme;
                    checkBoxStartOnWind.Checked = Settings.Default.startOnWind;

                    // Применяем черную тему, если нужно
                    if (Settings.Default.blackPheme)
                    {
                        ApplyDarkTheme();
                    }
                });
            }
            catch { }
        }

        /// <summary>
        /// Установка автозагрузки из настроек
        /// </summary>
        private void SetStartupFromSettings()
        {
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    string appPath = $"\"{Application.ExecutablePath}\"";

                    if (Settings.Default.startOnWind)
                    {
                        rk.SetValue("ZapretUI", appPath);
                        Debug.WriteLine("[SetStartupFromSettings] Автозагрузка включена");
                    }
                    else
                    {
                        rk.DeleteValue("ZapretUI", false);
                        Debug.WriteLine("[SetStartupFromSettings] Автозагрузка выключена");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SetStartupFromSettings] Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Применение темной темы
        /// </summary>
        private void ApplyDarkTheme()
        {
            try
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                this.ForeColor = Color.White;

                // Можно добавить дополнительные настройки темной темы
            }
            catch { }
        }

        // Обработчики событий настроек
        private void buttonApply_Click(object sender, EventArgs e)
        {
            Settings.Default.Save();
            SetStartupFromSettings();

            if (Settings.Default.blackPheme)
            {
                ApplyDarkTheme();
            }

            ShowToolTip("Настройки применены", 1000);
        }

        private void buttonSettingsShow_Click(object sender, EventArgs e)
        {
            if (tabControl1 != null)
            {
                tabControl1.Show();
                tabControl1.BringToFront();
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (tabControl1 != null)
            {
                tabControl1.Hide();
            }
        }

        private void checkBoxStartOnWind_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxStartOnWind != null)
            {
                Settings.Default.startOnWind = checkBoxStartOnWind.Checked;
            }
        }

        private void checkBoxBlackPheme_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxBlackPheme != null)
            {
                Settings.Default.blackPheme = checkBoxBlackPheme.Checked;
            }
        }

        /// <summary>
        /// Показать всплывающую подсказку
        /// </summary>
        private void ShowToolTip(string message, int duration)
        {
            SafeInvoke(() =>
            {
                try
                {
                    var toolTip = new ToolTip
                    {
                        AutoPopDelay = duration,
                        InitialDelay = 100,
                        ReshowDelay = 100,
                        ShowAlways = true
                    };

                    Point cursorPos = this.PointToClient(Cursor.Position);
                    toolTip.Show(message, this, cursorPos.X, cursorPos.Y - 40, duration);
                }
                catch { }
            });
        }

        // Обработчики для картинок
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            bool start = true;
            if (Settings.Default.showAgain == true)
            {
                start = ShowCheckBox("Запустить Discord?", "Discord");
            }

            if (start)
            {
                LaunchDiscord();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            bool start = true;
            if (Settings.Default.showAgain == true)
            {
                start = ShowCheckBox("Запустить YouTube в браузере по умолчанию?", "YouTube");
            }

            if (start)
            {
                LaunchYouTube();
            }
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            ShowToolTip("Запустить Discord", 2000);
        }

        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {
            ShowToolTip("Запустить YouTube", 2000);
        }

        /// <summary>
        /// Показать диалог с CheckBox
        /// </summary>
        public static bool ShowCheckBox(string text, string caption)
        {
            bool result = false;
            bool dontShowAgain = false;

            var dialogThread = new Thread(() =>
            {
                Form dialog = new Form()
                {
                    Width = 300,
                    Height = 180,
                    Text = caption,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = SystemColors.Window
                };

                Label messageLabel = new Label()
                {
                    Text = text,
                    Location = new Point(10, 20),
                    Size = new Size(260, 40),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                CheckBox checkBoxAgain = new CheckBox()
                {
                    Text = "Больше не показывать",
                    Location = new Point(10, 70),
                    Size = new Size(200, 20)
                };

                Button noButton = new Button()
                {
                    Text = "Нет",
                    Location = new Point(120, 100),
                    Size = new Size(75, 25),
                    DialogResult = DialogResult.No
                };

                Button yesButton = new Button()
                {
                    Text = "Да",
                    Location = new Point(205, 100),
                    Size = new Size(75, 25),
                    DialogResult = DialogResult.Yes
                };

                dialog.Controls.Add(messageLabel);
                dialog.Controls.Add(checkBoxAgain);
                dialog.Controls.Add(noButton);
                dialog.Controls.Add(yesButton);

                dialog.AcceptButton = yesButton;
                dialog.CancelButton = noButton;

                var dialogResult = dialog.ShowDialog();
                result = dialogResult == DialogResult.Yes;
                dontShowAgain = checkBoxAgain.Checked;
            });

            dialogThread.SetApartmentState(ApartmentState.STA);
            dialogThread.Start();
            dialogThread.Join();

            if (dontShowAgain)
            {
                Settings.Default.showAgain = false;
                Settings.Default.Save();
            }

            return result;
        }

        /// <summary>
        /// Запуск Discord
        /// </summary>
        private void LaunchDiscord()
        {
            try
            {
                string[] discordPaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Discord"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Discord"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Discord"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Discord")
                };

                foreach (var path in discordPaths)
                {
                    if (Directory.Exists(path))
                    {
                        var discordExe = Directory.GetFiles(path, "Discord.exe", SearchOption.AllDirectories)
                            .FirstOrDefault();

                        if (!string.IsNullOrEmpty(discordExe) && File.Exists(discordExe))
                        {
                            Process.Start(discordExe);
                            return;
                        }
                    }
                }

                // Если Discord не найден, открываем в браузере
                Process.Start("https://discord.com/app");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Не удалось запустить Discord:\n{ex.Message}", "Ошибка");
            }
        }

        /// <summary>
        /// Запуск YouTube
        /// </summary>
        private void LaunchYouTube()
        {
            try
            {
                Process.Start("https://www.youtube.com");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Не удалось открыть YouTube:\n{ex.Message}", "Ошибка");
            }
        }

        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        //private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }

}