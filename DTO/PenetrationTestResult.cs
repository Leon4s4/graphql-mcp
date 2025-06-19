namespace Graphql.Mcp.DTO;

public class PenetrationTestResult
{
    public string TestName { get; set; } = "";
    public string Description { get; set; } = "";
    public string TestQuery { get; set; } = "";
    public string ExpectedBehavior { get; set; } = "";
    public string Risk { get; set; } = "";
}