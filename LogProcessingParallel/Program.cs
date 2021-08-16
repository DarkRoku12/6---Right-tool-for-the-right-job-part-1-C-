using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace LogProcessingParallel
{
  using CBag = ConcurrentBag<ValueTuple<int , string>>;

  class Program
  {
    static HashSet<string> filePaths = new HashSet<string>();

    // Possible log types: "I" , "D" , "V" , "W" , "E" , "F"
    static Dictionary<char , SlotType> GenSlots<SlotType>() where SlotType : new()
    {
      return new Dictionary<char , SlotType>
      {
        { 'I' , new SlotType() },
        { 'D' , new SlotType() },
        { 'V' , new SlotType() },
        { 'W' , new SlotType() },
        { 'E' , new SlotType() },
        { 'F' , new SlotType() },
      };
    }

    static char GetLineIdentifier( string line )
    {
      var matches = Regex.Matches( line , @"[\d\-\.\s]+([A-Z])" ); // Match the line group.
      var identifier = matches[ 0 ].Groups[ 1 ].Value; // Get the line group.
      return identifier[ 0 ]; // Pick the character #0.
    }

    static string ComputeFileHash( string path )
    {
      using var md5 = MD5.Create();
      using var stream = File.OpenRead( path );
      var hash = md5.ComputeHash( stream );
      return BitConverter.ToString( hash ).Replace( "-" , String.Empty );
    }

    // Process log lines with simple sequential loops.
    static void AsSequentialVersion( string[] lines )
    {
      var slots = GenSlots<List<string>>();

      foreach( var line in lines )
      {
        var charIdentifier = GetLineIdentifier( line );
        slots[ charIdentifier ].Add( line ); // Store the line in the target slot.
      }

      var prefix = "__sequential";

      foreach( var entry in slots )
      {
        var path = $"./output/{prefix}_{entry.Key}.log";
        File.WriteAllLines( path , entry.Value );
        filePaths.Add( path );
      }
    }

    // Process log lines using Parallel.For().
    static void ParallelForVersion( string[] lines )
    {
      var slots = GenSlots<CBag>(); // Generate slots to store the classified log lines.

      Parallel.For( 0 , lines.Length , idx =>
      {
        var line = lines[ idx ];
        var charIdentifier = GetLineIdentifier( line );
        slots[ charIdentifier ].Add( (idx, line) ); // Store the line in the target slot.
      } );

      var prefix = "parallel_for";

      foreach( var entry in slots )
      {
        // Because both Parallel.For & ConcurrentBag does not guarantees the ordering, we must sort them before saving.
        var linesToWrite = entry.Value.ToList().OrderBy( v => v.Item1 ).Select( v => v.Item2 );
        var path = $"./output/{prefix}_{entry.Key}.log";
        File.WriteAllLines( path , linesToWrite );
        filePaths.Add( path );
      }
    }

    // Process log lines using AsParallel().
    static void AsParallelVersion( string[] lines )
    {
      var slots = lines.AsParallel().AsOrdered().GroupBy( line =>
      {
        return GetLineIdentifier( line );
      } ).AsSequential();

      var prefix = "_as_parallel";

      foreach( var entry in slots )
      {
        var path = $"./output/{prefix}_{entry.Key}.log";
        File.WriteAllLines( path , entry.ToArray() );
        filePaths.Add( path );
      }
    }

    static void Main( string[] args )
    {
      // Read the log file and store it into string[].
      var lines = File.ReadAllLines( "Android.log" );

      // Creates the directory if it doesn't exists, otherwise do nothing.
      Directory.CreateDirectory( "./output" );

      Stopwatch stopWatch = new Stopwatch();
      stopWatch.Start();

      // Parallel.For version.
      ParallelForVersion( lines );
      LogElapsed( "Parallel.For | Run #1 |" , stopWatch );

      // AsParallel version.
      AsParallelVersion( lines );
      LogElapsed( "AsParallel   | Run #1 |" , stopWatch );

      // Sequential version.
      AsSequentialVersion( lines );
      LogElapsed( "Sequential   | Run #1 |" , stopWatch );

      ///////////// Repeat in reverse order //////////////////

      // Sequential version.
      AsSequentialVersion( lines );
      LogElapsed( "Sequential   | Run #2 |" , stopWatch );

      // AsParallel version.
      AsParallelVersion( lines );
      LogElapsed( "AsParallel   | Run #2 |" , stopWatch );

      // Parallel.For version.
      ParallelForVersion( lines );
      LogElapsed( "Parallel.For | Run #2 |" , stopWatch );

      // Compute hashes (in order to test correct ordering of the output).
      foreach( var path in filePaths )
      {
        var hash = ComputeFileHash(path);
        Console.WriteLine( $"{path} -> hash: ${hash}" );
      }

      Console.WriteLine( "Finished!...(waiting for key)" );
      Console.ReadLine();
    }

    private static void LogElapsed( string text , Stopwatch stopWatch )
    {
      stopWatch.Stop();
      Console.WriteLine( $"{text} - {stopWatch.ElapsedMilliseconds}ms" );
      GC.Collect();
      GC.WaitForPendingFinalizers();
      stopWatch.Restart();
    }
  }
}
