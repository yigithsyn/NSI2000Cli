using System;
using System.IO;

namespace NSI2000;

/// <summary>
/// Beam (Frequency) list operations for a given measurement.
/// </summary>
public sealed class BeamList
{
    private readonly Measurement _meas;

    /// <summary>
    /// Initialises the BeamList with the given measurement.
    /// </summary>
    /// <param name="meas">The bound NSI2000 measurement.</param>
    public BeamList(Measurement meas)
    {
        _meas = meas ?? throw new ArgumentNullException(nameof(meas));
    }

    /// <summary>
    /// Returns the number of beams in the beam list
    /// </summary>
    /// <returns>Number of beams (frequencies) in the beam table.</returns>
    public int Count()
    {
        _meas.Console.RunScriptFile(@"C:\NSI2000\Script\ExportBeamCount.bas");

        string countFile = _meas.File.Replace(".NSI", "_BeamCount.txt",
                               StringComparison.OrdinalIgnoreCase);

        string line = System.IO.File.ReadAllLines(countFile)[0];
        int beamCount = int.Parse(line);

        System.IO.File.Delete(countFile);
        return beamCount;
    }

    /// <summary>
    /// Selects the beam at the given index from the beam table.
    /// </summary>
    /// <param name="beam">Zero-based beam index.</param>
    public void SelectBeam(int beam)
    {
        _meas.Console.SELECT_BEAM(beam);
    }
}