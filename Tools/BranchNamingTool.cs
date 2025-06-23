using System.ComponentModel;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static partial class BranchNamingTool
{
    [McpServerTool, Description(@"Professional Git branch naming system with intelligent ticket parsing and standardized formatting conventions.

This development workflow tool provides comprehensive branch naming including:
1. Automatic ticket number extraction and validation
2. Issue type categorization and standardization
3. Description normalization and formatting
4. Special character handling and sanitization
5. Convention compliance across development teams
6. Multi-project naming pattern support

Branch Naming Features:
- Intelligent ticket number detection from various formats
- Issue type validation (feature, bug, epic, hotfix, etc.)
- Description sanitization with underscore formatting
- Special character removal and safe encoding
- Consistent lowercase formatting conventions
- Team-standardized naming pattern enforcement

Supported Patterns:
- feature/12345-implement_user_authentication
- bug/67890-fix_login_validation_error
- epic/11111-complete_graphql_migration
- hotfix/99999-urgent_security_patch

Development Benefits:
- Ensures consistent branch naming across teams
- Facilitates automated CI/CD pipeline integration
- Improves project organization and tracking
- Enables efficient branch management and filtering
- Supports agile development workflows
- Maintains professional Git history standards

Essential for team collaboration and project management integration.")]
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