﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Boom
{
    public class Billing
    {
        [JsonProperty("comboAvailable")]
        public bool ComboAvailable { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("expiryDate")]
        public int ExpiryDate { get; set; }

        [JsonProperty("trialAvailable")]
        public bool TrialAvailable { get; set; }

        [JsonProperty("subscriptionRegion")]
        public string SubscriptionRegion { get; set; }
    }
}
