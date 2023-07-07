using System.Collections.Generic;

namespace UnoOnline.Model
{
    public class StatusPartida : ObservableObject
    {
        private Carta _ultimaCarta;
        private LinkedList<Jogador> _jogadores = new LinkedList<Jogador>();
        private string _status = "Esperando jogadores";
        private bool _sentidoEstaHorario = true;
        private Jogador _jogadorDaVez;

        public StatusPartida()
        {

        }

        public void MudarSentido()
        {
            SentidoEstaHorario = !SentidoEstaHorario;
        }
        public void AdicionaJogador(Jogador jogador)
        {
            Jogadores.AddLast(jogador);

            if (Jogadores.Count == 1)
            {
                JogadorDaVez = jogador;
            }
        }
        public Carta UltimaCarta
        {
            get { return _ultimaCarta; }
            set
            {
                _ultimaCarta = value;
                OnPropertyChanged("UltimaCarta");
            }
        }
        public LinkedList<Jogador> Jogadores
        {
            get { return _jogadores; }
            set
            {
                _jogadores = value;
                OnPropertyChanged("Jogadores");
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public bool SentidoEstaHorario
        {
            get => _sentidoEstaHorario; set
            {
                _sentidoEstaHorario = value;
                OnPropertyChanged("SentidoEstaHorario");
            }
        }

        public Jogador JogadorDaVez
        {
            get => _jogadorDaVez; set
            {
                _jogadorDaVez = value;
                OnPropertyChanged("JogadorDaVez");
            }
        }
    }
}
