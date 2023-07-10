using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UnoOnline
{
    public class Jogador : ObservableObject
    {
        private Guid _uuid;
        private string _nome;
        IList<Carta> _cartas = new ObservableCollection<Carta>();
        private bool _gritouUno;
        public string ConnectionId { get; set; }

        public Guid Uuid { get { return _uuid; } set { _uuid = value; OnPropertyChanged("Uuid"); } }
        public string Nome { get { return _nome; } set { _nome = value; OnPropertyChanged("Nome"); } }
        public IList<Carta> Cartas { get { return _cartas; } set { _cartas = value; OnPropertyChanged("Cartas"); } }

        public bool GritouUno
        {
            get
            {
                return _gritouUno;
            }

            set
            {
                _gritouUno = value;
                OnPropertyChanged("GritouUno");
            }
        }
    }
}
