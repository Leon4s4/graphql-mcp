using System.ComponentModel;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static partial class BranchNamingTool
{
    [McpServerTool, Description("Generates a Git branch name from a ticket number and issue type")]
    public static string GenerateBranchName(
        [Description("Ticket description in format: [number*] some text. Example: 57818 Test the graphql feature on account")]
        string ticketDescription,
        [Description("Issue type (feature, bug, epic, etc)")]
        string issueType)
    {
        var numberMatch = CreateTicketNumberPattern()
            .Match(ticketDescription);

        if (!numberMatch.Success)
            return "Error: Could not find a ticket number at the beginning of the description.";

        var ticketNumber = numberMatch.Groups[1].Value;

        var description = ticketDescription[(numberMatch.Index + numberMatch.Length)..]
            .Trim();


        var formattedDescription = RemoveSpecialCharacters()
            .Replace(description, "");
        formattedDescription = ReplaceSpacesWithUnderscoresRegex()
            .Replace(formattedDescription, "_");
        formattedDescription = formattedDescription.ToLowerInvariant();

        return $"{issueType.ToLowerInvariant()}/{ticketNumber}-{formattedDescription}";
    }

    [GeneratedRegex(@"^\s*(\d+)")]
    private static partial Regex CreateTicketNumberPattern();

    [GeneratedRegex(@"[^\w\s-]")]
    private static partial Regex RemoveSpecialCharacters();

    [GeneratedRegex(@"\s+")]
    private static partial Regex ReplaceSpacesWithUnderscoresRegex();
}