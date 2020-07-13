using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using CsvHelper;

namespace EP1
{
    class Program
    {
        static void Main(string[] args)
        {
            //ProcessamentoEP1();

            //ProcessamentoEP2();

            //ProcessamentoEP3();

            //ProcessamentoEP4();

            ProcessamentoEP5();
        }

        private static List<SnapshotInfectados> snapshots;

        private static int simulationCounter = 0;

        private static int maximumSimulationCounter = 10000;

        public static void ProcessamentoEP5()
        {
            Frequentador[] arranjoFrequentadores = ReadFileAndConstructGraph("traducao1.txt");
            
            var numeroFrequentadores = arranjoFrequentadores.Length;

            Frequentador infected = null;

            while (infected == null)
            {
                var randomFirstInfected = new Random().Next(0, numeroFrequentadores);
                infected = arranjoFrequentadores[randomFirstInfected];
            }


            snapshots = new List<SnapshotInfectados>();

            var infectedIndex = infected.Index;
            var c = 0.9;
            var r = 0.1;
            var testNumber = 1;

            var iteration = 0;

            Console.WriteLine($"Rodando simulacao C: {c} R: {r}");
            
            Simulate(c, r, infectedIndex, arranjoFrequentadores, iteration);
        }

        public static void Simulate(double c, double r, int index, Frequentador[] arranjoFrequentadores, int iterateCounter)
        {
            if (simulationCounter == maximumSimulationCounter)
            {
                GenerateSimulationResults();

                Environment.Exit(0);
            }
                
            simulationCounter++;

            GeraSnapshot(arranjoFrequentadores, simulationCounter);

            Console.WriteLine($"{simulationCounter}");

            var frequentador = arranjoFrequentadores[index];

            if(frequentador == null)
                return;
            
            if (iterateCounter == 0) //iteracao inicial, ou seja, primeira pessoa infectada
            {
                frequentador.Status = "I";
            }

            iterateCounter++;

            if (frequentador.Status == "I")
            {
                foreach (Frequentador adj in frequentador.Adjacentes)
                {
                    if (adj == null)
                        continue;

                    Simulate(c, r, adj.Index, arranjoFrequentadores, iterateCounter);
                }

                var x = new Random().NextDouble();

                if (x <= r && iterateCounter != 1)
                {
                    frequentador.Status = "R";
                }
            }
            else if (frequentador.Status == "S")
            {
                var x = new Random().NextDouble();

                if (x <= c)
                {
                    frequentador.Status = "I";

                    Simulate(c, r, frequentador.Index, arranjoFrequentadores, iterateCounter);
                }
            }
        }

        private static void GenerateSimulationResults()
        {
            Console.WriteLine("Escrevendo resultado em csv");

            using (var writer = new StreamWriter($"result\\simulacaoInfectados.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(snapshots);
            }
        }

        public static void GeraSnapshot(Frequentador[] arranjoFrequentadores, int counter)
        {
            var result = new SnapshotInfectados()
            {
                Iteracao = counter,
                SCounts = arranjoFrequentadores.Count(x => x?.Status == "S"),
                ICounts = arranjoFrequentadores.Count(x => x?.Status == "I"),
                RCounts = arranjoFrequentadores.Count(x => x?.Status == "R")
            };

            snapshots.Add(result);
        } 

        public static void ProcessamentoEP4()
        {
            var watchTotal = System.Diagnostics.Stopwatch.StartNew();

            Frequentador[] arranjoFrequentadores = ReadFileAndConstructGraph("traducao1.txt");

            var result = new List<PairAndDistance>();

            Console.WriteLine("Processando dados...");

            foreach (var i in arranjoFrequentadores) //percorrendo todo o arranjo de frequentadores
            {
                if(i == null)
                    continue;

                BreadthFirstPaths bfp = new BreadthFirstPaths(arranjoFrequentadores, i.Index); //computando busca em largura e marcando vertices dos caminhos possiveis

                foreach (var j in arranjoFrequentadores) //
                {
                    if(j == null)
                        continue;

                    var dist = bfp.GetDistance(j.Index);

                    if (dist == -1) //nao existe distancia, ou seja, sao vertices desconexos
                        continue;

                    var pairAndDistance = new PairAndDistance()
                    {
                        Pair1 = i.Index,
                        Pair2 = j.Index,
                        Distance = dist
                    };

                    result.Add(pairAndDistance);
                }
            }

            watchTotal.Stop();

            Console.WriteLine($"Tempo total para processar distancias: {watchTotal.ElapsedMilliseconds}ms");

            Console.WriteLine("Agrupando");

            var grouped = (from t in result
                group t by new { t.Distance }
                into grp
                select new HistogramaGrau()
                {
                    Grau = grp.Key.Distance,
                    NumeroFrequentadores = grp.Count()
                }).ToList();

            Console.WriteLine("Escrevendo resultado em csv");

            using (var writer = new StreamWriter("result\\histogramaDistancias.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(grouped);
            }

            Console.WriteLine("End");
        }

        public static Frequentador[] ReadFileAndConstructGraph(string path)
        {
            Frequentador[] arranjoFrequentadores = null;

            StreamReader reader = File.OpenText(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(' ');
                if (items.Length == 1)
                {
                    arranjoFrequentadores = new Frequentador[int.Parse(items[0])];
                    continue;
                }

                int v = int.Parse(items[0]);

                if (arranjoFrequentadores != null)
                {
                    var f = arranjoFrequentadores[v];
                    if (f == null)
                    {
                        f = new Frequentador()
                        {
                            Index = v
                        };
                    }

                    if (f.Adjacentes == null)
                        f.Adjacentes = new GenericList<Frequentador>();

                    f.Adjacentes.AddNode(new Frequentador()
                    {
                        Index = int.Parse(items[1])
                    });

                    arranjoFrequentadores[v] = f;
                }
            }

            return arranjoFrequentadores;
        }

        public static void ProcessamentoEP3()
        {
            Console.WriteLine("Iniciio processamento EP3");

            var grafo = GeraGrafoFrequentadores();

            var result = new List<FrequentadorComponente>();

            Console.WriteLine("Computando tamanho das componentes");

            foreach (var f in grafo)
            {
                var dfs = new DepthFirstSearch(grafo, f.Index);
                
                var frequentadorComponente = new FrequentadorComponente()
                {
                    f = f,
                    TamanhoComponenteConexa = dfs.Count()
                };

                result.Add(frequentadorComponente);
            }

            Console.WriteLine("Agrupando para verificar tamanho das componentes e incidencias");

            var grouped = (from t in result
                group t by new { t.TamanhoComponenteConexa }
                into grp
                select new HistogramaGrau()
                {
                    Grau = grp.Key.TamanhoComponenteConexa,
                    NumeroFrequentadores = grp.Select(x => x.TamanhoComponenteConexa).Count()
                }).ToList();

            Console.WriteLine("Escrevendo resultado em csv");

            using (var writer = new StreamWriter("result\\tamanhoComponentes.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(grouped);
            }

            Console.WriteLine("End");
        }

        public static void ProcessamentoEP2()
        {
            Console.WriteLine("Iniciando processamento EP2...");

            var arranjoFrequentadores = GeraGrafoFrequentadores();

            var grouped = (from t in arranjoFrequentadores
                group t by new { t.Grau }
                into grp
                select new HistogramaGrau()
                {
                    Grau = grp.Key.Grau,
                    NumeroFrequentadores = grp.Select(x => x.Id).Count()
                }).ToList();

            using (var writer = new StreamWriter("result\\grauFrequentadores.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(grouped);
            }

            var soma = 0;
            foreach (var freq in arranjoFrequentadores)
            {
                soma += freq.Grau;
            }

            //considerando que é um grafo bidirecional, o numero de arestas vai ser o grau de cada nó / 2.
            var numeroArestas = soma / 2;

            Console.WriteLine($"numero total de arestas: {numeroArestas}");
        }

        public static void ProcessamentoEP1()
        {
            var watchTotal = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

            var watchLeituraDB = System.Diagnostics.Stopwatch.StartNew();

            var frequentadores = ObterFrequentadores();
            watchLeituraDB.Stop();

            Console.WriteLine($"Tempo de leitura do BD: {watchLeituraDB.ElapsedMilliseconds}ms");

            var watchAgrupamentoProcessamento = System.Diagnostics.Stopwatch.StartNew();

            var locais = new List<Local>();

            //Agrupando e adicionando a lista os frequentadores dos destinos com coordenadas X e Y
            locais.AddRange((from t in frequentadores
                             group t by new { t.DestinoX, t.DestinoY }
                into grp
                             select new Local()
                             {
                                 xy = $"{grp.Key.DestinoX}{grp.Key.DestinoY}",
                                 coordenada_x = grp.Key.DestinoX,
                                 coordenada_y = grp.Key.DestinoY,
                                 NumeroFrequentadores = grp.Select(x => x.Id).Count()
                             }).ToList());

            //Agrupando e adicionando a lista os frequentadores das origens com coordenadas X e Y
            locais.AddRange((from t in frequentadores
                             group t by new { t.OrigemX, t.OrigemY }
                into grp
                             select new Local()
                             {
                                 xy = $"{grp.Key.OrigemX}{grp.Key.OrigemY}",
                                 coordenada_x = grp.Key.OrigemX,
                                 coordenada_y = grp.Key.OrigemY,
                                 NumeroFrequentadores = grp.Select(x => x.Id).Count()
                             }).ToList());

            //Somando numero de frequentadores origem e destino
            var agrupamentoLocais = (from t in locais
                                     group t by new { t.coordenada_x, t.coordenada_y }
                into grp
                                     select new Local()
                                     {
                                         xy = $"{grp.Key.coordenada_x}{grp.Key.coordenada_y}",
                                         NumeroFrequentadores = grp.Sum(x => x.NumeroFrequentadores)
                                     }).ToList();

            //Agrupando numero de locais pelo numero de frequentadores para utilizar no gráfico
            var locaisFrequentadores = (from t in agrupamentoLocais
                                        group t by new { t.NumeroFrequentadores }
                into grp
                                        select new
                                        {
                                            NrFrequentadores = grp.Key,
                                            NrLocais = grp.Select(x => x.xy).Count()
                                        }).ToList();

            watchAgrupamentoProcessamento.Stop();

            Console.WriteLine($"Tempo de agrupamento e processamento: {watchAgrupamentoProcessamento.ElapsedMilliseconds}ms");

            var watchEscreverCSV = System.Diagnostics.Stopwatch.StartNew();

            using (var writer = new StreamWriter("result\\locaisFrequentadores.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(locaisFrequentadores);
            }

            watchAgrupamentoProcessamento.Stop();

            Console.WriteLine($"Tempo de escrita no CSV: {watchEscreverCSV.ElapsedMilliseconds}ms");

            watchTotal.Stop();

            Console.WriteLine($"Tempo total: {watchTotal.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Consultando arquivo .dbf para obter lista de frequentadores.
        /// </summary>
        /// <returns></returns>
        public static List<Frequentador> ObterFrequentadores()
        {
            Console.WriteLine("Obtendo frequentadores do banco de dados...");

            var file = "DB\\OD_2017.dbf";

            var result = new List<Frequentador>();

            using var dbfDataReader = new DbfDataReader.DbfDataReader(file);
            while (dbfDataReader.Read())
            {
                var id = Convert.ToString(dbfDataReader["ID_PESS"]);
                var origemX = Convert.ToInt32(dbfDataReader["CO_O_X"]);
                var origemY = Convert.ToInt32(dbfDataReader["CO_O_Y"]);
                var destinoX = Convert.ToInt32(dbfDataReader["CO_D_X"]);
                var destinoY = Convert.ToInt32(dbfDataReader["CO_D_Y"]);

                if(origemX == 0 ||
                   origemY == 0 ||
                   destinoX == 0 ||
                   destinoY == 0)
                    continue;

                result.Add(new Frequentador()
                {
                    Id = id,
                    OrigemX = origemX,
                    OrigemY = origemY,
                    DestinoX = destinoX,
                    DestinoY = destinoY
                });
            }

            return result;
        }

        public static Frequentador[] GeraGrafoFrequentadores()
        {
            var frequentadores = ObterFrequentadores();

            var locais = new List<Local>();

            Console.WriteLine("Agrupando por local...");

            //agrupando por local xy e adicionando todos os frequentadores deste local
            locais.AddRange((from t in frequentadores
                group t by new { t.DestinoX, t.DestinoY }
                into grp
                select new Local()
                {
                    xy = $"{grp.Key.DestinoX}{grp.Key.DestinoY}",
                    coordenada_x = grp.Key.DestinoX,
                    coordenada_y = grp.Key.DestinoY,
                    NumeroFrequentadores = grp.Select(x => x.Id).Count(),
                    Frequentadores = grp.ToList()
                }).ToList());

            //criando estrutura de grafo em listas de adjacencias de frequentadores
            int index = 0;
            var arranjoFrequentadores = new Frequentador[frequentadores.Count];

            foreach (var l in locais)
            {
                foreach (var frequentador in l.Frequentadores)
                {
                    frequentador.Index = index;

                    arranjoFrequentadores[index] = frequentador;

                    frequentador.Adjacentes = new GenericList<Frequentador>();

                    var grau = 0;

                    foreach (var freqAdjacente in l.Frequentadores)
                    {
                        if (freqAdjacente.Id == frequentador.Id)
                            continue;

                        frequentador.Adjacentes.AddNode(freqAdjacente);

                        grau++;
                    }

                    frequentador.Grau = grau;

                    index++;
                }
            }

            return arranjoFrequentadores;
        }
    }
}

