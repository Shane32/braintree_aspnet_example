using Braintree;
using BraintreeASPExample.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BraintreeASPExample.Controllers
{
    public class CheckoutsController : Controller
    {
        public IBraintreeConfiguration config = new BraintreeConfiguration();

        public static readonly TransactionStatus[] transactionSuccessStatuses = {
            TransactionStatus.AUTHORIZED,
            TransactionStatus.AUTHORIZING,
            TransactionStatus.SETTLED,
            TransactionStatus.SETTLING,
            TransactionStatus.SETTLEMENT_CONFIRMED,
            TransactionStatus.SETTLEMENT_PENDING,
            TransactionStatus.SUBMITTED_FOR_SETTLEMENT
        };

        public ActionResult New()
        {
            var gateway = config.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            return View();
        }

        public ActionResult Create()
        {
            var gateway = config.GetGateway();
            decimal amount;

            Customer btCustomer = null;
            Result<Customer> customerResult = null;

            try
            {
                btCustomer = gateway.Customer.Find("1");
            }
            catch { }

            if (btCustomer == null)
            {
                CustomerRequest customerRequest = new CustomerRequest
                {
                    CustomerId = "1",
                    Id = "1",
                    Email = "candyland@test.com"
                };


                customerResult = gateway.Customer.Create(customerRequest);
            }

            try
            {
                amount = Convert.ToDecimal(Request["amount"]);
            }
            catch (FormatException e)
            {
                TempData["Flash"] = "Error: 81503: Amount is an invalid format.";
                return RedirectToAction("New");
            }

            var nonce = Request["payment_method_nonce"];
            var paymentType = Request["payment_type"];
            var details = Request["details"];
            var billingDetails = Request["billingAddress"];

            PaymentDetails paymentDetails = null;
            GooglePayDetails googlePayDetails = null;
            BillingAddress billingAddressDetails = null;


            if (!String.IsNullOrEmpty(details))
            {
                if (paymentType == "AndroidPayCard")
                {
                    googlePayDetails = JsonConvert.DeserializeObject<GooglePayDetails>(details);
                } else
                {
                    paymentDetails = JsonConvert.DeserializeObject<PaymentDetails>(details);
                }
            }


            if (!String.IsNullOrEmpty(billingDetails))
            {
                billingAddressDetails = JsonConvert.DeserializeObject<BillingAddress>(billingDetails);
            }

            //Don't need this right now
            //var lastFour = Request["last_four"];
            //var cardType = Request["card_type"];


            //Useful for creating payments without creating a transaction (myaccount paymentmethod page)
            //var paymentRequest = new PaymentMethodRequest
            //{
            //    CustomerId = "1",
            //    PaymentMethodNonce = nonce,
            //};

            //Result<PaymentMethod> paymentMethodResult = gateway.PaymentMethod.Create(paymentRequest);

            //if (paymentMethodResult == null)
            //{
            //    return View("Unable to process payment. Please try again.");
            //}

            AddressRequest shippingAddress = new AddressRequest();

            if (paymentDetails != null)
            {
                //splits names into first and last, will not work with names like da Silva
                var names = paymentDetails.ShippingAddress.RecipientName.Split(' ');

                shippingAddress.CountryCodeAlpha2 = paymentDetails.ShippingAddress.CountryCode;
                shippingAddress.FirstName = names[0];
                shippingAddress.LastName = names[1];
                shippingAddress.Locality = paymentDetails.ShippingAddress.City;
                shippingAddress.PostalCode = paymentDetails.ShippingAddress.PostalCode;
                shippingAddress.Region = paymentDetails.ShippingAddress.State;
                shippingAddress.StreetAddress = paymentDetails.ShippingAddress.Line1;
                shippingAddress.ExtendedAddress = paymentDetails.ShippingAddress.Line2;
            }

            AddressRequest billingAddress = new AddressRequest();

            if (billingAddressDetails != null)
            {
                //splits names into first and last, will not work with names like da Silva
                var names = billingAddressDetails.Name.Split(' ');

                billingAddress.CountryCodeAlpha2 = billingAddressDetails.CountryCode;
                billingAddress.FirstName = names[0];
                billingAddress.LastName = names[1];
                billingAddress.Locality = billingAddressDetails.City;
                billingAddress.PostalCode = billingAddressDetails.PostalCode;
                billingAddress.Region = billingAddressDetails.State;
                billingAddress.StreetAddress = billingAddressDetails.Address1;
                billingAddress.ExtendedAddress = billingAddressDetails.Address2;
            }

            TransactionLineItemRequest[] transactionLineItemRequests = new TransactionLineItemRequest[1];
            transactionLineItemRequests[0] = new TransactionLineItemRequest
            {
                Description = "A cool test bolt to test things with.",
                Quantity = 1,
                UnitAmount = 1.56M,
                Name = "8T9999 Test Bolt",
                LineItemKind = TransactionLineItemKind.DEBIT,
                ProductCode = "8T9999-TEST",
                TaxAmount = 0.09M,
                UnitTaxAmount = 0.09M,
                UnitOfMeasure = "lbs",
                CommodityCode = "31161600",
                TotalAmount = 1.65M
            };


            var transactionRequest = new TransactionRequest
            {
                Amount = 8.64M,
                PaymentMethodNonce = nonce,
                CustomerId = "1",
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                ShippingAmount = 6.99M,
                LineItems = transactionLineItemRequests,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true,
                    StoreInVaultOnSuccess = true,
                    StoreShippingAddressInVault = true,
                    //AddBillingAddressToPaymentMethod = true,
                },
            };

            //TODO: add this if paymentmethodtoken exists
            //TransactionCreditCardRequest ccr = new TransactionCreditCardRequest
            //{
            //    CVV = cvv,
            //};
            //transactionRequest.CreditCard = ccr;

            Result<Transaction> transactionResult = gateway.Transaction.Sale(transactionRequest);
            if (transactionResult.IsSuccess())
            {
                Transaction transaction = transactionResult.Target;

                return RedirectToAction("Show", new { id = transaction.Id });
            }
            else if (transactionResult.Transaction != null)
            {
                return RedirectToAction("Show", new { id = transactionResult.Transaction.Id });
            }
            else
            {
                string errorMessages = "";
                foreach (ValidationError error in transactionResult.Errors.DeepAll())
                {
                    errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                }
                TempData["Flash"] = errorMessages;
                return RedirectToAction("New");
            }

        }

        public ActionResult VerifyCVV()
        {
            var gateway = config.GetGateway();
            var nonce = Request["cvv_nonce"];
            var paymentToken = Request["payment_method_token"];

            //Example of verify existing CVV
            //PaymentMethodRequest request = new PaymentMethodRequest
            //{
            //    PaymentMethodNonce = nonce,
            //    Options = new PaymentMethodOptionsRequest
            //    {
            //        VerifyCard = true
            //    }
            //};
            //Result<PaymentMethod> verifyResult = gateway.PaymentMethod.Update(paymentToken, request);

            //Example of processing a transaction and verifying the CVV
            var transactionRequest = new TransactionRequest
            {
                Amount = 20,
                PaymentMethodNonce = nonce,
                PaymentMethodToken = paymentToken,
                CustomerId = "1",
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true,
                    //AddBillingAddressToPaymentMethod = true,
                },
            };

            Result<Transaction> transactionResult = gateway.Transaction.Sale(transactionRequest);


            if (transactionResult.CreditCardVerification.CvvResponseCode == "M")
            {
                return View("Transaction Processed and CVV verified");
            } else
            {
                return View("Transaction Failed and CVV invalid/not provided");
            }
            
        }

        public ActionResult RetryPaypal()
        {
            var gateway = config.GetGateway();
            var nonce = Request["paypal_nonce"];
            var paymentToken = Request["paypal_token"];

            //Example of verify existing CVV
            //PaymentMethodRequest request = new PaymentMethodRequest
            //{
            //    PaymentMethodNonce = nonce,
            //    Options = new PaymentMethodOptionsRequest
            //    {
            //        VerifyCard = true
            //    }
            //};
            //Result<PaymentMethod> verifyResult = gateway.PaymentMethod.Update(paymentToken, request);


            //Example of processing a transaction and verifying the CVV
            var transactionRequest = new TransactionRequest
            {
                Amount = 30,
                PaymentMethodNonce = nonce,
                PaymentMethodToken = paymentToken,
                CustomerId = "1",
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true,
                    //AddBillingAddressToPaymentMethod = true,
                },
            };

            Result<Transaction> transactionResult = gateway.Transaction.Sale(transactionRequest);

            return View("PayPal transaction successful!");
        }


        public ActionResult RetryGooglePay()
        {
            var gateway = config.GetGateway();
            var nonce = Request["google_nonce"];
            var paymentToken = Request["google_token"];

            //Example of verify existing CVV
            //PaymentMethodRequest request = new PaymentMethodRequest
            //{
            //    PaymentMethodNonce = nonce,
            //    Options = new PaymentMethodOptionsRequest
            //    {
            //        VerifyCard = true
            //    }
            //};
            //Result<PaymentMethod> verifyResult = gateway.PaymentMethod.Update(paymentToken, request);


            //Example of processing a transaction and verifying the CVV
            var transactionRequest = new TransactionRequest
            {
                Amount = 40,
                PaymentMethodNonce = nonce,
                PaymentMethodToken = paymentToken,
                CustomerId = "1",
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true,
                    //AddBillingAddressToPaymentMethod = true,
                },
            };

            Result<Transaction> transactionResult = gateway.Transaction.Sale(transactionRequest);

            return View("Google Pay transaction successful!");
        }

        public ActionResult Show(String id)
        {
            var gateway = config.GetGateway(); 
            var clientToken = gateway.ClientToken.Generate();
            Transaction transaction = gateway.Transaction.Find(id);

            if (transactionSuccessStatuses.Contains(transaction.Status))
            {
                TempData["header"] = "Sweet Success!";
                TempData["icon"] = "success";
                TempData["message"] = "Your test transaction has been successfully processed. See the Braintree API response and try again.";
            }
            else
            {
                TempData["header"] = "Transaction Failed";
                TempData["icon"] = "fail";
                TempData["message"] = "Your test transaction has a status of " + transaction.Status + ". See the Braintree API response and try again.";
            };

            ViewBag.ClientToken = clientToken;
            ViewBag.Transaction = transaction;
            return View();
        }
    }
}
