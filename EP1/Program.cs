using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace EP1
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

            Console.WriteLine("Lendo base de dados de frequentadores...");

            var frequentadores = ObterFrequentadores();

            var locais = new List<Local>();

            Console.WriteLine($"{frequentadores.Count} Frequentadores obtidos... iniciando agrupamento");

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

            Console.WriteLine("Escrevendo arquivos resultados em csv...");

            using (var writer = new StreamWriter("result\\locaisFrequentadores.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(locaisFrequentadores);
            }

            Console.WriteLine("Finalizado.");

            watch.Stop();

            Console.WriteLine($"Time elapsed: {watch.ElapsedMilliseconds}ms");;
        }

        /// <summary>
        /// Consultando arquivo .dbf para obter lista de frequentadores.
        /// </summary>
        /// <returns></returns>
        public static List<Frequentador> ObterFrequentadores()
        {
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
    }
}
