namespace RPC.Console.Utility
{
    using RPC.Console.Dto;
    using System;
    using System.Globalization;

    public static class UtilityBundle
    {
        public static void FindPath(string[] arguments, ref string path, ref bool? isFile)
        {
            if (arguments.Length == 1)
            {
                isFile = false;
                path = Directory.GetCurrentDirectory();
            }
            else if (arguments.Length == 3)
            {
                if (arguments[1] == "--f")
                {
                    isFile = true;
                    path = arguments[2];
                }
                else if (arguments[1] == "--d")
                {
                    isFile = false;
                    path = arguments[2];
                }
            }
            else
            {
                Console.WriteLine("Use: --f <PathToFile>");
                Console.WriteLine("Use: --d <DirectoryToScan>");
            }
        }

        public static List<string>? FileFinding(bool? isFile, string path)
        {
            var csvFiles = new List<string>();
            if (isFile is not null && !string.IsNullOrWhiteSpace(path))
            {
                if (isFile.Value && path.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase) && IsValidPath(path))
                {
                    csvFiles.Add(path);
                }
                else if (isFile.Value)
                {
                    Console.WriteLine("No CSV file found");
                }
                else if (!isFile.Value)
                {
                    foreach (var file in Directory.GetFiles(path))
                    {
                        if (file.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase) && IsValidPath(file))
                        {
                            csvFiles.Add(file);
                        }
                    }
                }
            }
            if (csvFiles.Count == 0)
            {
                Console.WriteLine("No CSV file found");
                return null;
            }
            return csvFiles;
        }

        public static Dictionary<int, List<DzienPracy>> DataParsing(List<string> csvFiles)
        {
            var counter = 0;
            var companyData = new Dictionary<int, List<DzienPracy>>();
            foreach (var csvFile in csvFiles)
            {
                counter++;
                companyData.Add(counter, new List<DzienPracy>());
                var lines = File.ReadAllLines(csvFile);
                foreach (var line in lines)
                {
                    var words = line.Split(';');
                    var workDay = new DzienPracy();
                    if (words.Length == 4)
                    {
                        workDay.KodPracownika = words[0];
                        workDay.Data = DateTime.ParseExact(words[1], "yyyy-MM-dd", CultureInfo.CurrentCulture);
                        if (words[3] == "WE")
                        {
                            workDay.GodzinaWejscia = 
                                !string.IsNullOrEmpty(words[2]) ?
                                    TimeSpan.ParseExact(words[2], @"h\:mm", CultureInfo.CurrentCulture) :
                                    default;
                        }
                        else if (words[3] == "WY")
                        {
                            workDay.GodzinaWyjscia = 
                                !string.IsNullOrEmpty(words[2]) ? 
                                    TimeSpan.ParseExact(words[2], @"h\:mm", CultureInfo.CurrentCulture) : 
                                    default;
                        }
                    }
                    else if (words.Length == 5)
                    {
                        workDay.KodPracownika = words[0];
                        workDay.Data = DateTime.ParseExact(words[1], "yyyy-MM-dd", CultureInfo.CurrentCulture);
                        workDay.GodzinaWejscia = TimeSpan.ParseExact(words[2], @"h\:mm\:ss", CultureInfo.CurrentCulture);
                        workDay.GodzinaWyjscia = TimeSpan.ParseExact(words[3], @"h\:mm\:ss", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        Console.WriteLine("Error occured during data parsing");
                        Environment.Exit(0);
                    }
                    companyData[counter].Add(workDay);
                }
            }
            return companyData;
        }

        public static Dictionary<int, List<DzienPracy>> DataFiltering(Dictionary<int, List<DzienPracy>> companiesDatas)
        {
            foreach (var companyDatas in companiesDatas)
            {
                var groupedData =
                    companyDatas.Value
                        .Where(
                            wd =>
                            (wd.GodzinaWyjscia == default && wd.GodzinaWejscia != default) ||
                            (wd.GodzinaWyjscia != default && wd.GodzinaWejscia == default)
                        )
                        .GroupBy(wd => (wd.KodPracownika, wd.Data))
                        .Where(g => g.Count() == 2);
                foreach (var gd in groupedData)
                {
                    var first = gd.First();
                    var last = gd.Last();

                    if (first.GodzinaWyjscia == default && last.GodzinaWyjscia != default)
                    {
                        first.GodzinaWyjscia = last.GodzinaWyjscia;
                    }
                    else if (first.GodzinaWyjscia != default && last.GodzinaWyjscia == default)
                    {
                        last.GodzinaWyjscia = first.GodzinaWyjscia;
                    }

                    if (first.GodzinaWejscia == default && last.GodzinaWejscia != default)
                    {
                        first.GodzinaWejscia = last.GodzinaWejscia;
                    }
                    else if (first.GodzinaWejscia != default && last.GodzinaWejscia == default)
                    {
                        last.GodzinaWejscia = first.GodzinaWejscia;
                    }
                }
            }
            foreach (var companyDatas in companiesDatas)
            {
                var roster = companyDatas.Value;
                roster =
                    companyDatas.Value.DistinctBy(e => (e.KodPracownika, e.Data)).OrderBy(e => e.KodPracownika).ThenBy(e => e.Data).ToList();
                companyDatas.Value.Clear();
                companyDatas.Value.AddRange(roster);
            }
            return companiesDatas;
        }

        public static void DataPrinting(Dictionary<int, List<DzienPracy>> companiesDatas)
        {
            var filteredDatas = DataFiltering(companiesDatas);
            foreach (var filteredData in filteredDatas)
            {
                Console.WriteLine("***********************************");
                Console.WriteLine($"Dane firmowe {filteredData.Key}");
                Console.WriteLine("***********************************");
                Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                var groups = filteredData.Value.GroupBy(k => k.KodPracownika);
                foreach (var group in groups)
                {
                    Console.WriteLine("***********************************");
                    Console.WriteLine($"Dane pracownika {group.Key}");
                    Console.WriteLine("***********************************");
                    group.ToList().ForEach(x => Console.WriteLine($"{x.Data:yyyy-MM-dd}: WE {x.GodzinaWejscia} / WY {x.GodzinaWyjscia}"));
                }
            }
        }

        private static bool IsValidPath(string path, bool allowRelativePaths = false)
        {
            bool isValid;
            try
            {
                string fullPath = Path.GetFullPath(path);

                if (allowRelativePaths)
                {
                    isValid = Path.IsPathRooted(path);
                }
                else
                {
                    isValid = string.IsNullOrEmpty((Path.GetPathRoot(path) ?? string.Empty).Trim(new char[] { '\\', '/' })) == false;
                }
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }
    }
}
