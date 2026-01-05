using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace ZapretUI
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Проверяем права администратора
            if (!IsRunningAsAdministrator())
            {
                // Перезапускаем с правами администратора
                RestartAsAdministrator();
                return;
            }

            // Настраиваем обработку неперехваченных исключений
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Настройка приложения
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Проверяем, не запущен ли уже экземпляр приложения
            if (IsAlreadyRunning())
            {
                MessageBox.Show("Приложение уже запущено!", "ZapretUI",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Создаем и запускаем главную форму
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                HandleFatalError(ex);
            }
        }

        /// <summary>
        /// Проверка, запущено ли приложение от имени администратора
        /// </summary>
        private static bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Перезапуск приложения с правами администратора
        /// </summary>
        private static void RestartAsAdministrator()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                Verb = "runas" // Запуск от имени администратора
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось запустить приложение от имени администратора:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Проверка, запущен ли уже экземпляр приложения
        /// </summary>
        private static bool IsAlreadyRunning()
        {
            string appName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            Process[] processes = Process.GetProcessesByName(appName);
            return processes.Length > 1;
        }

        /// <summary>
        /// Обработчик исключений в UI-потоке
        /// </summary>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleError(e.Exception, "UI Thread Exception");
        }

        /// <summary>
        /// Обработчик неперехваченных исключений
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleError(ex, "Unhandled Exception");
            }
        }

        /// <summary>
        /// Обработка ошибок
        /// </summary>
        private static void HandleError(Exception ex, string source)
        {
            string errorMessage = $"[{source}]\n\n" +
                                $"Сообщение: {ex.Message}\n\n" +
                                $"Тип: {ex.GetType().Name}\n\n" +
                                $"StackTrace:\n{ex.StackTrace}";

            // Записываем в лог
            try
            {
                string logPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "error.log");

                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {errorMessage}\n\n");
            }
            catch { }

            // Показываем пользователю
            MessageBox.Show($"Произошла ошибка:\n{ex.Message}\n\n" +
                          "Подробности записаны в error.log",
                          "Ошибка приложения",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
        }

        /// <summary>
        /// Обработка фатальных ошибок
        /// </summary>
        private static void HandleFatalError(Exception ex)
        {
            string errorDetails = $"[FATAL ERROR]\n\n" +
                                $"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                $"Сообщение: {ex.Message}\n" +
                                $"Тип: {ex.GetType().Name}\n\n" +
                                $"StackTrace:\n{ex.StackTrace}";

            try
            {
                string logPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "crash.log");

                File.WriteAllText(logPath, errorDetails);
            }
            catch { }

            MessageBox.Show($"Критическая ошибка при запуске приложения:\n{ex.Message}\n\n" +
                          "Приложение будет закрыто.",
                          "Критическая ошибка",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
        }

    }
}