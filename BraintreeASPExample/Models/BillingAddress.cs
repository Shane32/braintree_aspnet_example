using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BraintreeASPExample.Models
{
    public partial class BillingAddress
    {
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("address3")]
        public string Address3 { get; set; }

        [JsonProperty("sortingCode")]
        public string SortingCode { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("locality")]
        public string City { get; set; }

        [JsonProperty("administrativeArea")]
        public string State { get; set; }
    }
}
