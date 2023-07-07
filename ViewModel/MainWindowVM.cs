using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UnoOnline.Model;

namespace UnoOnline.ViewModel
{
    public class MainWindowVM : ObservableObject
    {
        private ObservableCollection<Carta> _cartas = new ObservableCollection<Carta>();
        private HubConnection hubConnection;
        private StatusPartida statusPartida;
        private Jogador _jogador;
        private Visibility _visibilidadeTelaLogin;
        private Visibility _visibilidadeStackPanelLogin;
        private Visibility _visibilidadeMsgAguardandoJogadores;

        public ICommand JogarCartaComando { get; set; }
        public ICommand InformarNomeComando { get; set; }

        public MainWindowVM()
        {
            var ccs = JsonConvert.DeserializeObject<List<Carta>>(File.ReadAllText("Resources/baralho-uno.json"));
            Cartas = new ObservableCollection<Carta>(ccs);

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
            VisibilidadeStackPanelLogin = Visibility.Visible;

            StatusPartida = new StatusPartida();

            Jogador = new Jogador()
            {
                Uuid = Guid.NewGuid(),
                Nome = "Jogador"
            };
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
            await hubConnection.SendAsync("EntrarEmPartida", Jogador);
        }
        private void JogarCarta(object obj)
        {
            StatusPartida.UltimaCarta = (Carta)obj;
        }

        private bool JogarCartaValidacao(object arg)
        {
            return true;
        }
        private async void InformarNome(object obj)
        {
            await SolicitarEntradaEmPartida();
        }

        private bool InformarNomeValidacao(object arg)
        {
            if (Jogador.Nome != null && Jogador.Nome.Length > 0)
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
        public Jogador Jogador
        {
            get
            {
                return _jogador;
            }

            set
            {
                _jogador = value;
                OnPropertyChanged("Jogador");
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
    }
}
