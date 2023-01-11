using RPC.Console.Utility;

var arguments = Environment.GetCommandLineArgs();

bool? isFile = false;
var path = string.Empty;

UtilityBundle.FindPath(arguments, ref path, ref isFile);
var csvFiles = UtilityBundle.FileFinding(isFile, path);
if (csvFiles is null)
{
    Environment.Exit(0);
}
var companiesDatas = UtilityBundle.DataParsing(csvFiles);
if (!companiesDatas.Any())
{
    Environment.Exit(0);
}
var filteredDatas = UtilityBundle.DataFiltering(companiesDatas);