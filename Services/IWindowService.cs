using System;

namespace UnoOnline.Services
{
    public interface IWindowService
    {
        void Show(object viewModel, Action<bool?, object> onCloseCallback);
        bool? ShowDialog(object viewModel, Action<bool?, object> onCloseCallback);
    }
}
