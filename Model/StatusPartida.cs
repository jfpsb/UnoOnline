using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace UnoOnline.Model
{
    public class StatusPartida : ObservableObject
    {
        private Carta _ultimaCarta;
        private LinkedList<Jogador> _jogadores = new LinkedList<Jogador>();
        private bool _sentidoEstaHorario = true;
        private Jogador _jogadorDaVez;
        private Dictionary<Guid, Carta> baralho = new Dictionary<Guid, Carta>();

        public StatusPartida()
        {
            GeraBaralho();
        }

        public void GeraBaralho()
        {
            baralho.Clear();
            var ccs = JsonConvert.DeserializeObject<List<Carta>>(File.ReadAllText("Resources/baralho-uno.json"));
            foreach (var c in ccs)
            {
                for (int i = 0; i < c.Quantidade; i++)
                {
                    var c2 = new Carta()
                    {
                        Uuid = Guid.NewGuid(),
                        Codigo = c.Codigo,
                        Quantidade = c.Quantidade,
                        Imagem = c.Imagem
                    };

                    string[] tipoArray = c2.Codigo.Split('-');

                    switch (tipoArray[0])
                    {
                        case "amarelo":
                            c2.Tipo = "numeral";
                            c2.Numero = tipoArray[1];
                            c2.Cor = "amarelo";
                            break;
                        case "verde":
                            c2.Tipo = "numeral";
                            c2.Numero = tipoArray[1];
                            c2.Cor = "verde";
                            break;
                        case "azul":
                            c2.Tipo = "numeral";
                            c2.Numero = tipoArray[1];
                            c2.Cor = "azul";
                            break;
                        case "vermelho":
                            c2.Tipo = "numeral";
                            c2.Numero = tipoArray[1];
                            c2.Cor = "vermelho";
                            break;
                        case "inverter":
                            c2.Tipo = tipoArray[0];
                            c2.Cor = tipoArray[1];
                            break;
                        case "bloqueio":
                            c2.Tipo = tipoArray[0];
                            c2.Cor = tipoArray[1];
                            break;
                        default:
                            c2.Tipo = $"{tipoArray[0]}-{tipoArray[1]}";
                            if (tipoArray[1] == "maisdois")
                            {
                                c2.Cor = tipoArray[2];
                            }
                            break;
                    }

                    baralho.Add(c2.Uuid, c2);
                }
            }
        }
        public IList<Carta> RetornaCartasDoBaralho(int quantCartas, bool ignoraCoringas)
        {
            List<Carta> cartasSelecionadas = new List<Carta>();
            List<Carta> cartasValidas;

            for (int i = 0; i < quantCartas; i++)
            {
                cartasValidas = baralho.Where(w => w.Value.EstaEmBaralho).Select(s => s.Value).ToList();
                var v = baralho.Where(w => !w.Value.EstaEmBaralho).ToList();

                if (ignoraCoringas)
                    cartasValidas = cartasValidas.Where(p => !p.Tipo.StartsWith("coringa")).ToList();

                var carta = cartasValidas[new Random().Next(cartasValidas.Count - 1)];
                carta.EstaEmBaralho = false;
                cartasSelecionadas.Add(carta);
            }

            return cartasSelecionadas;
        }
        public void MudarSentido()
        {
            SentidoEstaHorario = !SentidoEstaHorario;
        }
        public void PassarVez(int casas, Jogador jogador)
        {
            var linkedNode = Jogadores.Find(jogador);
            for (int i = 0; i < casas; i++)
            {
                if (SentidoEstaHorario)
                {
                    linkedNode = linkedNode.Next;
                    if (linkedNode == null)
                    {
                        linkedNode = Jogadores.Find(Jogadores.First());
                    }
                }
                else
                {
                    linkedNode = linkedNode.Previous;
                    if (linkedNode == null)
                    {
                        linkedNode = Jogadores.Find(Jogadores.Last());
                    }
                }
            }
            JogadorDaVez = linkedNode.Value;
        }
        public void ComprarCartas(Jogador jogador, int quantCartas)
        {
            var linkedNode = Jogadores.Find(jogador);

            foreach (var item in RetornaCartasDoBaralho(quantCartas, false))
            {
                linkedNode.Value.Cartas.Add(item);
            }
        }
        public Jogador RetornaProximoJogador(Jogador jogador)
        {
            //LinkedNode representa o node atual na vez
            var linkedNode = Jogadores.Find(jogador);

            //Na sequencia linkednode recebe o valor do proximo node da sequencia
            if (SentidoEstaHorario)
            {
                linkedNode = linkedNode.Next;
                if (linkedNode == null)
                {
                    linkedNode = Jogadores.Find(Jogadores.First());
                }
            }
            else
            {
                linkedNode = linkedNode.Previous;
                if (linkedNode == null)
                {
                    linkedNode = Jogadores.Find(Jogadores.Last());
                }
            }

            //Retorno proximo jogador da sequencia
            return linkedNode.Value;
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
        public Dictionary<Guid, Carta> Baralho
        {
            get
            {
                return baralho;
            }

            set
            {
                baralho = value;
                OnPropertyChanged("Baralho");
            }
        }
    }
}
