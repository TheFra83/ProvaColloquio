using System.Windows;

namespace ProvaColloquio
{
    class WindowService : IWindowService
    {
        public void showWindow(object viewModel)
        {
            var win = new Window();
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.Content = viewModel;
            win.Show();
        }
    }
}
