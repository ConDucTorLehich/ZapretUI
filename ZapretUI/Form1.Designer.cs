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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonReboot = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.galkaYoutube = new System.Windows.Forms.PictureBox();
            this.galkaDiscord = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonUpd = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.crossYoutube = new System.Windows.Forms.PictureBox();
            this.crossDiscord = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            ((System.ComponentModel.ISupportInitialize)(this.galkaYoutube)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.galkaDiscord)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.crossYoutube)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.crossDiscord)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "ZapretYT&DS";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Segoe Print", 24F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(-26, -4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(329, 171);
            this.label1.TabIndex = 0;
            this.label1.Text = "Zapret YouTube & Discord By ConDucTor";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.UseMnemonic = false;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 170);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(260, 21);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.Text = "Выберите тип запрета";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 210);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(93, 38);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(179, 210);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(93, 38);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonReboot
            // 
            this.buttonReboot.Enabled = false;
            this.buttonReboot.Location = new System.Drawing.Point(12, 210);
            this.buttonReboot.Name = "buttonReboot";
            this.buttonReboot.Size = new System.Drawing.Size(93, 38);
            this.buttonReboot.TabIndex = 4;
            this.buttonReboot.Text = "Reboot";
            this.buttonReboot.UseVisualStyleBackColor = true;
            this.buttonReboot.Visible = false;
            this.buttonReboot.Click += new System.EventHandler(this.buttonReboot_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(-1, 492);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(286, 30);
            this.label2.TabIndex = 5;
            this.label2.Text = "Upd Available!(or not)                 ver. scr: 1.7.2b/ui: 0.9.9";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(12, 251);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(231, 28);
            this.labelStatus.TabIndex = 7;
            this.labelStatus.Text = "Status: Stopped";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 466);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(260, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Проверка обновлений (не реализовано)";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // galkaYoutube
            // 
            this.galkaYoutube.BackColor = System.Drawing.Color.Transparent;
            this.galkaYoutube.Image = global::ZapretUI.Properties.Resources.galka;
            this.galkaYoutube.Location = new System.Drawing.Point(227, 368);
            this.galkaYoutube.Name = "galkaYoutube";
            this.galkaYoutube.Size = new System.Drawing.Size(45, 43);
            this.galkaYoutube.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.galkaYoutube.TabIndex = 12;
            this.galkaYoutube.TabStop = false;
            this.galkaYoutube.Visible = false;
            // 
            // galkaDiscord
            // 
            this.galkaDiscord.BackColor = System.Drawing.Color.Transparent;
            this.galkaDiscord.Image = global::ZapretUI.Properties.Resources.galka;
            this.galkaDiscord.Location = new System.Drawing.Point(227, 297);
            this.galkaDiscord.Name = "galkaDiscord";
            this.galkaDiscord.Size = new System.Drawing.Size(45, 43);
            this.galkaDiscord.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.galkaDiscord.TabIndex = 11;
            this.galkaDiscord.TabStop = false;
            this.galkaDiscord.Visible = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::ZapretUI.Properties.Resources.discordLogo;
            this.pictureBox1.Location = new System.Drawing.Point(5, 282);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(211, 77);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // buttonUpd
            // 
            this.buttonUpd.BackColor = System.Drawing.Color.Transparent;
            this.buttonUpd.BackgroundImage = global::ZapretUI.Properties.Resources.upd;
            this.buttonUpd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonUpd.Location = new System.Drawing.Point(235, 251);
            this.buttonUpd.Name = "buttonUpd";
            this.buttonUpd.Size = new System.Drawing.Size(30, 28);
            this.buttonUpd.TabIndex = 6;
            this.buttonUpd.UseVisualStyleBackColor = false;
            this.buttonUpd.Click += new System.EventHandler(this.buttonUpd_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.Image = global::ZapretUI.Properties.Resources.YouTubeLogo;
            this.pictureBox2.Location = new System.Drawing.Point(-7, 338);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(211, 98);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 9;
            this.pictureBox2.TabStop = false;
            // 
            // crossYoutube
            // 
            this.crossYoutube.BackColor = System.Drawing.Color.Transparent;
            this.crossYoutube.Image = global::ZapretUI.Properties.Resources.krestik;
            this.crossYoutube.Location = new System.Drawing.Point(227, 368);
            this.crossYoutube.Name = "crossYoutube";
            this.crossYoutube.Size = new System.Drawing.Size(45, 43);
            this.crossYoutube.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.crossYoutube.TabIndex = 14;
            this.crossYoutube.TabStop = false;
            this.crossYoutube.Visible = false;
            // 
            // crossDiscord
            // 
            this.crossDiscord.BackColor = System.Drawing.Color.Transparent;
            this.crossDiscord.Image = global::ZapretUI.Properties.Resources.krestik;
            this.crossDiscord.Location = new System.Drawing.Point(227, 297);
            this.crossDiscord.Name = "crossDiscord";
            this.crossDiscord.Size = new System.Drawing.Size(45, 43);
            this.crossDiscord.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.crossDiscord.TabIndex = 13;
            this.crossDiscord.TabStop = false;
            this.crossDiscord.Visible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(278, 24);
            this.menuStrip1.TabIndex = 15;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(278, 517);
            this.Controls.Add(this.galkaYoutube);
            this.Controls.Add(this.galkaDiscord);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonUpd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.crossYoutube);
            this.Controls.Add(this.crossDiscord);
            this.Controls.Add(this.buttonReboot);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "РКН Сосамба!";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.galkaYoutube)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.galkaDiscord)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.crossYoutube)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.crossDiscord)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MenuStrip menuStrip1;
    }
}

