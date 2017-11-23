using System;
using System.Security.Cryptography.X509Certificates;
using Com.MasterCard.Masterpass.Merchant.Api;
using Com.MasterCard.Masterpass.Merchant.Model;
using Com.MasterCard.Sdk.Core;
using Com.MasterCard.Sdk.Core.Api;
using Com.MasterCard.Sdk.Core.Model;
using Nancy;

namespace ConsoleApp
{
    public class RootRoutes: NancyModule
    {
		public RootRoutes()
		{
            Get["/"] = ExpressSetup;

            Get["/express"] = ExpressCallback;

            Get["/express/executeExpress"] = ExecuteExpress;

         //   Get["/shop/order/payment/process/"] = StandardCheckout;
		}

        private dynamic ExpressSetup(dynamic parameters)
        {
            _setConfigurations();

            var callbackUrl = Contants.callbackURL + "express/";

            RequestTokenResponse requestTokenResponse1 = RequestTokenApi.Create(callbackUrl);
            string pairingRequestToken = requestTokenResponse1.OauthToken;
            RequestTokenResponse requestTokenResponse2 = RequestTokenApi.Create(callbackUrl);
            string requestToken = requestTokenResponse2.OauthToken;

            //Create an instance of MerchantInitializationRequest
            MerchantInitializationRequest merchantInitializationRequest = new MerchantInitializationRequest()
                .WithOriginUrl(Contants.callbackURL)
                .WithOAuthToken(pairingRequestToken);

            //Call the MerchantInitializationApi Service with required params
            MerchantInitializationResponse merchantInitializationResponse = MerchantInitializationApi.Create(merchantInitializationRequest);

            var model = new
            {
                requestToken = requestToken,
                pairingRequestToken = pairingRequestToken,
                merchantCheckoutId = Contants.checkoutId,
                callbackUrl = callbackUrl
            };

            return View["express_setup.htm", model];
          
        }

        private dynamic ExpressCallback(dynamic parameters)
        {
            var mpstatus = this.Request.Query["mpstatus"];
            var checkout_resource_url = this.Request.Query["checkout_resource_url"];
            var oauth_verifier = this.Request.Query["oauth_verifier"];
            var oauth_token = this.Request.Query["oauth_token"];
            var pairing_verifier = this.Request.Query["pairing_verifier"];
            var pairing_token = this.Request.Query["pairing_token"];

            _setConfigurations();

            AccessTokenResponse accessTokenResponse1 = AccessTokenApi.Create(oauth_token, oauth_verifier);
            string accessToken = accessTokenResponse1.OauthToken;

            AccessTokenResponse accessTokenResponse2 = AccessTokenApi.Create(pairing_token, pairing_verifier);
            string longAccessToken = accessTokenResponse2.OauthToken; // store for future requests

 
            String checkoutResourceUrl = checkout_resource_url.ToString();
            String checkoutId = checkoutResourceUrl.IndexOf('?') != -1 ? checkoutResourceUrl.Substring(checkoutResourceUrl.LastIndexOf('/') + 1).Split('?')[0] : checkoutResourceUrl.Substring(checkoutResourceUrl.LastIndexOf('/') + 1);

            Checkout checkout = CheckoutApi.Show(checkoutId, accessToken);

            Card card = checkout.Card;
            Address billingAddress = card.BillingAddress;
            Contact contact = checkout.Contact;
            AuthenticationOptions authOptions = checkout.AuthenticationOptions;
            string preCheckoutTransactionId = checkout.PreCheckoutTransactionId;
            ShippingAddress shippingAddress = checkout.ShippingAddress;
            string transactionId = checkout.TransactionId;
            string walletId = checkout.WalletID;

            /// AQUI DEVE SER CHAMADO O GATEWAY DE PAGAMENTO PARA EXECUTAR O 
            /// PAGAMENTO COM OS DADOS RECUPERADOS ACIMA...

            /// UMA VEZ QUE O PAGAMENTO FOI EXECUTADO, CONTINUAR COM O PASSO ABAIXO

            //Create an instance of MerchantTransactions
            MerchantTransactions merchantTransactions = new MerchantTransactions()
                        .With_MerchantTransactions(new MerchantTransaction()
                        .WithTransactionId(transactionId)
                        .WithPurchaseDate("2017-05-27T12:38:40.479+05:30")
                        .WithExpressCheckoutIndicator(false)
                        .WithApprovalCode("sample")
                        .WithTransactionStatus("Success")
                        .WithOrderAmount((long)76239)
                        .WithCurrency("USD")
                        .WithConsumerKey(Contants.consumerKey));

            //Call the PostbackService with required params
            MerchantTransactions merchantTransactionsResponse = PostbackApi.Create(merchantTransactions);


            /// FIM DO CHECKOUT WITH PAIRING
            /// 
            /// INICIO DE UM EXPRESS CHECKOUT

            var model = new
            {
                transactionId = transactionId,
                longAccessToken = longAccessToken
            };

            return View["express_execute.htm", model];

            //return request_token;
        }

        private dynamic ExecuteExpress(dynamic parameters) {

            _setConfigurations();

            var longAccessToken = this.Request.Query["longAccessToken"];

            //Create an instance of PrecheckoutDataRequest
            PrecheckoutDataRequest precheckoutDataRequest = new PrecheckoutDataRequest()
                    .WithPairingDataTypes(new PairingDataTypes()
                        .WithPairingDataType(new PairingDataType()
                            .WithType("CARD"))

                        .WithPairingDataType(new PairingDataType()
                            .WithType("ADDRESS"))

                        .WithPairingDataType(new PairingDataType()
                            .WithType("PROFILE")));

            //Call the PrecheckoutDataApi with required params
            PrecheckoutDataResponse precheckoutDataResponse = PrecheckoutDataApi.Create(longAccessToken, precheckoutDataRequest);

            ExpressCheckoutRequest expressCheckoutRequest = new ExpressCheckoutRequest
            {
                PrecheckoutTransactionId = precheckoutDataResponse.PrecheckoutData.PrecheckoutTransactionId,  // from precheckout data
                MerchantCheckoutId = Contants.checkoutId,
                OriginUrl = Contants.callbackURL,
                CurrencyCode = "USD",
                AdvancedCheckoutOverride = true, // set to true to disable 3-DS authentication
                OrderAmount = 1299,
                DigitalGoods = false,
                CardId = precheckoutDataResponse.PrecheckoutData.Cards.Card[0].CardId,
                ShippingAddressId = precheckoutDataResponse.PrecheckoutData.ShippingAddresses.ShippingAddress[0].AddressId,
            };
            ExpressCheckoutResponse response = ExpressCheckoutApi.Create(longAccessToken, expressCheckoutRequest);

            Checkout checkout = response.Checkout;

            Card card = checkout.Card;
            Address billingAddress = card.BillingAddress;
            Contact contact = checkout.Contact;
            AuthenticationOptions authOptions = checkout.AuthenticationOptions;
            string preCheckoutTransactionId = checkout.PreCheckoutTransactionId;
            ShippingAddress shippingAddress = checkout.ShippingAddress;
            string transactionId = checkout.TransactionId;
            string walletId = checkout.WalletID;

            /// AQUI DEVE SER CHAMADO O GATEWAY DE PAGAMENTO PARA EXECUTAR O 
            /// PAGAMENTO COM OS DADOS RECUPERADOS ACIMA...

            /// UMA VEZ QUE O PAGAMENTO FOI EXECUTADO, CONTINUAR COM O PASSO ABAIXO

            //Create an instance of MerchantTransactions
            MerchantTransactions merchantTransactions = new MerchantTransactions()
                    .With_MerchantTransactions(new MerchantTransaction()
                        .WithTransactionId(transactionId)
                        .WithPurchaseDate("2017-05-27T12:38:40.479+05:30")
                        .WithExpressCheckoutIndicator(false)
                        .WithApprovalCode("sample")
                        .WithTransactionStatus("Success")
                        .WithOrderAmount((long)76239)
                        .WithCurrency("USD")
                        .WithConsumerKey(Contants.consumerKey));

            //Call the PostbackService with required params
            MerchantTransactions merchantTransactionsResponse = PostbackApi.Create(merchantTransactions);


            return "Express Checkout realizado com sucesso. TransactionId:" + transactionId;
        }

		private dynamic StandardCheckout(dynamic parameters)
		{
			var cartId = this.Request.Query["cartId"];
            var oauth_verifier = this.Request.Query["oauth_verifier"];

          //  var SC = new StandardCheckout(cartId, oauth_verifier);

			return "END....";
		}

        private static void _setConfigurations()
        {
            var privateKey = new X509Certificate2(Contants.keystorePath, Contants.keystorePassword).PrivateKey;

            MasterCardApiConfiguration.Sandbox = Convert.ToBoolean(Contants.isSandbox);
            MasterCardApiConfiguration.ConsumerKey = Contants.consumerKey;
            MasterCardApiConfiguration.PrivateKey = privateKey;
        }

    }
}
