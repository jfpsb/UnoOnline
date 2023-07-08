using System;

namespace UnoOnline.ViewModel
{
    /// <summary>
    /// Usado nas ViewModels para disponibilizar um evento para solicitar o fechamento da View pela ViewModel.
    /// Para adicionar o evento Close faça através da View utilizando o evento Loaded da janela.
    /// <example>
    /// <code>
    /// if (DataContext is IRequestClose)
    /// {
    ///     (DataContext as IRequestClose).RequestClose += (_, __) =>
    ///     {
    ///         Close();
    ///     };
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public interface IRequestClose
    {
        event EventHandler<EventArgs> RequestClose;
    }
}
