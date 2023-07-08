using System.Windows;
using UnoOnline.ViewModel;

namespace UnoOnline.View
{
    /// <summary>
    /// Interaction logic for TelaEscolherCor.xaml
    /// </summary>
    public partial class TelaEscolherCor : Window
    {
        public TelaEscolherCor()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IRequestClose)
            {
                (DataContext as IRequestClose).RequestClose += (_, __) =>
                {
                    Close();
                };
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = (DataContext as IDialogResult).ResultadoDialog();
        }
    }
}
