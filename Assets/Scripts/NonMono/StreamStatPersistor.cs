using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StreamStatPersistor : IStatPersistor {
    private Dictionary<string, decimal> values = new Dictionary<string, decimal>();
    private Stream sourceStream;

    public StreamStatPersistor(Stream source) {
        sourceStream = source;
        ImportValues();
    }

    public decimal GetValue(string name) {
        if (values.ContainsKey(name)) {
            return values[name];
        } else {
            return 0m;
        }
    }

    public void SetValue(string name, decimal value) {
        values[name] = value;
    }

    public void ImportValues() {
        sourceStream.Seek(0, SeekOrigin.Begin);
        var r = new StreamReader(sourceStream);

        values.Clear();
        while (!r.EndOfStream) {
            string key = r.ReadLine();
            decimal value = decimal.Parse(r.ReadLine());
            values[key] = value;
        }
    }

    public void ExportValues() {
        sourceStream.Seek(0, SeekOrigin.Begin);
        var w = new StreamWriter(sourceStream);        
        foreach (var kvp in values) {
            w.WriteLine(kvp.Key);
            w.WriteLine(kvp.Value);
        }
        w.Flush();
    }
}
