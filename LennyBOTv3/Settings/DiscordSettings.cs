﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LennyBOTv3.Settings
{
    public record DiscordSettings
    {
        public static string SectionKey => nameof(DiscordSettings);

        [Required, NotNull]
        public string? Token { get; set; }

        [Required, NotNull]
        public string? Prefix { get; set; }
    }
}
