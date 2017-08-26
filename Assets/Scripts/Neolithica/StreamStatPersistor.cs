using System.IO;

namespace Neolithica {
    public sealed class StreamStatPersistor : StatPersistor {
        private Stream mSourceStream;

        public StreamStatPersistor(Stream source) {
            mSourceStream = source;
            ImportValues();
        }

        public override void ImportValues() {
            mSourceStream.Seek(0, SeekOrigin.Begin);
            var r = new StreamReader(mSourceStream);

            values.Clear();
            while (!r.EndOfStream) {
                string key = r.ReadLine();
                decimal value = decimal.Parse(r.ReadLine());
                values[key] = value;
            }
        }

        public override void ExportValues() {
            mSourceStream.Seek(0, SeekOrigin.Begin);
            var w = new StreamWriter(mSourceStream);
            foreach (var kvp in values) {
                w.WriteLine(kvp.Key);
                w.WriteLine(kvp.Value);
            }
            w.Flush();
        }

        public override void Destroy() {
            mSourceStream.Close();
            mSourceStream.Dispose();
        }
    }
}
