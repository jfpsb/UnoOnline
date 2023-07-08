using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace UnoOnline.Services
{
    public class WindowService : IWindowService
    {
        private static Dictionary<Type, Type> mapeamentoViewModel = new Dictionary<Type, Type>();

        public static void RegistrarWindow<TView, TViewModel>()
        {
            mapeamentoViewModel.Add(typeof(TViewModel), typeof(TView));
        }
        public void Show(object viewModel, Action<bool?, object> onCloseCallback)
        {
            var typeView = mapeamentoViewModel[viewModel.GetType()];
            if (typeView == null)
                throw new ArgumentException("Não foi possível abrir janela.", "typeView",
                    new NullReferenceException($"A janela referente à ViewModel {viewModel.GetType().Name} não foi registrada. Registre em VandaModaIntimaVM."));
            ShowInternal(typeView, viewModel, onCloseCallback);
        }

        public bool? ShowDialog(object viewModel, Action<bool?, object> onCloseCallback)
        {
            if (!mapeamentoViewModel.ContainsKey(viewModel.GetType()))
                throw new ArgumentException("Não foi possível abrir janela.", "typeView",
                    new NullReferenceException($"A janela referente à ViewModel {viewModel.GetType().Name} não foi registrada. Registre em VandaModaIntimaVM."));

            var typeView = mapeamentoViewModel[viewModel.GetType()];
            return ShowDialogInternal(typeView, viewModel, onCloseCallback);
        }

        private bool? ShowDialogInternal(Type typeView, object viewModel, Action<bool?, object> onCloseCallback)
        {
            var window = Activator.CreateInstance(typeView) as Window;

            if (onCloseCallback != null)
            {
                CancelEventHandler eventHandler = null;
                eventHandler = (s, e) =>
                {
                    onCloseCallback(window.DialogResult, viewModel);
                    window.Closing -= eventHandler;
                };
                window.Closing += eventHandler;
            }

            window.DataContext = viewModel;
            return window.ShowDialog();
        }

        private void ShowInternal(Type typeView, object viewModel, Action<bool?, object> onCloseCallback)
        {
            var window = Activator.CreateInstance(typeView) as Window;

            if (onCloseCallback != null)
            {
                CancelEventHandler eventHandler = null;
                eventHandler = (s, e) =>
                {
                    onCloseCallback(window.DialogResult, viewModel);
                    window.Closing -= eventHandler;
                };
                window.Closing += eventHandler;
            }

            window.DataContext = viewModel;
            window.Show();
        }
    }
}
