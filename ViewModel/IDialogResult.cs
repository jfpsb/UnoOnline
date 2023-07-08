namespace UnoOnline.ViewModel
{
    /// <summary>
    /// Interface Para Implementar Em ViewModels Em Que Suas Views Retornam Um Resultado Quando Abertas Usando ShowDialog.
    /// </summary>
    public interface IDialogResult
    {
        bool? ResultadoDialog();
    }
}
