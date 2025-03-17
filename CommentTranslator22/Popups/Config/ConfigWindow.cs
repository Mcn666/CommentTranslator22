using CommentTranslator22.Translate;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CommentTranslator22.Popups.Config
{
    public partial class ConfigWindow : Form
    {
        public static ConfigWindowModel Model { get; } = new ConfigWindowModel();

        public ConfigWindow()
        {
            InitializeComponent();
            LoadConfig();
            SetLanguage();
            DataBingding();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        /// <summary>
        /// 系统主题更改时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            //UpdateDarkOrLightMode();
        }

        private void SetLanguage()
        {
            this.checkBox1.Text = ConfigWindowLanguage.GetLanguage("ut");
            this.checkBox2.Text = ConfigWindowLanguage.GetLanguage("up");
            this.checkBox3.Text = ConfigWindowLanguage.GetLanguage("ud");
            this.checkBox4.Text = ConfigWindowLanguage.GetLanguage("um");
            this.label1.Text = ConfigWindowLanguage.GetLanguage("ts");
            this.label2.Text = ConfigWindowLanguage.GetLanguage("sl");
            this.label3.Text = ConfigWindowLanguage.GetLanguage("tl");
        }

        private void DataBingding()
        {
            this.checkBox1.DataBindings.Add("Checked", Model, "UseDefaultTranslation", false, DataSourceUpdateMode.OnPropertyChanged);
            this.checkBox2.DataBindings.Add("Checked", Model, "UsePhraseTranslation", false, DataSourceUpdateMode.OnPropertyChanged);
            this.checkBox3.DataBindings.Add("Checked", Model, "UseDictionaryTranslation", false, DataSourceUpdateMode.OnPropertyChanged);
            this.checkBox4.DataBindings.Add("Checked", Model, "UseMask", false, DataSourceUpdateMode.OnPropertyChanged);
            //this.checkedListBox1.DataBindings.Add("Items", model, "UseMaskType", false, DataSourceUpdateMode.OnPropertyChanged);
            this.comboBox1.DataBindings.Add("SelectedIndex", Model, "ServerInt", false, DataSourceUpdateMode.OnPropertyChanged);
            this.comboBox2.DataBindings.Add("SelectedIndex", Model, "SourceLanguageInt", false, DataSourceUpdateMode.OnPropertyChanged);
            this.comboBox3.DataBindings.Add("SelectedIndex", Model, "TargetLanguageInt", false, DataSourceUpdateMode.OnPropertyChanged);
            this.textBox1.DataBindings.Add("Text", Model, "AppId", false, DataSourceUpdateMode.OnPropertyChanged);
            this.textBox2.DataBindings.Add("Text", Model, "SecretKey", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        /// <summary>
        /// 更新深色模式或浅色模式
        /// </summary>
        public void UpdateDarkOrLightMode()
        {
            var isDarkMode = IsSystemInDarkMode();
            if (isDarkMode)
            {
                this.BackColor = Color.Black;
            }
            else
            {
                this.BackColor = Color.White;
                this.BackColor = Color.FromArgb(0xF5F5F5);
            }
        }

        /// <summary>
        /// 判断系统是否处于深色模式
        /// </summary>
        /// <returns></returns>
        public static bool IsSystemInDarkMode()
        {
            // 注册表路径
            string registryPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            string valueName = "AppsUseLightTheme";

            // 读取注册表值
            object registryValue = Registry.GetValue(registryPath, valueName, 1);

            // 如果值为 0，表示深色模式；1 表示浅色模式
            if (registryValue != null && registryValue is int v)
            {
                return v == 0;
            }

            // 默认返回浅色模式
            return false;
        }

        /// <summary>
        /// 获取当前语言
        /// </summary>
        /// <returns></returns>
        public static LanguageEnum GetCurrentCulture()
        {
            string currentCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
            switch (currentCulture)
            {
                case "ja-JP":
                    return LanguageEnum.日本語;
                case "zh-CN":
                    return LanguageEnum.简体中文;
                case "zh-TW":
                    return LanguageEnum.繁體中文;
                case "en-US":
                    return LanguageEnum.English;
                default:
                    return LanguageEnum.简体中文;
            }
        }

        private void ConfigWindow_Deactivate(object sender, EventArgs e)
        {
            this.SaveConfig();
            this.Close();
        }

        private void LoadConfig()
        {
            var path = GetConfigFolderPath();
            var file = Path.Combine(path, "config.json");
            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                var temp = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigWindowModel>(json);
                if (temp != null)
                {
                    Model.UseDefaultTranslation = temp.UseDefaultTranslation;
                    Model.UsePhraseTranslation = temp.UsePhraseTranslation;
                    Model.UseDictionaryTranslation = temp.UseDictionaryTranslation;
                    Model.ServerInt = temp.ServerInt;
                    Model.SourceLanguageInt = temp.SourceLanguageInt;
                    Model.TargetLanguageInt = temp.TargetLanguageInt;
                    Model.AppId = temp.AppId;
                    Model.SecretKey = temp.SecretKey;
                    Model.UseMask = temp.UseMask;
                    //Model.UseMaskType = temp.UseMaskType;
                }
            }
        }

        private void SaveConfig()
        {
            var path = GetConfigFolderPath();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(Model, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(path, "config.json"), json);
        }

        private string GetConfigFolderPath()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CommentTranslator22");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}
