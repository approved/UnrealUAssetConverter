namespace UConvertPlugin
{
    public interface IConverterPlugin
    {
        public string GetPropertyName();
        public bool HasTagData();
        public bool HasTagValue();
        public void DeserializePropertyTagData(IAssetConverter converter);
        public void DeserializePropertyTagValue(IAssetConverter converter);
        public byte[] SerializePropertyTagData(IAssetConverter converter);
        public byte[] SerializePropertyTagValue(IAssetConverter converter);
    }
}
