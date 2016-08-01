public interface IStatPersistor {
    decimal GetValue(string name);
    void SetValue(string name, decimal value);
    void ImportValues();
    void ExportValues();
}
