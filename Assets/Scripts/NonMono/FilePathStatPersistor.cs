using System.IO;

public class FilePathStatPersistor : StatPersistor {
    private string filePath;

    public FilePathStatPersistor(string path) {
        filePath = path;
    }

    public override void ImportValues() {
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Read)) {
            using (var reader = new StreamReader(fileStream)) {
                var key = reader.ReadLine();
                var value = decimal.Parse(reader.ReadLine());
                values[key] = value;
            }
        }
        throw new System.NotImplementedException();
    }

    public override void ExportValues() {
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
            using (var writer = new StreamWriter(fileStream)) {
                foreach (var kvp in values) {
                    writer.WriteLine(kvp.Key);
                    writer.WriteLine(kvp.Value);
                }
            }
        }
    }

    public override void Destroy() {
        //don't need to do anything, we throw away our file stream after every import or export
    }
}