using Newtonsoft.Json;

namespace ThreeTP.Payment.Application.DTOs.Responses.BIN_Checker;

public class BinlookupResponse
{
    public string country { get; set; }

    [JsonProperty("country-code")] public string countrycode { get; set; }

    [JsonProperty("card-brand")] public string cardbrand { get; set; }

    [JsonProperty("ip-city")] public string ipcity { get; set; }

    [JsonProperty("ip-blocklists")] public List<object> ipblocklists { get; set; }

    [JsonProperty("ip-country-code3")] public string ipcountrycode3 { get; set; }

    [JsonProperty("is-commercial")] public bool iscommercial { get; set; }

    [JsonProperty("ip-country")] public string ipcountry { get; set; }

    [JsonProperty("bin-number")] public string binnumber { get; set; }
    public string issuer { get; set; }

    [JsonProperty("issuer-website")] public string issuerwebsite { get; set; }

    [JsonProperty("ip-region")] public string ipregion { get; set; }
    public bool valid { get; set; }

    [JsonProperty("card-type")] public string cardtype { get; set; }

    [JsonProperty("is-prepaid")] public bool isprepaid { get; set; }

    [JsonProperty("ip-blocklisted")] public bool ipblocklisted { get; set; }

    [JsonProperty("card-category")] public string cardcategory { get; set; }

    [JsonProperty("issuer-phone")] public string issuerphone { get; set; }

    [JsonProperty("currency-code")] public string currencycode { get; set; }

    [JsonProperty("ip-matches-bin")] public bool ipmatchesbin { get; set; }

    [JsonProperty("country-code3")] public string countrycode3 { get; set; }

    [JsonProperty("ip-country-code")] public string ipcountrycode { get; set; }
}