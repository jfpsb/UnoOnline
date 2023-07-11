using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UnoOnline.Model;
using UnoOnline.Services;
using UnoOnline.View;

namespace UnoOnline.ViewModel
{
    public class MainWindowVM : ObservableObject, IRequestClose, IDialogResult
    {
        private ObservableCollection<Carta> _cartas = new ObservableCollection<Carta>();
        private HubConnection hubConnection;
        private StatusPartida statusPartida;
        private Jogador _jogador1;
        private Jogador _jogador2;
        private Jogador _jogador3;
        private Jogador _jogador4;
        private Visibility _visibilidadeTelaLogin;
        private Visibility _visibilidadeStackPanelLogin;
        private Visibility _visibilidadeMsgAguardandoJogadores;
        private string _corEscolhida;
        private bool resultadoEscolherCor;
        private string _sequenciaEventosJogo = "";
        private string bannerCentral = "AGUARDANDO MAIS JOGADORES";
        private string _timerJogada;

        public event EventHandler<EventArgs> RequestClose;

        public ICommand JogarCartaComando { get; set; }
        public ICommand InformarNomeComando { get; set; }
        public ICommand EscolherCorComando { get; set; }
        public ICommand ComprarCartaComando { get; set; }
        public ICommand GritarUnoComando { get; set; }

        public MainWindowVM()
        {
            WindowService.RegistrarWindow<TelaEscolherCor, MainWindowVM>();

            hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5061/hubs/unoonline")
                .Build();

            hubConnection.On<string>("RecebeEventoJogo", (txt) =>
            {
                SequenciaEventosJogo += $"{txt}\n";
            });

            hubConnection.On<string>("AtualizarTimer", (str) =>
            {
                TimerJogada = str;
            });

            hubConnection.On<Jogador>("AcabouPartida", (jogador) =>
            {
                BannerCentral = $"{jogador.Nome.ToUpper()} GANHOU A PARTIDA!\n\nFECHE E ABRA NOVAMENTE PARA INICIAR NOVA PARTIDA";
                statusPartida.Jogadores.Clear();
                VisibilidadeMsgAguardandoJogadores = Visibility.Visible;
                VisibilidadeTelaLogin = Visibility.Visible;
                VisibilidadeStackPanelLogin = Visibility.Hidden;
                hubConnection.StopAsync();
            });

            hubConnection.On<StatusPartida>("AtualizarStatusPartida", (status) =>
            {
                //se receber atualização da partida enquanto tela de escolher cor significa
                //ou que jogador recebeu corte ou que jogador perdeu a vez por tempo então
                //a tela é fechada
                Application.Current.Dispatcher.Invoke(() => { RequestClose?.Invoke(this, null); });

                StatusPartida = status;

                if (StatusPartida.Jogadores.Count <= 1)
                {
                    VisibilidadeMsgAguardandoJogadores = Visibility.Visible;
                    VisibilidadeStackPanelLogin = Visibility.Hidden;
                    VisibilidadeTelaLogin = Visibility.Visible;
                    bannerCentral = "AGUARDANDO MAIS JOGADORES";
                }
                else
                {
                    VisibilidadeTelaLogin = Visibility.Hidden;

                    Jogador2 = new Jogador();
                    Jogador3 = new Jogador();
                    Jogador4 = new Jogador();

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

            //Conecta ao servidor
            hubConnection.StartAsync();

            JogarCartaComando = new RelayCommand(JogarCarta, JogarCartaValidacao);
            InformarNomeComando = new RelayCommand(InformarNome, InformarNomeValidacao);
            EscolherCorComando = new RelayCommand(EscolherCor);
            ComprarCartaComando = new RelayCommand(ComprarCarta, ComprarCartaValidacao);
            GritarUnoComando = new RelayCommand(GritarUno, GritarUnoValidacao);
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
        }

        private void GritarUno(object obj)
        {
            Jogador1.GritouUno = true;
        }

        private bool GritarUnoValidacao(object arg)
        {
            if (StatusPartida.JogadorDaVez?.Uuid == Jogador1.Uuid && Jogador1.Cartas.Count == 2 && !Jogador1.GritouUno) return true;
            return false;
        }

        private bool ComprarCartaValidacao(object arg)
        {
            if (Jogador1.Uuid == StatusPartida.JogadorDaVez?.Uuid) return true;

            return false;
        }

        private async void ComprarCarta(object obj)
        {
            Jogador1.GritouUno = false;
            await hubConnection.SendAsync("ComprarCarta", Jogador1);
        }

        private void EscolherCor(object obj)
        {
            CorEscolhida = obj as string;
            StatusPartida.UltimaCarta.Cor = CorEscolhida; //Obrigatoriamente a última carta é coringa
            resultadoEscolherCor = true;
            RequestClose?.Invoke(this, null);
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
            Carta carta = (Carta)obj;

            if (carta.Tipo.StartsWith("coringa-maisquatro") || carta.Tipo.StartsWith("coringa-cores"))
            {
                //Guarda cor da carta atual caso usuário não escolha nenhuma cor na tela
                var corAtual = StatusPartida.UltimaCarta.Cor;
                new WindowService().ShowDialog(this, (result, vm) =>
                {
                    if (result == true)
                    {
                        carta.Cor = CorEscolhida;
                    }
                    else
                    {
                        carta.Cor = corAtual;
                    }
                });
            }

            await hubConnection.SendAsync("EnviaUltimaCartaJogada", carta, Jogador1);
        }

        private bool JogarCartaValidacao(object arg)
        {
            Carta carta = arg as Carta;
            var ultimaCarta = StatusPartida.UltimaCarta;

            if (Jogador1.Uuid == StatusPartida.JogadorDaVez.Uuid)
            {
                //Pode jogar se ...
                if (ultimaCarta.Cor.Equals(carta.Cor)) return true; //Se carta for de mesma cor
                if (ultimaCarta.Tipo.Contains("bloqueio") && carta.Tipo.Contains("bloqueio")) return true; //Última carta for de bloqueio e possuir carta bloqueio
                if (ultimaCarta.Tipo.Contains("maisdois") && carta.Tipo.Contains("maisdois")) return true;//Última carta for de mais dois e possuir mais dois
                if (ultimaCarta.Numero != null && carta.Numero != null && ultimaCarta.Numero == carta.Numero) return true; //Cartas possuírem mesmo número
                if (carta.Tipo.Contains("cores") || carta.Tipo.Contains("maisquatro")) return true; //Se for coringa mais quatro ou de cores
                if (ultimaCarta.Tipo.Contains("maisquatro") && ultimaCarta.Cor.Equals(carta.Cor))
                {
                    return true;
                };
            }
            else
            {
                //Jogador não pode cortar se possuir apenas duas cartas
                if (Jogador1.Cartas.Count == 2) return false;
                if (ultimaCarta.Tipo == carta.Tipo && ultimaCarta.Numero == carta.Numero && ultimaCarta.Cor == carta.Cor) return true;
                if (ultimaCarta.Tipo.Contains("cores") && carta.Tipo.Contains("cores")) return true;
                if (ultimaCarta.Tipo.Contains("maisquatro") && carta.Tipo.Contains("maisquatro")) return true;
            }

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

        public bool? ResultadoDialog()
        {
            return resultadoEscolherCor;
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

        public string CorEscolhida
        {
            get
            {
                return _corEscolhida;
            }

            set
            {
                _corEscolhida = value;
                OnPropertyChanged("CorEscolhida");
            }
        }

        public string SequenciaEventosJogo
        {
            get
            {
                return _sequenciaEventosJogo;
            }

            set
            {
                _sequenciaEventosJogo = value;
                OnPropertyChanged("SequenciaEventosJogo");
            }
        }

        public string BannerCentral
        {
            get
            {
                return bannerCentral;
            }

            set
            {
                bannerCentral = value;
                OnPropertyChanged("BannerCentral");
            }
        }

        public string TimerJogada
        {
            get
            {
                return _timerJogada;
            }

            set
            {
                _timerJogada = value;
                OnPropertyChanged("TimerJogada");
            }
        }
    }
}
