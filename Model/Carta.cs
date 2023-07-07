using System;

namespace UnoOnline
{
    public class Carta : ObservableObject
    {
        private Guid _uuid;
        private string _codigo;
        private string _imagem;
        private int _quantidade;
        private string _tipo;
        private string _cor;
        private bool _estaEmBaralho = true;

        public Guid Uuid
        {
            get { return _uuid; }
            set { _uuid = value;
                OnPropertyChanged("Uuid");
            }
        }

        public string Codigo
        {
            get { return _codigo; }
            set { _codigo = value;
                OnPropertyChanged("Codigo");
            }
        }

        public string Imagem
        {
            get { return _imagem; }
            set { _imagem = value;
                OnPropertyChanged("Imagem");
            }
        }

        public int Quantidade
        {
            get
            {
                return _quantidade;
            }
            set { _quantidade = value;
                OnPropertyChanged("Quantidade");
            }
        }
        public string Tipo
        {
            get => _tipo;
            set { _tipo = value;
                OnPropertyChanged("Tipo");
            }
        }
        public string Cor
        {
            get { return _cor; }
            set { _cor = value;
                OnPropertyChanged("Cor");
            }
        }
        public bool EstaEmBaralho
        {
            get { return _estaEmBaralho; }
            set { _estaEmBaralho = value;
                OnPropertyChanged("EstaEmBaralho");
            }
        }
    }
}
