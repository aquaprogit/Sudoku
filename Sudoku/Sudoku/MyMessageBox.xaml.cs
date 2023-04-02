using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Sudoku
{
    public partial class MyMessageBox : Window
    {
        internal string Caption
        {
            get => Title;
            set => Title = value;
        }

        internal string Message
        {
            get => TextBlock_Message.Text;
            set => TextBlock_Message.Text = value;
        }

        internal string OkButtonText
        {
            get => Label_Ok.Content.ToString();
            set => Label_Ok.Content = value.TryAddKeyboardAccellerator();
        }

        internal string CancelButtonText
        {
            get => Label_Cancel.Content.ToString();
            set => Label_Cancel.Content = value.TryAddKeyboardAccellerator();
        }

        internal string YesButtonText
        {
            get => Label_Yes.Content.ToString();
            set => Label_Yes.Content = value.TryAddKeyboardAccellerator();
        }

        internal string NoButtonText
        {
            get => Label_No.Content.ToString();
            set => Label_No.Content = value.TryAddKeyboardAccellerator();
        }

        public MessageBoxResult Result { get; set; }
        internal MyMessageBox(string message)
        {
            InitializeComponent();

            Message = message;
            Image_MessageBox.Visibility = Visibility.Collapsed;
            DisplayButtons(MessageBoxButton.OK);
        }

        internal MyMessageBox(string message, string caption)
        {
            InitializeComponent();

            Message = message;
            Caption = caption;
            Image_MessageBox.Visibility = Visibility.Collapsed;
            DisplayButtons(MessageBoxButton.OK);
        }

        internal MyMessageBox(string message, string caption, MessageBoxButton button)
        {
            InitializeComponent();

            Message = message;
            Caption = caption;
            Image_MessageBox.Visibility = Visibility.Collapsed;

            DisplayButtons(button);
        }

        internal MyMessageBox(string message, string caption, MessageBoxImage image)
        {
            InitializeComponent();

            Message = message;
            Caption = caption;
            DisplayImage(image);
            DisplayButtons(MessageBoxButton.OK);
        }

        internal MyMessageBox(string message, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            InitializeComponent();

            Message = message;
            Caption = caption;
            Image_MessageBox.Visibility = Visibility.Collapsed;

            DisplayButtons(button);
            DisplayImage(image);
        }

        private void DisplayButtons(MessageBoxButton button)
        {
            List<System.Windows.Controls.Button> resultButtons = new List<System.Windows.Controls.Button>()
            {
                Button_OK,
                Button_Cancel,
                Button_Yes,
                Button_No
            };
            Dictionary<MessageBoxButton, List<System.Windows.Controls.Button>> dependencyButton = new Dictionary<MessageBoxButton, List<System.Windows.Controls.Button>>()
            {
                {MessageBoxButton.OK, new List<System.Windows.Controls.Button>() {Button_OK}},
                {MessageBoxButton.OKCancel, new List<System.Windows.Controls.Button>() {Button_OK, Button_Cancel}},
                {MessageBoxButton.YesNo, new List<System.Windows.Controls.Button>() {Button_Yes, Button_No}},
                {MessageBoxButton.YesNoCancel, new List<System.Windows.Controls.Button>() {Button_Yes, Button_No, Button_Cancel}}
            };
            void HideExept(List<System.Windows.Controls.Button> buttons)
            {
                resultButtons.ForEach(c => c.Visibility = Visibility.Collapsed);
                buttons.ToList().ForEach(c => c.Visibility = Visibility.Visible);
                buttons[0].Focus();
            }
            HideExept(dependencyButton[button]);
        }

        private void DisplayImage(MessageBoxImage image)
        {
            string path;

            switch (image)
            {
                case MessageBoxImage.Exclamation:
                    path = "/Assets/warning.png";
                    break;
                case MessageBoxImage.Error:
                    path = "/Assets/error.png";
                    break;
                case MessageBoxImage.Information:
                    path = "/Assets/info.png";
                    break;
                default:
                    path = "/Assets/warning.png";
                    break;
            }

            Image_MessageBox.Source = new BitmapImage(new Uri(path, UriKind.Relative));
            Image_MessageBox.Visibility = Visibility.Visible;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }
    }
    public partial class MyMessageBox
    {
        public static MessageBoxResult Show(string messageBoxText)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText);
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption);
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText) {
                Owner = owner
            };
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption) {
                Owner = owner
            };
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, button);
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, button, icon);
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowOK(string messageBoxText, string caption, string okButtonText)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, MessageBoxButton.OK) {
                OkButtonText = okButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowOK(string messageBoxText, string caption, string okButtonText, MessageBoxImage icon)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, MessageBoxButton.OK, icon) {
                OkButtonText = okButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, MessageBoxButton.OKCancel) {
                OkButtonText = okButtonText,
                CancelButtonText = cancelButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText, MessageBoxImage icon)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, MessageBoxButton.OKCancel, icon) {
                OkButtonText = okButtonText,
                CancelButtonText = cancelButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, MessageBoxButton.YesNo) {
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText, MessageBoxImage icon)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, MessageBoxButton.YesNo, icon) {
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, MessageBoxButton.YesNoCancel) {
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText, MessageBoxImage icon)
        {
            MyMessageBox msg = new MyMessageBox(messageBoxText, caption, MessageBoxButton.YesNoCancel, icon) {
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }
    }
}