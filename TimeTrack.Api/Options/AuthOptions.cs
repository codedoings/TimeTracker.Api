namespace TimeTrack.Api.Options
{
    public class AuthOptions
    {
        public string GoogleClientId { get; set; } = "";
        public string PublicKey { get; set; } = "";
        public string PrivateKey { get; set; } = "";
        public string JwtAudience { get; set; } = "";
        public string JwtIssuer { get; set; } = "";
    }
}
