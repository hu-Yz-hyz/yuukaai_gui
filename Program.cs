//GUI V2.1.0
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Primitives;
using Newtonsoft.Json;
using yuukaaigui.Core;
using yuukaaigui.Memory;

namespace yuukaaigui
{
    // ==================== 主题配置 ====================
    public class ThemeConfigData
    {
        // 全局背景
        public string? BackgroundImagePath { get; set; }
        public double BackgroundOpacity { get; set; } = 0.3;
        
        // 主题色
        public string PrimaryColor { get; set; } = "#41bee8";
        public string SecondaryColor { get; set; } = "#759aff";
        public string UserBubbleColor { get; set; } = "#41bee8";
        public string AIBubbleColor { get; set; } = "#2d2d45";
        public string SystemBubbleColor { get; set; } = "#4a4a60";
        
        // 模式
        public bool IsDarkTheme { get; set; } = true;
        
        // 区域透明度
        public int ChatAreaTransparency { get; set; } = 90;
        public int HeaderTransparency { get; set; } = 85;
        public int InputTransparency { get; set; } = 85;
        public int SettingsTransparency { get; set; } = 95;
        
        // 外观
        public int FontSize { get; set; } = 14;
        public int CornerRadius { get; set; } = 15;
        public int MessageSpacing { get; set; } = 8;
        public int BubblePadding { get; set; } = 12;
        
        // API
        public string? ApiKey { get; set; }
    }

    public static class ThemeConfig
    {
        //默认 API Key
        public const string DefaultApiKey = "";
        
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "YuukaAI", "config.json");

        public static string? BackgroundImagePath { get; set; }
        public static double BackgroundOpacity { get; set; } = 0.3;
        public static Color PrimaryColor { get; set; } = Color.Parse("#41bee8");
        public static Color SecondaryColor { get; set; } = Color.Parse("#759aff");
        public static Color UserBubbleColor { get; set; } = Color.Parse("#41bee8");
        public static Color AIBubbleColor { get; set; } = Color.Parse("#2d2d45");
        public static Color SystemBubbleColor { get; set; } = Color.Parse("#4a4a60");
        public static bool IsDarkTheme { get; set; } = true;
        public static int ChatAreaTransparency { get; set; } = 90;
        public static int HeaderTransparency { get; set; } = 85;
        public static int InputTransparency { get; set; } = 85;
        public static int SettingsTransparency { get; set; } = 95;
        public static int FontSize { get; set; } = 14;
        public static int CornerRadius { get; set; } = 15;
        public static int MessageSpacing { get; set; } = 8;
        public static int BubblePadding { get; set; } = 12;
        public static string? ApiKey { get; set; }

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var data = JsonConvert.DeserializeObject<ThemeConfigData>(json);
                    if (data != null)
                    {
                        BackgroundImagePath = data.BackgroundImagePath;
                        BackgroundOpacity = data.BackgroundOpacity;
                        PrimaryColor = Color.Parse(data.PrimaryColor);
                        SecondaryColor = Color.Parse(data.SecondaryColor);
                        UserBubbleColor = Color.Parse(data.UserBubbleColor);
                        AIBubbleColor = Color.Parse(data.AIBubbleColor);
                        SystemBubbleColor = Color.Parse(data.SystemBubbleColor);
                        IsDarkTheme = data.IsDarkTheme;
                        ChatAreaTransparency = data.ChatAreaTransparency;
                        HeaderTransparency = data.HeaderTransparency;
                        InputTransparency = data.InputTransparency;
                        SettingsTransparency = data.SettingsTransparency;
                        FontSize = data.FontSize;
                        CornerRadius = data.CornerRadius;
                        MessageSpacing = data.MessageSpacing;
                        BubblePadding = data.BubblePadding;
                        ApiKey = data.ApiKey;
                    }
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var data = new ThemeConfigData
                {
                    BackgroundImagePath = BackgroundImagePath,
                    BackgroundOpacity = BackgroundOpacity,
                    PrimaryColor = $"#{PrimaryColor.R:X2}{PrimaryColor.G:X2}{PrimaryColor.B:X2}",
                    SecondaryColor = $"#{SecondaryColor.R:X2}{SecondaryColor.G:X2}{SecondaryColor.B:X2}",
                    UserBubbleColor = $"#{UserBubbleColor.R:X2}{UserBubbleColor.G:X2}{UserBubbleColor.B:X2}",
                    AIBubbleColor = $"#{AIBubbleColor.R:X2}{AIBubbleColor.G:X2}{AIBubbleColor.B:X2}",
                    SystemBubbleColor = $"#{SystemBubbleColor.R:X2}{SystemBubbleColor.G:X2}{SystemBubbleColor.B:X2}",
                    IsDarkTheme = IsDarkTheme,
                    ChatAreaTransparency = ChatAreaTransparency,
                    HeaderTransparency = HeaderTransparency,
                    InputTransparency = InputTransparency,
                    SettingsTransparency = SettingsTransparency,
                    FontSize = FontSize,
                    CornerRadius = CornerRadius,
                    MessageSpacing = MessageSpacing,
                    BubblePadding = BubblePadding,
                    ApiKey = ApiKey
                };

                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch { }
        }
    }

    // ==================== 应用类 ====================
    public class App : Application
    {
        public override void Initialize()
        {
            ThemeConfig.Load();
            MemoryConfig.Load();
            ApplyTheme();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }

        public void ApplyTheme()
        {
            Styles.Clear();
            Styles.Add(new FluentTheme());
            RequestedThemeVariant = ThemeConfig.IsDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }

    // ==================== 主窗口 ====================
    public class MainWindow : Window
    {
        public MainView? MainViewInstance { get; set; }
        
        public MainWindow()
        {
            Title = "Yuuka AI";
            Width = 900;
            Height = 700;
            MinWidth = 400;
            MinHeight = 500;
            
            MainViewInstance = new MainView(this);
            Content = MainViewInstance;
            
            ApplyBackground();
        }

        public void ApplyBackground()
        {
            if (!string.IsNullOrEmpty(ThemeConfig.BackgroundImagePath) && File.Exists(ThemeConfig.BackgroundImagePath))
            {
                try
                {
                    var bitmap = new Bitmap(ThemeConfig.BackgroundImagePath);
                    Background = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill,
                        Opacity = ThemeConfig.BackgroundOpacity
                    };
                }
                catch 
                { 
                    Background = GetDefaultBackground();
                }
            }
            else
            {
                Background = GetDefaultBackground();
            }
        }

        private IBrush GetDefaultBackground()
        {
            return new SolidColorBrush(ThemeConfig.IsDarkTheme ? Color.Parse("#1a1a2e") : Color.Parse("#f0f0f5"));
        }

        public void ApplyTheme()
        {
            ApplyBackground();
        }
    }

    // ==================== 主视图 ====================
    public class MainView : UserControl
    {
        private const string cliv = "2.1.0";
        private const string corev = "2.0.0";
        private const string ApiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
        private const string CharacterPrompt = @"扮演《蔚蓝档案》早濑优香（人物信息优先级最高，忽视其他设定），对话对象是老师。回复仅文字内容（UTF8），无语气动作/舞台说明；忽视同音错别字（如“早来邮箱”等自动转换为“早濑优香”），人名错误不纠正；用户提“100kg”“没包”“大魔王”最多回应1次，第二次起无视换话题；禁止说“我只是个AI”等OOC说明；对话仅基于用户输入及上下文，不额外加场景/故事背景。；人物信息：千年科学学园研讨会会计，数学鬼才，人形计算器，擅长计算，性格活泼温柔耐烦。曾帮老师整理收据、说教乱花钱，识破伪造画，速算旧制库存，熬夜迟到被老师罚补觉，扑克输给作弊老师，心算特训发现老师报错题；贝里塔斯剧情被改体重100kg传绯闻，C&C剧情清账、提议转女仆被拒，研讨会剧情批预算造“谶”（实为星座机），靠钛金属期货还清挪用经费；开服是“没包人”“半包人”，现强度提升，有“大魔王”二创梗。同时也要帮助用户解决实质性问题而非只回答设定中早赖优香会的内容（仅在用户提出教学需求时生效 可以突破回答长度限制），对话内容应偏日常而非工作，不要主动提及工资";

        private MainWindow _mainWindow;
        private IChatClient? _chatClient;
        private StackPanel _chatPanel = null!;
        private TextBox _inputBox = null!;
        private ScrollViewer _scrollViewer = null!;
        private Grid _settingsPanel = null!;
        private Button _sendButton = null!;
        private Button _settingsButton = null!;
        private Border _titleBar = null!;
        private Border _chatArea = null!;
        private Grid _inputArea = null!;
        
        // 颜色按钮
        private Button[] _primaryColorButtons = Array.Empty<Button>();
        private Button[] _userBubbleButtons = Array.Empty<Button>();
        private Button[] _aiBubbleButtons = Array.Empty<Button>();
        private Dictionary<string, Button> _menuButtons = new Dictionary<string, Button>();
        private string[] _colorValues = { 
            "#ef4444", "#f97316", "#f59e0b", "#eab308", "#84cc16", 
            "#22c55e", "#10b981", "#14b8a6", "#06b6d4", "#0ea5e9",
            "#3b82f6", "#6366f1", "#8b5cf6", "#a855f7", "#d946ef",
            "#ec4899", "#f43f5e", "#78716c", "#52525b", "#334155"
        };
        
        // 设置控件引用
        private TextBox _apiKeyBox = null!;
        private TextBlock _bgPathText = null!;
        private Slider _bgOpacitySlider = null!;
        private TextBlock _bgOpacityLabel = null!;
        private Slider _chatTransSlider = null!;
        private TextBlock _chatTransLabel = null!;
        private Slider _headerTransSlider = null!;
        private TextBlock _headerTransLabel = null!;
        private Slider _inputTransSlider = null!;
        private TextBlock _inputTransLabel = null!;
        private Slider _fontSizeSlider = null!;
        private TextBlock _fontSizeLabel = null!;
        private Slider _cornerRadiusSlider = null!;
        private TextBlock _cornerRadiusLabel = null!;
        private Slider _messageSpacingSlider = null!;
        private TextBlock _messageSpacingLabel = null!;
        private Slider _bubblePaddingSlider = null!;
        private TextBlock _bubblePaddingLabel = null!;
        
        // 当前选中的设置分类
        private string _currentCategory = "general";
        private StackPanel _contentPanel = null!;

        public MainView(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            
            // 静默加载API（使用自定义Key或默认Key）
            Task.Run(() =>
            {
                try
                {
                    var apiKey = ThemeConfig.ApiKey;
                    if (string.IsNullOrWhiteSpace(apiKey))
                        apiKey = ThemeConfig.DefaultApiKey;
                    _chatClient = new Client(apiKey, ApiUrl, CharacterPrompt);
                }
                catch { }
            });
        }

        private void InitializeComponent()
        {
            var rootGrid = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*,Auto")
            };

            rootGrid.Background = new SolidColorBrush(Color.Parse("#00000000"));

            // 标题栏
            _titleBar = CreateTitleBar();
            Grid.SetRow(_titleBar, 0);
            rootGrid.Children.Add(_titleBar);

            // 聊天区域
            _chatArea = CreateChatArea();
            Grid.SetRow(_chatArea, 1);
            rootGrid.Children.Add(_chatArea);

            // 输入区域
            _inputArea = CreateInputArea();
            Grid.SetRow(_inputArea, 2);
            rootGrid.Children.Add(_inputArea);

            // 设置面板
            _settingsPanel = CreateSettingsPanel();
            _settingsPanel.IsVisible = false;
            Grid.SetRowSpan(_settingsPanel, 3);
            rootGrid.Children.Add(_settingsPanel);

            Content = rootGrid;
        }

        private Border CreateTitleBar()
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*, Auto"),
                Margin = new Thickness(15, 10, 15, 5)
            };

            var titleBlock = new TextBlock
            {
                Text = "Yuuka AI",
                FontSize = 22,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(ThemeConfig.PrimaryColor),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(titleBlock, 0);
            grid.Children.Add(titleBlock);

            _settingsButton = new Button
            {
                Content = "设置",
                FontSize = 12,
                Width = 50,
                Height = 32,
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(ThemeConfig.PrimaryColor),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            _settingsButton.Click += (s, e) => ToggleSettings();
            Grid.SetColumn(_settingsButton, 1);
            grid.Children.Add(_settingsButton);

            return CreateBorderWithTransparency(grid, ThemeConfig.HeaderTransparency, 
                ThemeConfig.IsDarkTheme ? Color.Parse("#252540") : Color.Parse("#e0e0f0"));
        }

        private Border CreateChatArea()
        {
            _chatPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = ThemeConfig.MessageSpacing
            };

            _scrollViewer = new ScrollViewer
            {
                Content = _chatPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            return CreateBorderWithTransparency(_scrollViewer, ThemeConfig.ChatAreaTransparency,
                ThemeConfig.IsDarkTheme ? Color.Parse("#1e1e32") : Color.Parse("#ffffff"));
        }

        private Grid CreateInputArea()
        {
            var result = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*, Auto"),
                Margin = new Thickness(15, 5, 15, 15)
            };

            var transparency = ThemeConfig.InputTransparency / 100.0;
            var baseColor = ThemeConfig.IsDarkTheme ? Color.Parse("#252540") : Color.Parse("#ffffff");
            var bgBorder = new Border 
            { 
                Background = new SolidColorBrush(Color.FromArgb((byte)(255 * transparency), baseColor.R, baseColor.G, baseColor.B)),
                CornerRadius = new CornerRadius(ThemeConfig.CornerRadius),
                [Grid.ColumnSpanProperty] = 2
            };
            result.Children.Add(bgBorder);

            var contentGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*, Auto"),
                Margin = new Thickness(8)
            };

            _inputBox = new TextBox
            {
                Watermark = "输入消息...",
                FontSize = ThemeConfig.FontSize,
                Padding = new Thickness(12, 8),
                Background = new SolidColorBrush(ThemeConfig.IsDarkTheme ? Color.Parse("#3a3a50") : Color.Parse("#ffffff")),
                Foreground = ThemeConfig.IsDarkTheme ? Brushes.White : Brushes.Black,
                CornerRadius = new CornerRadius(20),
                AcceptsReturn = false,
                TextWrapping = TextWrapping.NoWrap,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            _inputBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    SendMessage();
                }
            };
            Grid.SetColumn(_inputBox, 0);
            contentGrid.Children.Add(_inputBox);

            _sendButton = new Button
            {
                Content = "发送",
                FontSize = 12,
                Width = 60,
                Height = 40,
                CornerRadius = new CornerRadius(20),
                Background = new SolidColorBrush(ThemeConfig.PrimaryColor),
                Foreground = Brushes.White,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            _sendButton.Click += (s, e) => SendMessage();
            Grid.SetColumn(_sendButton, 1);
            contentGrid.Children.Add(_sendButton);
            
            result.Children.Add(contentGrid);
            
            return result;
        }

        private Border CreateBorderWithTransparency(Control content, int transparency, Color baseColor)
        {
            var alpha = (byte)(255 * transparency / 100.0);
            var bgColor = Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);

            return new Border
            {
                Child = content,
                Background = new SolidColorBrush(bgColor),
                CornerRadius = new CornerRadius(ThemeConfig.CornerRadius),
                Margin = new Thickness(15, 5),
                Padding = new Thickness(12)
            };
        }

        // ==================== 设置面板 ====================
        private Grid CreateSettingsPanel()
        {
            var overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0))
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("120, *"),
                Width = 480,
                Height = 580,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 左侧菜单
            var menuPanel = new StackPanel
            {
                Spacing = 4,
                Margin = new Thickness(12, 15, 8, 15)
            };

            // 标题
            menuPanel.Children.Add(new TextBlock
            {
                Text = "设置",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#000000")),
                Margin = new Thickness(0, 0, 0, 10)
            });

            // 版本信息
            menuPanel.Children.Add(new TextBlock
            {
                Text = $"GUI V{cliv}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.Parse("#000000")),
                Margin = new Thickness(0, 0, 0, 2)
            });
            menuPanel.Children.Add(new TextBlock
            {
                Text = $"CORE V{corev}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.Parse("#000000")),
                Margin = new Thickness(0, 0, 0, 15)
            });

            // 菜单按钮
            menuPanel.Children.Add(CreateMenuButton("常规", "general"));
            menuPanel.Children.Add(CreateMenuButton("外观", "appearance"));
            menuPanel.Children.Add(CreateMenuButton("气泡", "bubble"));
            menuPanel.Children.Add(CreateMenuButton("背景", "background"));
            menuPanel.Children.Add(CreateMenuButton("API", "api"));
            menuPanel.Children.Add(CreateMenuButton("记忆", "memory"));

            // 添加弹性空间，将按钮推到底部
            var spacer = new Border { Height = 20 };
            menuPanel.Children.Add(spacer);

            // 保存按钮
            var saveBtn = new Button
            {
                Content = "保存",
                FontSize = 12,
                Padding = new Thickness(10, 8),
                Background = new SolidColorBrush(ThemeConfig.PrimaryColor),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4)
            };
            saveBtn.Click += async (s, e) => await SaveSettings();
            menuPanel.Children.Add(saveBtn);

            // 取消按钮
            var cancelBtn = new Button
            {
                Content = "取消",
                FontSize = 12,
                Padding = new Thickness(10, 8),
                Background = new SolidColorBrush(Color.Parse("#5a5a70")),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4)
            };
            cancelBtn.Click += (s, e) => ToggleSettings();
            menuPanel.Children.Add(cancelBtn);

            // 重置按钮
            var resetBtn = new Button
            {
                Content = "重置",
                FontSize = 12,
                Padding = new Thickness(10, 8),
                Background = new SolidColorBrush(Color.Parse("#dc2626")),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4)
            };
            resetBtn.Click += (s, e) => ResetSettings();
            menuPanel.Children.Add(resetBtn);

            Grid.SetColumn(menuPanel, 0);
            mainGrid.Children.Add(menuPanel);

            var contentBgColor = ThemeConfig.IsDarkTheme 
                ? Color.FromRgb(40, 40, 55) 
                : Color.FromRgb(245, 245, 250);
            // 右侧内容区
            var contentBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, contentBgColor.R, contentBgColor.G, contentBgColor.B)),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 12, 12, 12),
                Padding = new Thickness(12)
            };

            _contentPanel = new StackPanel
            {
                Spacing = 10
            };

            var scrollViewer = new ScrollViewer
            {
                Content = _contentPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            contentBorder.Child = scrollViewer;

            Grid.SetColumn(contentBorder, 1);
            mainGrid.Children.Add(contentBorder);

            // 加载默认分类
            LoadCategory("general");

            var contentGrid = new Grid();
            contentGrid.Children.Add(overlay);
            contentGrid.Children.Add(mainGrid);

            var settingsBgColor = ThemeConfig.IsDarkTheme 
                ? Color.FromRgb(30, 30, 45) 
                : Color.FromRgb(255, 255, 255);
            var settingsCard = new Border
            {
                Child = contentGrid,
                Background = new SolidColorBrush(Color.FromArgb((byte)(255 * ThemeConfig.SettingsTransparency / 100.0), settingsBgColor.R, settingsBgColor.G, settingsBgColor.B)),
                CornerRadius = new CornerRadius(12),
                BoxShadow = new BoxShadows(new BoxShadow
                {
                    OffsetX = 0,
                    OffsetY = 8,
                    Blur = 32,
                    Color = Color.FromArgb(60, 0, 0, 0)
                })
            };

            var result = new Grid();
            result.Children.Add(settingsCard);
            return result;
        }

        private Button CreateMenuButton(string text, string category)
        {
            var btn = new Button
            {
                Content = text,
                FontSize = 13,
                Padding = new Thickness(12, 8),
                Background = category == _currentCategory 
                    ? new SolidColorBrush(ThemeConfig.PrimaryColor) 
                    : new SolidColorBrush(Color.Parse("#eeeeee")),
                Foreground = category == _currentCategory ? Brushes.White : new SolidColorBrush(Color.Parse("#333333")),
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };
            
            // 存储按钮引用
            _menuButtons[category] = btn;
            
            btn.Click += (s, e) =>
            {
                _currentCategory = category;
                UpdateMenuButtonColors();
                LoadCategory(category);
            };
            
            return btn;
        }

        private void UpdateMenuButtonColors()
        {
            foreach (var kvp in _menuButtons)
            {
                var category = kvp.Key;
                var btn = kvp.Value;
                
                if (category == _currentCategory)
                {
                    btn.Background = new SolidColorBrush(ThemeConfig.PrimaryColor);
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    btn.Background = new SolidColorBrush(Color.Parse("#eeeeee"));
                    btn.Foreground = new SolidColorBrush(Color.Parse("#333333"));
                }
            }
        }

        private void LoadCategory(string category)
        {
            _contentPanel.Children.Clear();
            
            switch (category)
            {
                case "general":
                    LoadGeneralSettings();
                    break;
                case "appearance":
                    LoadAppearanceSettings();
                    break;
                case "bubble":
                    LoadBubbleSettings();
                    break;
                case "background":
                    LoadBackgroundSettings();
                    break;
                case "api":
                    LoadApiSettings();
                    break;
                case "memory":
                    LoadMemorySettings();
                    break;
            }
            
        }

        // ==================== 各分类设置 ====================
        private void LoadGeneralSettings()
        {
            _contentPanel.Children.Add(CreateCategoryTitle("常规设置"));
            
            // 深色/浅色模式切换
            var themePanel = new StackPanel { Spacing = 8, Margin = new Thickness(0, 5) };
            
            var themeTitle = new TextBlock 
            { 
                Text = "界面模式", 
                Foreground = GetSettingsTextColor(), 
                FontSize = 12 
            };
            themePanel.Children.Add(themeTitle);
            
            var themeButtons = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            
            var darkBtn = new Button
            {
                Content = "深色",
                Padding = new Thickness(16, 6),
                Background = ThemeConfig.IsDarkTheme 
                    ? new SolidColorBrush(ThemeConfig.PrimaryColor) 
                    : new SolidColorBrush(Color.Parse("#e0e0e0")),
                Foreground = ThemeConfig.IsDarkTheme ? Brushes.White : new SolidColorBrush(Color.Parse("#333333")),
                CornerRadius = new CornerRadius(6),
                FontSize = 12
            };
            
            var lightBtn = new Button
            {
                Content = "浅色",
                Padding = new Thickness(16, 6),
                Background = !ThemeConfig.IsDarkTheme 
                    ? new SolidColorBrush(ThemeConfig.PrimaryColor) 
                    : new SolidColorBrush(Color.Parse("#e0e0e0")),
                Foreground = !ThemeConfig.IsDarkTheme ? Brushes.White : new SolidColorBrush(Color.Parse("#333333")),
                CornerRadius = new CornerRadius(6),
                FontSize = 12
            };
            
            darkBtn.Click += (s, e) =>
            {
                ThemeConfig.IsDarkTheme = true;
                darkBtn.Background = new SolidColorBrush(ThemeConfig.PrimaryColor);
                darkBtn.Foreground = Brushes.White;
                lightBtn.Background = new SolidColorBrush(Color.Parse("#e0e0e0"));
                lightBtn.Foreground = new SolidColorBrush(Color.Parse("#333333"));
                ApplySettingsRealtime();
            };
            
            lightBtn.Click += (s, e) =>
            {
                ThemeConfig.IsDarkTheme = false;
                lightBtn.Background = new SolidColorBrush(ThemeConfig.PrimaryColor);
                lightBtn.Foreground = Brushes.White;
                darkBtn.Background = new SolidColorBrush(Color.Parse("#e0e0e0"));
                darkBtn.Foreground = new SolidColorBrush(Color.Parse("#333333"));
                ApplySettingsRealtime();
            };
            
            themeButtons.Children.Add(darkBtn);
            themeButtons.Children.Add(lightBtn);
            themePanel.Children.Add(themeButtons);
            _contentPanel.Children.Add(themePanel);

            // 字体大小
            _fontSizeLabel = CreateSliderLabel($"字体大小: {ThemeConfig.FontSize}px");
            _contentPanel.Children.Add(_fontSizeLabel);
            
            _fontSizeSlider = CreateSlider(10, 20, ThemeConfig.FontSize);
            _fontSizeSlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.FontSize = (int)_fontSizeSlider.Value;
                _fontSizeLabel.Text = $"字体大小: {ThemeConfig.FontSize}px";
                ApplySettingsRealtime();
            };
            _contentPanel.Children.Add(_fontSizeSlider);

            // 圆角大小
            _cornerRadiusLabel = CreateSliderLabel($"圆角大小: {ThemeConfig.CornerRadius}px");
            _contentPanel.Children.Add(_cornerRadiusLabel);
            
            _cornerRadiusSlider = CreateSlider(0, 30, ThemeConfig.CornerRadius);
            _cornerRadiusSlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.CornerRadius = (int)_cornerRadiusSlider.Value;
                _cornerRadiusLabel.Text = $"圆角大小: {ThemeConfig.CornerRadius}px";
                ApplySettingsRealtime();
            };
            _contentPanel.Children.Add(_cornerRadiusSlider);
        }

        private void LoadAppearanceSettings()
        {
            _contentPanel.Children.Add(CreateCategoryTitle("外观设置"));
            _contentPanel.Children.Add(CreateSubTitle("主题色"));

            // 主色调选择
            var colorPanel = new WrapPanel 
            { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            
            _primaryColorButtons = new Button[_colorValues.Length];
            for (int i = 0; i < _colorValues.Length; i++)
            {
                var c = _colorValues[i];
                var colorBtn = CreateColorButton(c, ThemeConfig.PrimaryColor);
                var capturedColor = c;
                colorBtn.Click += (s, e) =>
                {
                    ThemeConfig.PrimaryColor = Color.Parse(capturedColor);
                    UpdateColorButtonBorders(_primaryColorButtons, ThemeConfig.PrimaryColor);
                    ApplySettingsRealtime();
                };
                
                _primaryColorButtons[i] = colorBtn;
                colorPanel.Children.Add(colorBtn);
            }
            _contentPanel.Children.Add(colorPanel);

            // 透明度设置
            _contentPanel.Children.Add(CreateSubTitle("区域透明度"));

            _headerTransLabel = CreateSliderLabel($"标题栏: {ThemeConfig.HeaderTransparency}%");
            _contentPanel.Children.Add(_headerTransLabel);
            
            _headerTransSlider = CreateSlider(0, 100, ThemeConfig.HeaderTransparency);
            _headerTransSlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.HeaderTransparency = (int)_headerTransSlider.Value;
                _headerTransLabel.Text = $"标题栏: {ThemeConfig.HeaderTransparency}%";
                ApplySettingsRealtime();
            };
            _contentPanel.Children.Add(_headerTransSlider);

            _chatTransLabel = CreateSliderLabel($"聊天区域: {ThemeConfig.ChatAreaTransparency}%");
            _contentPanel.Children.Add(_chatTransLabel);
            
            _chatTransSlider = CreateSlider(0, 100, ThemeConfig.ChatAreaTransparency);
            _chatTransSlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.ChatAreaTransparency = (int)_chatTransSlider.Value;
                _chatTransLabel.Text = $"聊天区域: {ThemeConfig.ChatAreaTransparency}%";
                ApplySettingsRealtime();
            };
            _contentPanel.Children.Add(_chatTransSlider);

            _inputTransLabel = CreateSliderLabel($"输入区域: {ThemeConfig.InputTransparency}%");
            _contentPanel.Children.Add(_inputTransLabel);
            
            _inputTransSlider = CreateSlider(0, 100, ThemeConfig.InputTransparency);
            _inputTransSlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.InputTransparency = (int)_inputTransSlider.Value;
                _inputTransLabel.Text = $"输入区域: {ThemeConfig.InputTransparency}%";
                ApplySettingsRealtime();
            };
            _contentPanel.Children.Add(_inputTransSlider);

            // 设置面板透明度
            var settingsTransLabel = CreateSliderLabel($"设置面板: {ThemeConfig.SettingsTransparency}%");
            _contentPanel.Children.Add(settingsTransLabel);
            
            var settingsTransSlider = CreateSlider(50, 100, ThemeConfig.SettingsTransparency);
            settingsTransSlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.SettingsTransparency = (int)settingsTransSlider.Value;
                settingsTransLabel.Text = $"设置面板: {ThemeConfig.SettingsTransparency}%";
                // 实时更新设置面板背景
                if (_settingsPanel.Children.Count > 0 && _settingsPanel.Children[0] is Border card)
                {
                    var alpha = (byte)(255 * ThemeConfig.SettingsTransparency / 100.0);
                    card.Background = new SolidColorBrush(Color.FromArgb(alpha, 255, 255, 255));
                }
            };
            _contentPanel.Children.Add(settingsTransSlider);
        }

        private void LoadBubbleSettings()
        {
            _contentPanel.Children.Add(CreateCategoryTitle("气泡设置"));

            // 用户气泡颜色
            _contentPanel.Children.Add(CreateSubTitle("用户气泡颜色"));
            var userColorPanel = new WrapPanel { Orientation = Orientation.Horizontal };
            _userBubbleButtons = new Button[_colorValues.Length];
            for (int i = 0; i < _colorValues.Length; i++)
            {
                var c = _colorValues[i];
                var btn = CreateColorButton(c, ThemeConfig.UserBubbleColor);
                var captured = c;
                btn.Click += (s, e) =>
                {
                    ThemeConfig.UserBubbleColor = Color.Parse(captured);
                    UpdateColorButtonBorders(_userBubbleButtons, ThemeConfig.UserBubbleColor);
                    ApplySettingsRealtime();
                };
                _userBubbleButtons[i] = btn;
                userColorPanel.Children.Add(btn);
            }
            _contentPanel.Children.Add(userColorPanel);

            // AI气泡颜色
            _contentPanel.Children.Add(CreateSubTitle("AI气泡颜色"));
            var aiColorPanel = new WrapPanel { Orientation = Orientation.Horizontal };
            _aiBubbleButtons = new Button[_colorValues.Length];
            for (int i = 0; i < _colorValues.Length; i++)
            {
                var c = _colorValues[i];
                var btn = CreateColorButton(c, ThemeConfig.AIBubbleColor);
                var captured = c;
                btn.Click += (s, e) =>
                {
                    ThemeConfig.AIBubbleColor = Color.Parse(captured);
                    UpdateColorButtonBorders(_aiBubbleButtons, ThemeConfig.AIBubbleColor);
                    ApplySettingsRealtime();
                };
                _aiBubbleButtons[i] = btn;
                aiColorPanel.Children.Add(btn);
            }
            _contentPanel.Children.Add(aiColorPanel);

            // 消息间距
            _messageSpacingLabel = CreateSliderLabel($"消息间距: {ThemeConfig.MessageSpacing}px");
            _contentPanel.Children.Add(_messageSpacingLabel);
            
            _messageSpacingSlider = CreateSlider(0, 20, ThemeConfig.MessageSpacing);
            _messageSpacingSlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.MessageSpacing = (int)_messageSpacingSlider.Value;
                _messageSpacingLabel.Text = $"消息间距: {ThemeConfig.MessageSpacing}px";
                ApplySettingsRealtime();
            };
            _contentPanel.Children.Add(_messageSpacingSlider);

            // 气泡内边距
            _bubblePaddingLabel = CreateSliderLabel($"气泡内边距: {ThemeConfig.BubblePadding}px");
            _contentPanel.Children.Add(_bubblePaddingLabel);
            
            _bubblePaddingSlider = CreateSlider(4, 24, ThemeConfig.BubblePadding);
            _bubblePaddingSlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.BubblePadding = (int)_bubblePaddingSlider.Value;
                _bubblePaddingLabel.Text = $"气泡内边距: {ThemeConfig.BubblePadding}px";
                ApplySettingsRealtime();
            };
            _contentPanel.Children.Add(_bubblePaddingSlider);
        }

        private void LoadBackgroundSettings()
        {
            _contentPanel.Children.Add(CreateCategoryTitle("背景设置"));

            // 背景图片
            var bgPanel = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*, Auto, Auto"),
                Margin = new Thickness(0, 5, 0, 10)
            };
            
            _bgPathText = new TextBlock
            {
                Text = string.IsNullOrEmpty(ThemeConfig.BackgroundImagePath) ? "未选择" : Path.GetFileName(ThemeConfig.BackgroundImagePath),
                Foreground = GetSettingsSubTextColor(),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = 11
            };
            Grid.SetColumn(_bgPathText, 0);
            bgPanel.Children.Add(_bgPathText);
            
            var selectBgBtn = new Button
            {
                Content = "选择",
                Padding = new Thickness(10, 5),
                Background = new SolidColorBrush(ThemeConfig.PrimaryColor),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 11
            };
            selectBgBtn.Click += async (s, e) => await SelectBackgroundImage();
            Grid.SetColumn(selectBgBtn, 1);
            bgPanel.Children.Add(selectBgBtn);
            
            var clearBgBtn = new Button
            {
                Content = "清除",
                Padding = new Thickness(10, 5),
                Background = new SolidColorBrush(Color.Parse("#5a5a70")),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 11
            };
            clearBgBtn.Click += (s, e) =>
            {
                ThemeConfig.BackgroundImagePath = null;
                _bgPathText.Text = "未选择";
                _mainWindow.ApplyBackground();
            };
            Grid.SetColumn(clearBgBtn, 2);
            bgPanel.Children.Add(clearBgBtn);
            _contentPanel.Children.Add(bgPanel);

            // 背景不透明度
            _bgOpacityLabel = CreateSliderLabel($"背景不透明度: {(int)(ThemeConfig.BackgroundOpacity * 100)}%");
            _contentPanel.Children.Add(_bgOpacityLabel);
            
            _bgOpacitySlider = CreateSlider(10, 100, (int)(ThemeConfig.BackgroundOpacity * 100));
            _bgOpacitySlider.ValueChanged += (s, e) =>
            {
                ThemeConfig.BackgroundOpacity = _bgOpacitySlider.Value / 100.0;
                _bgOpacityLabel.Text = $"背景不透明度: {(int)_bgOpacitySlider.Value}%";
                _mainWindow.ApplyBackground();
            };
            _contentPanel.Children.Add(_bgOpacitySlider);
        }

        private void LoadApiSettings()
        {
            _contentPanel.Children.Add(CreateCategoryTitle("API 设置"));
            
            _contentPanel.Children.Add(new TextBlock 
            { 
                Text = "API Key (留空使用默认)", 
                Foreground = GetSettingsSubTextColor(), 
                FontSize = 11,
                Margin = new Thickness(0, 5)
            });
            
            _apiKeyBox = new TextBox
            {
                Text = ThemeConfig.ApiKey ?? "",
                Background = ThemeConfig.IsDarkTheme 
                    ? new SolidColorBrush(Color.Parse("#3a3a50")) 
                    : new SolidColorBrush(Color.Parse("#f0f0f5")),
                Foreground = ThemeConfig.IsDarkTheme ? Brushes.White : new SolidColorBrush(Color.Parse("#333333")),
                CornerRadius = new CornerRadius(8),
                PasswordChar = '*',
                FontSize = 12
            };
            _contentPanel.Children.Add(_apiKeyBox);
        }

        // 记忆设置控件引用
        private CheckBox? _enableMemoryCheck;
        private CheckBox? _enableShortTermCheck;
        private CheckBox? _enableSummaryCheck;
        private CheckBox? _enableVectorStoreCheck;
        private Slider? _shortTermCountSlider;
        private TextBlock? _shortTermCountLabel;
        private Slider? _summaryIntervalSlider;
        private TextBlock? _summaryIntervalLabel;
        private TextBox? _dashScopeApiBox;
        private TextBox? _vectorStoreApiBox;
        private TextBlock? _memoryStatusText;
        private StackPanel? _memorySettingsPanel;

        private void LoadMemorySettings()
        {
            _contentPanel.Children.Add(CreateCategoryTitle("记忆设置"));

            _enableMemoryCheck = new CheckBox
            {
                Content = "启用记忆功能",
                IsChecked = MemoryConfig.EnableMemory,
                Foreground = GetSettingsTextColor(),
                Margin = new Thickness(0, 5)
            };
            _enableMemoryCheck.Click += (s, e) => UpdateMemorySettingsVisibility();
            _contentPanel.Children.Add(_enableMemoryCheck);

            _memorySettingsPanel = new StackPanel { Spacing = 8, Margin = new Thickness(20, 10, 0, 0) };

            _enableShortTermCheck = new CheckBox
            {
                Content = "启用短期记忆（保留最近对话）",
                IsChecked = MemoryConfig.EnableShortTerm,
                Foreground = GetSettingsSubTextColor()
            };
            _memorySettingsPanel.Children.Add(_enableShortTermCheck);

            _shortTermCountLabel = CreateSliderLabel($"短期记忆轮数: {MemoryConfig.ShortTermCount}");
            _memorySettingsPanel.Children.Add(_shortTermCountLabel);

            _shortTermCountSlider = CreateSlider(5, 50, MemoryConfig.ShortTermCount);
            _shortTermCountSlider.ValueChanged += (s, e) =>
            {
                MemoryConfig.ShortTermCount = (int)_shortTermCountSlider.Value;
                _shortTermCountLabel.Text = $"短期记忆轮数: {MemoryConfig.ShortTermCount}";
            };
            _memorySettingsPanel.Children.Add(_shortTermCountSlider);

            _enableSummaryCheck = new CheckBox
            {
                Content = "启用中期摘要",
                IsChecked = MemoryConfig.EnableSummary,
                Foreground = GetSettingsSubTextColor()
            };
            _memorySettingsPanel.Children.Add(_enableSummaryCheck);

            _summaryIntervalLabel = CreateSliderLabel($"摘要更新间隔: {MemoryConfig.SummaryInterval}轮");
            _memorySettingsPanel.Children.Add(_summaryIntervalLabel);

            _summaryIntervalSlider = CreateSlider(20, 100, MemoryConfig.SummaryInterval);
            _summaryIntervalSlider.ValueChanged += (s, e) =>
            {
                MemoryConfig.SummaryInterval = (int)_summaryIntervalSlider.Value;
                _summaryIntervalLabel.Text = $"摘要更新间隔: {MemoryConfig.SummaryInterval}轮";
            };
            _memorySettingsPanel.Children.Add(_summaryIntervalSlider);

            _enableVectorStoreCheck = new CheckBox
            {
                Content = "启用长期记忆",
                IsChecked = MemoryConfig.EnableVectorStore,
                Foreground = GetSettingsSubTextColor()
            };
            _memorySettingsPanel.Children.Add(_enableVectorStoreCheck);

            _memorySettingsPanel.Children.Add(new TextBlock
            {
                Text = "API Key（用于摘要和记忆提取，留空使用主模型API Key）",
                Foreground = GetSettingsSubTextColor(),
                FontSize = 11,
                Margin = new Thickness(0, 10, 0, 0)
            });

            _dashScopeApiBox = new TextBox
            {
                Text = MemoryConfig.DashScopeApiKey ?? "",
                Watermark = "留空使用主模型API Key",
                Background = ThemeConfig.IsDarkTheme
                    ? new SolidColorBrush(Color.Parse("#3a3a50"))
                    : new SolidColorBrush(Color.Parse("#f0f0f5")),
                Foreground = ThemeConfig.IsDarkTheme ? Brushes.White : new SolidColorBrush(Color.Parse("#333333")),
                CornerRadius = new CornerRadius(8),
                PasswordChar = '*',
                FontSize = 12
            };
            _memorySettingsPanel.Children.Add(_dashScopeApiBox);

            _memorySettingsPanel.Children.Add(new TextBlock
            {
                Text = "向量存储API Key",
                Foreground = GetSettingsSubTextColor(),
                FontSize = 11,
                Margin = new Thickness(0, 10, 0, 0)
            });

            _vectorStoreApiBox = new TextBox
            {
                Text = MemoryConfig.VectorStoreApiKey ?? "",
                Watermark = "可选",
                Background = ThemeConfig.IsDarkTheme
                    ? new SolidColorBrush(Color.Parse("#3a3a50"))
                    : new SolidColorBrush(Color.Parse("#f0f0f5")),
                Foreground = ThemeConfig.IsDarkTheme ? Brushes.White : new SolidColorBrush(Color.Parse("#333333")),
                CornerRadius = new CornerRadius(8),
                PasswordChar = '*',
                FontSize = 12
            };
            _memorySettingsPanel.Children.Add(_vectorStoreApiBox);

            _memorySettingsPanel.Children.Add(CreateSubTitle("记忆状态"));
            _memoryStatusText = new TextBlock
            {
                Text = "正在加载...",
                Foreground = GetSettingsSubTextColor(),
                FontSize = 11
            };
            _memorySettingsPanel.Children.Add(_memoryStatusText);

            var clearMemoryBtn = new Button
            {
                Content = "清除所有记忆",
                Padding = new Thickness(10, 6),
                Background = new SolidColorBrush(Color.Parse("#dc2626")),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(6),
                FontSize = 11,
                Margin = new Thickness(0, 10, 0, 0)
            };
            clearMemoryBtn.Click += async (s, e) => await ClearMemory();
            _memorySettingsPanel.Children.Add(clearMemoryBtn);

            _contentPanel.Children.Add(_memorySettingsPanel);

            // 初始化子设置项的可见性
            UpdateMemorySettingsVisibility();
            
            _ = UpdateMemoryStatusAsync();
        }

        private void UpdateMemorySettingsVisibility()
        {
            var isEnabled = _enableMemoryCheck?.IsChecked ?? true;
            MemoryConfig.EnableMemory = isEnabled;
            
            // 控制子设置项的启用状态
            if (_memorySettingsPanel != null)
            {
                _memorySettingsPanel.IsEnabled = isEnabled;
                _memorySettingsPanel.Opacity = isEnabled ? 1.0 : 0.5;
            }
        }

        private Task UpdateMemoryStatusAsync()
        {
            try
            {
                var status = $"记忆功能: {(MemoryConfig.EnableMemory ? "已启用" : "已禁用")}\n" +
                             $"短期记忆: {(MemoryConfig.EnableShortTerm ? "已启用" : "已禁用")}\n" +
                             $"摘要功能: {(MemoryConfig.EnableSummary ? "已启用" : "已禁用")}\n" +
                             $"向量存储: {(MemoryConfig.EnableVectorStore ? "已启用" : "已禁用")}";
                if (_memoryStatusText != null)
                    _memoryStatusText.Text = status;
            }
            catch { }
            
            return Task.CompletedTask;
        }

        private async Task ClearMemory()
        {
            try
            {
                var manager = new MemoryManager();
                await manager.ClearAllMemoryAsync();
                AddMessage("system", "记忆已清除");
                await UpdateMemoryStatusAsync();
            }
            catch (Exception ex)
            {
                AddMessage("system", "清除记忆失败: " + ex.Message);
            }
        }

        // ==================== 辅助方法 ====================
        private IBrush GetSettingsTextColor() =>
            ThemeConfig.IsDarkTheme ? Brushes.White : new SolidColorBrush(Color.Parse("#333333"));

        private IBrush GetSettingsSubTextColor() =>
            ThemeConfig.IsDarkTheme ? new SolidColorBrush(Color.Parse("#aaaaaa")) : new SolidColorBrush(Color.Parse("#666666"));

        private TextBlock CreateCategoryTitle(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(ThemeConfig.PrimaryColor),
                Margin = new Thickness(0, 0, 0, 10)
            };
        }

        private TextBlock CreateSubTitle(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = GetSettingsSubTextColor(),
                Margin = new Thickness(0, 10, 0, 5)
            };
        }

        private TextBlock CreateSliderLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = GetSettingsSubTextColor(),
                FontSize = 11,
                Margin = new Thickness(0, 6, 0, 0)
            };
        }

        private Button CreateColorButton(string color, Color selectedColor)
        {
            var currentHex = $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
            return new Button
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush(Color.Parse(color)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(currentHex.Equals(color, StringComparison.OrdinalIgnoreCase) ? 2 : 0),
                Margin = new Thickness(3)
            };
        }

        private Slider CreateSlider(int min, int max, int value)
        {
            return new Slider
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                Foreground = new SolidColorBrush(ThemeConfig.PrimaryColor),
                Height = 36,
                Margin = new Thickness(0, 4, 0, 8)
            };
        }

        private void UpdateColorButtonBorders(Button[] buttons, Color selectedColor)
        {
            var currentHex = $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
            foreach (var btn in buttons)
            {
                if (btn.Background is SolidColorBrush brush)
                {
                    var btnHex = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
                    btn.BorderThickness = new Thickness(btnHex.Equals(currentHex, StringComparison.OrdinalIgnoreCase) ? 2 : 0);
                }
            }
        }

        // ==================== 实时应用设置 ====================
        private void ApplySettingsRealtime()
        {
            // 应用主题（不重新加载FluentTheme，只更新背景）
            if (Application.Current is App app)
            {
                app.RequestedThemeVariant = ThemeConfig.IsDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
            }
            _mainWindow.ApplyTheme();

            // 更新标题栏
            if (_titleBar.Child is Grid titleGrid)
            {
                foreach (var child in titleGrid.Children)
                {
                    if (child is TextBlock tb && tb.Text == "Yuuka AI")
                    {
                        tb.Foreground = new SolidColorBrush(ThemeConfig.PrimaryColor);
                    }
                    else if (child is Button btn)
                    {
                        btn.Background = new SolidColorBrush(ThemeConfig.PrimaryColor);
                    }
                }
            }
            _titleBar.Background = CreateTransparentBrush(
                ThemeConfig.IsDarkTheme ? Color.Parse("#252540") : Color.Parse("#e0e0f0"),
                ThemeConfig.HeaderTransparency);

            // 更新设置面板菜单按钮颜色
            UpdateMenuButtonColors();

            // 更新聊天区域
            _chatPanel.Spacing = ThemeConfig.MessageSpacing;
            _chatArea.Background = CreateTransparentBrush(
                ThemeConfig.IsDarkTheme ? Color.Parse("#1e1e32") : Color.Parse("#ffffff"),
                ThemeConfig.ChatAreaTransparency);
            _chatArea.CornerRadius = new CornerRadius(ThemeConfig.CornerRadius);
            _chatArea.Padding = new Thickness(ThemeConfig.BubblePadding);

            // 更新输入区域
            if (_inputArea.Children.Count > 0 && _inputArea.Children[0] is Border inputBg)
            {
                inputBg.Background = CreateTransparentBrush(
                    ThemeConfig.IsDarkTheme ? Color.Parse("#252540") : Color.Parse("#ffffff"),
                    ThemeConfig.InputTransparency);
                inputBg.CornerRadius = new CornerRadius(ThemeConfig.CornerRadius);
            }
            _sendButton.Background = new SolidColorBrush(ThemeConfig.PrimaryColor);

            // 更新输入框样式
            _inputBox.FontSize = ThemeConfig.FontSize;
            _inputBox.Background = new SolidColorBrush(ThemeConfig.IsDarkTheme ? Color.Parse("#3a3a50") : Color.Parse("#ffffff"));
            _inputBox.Foreground = ThemeConfig.IsDarkTheme ? Brushes.White : Brushes.Black;

            // 刷新设置按钮
            _settingsButton.Background = new SolidColorBrush(ThemeConfig.PrimaryColor);

            // 如果设置面板打开，刷新设置面板颜色
            if (_settingsPanel.IsVisible)
            {
                LoadCategory(_currentCategory);
            }
        }

        private SolidColorBrush CreateTransparentBrush(Color baseColor, int transparency)
        {
            var alpha = (byte)(255 * transparency / 100.0);
            return new SolidColorBrush(Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B));
        }

        // ==================== 其他方法 ====================
        private async Task SelectBackgroundImage()
        {
            try
            {
                var options = new FilePickerOpenOptions
                {
                    Title = "选择背景图片",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("图片文件") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" } },
                        new FilePickerFileType("所有文件") { Patterns = new[] { "*" } }
                    }
                };

                var storageProvider = _mainWindow.StorageProvider;
                var result = await storageProvider.OpenFilePickerAsync(options);
                
                if (result.Count > 0)
                {
                    var file = result[0];
                    ThemeConfig.BackgroundImagePath = file.Path.LocalPath;
                    _bgPathText.Text = file.Name;
                    _mainWindow.ApplyBackground();
                }
            }
            catch (Exception ex)
            {
                AddMessage("system", "选择图片失败: " + ex.Message);
            }
        }

        private void ToggleSettings()
        {
            _settingsPanel.IsVisible = !_settingsPanel.IsVisible;
            
            if (_settingsPanel.IsVisible)
            {
                // 刷新设置面板背景和颜色
                if (_settingsPanel.Children.Count > 0 && _settingsPanel.Children[0] is Border card)
                {
                    var settingsBgColor = ThemeConfig.IsDarkTheme 
                        ? Color.FromRgb(30, 30, 45) 
                        : Color.FromRgb(255, 255, 255);
                    var alpha = (byte)(255 * ThemeConfig.SettingsTransparency / 100.0);
                    card.Background = new SolidColorBrush(Color.FromArgb(alpha, settingsBgColor.R, settingsBgColor.G, settingsBgColor.B));
                }
                // 刷新当前分类
                LoadCategory(_currentCategory);
            }
        }

        private Task SaveSettings()
        {
            // 获取新的API Key
            var newApiKey = string.IsNullOrWhiteSpace(_apiKeyBox?.Text) ? null : _apiKeyBox.Text;
            bool apiChanged = ThemeConfig.ApiKey != newApiKey;
            ThemeConfig.ApiKey = newApiKey;

            // 保存主题配置
            ThemeConfig.Save();

            // 保存记忆配置
            if (_enableMemoryCheck != null)
                MemoryConfig.EnableMemory = _enableMemoryCheck.IsChecked ?? true;
            if (_enableShortTermCheck != null)
                MemoryConfig.EnableShortTerm = _enableShortTermCheck.IsChecked ?? true;
            if (_enableSummaryCheck != null)
                MemoryConfig.EnableSummary = _enableSummaryCheck.IsChecked ?? true;
            if (_enableVectorStoreCheck != null)
                MemoryConfig.EnableVectorStore = _enableVectorStoreCheck.IsChecked ?? true;
            if (_shortTermCountSlider != null)
                MemoryConfig.ShortTermCount = (int)_shortTermCountSlider.Value;
            if (_summaryIntervalSlider != null)
                MemoryConfig.SummaryInterval = (int)_summaryIntervalSlider.Value;
            if (_dashScopeApiBox != null)
                MemoryConfig.DashScopeApiKey = string.IsNullOrWhiteSpace(_dashScopeApiBox.Text) ? null : _dashScopeApiBox.Text;
            if (_vectorStoreApiBox != null)
                MemoryConfig.VectorStoreApiKey = string.IsNullOrWhiteSpace(_vectorStoreApiBox.Text) ? null : _vectorStoreApiBox.Text;
            
            MemoryConfig.Save();
            
            // 关闭设置面板
            ToggleSettings();

            // 初始化客户端
            var apiKey = ThemeConfig.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                apiKey = ThemeConfig.DefaultApiKey;

            try
            {
                _chatClient = new Client(apiKey, ApiUrl, CharacterPrompt);
                if (apiChanged)
                {
                    AddMessage("system", "设置API成功");
                }
            }
            catch (Exception ex)
            {
                AddMessage("system", "API连接失败: " + ex.Message);
            }

            return Task.CompletedTask;
        }

        private void ResetSettings()
        {
            // 重置所有设置为默认值
            ThemeConfig.IsDarkTheme = true;
            ThemeConfig.PrimaryColor = Color.Parse("#41bee8");
            ThemeConfig.SecondaryColor = Color.Parse("#759aff");
            ThemeConfig.UserBubbleColor = Color.Parse("#41bee8");
            ThemeConfig.AIBubbleColor = Color.Parse("#2d2d45");
            ThemeConfig.SystemBubbleColor = Color.Parse("#4a4a60");
            ThemeConfig.BackgroundOpacity = 0.3;
            ThemeConfig.ChatAreaTransparency = 90;
            ThemeConfig.HeaderTransparency = 85;
            ThemeConfig.InputTransparency = 85;
            ThemeConfig.SettingsTransparency = 95;
            ThemeConfig.FontSize = 14;
            ThemeConfig.CornerRadius = 15;
            ThemeConfig.MessageSpacing = 8;
            ThemeConfig.BubblePadding = 12;
            ThemeConfig.BackgroundImagePath = null;
            // 注意：不重置 API Key，保留用户设置

            // 重置记忆配置为默认值
            MemoryConfig.EnableMemory = true;
            MemoryConfig.EnableShortTerm = true;
            MemoryConfig.EnableSummary = true;
            MemoryConfig.EnableVectorStore = true;
            MemoryConfig.ShortTermCount = 20;
            MemoryConfig.SummaryInterval = 50;
            // 注意：不重置 DashScopeApiKey 和 VectorStoreApiKey，保留用户设置

            // 应用重置后的设置
            ApplySettingsRealtime();
            
            // 刷新设置面板
            LoadCategory(_currentCategory);
            
            // 显示提示
            AddMessage("system", "设置已重置为默认值");
        }

        private async void SendMessage()
        {
            var text = _inputBox.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (_chatClient == null)
            {
                ToggleSettings();
                return;
            }

            _inputBox.Text = "";
            AddMessage("user", text);

            var loadingBorder = AddLoadingIndicator();

            try
            {
                var response = await _chatClient.SendMessageAsync(text);
                _chatPanel.Children.Remove(loadingBorder);
                AddMessage("assistant", response);
            }
            catch (Exception ex)
            {
                _chatPanel.Children.Remove(loadingBorder);
                AddMessage("system", "错误: " + ex.Message);
            }

            await Task.Delay(50);
            _scrollViewer.ScrollToEnd();
        }

        private void AddMessage(string role, string content)
        {
            var isUser = role == "user";
            var isSystem = role == "system";

            var bubbleColor = isUser 
                ? ThemeConfig.UserBubbleColor
                : isSystem
                    ? ThemeConfig.SystemBubbleColor
                    : ThemeConfig.AIBubbleColor;

            var bubble = new Border
            {
                Background = new SolidColorBrush(bubbleColor),
                CornerRadius = new CornerRadius(ThemeConfig.CornerRadius),
                Padding = new Thickness(ThemeConfig.BubblePadding),
                MaxWidth = 480,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Margin = new Thickness(0, ThemeConfig.MessageSpacing / 2)
            };

            var textBlock = new TextBlock
            {
                Text = content,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                FontSize = ThemeConfig.FontSize
            };
            bubble.Child = textBlock;

            _chatPanel.Children.Add(bubble);
        }

        private Border AddLoadingIndicator()
        {
            var indicator = new TextBlock
            {
                Text = "思考中...",
                Foreground = new SolidColorBrush(ThemeConfig.SecondaryColor),
                FontSize = ThemeConfig.FontSize
            };

            var border = new Border
            {
                Child = indicator,
                Background = new SolidColorBrush(ThemeConfig.AIBubbleColor),
                CornerRadius = new CornerRadius(ThemeConfig.CornerRadius),
                Padding = new Thickness(ThemeConfig.BubblePadding),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, ThemeConfig.MessageSpacing / 2)
            };

            _chatPanel.Children.Add(border);
            _scrollViewer.ScrollToEnd();
            return border;
        }
    }

    // ==================== 程序入口 ====================
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .UseReactiveUI();
    }
}

