namespace Client.Infrastructure.Models
{
    public class NeoSettings
    {
        public string RpcUrl { get; set; }
        public string ProtocolSettingsConfigFile { get; set; }
        public string PlatformTokenHash { get; set; }
        public string LockTokenVaultContractHash { get; set; }
        public string VestingTokenVaultContractHash { get; set; }
    }
}
