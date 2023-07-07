using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UnoOnline.Model;
using static System.Net.Mime.MediaTypeNames;

namespace UnoOnline.ViewModel
{
    public class MainWindowVM : ObservableObject
    {
        private ObservableCollection<Carta> _cartas = new ObservableCollection<Carta>();
        private HubConnection hubConnection;
        private StatusPartida statusPartida;
        private Jogador _jogador1;
        private Jogador _jogador2;
        private Jogador _jogador3;
        private Jogador _jogador4;
        private LinkedList<Jogador> jogadores = new LinkedList<Jogador>();
        private Visibility _visibilidadeTelaLogin;
        private Visibility _visibilidadeStackPanelLogin;
        private Visibility _visibilidadeMsgAguardandoJogadores;

        private string _nomeJogador1;
        private string _nomeJogador2;
        private string _nomeJogador3;
        private string _nomeJogador4;
        private int _quantCarta1;
        private int _quantCarta2;
        private int _quantCarta3;
        private int _quantCarta4;

        public ICommand JogarCartaComando { get; set; }
        public ICommand InformarNomeComando { get; set; }

        public MainWindowVM()
        {
            hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5061/hubs/unoonline")
                .Build();

            hubConnection.On<string>("RecebeMensagemTeste", (txt) =>
            {
                MessageBox.Show(txt);
            });

            hubConnection.On<StatusPartida>("AtualizarStatusPartida", (status) =>
            {
                StatusPartida = status;

                if (StatusPartida.Jogadores.Count <= 1)
                {
                    VisibilidadeMsgAguardandoJogadores = Visibility.Visible;
                    VisibilidadeStackPanelLogin = Visibility.Hidden;
                }
                else
                {
                    VisibilidadeTelaLogin = Visibility.Hidden;

                    if (StatusPartida.Jogadores.Count == 2)
                    {
                        Jogador jogador = StatusPartida.Jogadores.Where(w => w.Uuid == Jogador1.Uuid).First();
                        var playerNode = StatusPartida.Jogadores.Find(jogador);
                        Jogador1 = playerNode.Value;

                        Cartas = new ObservableCollection<Carta>(playerNode.Value.Cartas);

                        var next = playerNode.Next;

                        if (next == null)
                        {
                            next = StatusPartida.Jogadores.Find(StatusPartida.Jogadores.First());
                        }

                        Jogador3 = next.Value;
                    }
                    else
                    {
                        Jogador jogador = StatusPartida.Jogadores.Where(w => w.Uuid == Jogador1.Uuid).First();
                        var playerNode = StatusPartida.Jogadores.Find(jogador);

                        for (int i = 0; i < StatusPartida.Jogadores.Count; i++)
                        {
                            this.GetType().GetProperty($"Jogador{i + 1}").SetValue(this, playerNode.Value);

                            if (i == 0)
                            {
                                Cartas = new ObservableCollection<Carta>(playerNode.Value.Cartas);
                            }

                            playerNode = playerNode.Next;
                            if (playerNode == null)
                            {
                                playerNode = StatusPartida.Jogadores.Find(StatusPartida.Jogadores.First());
                            }
                        }
                    }
                }
            });

            hubConnection.On<string>("MensagemMessageBox", (msg) =>
            {
                MessageBox.Show(msg);
            });

            hubConnection.StartAsync();

            JogarCartaComando = new RelayCommand(JogarCarta, JogarCartaValidacao);
            InformarNomeComando = new RelayCommand(InformarNome, InformarNomeValidacao);
            VisibilidadeMsgAguardandoJogadores = Visibility.Hidden;
            VisibilidadeStackPanelLogin = Visibility.Visible;
            VisibilidadeTelaLogin = Visibility.Visible;

            StatusPartida = new StatusPartida();

            Jogador1 = new Jogador()
            {
                Uuid = Guid.NewGuid(),
                Nome = "Jogador"
            };

            Jogador2 = new Jogador()
            {
                Nome = "Sem Jogador"
            };

            Jogador3 = new Jogador()
            {
                Nome = "Sem Jogador"
            };

            Jogador4 = new Jogador()
            {
                Nome = "Sem Jogador"
            };

            jogadores.AddLast(Jogador1);
            jogadores.AddLast(Jogador2);
            jogadores.AddLast(Jogador3);
            jogadores.AddLast(Jogador4);
        }
        /// <summary>
        /// Envia o estado da partida para o hub após feita jogada.
        /// </summary>
        /// <returns></returns>
        public async Task EnviaStatusDaPartidaParaServidor()
        {
            await hubConnection.SendAsync("EnviarStatusPartida", StatusPartida);
        }
        public async Task SolicitarEntradaEmPartida()
        {
            await hubConnection.SendAsync("EntrarEmPartida", Jogador1);
        }
        private async void JogarCarta(object obj)
        {
            await hubConnection.SendAsync("EnviaUltimaCartaJogada", (Carta)obj, Jogador1);
        }

        private bool JogarCartaValidacao(object arg)
        {
            if (Jogador1.Uuid == StatusPartida.JogadorDaVez.Uuid) return true;
            return false;
        }
        private async void InformarNome(object obj)
        {
            await SolicitarEntradaEmPartida();
        }

        private bool InformarNomeValidacao(object arg)
        {
            if (Jogador1.Nome != null && Jogador1.Nome.Length > 0)
                return true;

            return false;
        }
        public ObservableCollection<Carta> Cartas
        {
            get
            {
                return _cartas;
            }
            set
            {
                _cartas = value;
                OnPropertyChanged("Cartas");
            }
        }
        public StatusPartida StatusPartida
        {
            get => statusPartida;
            set
            {
                statusPartida = value;
                OnPropertyChanged("StatusPartida");
            }
        }

        public Visibility VisibilidadeTelaLogin
        {
            get => _visibilidadeTelaLogin;
            set
            {
                _visibilidadeTelaLogin = value;
                OnPropertyChanged("VisibilidadeTelaLogin");
            }
        }

        public Visibility VisibilidadeStackPanelLogin
        {
            get => _visibilidadeStackPanelLogin;
            set
            {
                _visibilidadeStackPanelLogin = value;
                OnPropertyChanged("VisibilidadeStackPanelLogin");
            }
        }

        public Visibility VisibilidadeMsgAguardandoJogadores
        {
            get => _visibilidadeMsgAguardandoJogadores;
            set
            {
                _visibilidadeMsgAguardandoJogadores = value;
                OnPropertyChanged("VisibilidadeMsgAguardandoJogadores");
            }
        }

        public string NomeJogador1
        {
            get
            {
                return _nomeJogador1;
            }

            set
            {
                _nomeJogador1 = value;
                OnPropertyChanged("NomeJogador1");
            }
        }

        public string NomeJogador2
        {
            get
            {
                return _nomeJogador2;
            }

            set
            {
                _nomeJogador2 = value;
                OnPropertyChanged("NomeJogador2");
            }
        }

        public string NomeJogador3
        {
            get
            {
                return _nomeJogador3;
            }

            set
            {
                _nomeJogador3 = value;
                OnPropertyChanged("NomeJogador3");
            }
        }

        public string NomeJogador4
        {
            get
            {
                return _nomeJogador4;
            }

            set
            {
                _nomeJogador4 = value;
                OnPropertyChanged("NomeJogador4");
            }
        }

        public int QuantCarta1
        {
            get
            {
                return _quantCarta1;
            }

            set
            {
                _quantCarta1 = value;
                OnPropertyChanged("QuantCarta1");
            }
        }

        public int QuantCarta2
        {
            get
            {
                return _quantCarta2;
            }

            set
            {
                _quantCarta2 = value;
                OnPropertyChanged("QuantCarta2");
            }
        }

        public int QuantCarta3
        {
            get
            {
                return _quantCarta3;
            }

            set
            {
                _quantCarta3 = value;
                OnPropertyChanged("QuantCarta3");
            }
        }

        public int QuantCarta4
        {
            get
            {
                return _quantCarta4;
            }

            set
            {
                _quantCarta4 = value;
                OnPropertyChanged("QuantCarta4");
            }
        }
        public Jogador Jogador1
        {
            get
            {
                return _jogador1;
            }

            set
            {
                _jogador1 = value;
                OnPropertyChanged("Jogador1");
            }
        }
        public Jogador Jogador2
        {
            get
            {
                return _jogador2;
            }

            set
            {
                _jogador2 = value;
                OnPropertyChanged("Jogador2");
            }
        }

        public Jogador Jogador3
        {
            get
            {
                return _jogador3;
            }

            set
            {
                _jogador3 = value;
                OnPropertyChanged("Jogador3");
            }
        }

        public Jogador Jogador4
        {
            get
            {
                return _jogador4;
            }

            set
            {
                _jogador4 = value;
                OnPropertyChanged("Jogador4");
            }
        }
    }
}
