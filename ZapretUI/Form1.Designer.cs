using System.Drawing;
using System.Windows.Forms;

namespace ZapretUI
{
    partial class Form1
    {
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

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]

        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("test_UI.Form1", typeof(Form1).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Point, аналогичного {X=131,Y=17}.
        /// </summary>
        internal static System.Drawing.Point contextMenuStrip1_TrayLocation
        {
            get
            {
                object obj = ResourceManager.GetObject("contextMenuStrip1.TrayLocation", resourceCulture);
                return ((System.Drawing.Point)(obj));
            }
        }

        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Icon, аналогичного (Значок).
        /// </summary>
        internal static System.Drawing.Icon notifyIcon1_Icon
        {
            get
            {
                object obj = ResourceManager.GetObject("notifyIcon1.Icon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }

        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Point, аналогичного {X=17,Y=17}.
        /// </summary>
        internal static System.Drawing.Point notifyIcon1_TrayLocation
        {
            get
            {
                object obj = ResourceManager.GetObject("notifyIcon1.TrayLocation", resourceCulture);
                return ((System.Drawing.Point)(obj));
            }
        }

        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Icon, аналогичного (Значок).
        /// </summary>
        internal static System.Drawing.Icon runningICO
        {
            get
            {
                object obj = ResourceManager.GetObject("runningICO", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }

        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Icon, аналогичного (Значок).
        /// </summary>
        internal static System.Drawing.Icon stoppedICO
        {
            get
            {
                object obj = ResourceManager.GetObject("stoppedICO", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        private NotifyIcon notifyIcon1;
        private System.ComponentModel.IContainer components;
        private Label label1;
        private ComboBox comboBox1;
        private Button buttonStart;
        private Button buttonStop;
        private Button buttonReboot;
        private Label label2;
        private Button buttonUpd;
        private Label labelStatus;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Button button1;
        private PictureBox galkaDiscord;
        private PictureBox galkaYoutube;
        private PictureBox crossYoutube;
        private PictureBox crossDiscord;

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            notifyIcon1 = new NotifyIcon(components);
            label1 = new Label();
            comboBox1 = new ComboBox();
            buttonStart = new Button();
            buttonStop = new Button();
            buttonReboot = new Button();
            label2 = new Label();
            buttonUpd = new Button();
            labelStatus = new Label();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            button1 = new Button();
            galkaDiscord = new PictureBox();
            galkaYoutube = new PictureBox();
            crossYoutube = new PictureBox();
            crossDiscord = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)galkaDiscord).BeginInit();
            ((System.ComponentModel.ISupportInitialize)galkaYoutube).BeginInit();
            ((System.ComponentModel.ISupportInitialize)crossYoutube).BeginInit();
            ((System.ComponentModel.ISupportInitialize)crossDiscord).BeginInit();
            SuspendLayout();
            // 
            // notifyIcon1
            // 
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe Print", 24F, FontStyle.Italic, GraphicsUnit.Point, 204);
            label1.Location = new Point(-26, -4);
            label1.Name = "label1";
            label1.Size = new Size(329, 171);
            label1.TabIndex = 0;
            label1.Text = "Zapret YouTube & Discord By ConDucTor";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.UseMnemonic = false;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(12, 170);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(260, 23);
            comboBox1.TabIndex = 1;
            comboBox1.Text = "Выберите тип запрета";
            // 
            // buttonStart
            // 
            buttonStart.Location = new Point(12, 210);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(93, 38);
            buttonStart.TabIndex = 2;
            buttonStart.Text = "Start";
            buttonStart.UseVisualStyleBackColor = true;
            // 
            // buttonStop
            // 
            buttonStop.Location = new Point(179, 210);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(93, 38);
            buttonStop.TabIndex = 3;
            buttonStop.Text = "Stop";
            buttonStop.UseVisualStyleBackColor = true;
            // 
            // buttonReboot
            // 
            buttonReboot.Location = new Point(12, 210);
            buttonReboot.Name = "buttonReboot";
            buttonReboot.Size = new Size(93, 38);
            buttonReboot.TabIndex = 4;
            buttonReboot.Text = "Reboot";
            buttonReboot.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.Location = new Point(-1, 513);
            label2.Name = "label2";
            label2.Size = new Size(286, 30);
            label2.TabIndex = 5;
            label2.Text = "Upd Available!(or not)                                    ver. 1.7.2b";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonUpd
            // 
            buttonUpd.BackColor = Color.Transparent;
            buttonUpd.BackgroundImage = Properties.Resources.upd;
            buttonUpd.BackgroundImageLayout = ImageLayout.Zoom;
            buttonUpd.Location = new Point(235, 272);
            buttonUpd.Name = "buttonUpd";
            buttonUpd.Size = new Size(30, 28);
            buttonUpd.TabIndex = 6;
            buttonUpd.UseVisualStyleBackColor = false;
            // 
            // labelStatus
            // 
            labelStatus.Location = new Point(12, 272);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(231, 28);
            labelStatus.TabIndex = 7;
            labelStatus.Text = "Status:";
            labelStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.discordLogo;
            pictureBox1.Location = new Point(-1, 303);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(211, 77);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = Properties.Resources.YouTubeLogo;
            pictureBox2.Location = new Point(-13, 359);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(211, 98);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 9;
            pictureBox2.TabStop = false;
            // 
            // button1
            // 
            button1.Location = new Point(12, 487);
            button1.Name = "button1";
            button1.Size = new Size(260, 23);
            button1.TabIndex = 10;
            button1.Text = "Проверка обновлений(не реализовано)";
            button1.UseVisualStyleBackColor = true;
            // 
            // galkaDiscord
            // 
            galkaDiscord.BackColor = Color.Transparent;
            galkaDiscord.Image = Properties.Resources.galka;
            galkaDiscord.Location = new Point(227, 318);
            galkaDiscord.Name = "galkaDiscord";
            galkaDiscord.Size = new Size(45, 43);
            galkaDiscord.SizeMode = PictureBoxSizeMode.Zoom;
            galkaDiscord.TabIndex = 11;
            galkaDiscord.TabStop = false;
            // 
            // galkaYoutube
            // 
            galkaYoutube.BackColor = Color.Transparent;
            galkaYoutube.Image = Properties.Resources.galka;
            galkaYoutube.Location = new Point(227, 389);
            galkaYoutube.Name = "galkaYoutube";
            galkaYoutube.Size = new Size(45, 43);
            galkaYoutube.SizeMode = PictureBoxSizeMode.Zoom;
            galkaYoutube.TabIndex = 12;
            galkaYoutube.TabStop = false;
            // 
            // crossYoutube
            // 
            crossYoutube.BackColor = Color.Transparent;
            crossYoutube.Image = Properties.Resources.krestik;
            crossYoutube.Location = new Point(227, 389);
            crossYoutube.Name = "crossYoutube";
            crossYoutube.Size = new Size(45, 43);
            crossYoutube.SizeMode = PictureBoxSizeMode.Zoom;
            crossYoutube.TabIndex = 14;
            crossYoutube.TabStop = false;
            // 
            // crossDiscord
            // 
            crossDiscord.BackColor = Color.Transparent;
            crossDiscord.Image = Properties.Resources.krestik;
            crossDiscord.Location = new Point(227, 318);
            crossDiscord.Name = "crossDiscord";
            crossDiscord.Size = new Size(45, 43);
            crossDiscord.SizeMode = PictureBoxSizeMode.Zoom;
            crossDiscord.TabIndex = 13;
            crossDiscord.TabStop = false;
            // 
            // Form1
            // 
            AutoSize = true;
            ClientSize = new Size(284, 538);
            Controls.Add(crossYoutube);
            Controls.Add(crossDiscord);
            Controls.Add(galkaYoutube);
            Controls.Add(galkaDiscord);
            Controls.Add(button1);
            Controls.Add(pictureBox1);
            Controls.Add(buttonUpd);
            Controls.Add(label2);
            Controls.Add(buttonReboot);
            Controls.Add(buttonStop);
            Controls.Add(buttonStart);
            Controls.Add(comboBox1);
            Controls.Add(label1);
            Controls.Add(labelStatus);
            Controls.Add(pictureBox2);
            Name = "Form1";
            Text = "РКН Сосамба!";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)galkaDiscord).EndInit();
            ((System.ComponentModel.ISupportInitialize)galkaYoutube).EndInit();
            ((System.ComponentModel.ISupportInitialize)crossYoutube).EndInit();
            ((System.ComponentModel.ISupportInitialize)crossDiscord).EndInit();
            ResumeLayout(false);

        }

        #endregion
    }
}

