using System.Xml.Serialization;

namespace LennyBOTv3.Models
{
    [XmlRoot(ElementName = "city")]
    public record City
    {
        [XmlElement(ElementName = "coord")]
        public Coord? Coord { get; init; }

        /// <summary>
        /// Country code (GB, JP etc.)
        /// </summary>
        [XmlElement(ElementName = "country")]
        public string? Country { get; init; }

        /// <summary>
        /// City ID
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; init; }

        /// <summary>
        ///  City name
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; init; }

        [XmlElement(ElementName = "sun")]
        public Sun? Sun { get; init; }

        /// <summary>
        /// Shift in seconds from UTC
        /// </summary>
        [XmlElement(ElementName = "timezone")]
        public int Timezone { get; init; }
    }

    [XmlRoot(ElementName = "clouds")]
    public record Clouds
    {
        /// <summary>
        /// Name of the cloudiness
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; init; }

        /// <summary>
        /// Cloudiness
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; init; }
    }

    [XmlRoot(ElementName = "coord")]
    public record Coord
    {
        /// <summary>
        /// City geo location, latitude
        /// </summary>
        [XmlAttribute(AttributeName = "lat")]
        public double Lat { get; init; }

        /// <summary>
        /// City geo location, longitude
        /// </summary>
        [XmlAttribute(AttributeName = "lon")]
        public double Lon { get; init; }
    }

    [XmlRoot(ElementName = "direction")]
    public record Direction
    {
        /// <summary>
        ///  Code of the wind direction. Possible value is WSW, N, S etc.
        /// </summary>
        [XmlAttribute(AttributeName = "code")]
        public string? Code { get; init; }

        /// <summary>
        /// Full name of the wind direction.
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; init; }

        /// <summary>
        /// Wind direction, degrees (meteorological)
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; init; }
    }

    [XmlRoot(ElementName = "feels_like")]
    public record FeelsLike
    {
        /// <summary>
        /// Unit of measurements. Possible value is Celsius, Kelvin, Fahrenheit. Unit Default: Kelvin
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; init; }

        /// <summary>
        /// Temperature. This temperature parameter accounts for the human perception of weather.
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public double Value { get; init; }
    }

    [XmlRoot(ElementName = "gusts")]
    public record Gusts
    {
        /// <summary>
        /// Wind gust. Unit Default: meter/sec, Metric: meter/sec, Imperial: miles/hour
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public double Value { get; init; }
    }

    [XmlRoot(ElementName = "humidity")]
    public record Humidity
    {
        /// <summary>
        /// Humidity units, %
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; init; }

        /// <summary>
        /// Humidity value
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; init; }
    }

    [XmlRoot(ElementName = "lastupdate")]
    public record Lastupdate
    {
        /// <summary>
        /// Last time when data was updated
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public DateTime Value { get; init; }
    }

    [XmlRoot(ElementName = "current")]
    public record OpenWeatherMapModel
    {
        [XmlElement(ElementName = "city")]
        public City? City { get; init; }

        [XmlElement(ElementName = "clouds")]
        public Clouds? Clouds { get; init; }

        [XmlElement(ElementName = "feels_like")]
        public FeelsLike? FeelsLike { get; init; }

        [XmlElement(ElementName = "humidity")]
        public Humidity? Humidity { get; init; }

        [XmlElement(ElementName = "lastupdate")]
        public Lastupdate? Lastupdate { get; init; }

        [XmlElement(ElementName = "precipitation")]
        public Precipitation? Precipitation { get; init; }

        [XmlElement(ElementName = "pressure")]
        public Pressure? Pressure { get; init; }

        [XmlElement(ElementName = "temperature")]
        public Temperature? Temperature { get; init; }

        [XmlElement(ElementName = "visibility")]
        public Visibility? Visibility { get; init; }

        [XmlElement(ElementName = "weather")]
        public Weather? Weather { get; init; }

        [XmlElement(ElementName = "wind")]
        public Wind? Wind { get; init; }
    }

    [XmlRoot(ElementName = "precipitation")]
    public record Precipitation
    {
        /// <summary>
        /// Possible values are 'no", name of weather phenomena as 'rain', 'snow'
        /// </summary>
        [XmlAttribute(AttributeName = "mode")]
        public string? Mode { get; init; }

        /// <summary>
        /// Precipitation, mm
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; init; }
    }

    [XmlRoot(ElementName = "pressure")]
    public record Pressure
    {
        /// <summary>
        /// Pressure units, hPa
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; init; }

        /// <summary>
        /// Pressure value
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; init; }
    }

    [XmlRoot(ElementName = "speed")]
    public record Speed
    {
        /// <summary>
        /// Type of the wind
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string? Name { get; init; }

        /// <summary>
        /// Wind speed units, m/s
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; init; }

        /// <summary>
        /// Wind speed
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public double Value { get; init; }
    }

    [XmlRoot(ElementName = "sun")]
    public record Sun
    {
        /// <summary>
        /// Sunrise time
        /// </summary>
        [XmlAttribute(AttributeName = "rise")]
        public DateTime Rise { get; init; }

        /// <summary>
        ///  Sunset time
        /// </summary>
        [XmlAttribute(AttributeName = "set")]
        public DateTime Set { get; init; }
    }

    [XmlRoot(ElementName = "temperature")]
    public record Temperature
    {
        /// <summary>
        /// Maximum temperature at the moment of calculation. This is maximal currently observed
        /// temperature (within large megalopolises and urban areas), use this parameter optionally.
        /// </summary>
        [XmlAttribute(AttributeName = "max")]
        public double Max { get; init; }

        /// <summary>
        /// Minimum temperature at the moment of calculation. This is minimal currently observed
        /// temperature (within large megalopolises and urban areas), use this parameter optionally.
        /// </summary>
        [XmlAttribute(AttributeName = "min")]
        public double Min { get; init; }

        /// <summary>
        /// Unit of measurements. Possible value is Celsius, Kelvin, Fahrenheit.
        /// </summary>
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; init; }

        /// <summary>
        /// Temperature
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public double Value { get; init; }
    }

    [XmlRoot(ElementName = "visibility")]
    public record Visibility
    {
        /// <summary>
        /// Visibility, meter
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; init; }
    }

    [XmlRoot(ElementName = "weather")]
    public record Weather
    {
        /// <summary>
        /// Weather icon id
        /// </summary>
        [XmlAttribute(AttributeName = "icon")]
        public string? Icon { get; init; }

        /// <summary>
        /// Weather condition id
        /// </summary>
        [XmlAttribute(AttributeName = "number")]
        public int Number { get; init; }

        /// <summary>
        /// Weather condition name
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public string? Value { get; init; }
    }

    [XmlRoot(ElementName = "wind")]
    public record Wind
    {
        [XmlElement(ElementName = "direction")]
        public Direction? Direction { get; init; }

        [XmlElement(ElementName = "gusts")]
        public Gusts? Gusts { get; init; }

        [XmlElement(ElementName = "speed")]
        public Speed? Speed { get; init; }
    }
}
