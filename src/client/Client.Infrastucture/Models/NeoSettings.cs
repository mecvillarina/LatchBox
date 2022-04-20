namespace Client.Infrastructure.Models
{
    public class NeoSettings
    {
        public string Network { get; set; }
        public string RpcUrl { get; set; }
        public string ProtocolSettingsConfigFile { get; set; }
        public string PlatformTokenHash { get; set; }
        public string LockTokenVaultContractHash { get; set; }
    }
}
