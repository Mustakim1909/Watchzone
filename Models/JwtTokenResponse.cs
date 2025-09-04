using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Models
{
    public class JwtTokenResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("user_email")]
        public string UserEmail { get; set; }

        [JsonProperty("user_nicename")]
        public string UserDisplayName { get; set; }
    }
}
