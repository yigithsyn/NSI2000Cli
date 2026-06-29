using System.CommandLine;
using NSI2000;

// ─── Global seçenekler ────────────────────────────────────────────────────────

var fileOption = new Option<FileInfo?>(
    aliases: ["--file", "-f"],
    description: "Measurement file (.NSI) (if not given active NSI2000 is processed).")
{
    IsRequired = false
};

// ─── Root ─────────────────────────────────────────────────────────────────────

var rootCommand = new RootCommand("NSI2000 COM interface CLI tool")
{
    fileOption
};

// ─── version ──────────────────────────────────────────────────────────────────

var versionCommand = new Command("version", "Returns NSI2000 software version.");
versionCommand.SetHandler((FileInfo? file) =>
{
    using var meas = CreateMeasurement(file);
    if (meas is null) return;
    Console.WriteLine(meas.Nsi2000Version());
}, fileOption);

// ─── measure ──────────────────────────────────────────────────────────────────

var measureCommand = new Command("measure", "Ölçüm verilerine erişim komutları.");

var measureFreqCommand = new Command("freq", "Ölçüm frekansını Hz cinsinden gösterir.");
measureFreqCommand.SetHandler((FileInfo? file) =>
{
    using var meas = CreateMeasurement(file);
    if (meas is null) return;
    double hz  = meas.MeasFrequency();
    double ghz = hz / 1e9;
    // Console.WriteLine($"{ghz:F6} GHz  ({hz:F0} Hz)");
    Console.WriteLine($"{hz:F0}");
}, fileOption);

measureCommand.AddCommand(measureFreqCommand);

// ─── beamlist ─────────────────────────────────────────────────────────────────

var beamlistCommand = new Command("beamlist", "Beam List commands");

var beamlistCountCommand = new Command("count", " Returns the number of beams (frequencies) in the beam list");
beamlistCountCommand.SetHandler((FileInfo? file) =>
{
    using var meas = CreateMeasurement(file);
    if (meas is null) return;
    var beamlist = new BeamList(meas);
    Console.WriteLine($"{beamlist.Count()}");
}, fileOption);
beamlistCommand.AddCommand(beamlistCountCommand);

var beamIndexArgument = new Argument<int>("beam", "Beam index to select (1-based, must not exceed beam list count)");
var beamlistSelectCommand = new Command("select", "Selects a beam from the beam list by index");
beamlistSelectCommand.AddArgument(beamIndexArgument);
beamlistSelectCommand.SetHandler((FileInfo? file, int beam) =>
{
    using var meas = CreateMeasurement(file);
    if (meas is null) return;
    var beamlist = new BeamList(meas);
    int count = beamlist.Count();
    if (beam < 1 || beam > count)
    {
        Console.Error.WriteLine($"Error: Beam index '{beam}' is out of range. Valid range is 1 to {count}.");
        return;
    }
    beamlist.SelectBeam(beam);
}, fileOption, beamIndexArgument);
beamlistCommand.AddCommand(beamlistSelectCommand);

// ─── plot ─────────────────────────────────────────────────────────────────────

var plotCommand = new Command("plot", "Plot penceresi komutları.");

var plotClearCommand = new Command("clear", "Tüm plot pencerelerini temizler.");
plotClearCommand.SetHandler((FileInfo? file) =>
{
    using var meas = CreateMeasurement(file);
    if (meas is null) return;
    meas.ClearAllPlots();
    Console.WriteLine("Tüm plot pencereleri temizlendi.");
}, fileOption);

plotCommand.AddCommand(plotClearCommand);

// ─── Kayıt ────────────────────────────────────────────────────────────────────

rootCommand.AddGlobalOption(fileOption);
rootCommand.AddCommand(versionCommand);
rootCommand.AddCommand(beamlistCommand);
rootCommand.AddCommand(measureCommand);
rootCommand.AddCommand(plotCommand);

return await rootCommand.InvokeAsync(args);

// ─── Yardımcı ─────────────────────────────────────────────────────────────────

static Measurement? CreateMeasurement(FileInfo? file)
{
    try
    {
        return file is not null
            ? new Measurement(file.FullName)
            : new Measurement();
    }
    catch (FileNotFoundException ex)
    {
        Console.Error.WriteLine($"[Hata] Dosya bulunamadı: {ex.FileName}");
        return null;
    }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine($"[Hata] Geçersiz dosya: {ex.Message}");
        return null;
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"[Hata] NSI2000 COM bağlantısı kurulamadı: {ex.Message}");
        return null;
    }
}
