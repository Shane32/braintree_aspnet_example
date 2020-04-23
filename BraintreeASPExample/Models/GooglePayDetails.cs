using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BraintreeASPExample.Models
{
    public partial class GooglePayDetails
    {
        [JsonProperty("cardType")]
        public string CardType { get; set; }

        [JsonProperty("lastFour")]
        public string LastFour { get; set; }

        [JsonProperty("lastTwo")]
        public string LastTwo { get; set; }

        [JsonProperty("isNetworkTokenized")]
        public bool IsNetworkTokenized { get; set; }
    }
}
