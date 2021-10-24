using System.Text.Json.Serialization;

namespace LennyBOTv3.Models
{
    public record Current
    {
        /// <summary>
        /// Returns the cloud cover level in percentage.
        /// </summary>
        [JsonPropertyName("cloudcover")]
        public int? Cloudcover { get; init; }

        /// <summary>
        /// Returns the "Feels Like" temperature in the selected unit. (Default: Celsius)
        /// </summary>
        [JsonPropertyName("feelslike")]
        public double? Feelslike { get; init; }

        /// <summary>
        /// Returns the air humidity level in percentage.
        /// </summary>
        [JsonPropertyName("humidity")]
        public int? Humidity { get; init; }

        [JsonPropertyName("is_day")]
        public string? IsDay { get; init; }

        /// <summary>
        /// Returns the UTC time for when the returned whether data was collected.
        /// </summary>
        [JsonPropertyName("observation_time")]
        public string? ObservationTime { get; init; }

        /// <summary>
        /// Returns the precipitation level in the selected unit. (Default: MM - millimeters)
        /// </summary>
        [JsonPropertyName("precip")]
        public double? Precip { get; init; }

        /// <summary>
        /// Returns the air pressure in the selected unit. (Default: MB - millibar)
        /// </summary>
        [JsonPropertyName("pressure")]
        public double? Pressure { get; init; }

        /// <summary>
        /// Returns the temperature in the selected unit. (Default: Celsius)
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature { get; init; }

        /// <summary>
        /// Returns the UV index associated with the current weather condition.
        /// </summary>
        [JsonPropertyName("uv_index")]
        public int? UVIndex { get; init; }

        /// <summary>
        /// Returns the visibility level in the selected unit. (Default: kilometers)
        /// </summary>
        [JsonPropertyName("visibility")]
        public int? Visibility { get; init; }

        /// <summary>
        /// Returns the universal weather condition code associated with the current weather condition.
        /// You can download all available weather codes using this link:
        /// https://weatherstack.com/site_resources/weatherstack-weather-condition-codes.zip
        /// </summary>
        [JsonPropertyName("weather_code")]
        public int? WeatherCode { get; init; }

        /// <summary>
        /// Returns one or more weather description texts associated with the current weather condition.
        /// </summary>
        [JsonPropertyName("weather_descriptions")]
        public List<string?>? WeatherDescriptions { get; init; }

        /// <summary>
        /// Returns one or more PNG weather icons associated with the current weather condition.
        /// </summary>
        [JsonPropertyName("weather_icons")]
        public List<string?>? WeatherIcons { get; init; }

        /// <summary>
        /// Returns the wind degree.
        /// </summary>
        [JsonPropertyName("wind_degree")]
        public int? WindDegree { get; init; }

        /// <summary>
        /// Returns the wind direction.
        /// </summary>
        [JsonPropertyName("wind_dir")]
        public string? WindDir { get; init; }

        /// <summary>
        /// Returns the wind speed in the selected unit. (Default: kilometers/hour)
        /// </summary>
        [JsonPropertyName("wind_speed")]
        public double? WindSpeed { get; init; }
    }

    public record Error
    {
        [JsonPropertyName("code")]
        public int? Code { get; init; }

        [JsonPropertyName("info")]
        public string? Info { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }

        public override string ToString() => $"{Code}: {Info}";
    }

    public record Location
    {
        /// <summary>
        /// Returns the country name associated with the location used for this request.
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; init; }

        /// <summary>
        /// Returns the latitude coordinate associated with the location used for this request.
        /// </summary>
        [JsonPropertyName("lat")]
        public string? Lat { get; init; }

        /// <summary>
        /// Returns the local time of the location used for this request. (Example: 2019-09-07 08:14)
        /// </summary>
        [JsonPropertyName("localtime")]
        public string? Localtime { get; init; }

        /// <summary>
        /// Returns the local time (as UNIX timestamp) of the location used for this request. (Example: 1567844040)
        /// </summary>
        [JsonPropertyName("localtime_epoch")]
        public long? LocaltimeEpoch { get; init; }

        /// <summary>
        /// Returns the longitude coordinate associated with the location used for this request.
        /// </summary>
        [JsonPropertyName("lon")]
        public string? Lon { get; init; }

        /// <summary>
        /// Returns the name of the location used for this request.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>
        /// Returns the region name associated with the location used for this request.
        /// </summary>
        [JsonPropertyName("region")]
        public string? Region { get; init; }

        /// <summary>
        /// Returns the timezone ID associated with the location used for this request. (Example: America/New_York)
        /// </summary>
        [JsonPropertyName("timezone_id")]
        public string? TimezoneId { get; init; }

        /// <summary>
        /// Returns the UTC offset (in hours) of the timezone associated with the location used for this request. (Example: -4.0)
        /// </summary>
        [JsonPropertyName("utc_offset")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double? UtcOffset { get; init; }
    }

    public record Request
    {
        /// <summary>
        /// Returns the ISO-Code of the language used for this request.
        /// </summary>
        [JsonPropertyName("language")]
        public string? Language { get; init; }

        /// <summary>
        /// Returns the exact location identifier query used for this request.
        /// </summary>
        [JsonPropertyName("query")]
        public string? Query { get; init; }

        /// <summary>
        /// Returns the type of location lookup used for this request.
        /// Possible values: City, LatLon, IP, Zipcode
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        /// <summary>
        /// Returns the unit identifier used for this request:
        /// m for Metric, s for Scientific, f for Fahrenheit
        /// </summary>
        [JsonPropertyName("unit")]
        public string? Unit { get; init; }
    }

    /// <summary>
    /// https://weatherstack.com/documentation#current_weather
    /// </summary>
    public record WeatherstackModel
    {
        [JsonPropertyName("current")]
        public Current? Current { get; init; }

        /// <summary>
        /// Whenever an API request fails, the weatherstack API will return an error object in lightweight JSON format.
        /// Each error object contains an error code, an error type and an info object containing details about the error that occurred.
        /// </summary>
        [JsonPropertyName("error")]
        public Error? Error { get; init; }

        [JsonPropertyName("location")]
        public Location? Location { get; init; }

        [JsonPropertyName("request")]
        public Request? Request { get; init; }

        /// <summary>
        /// https://weatherstack.com/documentation#api_error_codes
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; init; } = true;
    }
}
