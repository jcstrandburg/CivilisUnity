using System.Collections.Generic;
using System.IO;

public class StreamStatPersistor : StatPersistor {
    private Stream sourceStream;

    public StreamStatPersistor(Stream source) {
        sourceStream = source;
        ImportValues();
    }

    public override void ImportValues() {
        sourceStream.Seek(0, SeekOrigin.Begin);
        var r = new StreamReader(sourceStream);

        values.Clear();
        while (!r.EndOfStream) {
            string key = r.ReadLine();
            decimal value = decimal.Parse(r.ReadLine());
            values[key] = value;
        }
    }

    public override void ExportValues() {
        sourceStream.Seek(0, SeekOrigin.Begin);
        var w = new StreamWriter(sourceStream);        
        foreach (var kvp in values) {
            w.WriteLine(kvp.Key);
            w.WriteLine(kvp.Value);
        }
        w.Flush();
    }

    public override void Destroy() {
        sourceStream.Close();
        sourceStream.Dispose();
    }
}
