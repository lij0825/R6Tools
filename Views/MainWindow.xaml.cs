using System;
using Wpf.Ui.Controls;

namespace R6Tools.Views
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                var iconUri = new Uri("pack://application:,,,/r6logo.ico", UriKind.RelativeOrAbsolute);
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
            }
            catch
            {
                // 아이콘 로드 실패 시에도 앱이 튕기지 않고 정상 구동
            }
        }
    }
}
