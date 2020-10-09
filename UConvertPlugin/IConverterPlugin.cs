namespace UConvertPlugin
{
    public interface IConverterPlugin
    {
        public string GetPropertyName();
        public bool IsPreProperty();
        public bool IsPostProperty();
        public void Deserialize(IAssetConverter converter, bool isPre);
        public byte[] Serialize(IAssetConverter converter, bool isPre);
    }
}
