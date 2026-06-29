using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NSI2000;

/// <summary>
/// Opens a measurement file or binds to the active NSI2000 measurement.
/// </summary>
public sealed class Measurement : IDisposable
{
    private readonly dynamic _server;
    private readonly dynamic _app;
    private readonly dynamic _console;
    private bool _disposed;

    /// <summary>Exposes the raw script-commands COM object for sub-classes.</summary>
    internal dynamic Console => _console;
    
    /// <summary>
    /// Path of the currently bound .NSI file.
    /// </summary>
    public string File { get; private set; }

    /// <summary>
    /// Initialises the NSI2000 COM connection.
    /// </summary>
    /// <param name="filename">
    /// Optional path to a .NSI measurement file.
    /// When omitted the currently active file in NSI2000 is used.
    /// </param>
    /// <exception cref="FileNotFoundException">
    /// Thrown when <paramref name="filename"/> does not exist on disk.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filename"/> is not a .NSI file.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the COM server cannot be reached or the file cannot be set.
    /// </exception>
    public Measurement(string? filename = null)
    {
        Type serverType = Type.GetTypeFromProgID("NSI2000.server")
            ?? throw new InvalidOperationException(
                "COM ProgID 'NSI2000.server' bulunamadı. NSI2000 kurulu ve kayıtlı olmalıdır.");

        _server  = Activator.CreateInstance(serverType)!;
        _app     = _server.AppConnection;
        _console = _app.ScriptCommands;

        if (filename is not null)
        {
            if (!System.IO.File.Exists(filename))
                throw new FileNotFoundException("Belirtilen dosya bulunamadı.", filename);

            string ext = Path.GetExtension(filename);
            if (!ext.Equals(".NSI", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Dosya adı geçerli bir .NSI dosyası olmalıdır.", nameof(filename));

            try
            {
                _console.DATA_FILENAME = filename;
                File = filename;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"NSI2000 üzerinde DATA_FILENAME ayarlanamadı: {ex.Message}", ex);
            }
        }
        else
        {
            File = (string)_console.DATA_FILENAME;
        }
    }

    /// <summary>
    /// Returns the beam frequency of the measurement.
    /// </summary>
    /// <returns> Select beam from beam table with index number.</returns>
    public double SelectBeam()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return (double)_console.MEAS_FREQUENCY * 1e9;
    }

    /// <summary>
    /// Returns the beam frequency of the measurement.
    /// </summary>
    /// <returns>Beam frequency in Hertz [Hz].</returns>
    public double MeasFrequency()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return (double)_console.MEAS_FREQUENCY * 1e9;
    }

    /// <summary>
    /// Clears all plot windows in NSI2000.
    /// </summary>
    public void ClearAllPlots()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _console.CLEAR_ALL_PLOTS();
    }

    /// <summary>
    /// Returns the NSI2000 software version string.
    /// </summary>
    /// <returns>Version string (e.g. "2023.1.0").</returns>
    public string Nsi2000Version()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return (string)_app.Version;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        ReleaseComObject(_console);
        ReleaseComObject(_app);
        ReleaseComObject(_server);
    }

    private static void ReleaseComObject(object? obj)
    {
        if (obj is null) return;
        try { Marshal.ReleaseComObject(obj); }
        catch { /* best-effort */ }
    }
}
