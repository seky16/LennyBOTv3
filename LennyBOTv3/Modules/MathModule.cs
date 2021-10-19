using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FixerSharp;

namespace LennyBOTv3.Modules
{
    public class MathModule : LennyBaseModule
    {
        [Command("calc")]
        [Description("Use https://api.mathjs.org/ to calculate stuff")]
        public async Task CalcCmdAsync(CommandContext ctx,
            [RemainingText, Description("The expression to be evaluated")] string expression)
        {
            expression = expression.Replace("`", string.Empty);
            var result = await Helpers.GetStringAsync($"http://api.mathjs.org/v4/?expr={expression}");
            await ctx.RespondAsync(Formatter.BlockCode($"{expression} = {result}", "yaml"));
        }

        [Command("conv")]
        [Description("Currency conversion using https://fixer.io/")]
        public async Task ConvCmdAsync(CommandContext ctx,
            [Description("The amount to be converted")] double amount,
            [Description("The three-letter currency code of the currency you would like to convert from")] string from,
            [RemainingText, Description("The three-letter currency code of the currency you would like to convert to")] string to)
        {
            if (to.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
                to = to[3..];

            var result = await Fixer.ConvertAsync(from, to, amount);
            result = Math.Round(result, 2);
            var embed = ctx.GetAuthorEmbedBuilder()
                .WithColor(DiscordColor.DarkGreen)
                .WithDescription($"{amount} {from.ToUpper()} = **{result} {to.ToUpper()}**");
            await ctx.RespondAsync(embed);
        }
    }
}
