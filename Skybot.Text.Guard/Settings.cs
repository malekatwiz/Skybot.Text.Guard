using System;

namespace Skybot.Text.Guard
{
    public static class Settings
    {
        public static readonly string AuthorityUri = GetEnvironmentVariable("Authority.Uri");
        public static readonly string TextoServiceUri = GetEnvironmentVariable("Texto.Uri");
        public static readonly string ClientId = GetEnvironmentVariable("ClientId");
        public static readonly string ClientSecret = GetEnvironmentVariable("ClientSecret");
        public static readonly string SecretKey = GetEnvironmentVariable("SecretKey");

        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
