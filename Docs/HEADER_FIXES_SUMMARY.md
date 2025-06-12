# HTTP Header Handling Fixes

## Issue Description
The GraphQL MCP server was experiencing schema introspection errors due to improper handling of HTTP headers, specifically the `Content-Type` header. The error message was:

```
Error during schema introspection: Misused header name, 'Content-Type'. Make sure request headers are used with HttpRequestMessage, response headers with HttpResponseMessage, and content headers with HttpContent objects.
```

## Root Cause
The issue occurred because content headers like `Content-Type` were being added to `HttpClient.DefaultRequestHeaders`, but these headers should be set on the content object itself (e.g., `StringContent`) rather than as request headers.

## Files Fixed

### 1. PerformanceMonitoringTools.cs
- **Location**: Lines 30-38
- **Fix**: Added `IsContentHeader()` check to skip content headers when adding to `DefaultRequestHeaders`
- **Added**: Helper method `IsContentHeader()` to identify content headers

### 2. QueryValidationTools.cs
- **Location**: Lines 307-315
- **Fix**: Added `IsContentHeader()` check and exception handling
- **Added**: Helper method `IsContentHeader()` at line 543

### 3. GraphQLSchemaTools.cs
- **Location**: Lines 387-401
- **Fix**: Added `IsContentHeader()` check in header processing loop
- **Added**: Helper method `IsContentHeader()` at line 580

### 4. SchemaIntrospectionTools.cs
- **Location**: Lines 21-29
- **Fix**: Added `IsContentHeader()` check for header filtering
- **Added**: Helper method `IsContentHeader()` at line 368

### 5. DynamicToolRegistry.cs
- **Location**: Lines 144-152
- **Fix**: Added `IsContentHeader()` check in header processing
- **Added**: Helper method `IsContentHeader()` at line 530

## IsContentHeader Helper Method
Added to each affected class to identify HTTP content headers that should not be added to request headers:

```csharp
private static bool IsContentHeader(string headerName)
{
    var contentHeaders = new[]
    {
        "Content-Type", "Content-Length", "Content-Encoding", "Content-Language",
        "Content-Location", "Content-MD5", "Content-Range", "Content-Disposition",
        "Expires", "Last-Modified"
    };
    return contentHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
}
```

## Additional Safety Measures
- Added `try-catch` blocks around `DefaultRequestHeaders.Add()` calls to handle restricted headers
- Maintained existing `StringContent` constructor usage which correctly sets `Content-Type: application/json`

## Testing
- ✅ Project builds successfully with only unrelated warnings
- ✅ Server starts without schema introspection errors
- ✅ All HTTP requests now properly separate content headers from request headers

## Impact
This fix resolves the schema introspection error that was preventing proper GraphQL schema analysis and tool functionality. The server can now correctly handle HTTP headers according to .NET HttpClient best practices.
