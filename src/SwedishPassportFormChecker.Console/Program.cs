using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Spectre.Console;
using SwedishPassportFormChecker.Console;

var endpoint = "";
var apiKey = "";
var modelId = "Pass_V2";
var fileUri = new Uri("");

var credential = new AzureKeyCredential(apiKey);
var client = new DocumentAnalysisClient(new Uri(endpoint), credential);

AnsiConsole.Status()
    .AutoRefresh(true)
    .Spinner(Spinner.Known.Default)
    .StartAsync("[yellow]Analyzing document[/]", async ctx =>
    {
        var operation = await client.StartAnalyzeDocumentFromUriAsync(modelId, fileUri);

        await operation.WaitForCompletionAsync();
        var result = operation.Value;
        var firstDocument = result.Documents.First();
        var model = ParseModel(firstDocument);

        var grid = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(10)).AddColumn();
        WriteStatus(grid, nameof(model.Meta_Datum), model.Meta_Datum, expectFilledIn: false);
        WriteStatus(grid, nameof(model.Meta_Diarienummer), model.Meta_Diarienummer, expectFilledIn: false);
        AnsiConsole.Write(new Panel(grid).Header("Meta"));

        grid = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(10)).AddColumn();
        if(!model.Typ_EndastNationelltIdKort && !model.Typ_EndastNationelltIdKort && !model.Typ_BådePassOchNationelltIdKort)
        {
            grid.AddRow("[b]Typ[/]", "[red]X Expected exact one type to be checked.[/]");
        } else
        {
            grid.AddRow("[b]Typ[/]", "[green]✓ One type was checked![/]");
        }

        AnsiConsole.Write(new Panel(grid).Header("Typ"));

        grid = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(10)).AddColumn();
        WriteStatus(grid, nameof(model.Minderårig_Personnummer), model.Minderårig_Personnummer, expectFilledIn: true);
        WriteStatus(grid, nameof(model.Minderårig_Efternamn), model.Minderårig_Efternamn, expectFilledIn: true);
        WriteStatus(grid, nameof(model.Minderårig_Förnamn), model.Minderårig_Förnamn, expectFilledIn: true);
        WriteStatus(grid, nameof(model.Minderårig_Längd), model.Minderårig_Längd, expectFilledIn: true);
        AnsiConsole.Write(new Panel(grid).Header("Minderårig"));


        grid = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(10)).AddColumn();
        WriteStatus(grid, nameof(model.Vårdnadshavare1_IdKontroll), model.Vårdnadshavare1_IdKontroll, expectFilledIn: false);
        WriteStatus(grid, nameof(model.Vårdnadshavare2_IdKontroll), model.Vårdnadshavare2_IdKontroll, expectFilledIn: false);
        AnsiConsole.Write(new Panel(grid).Header("Vårdnadshavare"));

    });



Console.ReadLine();


void WriteStatus(Grid grid, string key, string value, bool expectFilledIn) => grid.AddRow($"[b]{key}[/]", GetStatusExplanation(value, expectFilledIn));
string GetStatusExplanation(string value, bool expectFilledIn)
{
    var isEmpty = string.IsNullOrWhiteSpace(value);
    if(isEmpty && expectFilledIn)
    {
        return "[red]X Expected to be empty, but is filled in.[/]";
    }

    if (isEmpty && !expectFilledIn)
    {
        return "[green]✓ Expected to be empty and is empty![/]";
    }

    if (!isEmpty && expectFilledIn)
    {
        return "[green]✓ Expected to be filled in and is filled in![/]";
    }

    if (!isEmpty && !expectFilledIn)
    {
        return "[red]X Expected to be empty, but is filled in.[/]";
    }

    return string.Empty;
}

PassportFormCheckerModel ParseModel(AnalyzedDocument document)
{
    return new PassportFormCheckerModel
    {
        Meta_Datum = ParseField(document, "Meta_Datum"),
        Meta_Diarienummer = ParseField(document, "Meta_Diarienummer"),

        Typ_EndastPass = ParseBoolField(document, "Typ_EndastPass"),
        Typ_EndastNationelltIdKort = ParseBoolField(document, "Typ_EndastNationelltIdKort"),
        Typ_BådePassOchNationelltIdKort = ParseBoolField(document, "Typ_BådePassOchNationelltIdKort"),

        Minderårig_Efternamn = ParseField(document, "Minderårig_Efternamn"),
        Minderårig_Förnamn = ParseField(document, "Minderårig_Förnamn"),
        Minderårig_Längd = ParseField(document, "Minderårig_Längd"),
        Minderårig_Personnummer = ParseField(document, "Minderårig_Personnummer"),

        Vårdnadshavare1_IdKontroll = ParseField(document, "Vårdnadshavare1_IdKontroll"),
        Vårdnadshavare2_IdKontroll = ParseField(document, "Vårdnadshavare2_IdKontroll")
    };
}

bool ParseBoolField(AnalyzedDocument document, string key) => ParseField(document, key) != "unselected";
string ParseField(AnalyzedDocument document, string key) => document.Fields[key].Content;