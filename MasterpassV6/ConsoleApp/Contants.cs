using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp
{
    public static class Contants
    {
        public const String callbackURL = "http://oldpocket.com:8888/";
        public const String consumerKey = "n7642t-OahZXoMUsgig2c6pXj7L1j6i7gvSQ1Wwn3c2b48c0!d2b9255554fc4141a4276b8fb4708fa30000000000000000";
        public const String keystorePassword = "ccknfDZDxCNvl5p9ZUao";
        public const String keystorePath = "/Users/fabiogodoy/Downloads/exemplo-masterpass-v6-csharp/defaultSandboxKey-sandbox.p12";
        public const String checkoutId = "be77c1f97cc848e7bce27684726953c7";
        public const String isSandbox = "true";
//        const AsymmetricAlgorithm privateKey = new X509Certificate2(keystorePath, keystorePassword).PrivateKey;
    }
}
