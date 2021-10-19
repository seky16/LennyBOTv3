using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LennyBOTv3.Settings
{
    internal record ApiSettings
    {
        public static string SectionKey => nameof(ApiSettings);

        [Required, NotNull]
        public string? FixerSharpApiKey { get; set; }

        [Required, NotNull]
        public string? YouTubeApiKey { get; set; }
    }
}
