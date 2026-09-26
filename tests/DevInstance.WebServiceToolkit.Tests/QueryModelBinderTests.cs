using System.ComponentModel;
using DevInstance.WebServiceToolkit.Http.Query;
using Microsoft.AspNetCore.Http;

namespace DevInstance.WebServiceToolkit.Tests;

public class QueryModelBinderTests
{
    [QueryModel]
    public class SampleQuery
    {
        [DefaultValue(20)]
        public int Top { get; set; }
        public string? Search { get; set; }
        public string[]? SortBy { get; set; }
        public List<int>? Ids { get; set; }
        public IEnumerable<string>? Tags { get; set; }
        public DateTime? Since { get; set; }
    }

    private static HttpRequest Request(string query)
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString(query);
        return context.Request;
    }

    [Fact]
    public void String_IsBoundAsScalar_NotSplitIntoChars()
    {
        var ok = QueryModelBinder.TryBind<SampleQuery>(Request("?search=a,b c"), out var model, out var errors);

        Assert.True(ok, string.Join("; ", errors.Select(e => $"{e.Key}: {e.Value}")));
        Assert.Equal("a,b c", model.Search);
    }

    [Fact]
    public void Collections_AreCommaSeparated()
    {
        var ok = QueryModelBinder.TryBind<SampleQuery>(Request("?sortBy=-Name,Date&ids=1,2,3&tags=x,y"), out var model, out var errors);

        Assert.True(ok, string.Join("; ", errors.Select(e => $"{e.Key}: {e.Value}")));
        Assert.Equal(new[] { "-Name", "Date" }, model.SortBy);
        Assert.Equal(new List<int> { 1, 2, 3 }, model.Ids);
        Assert.Equal(new[] { "x", "y" }, model.Tags);
    }

    [Fact]
    public void Defaults_AndUtcDates()
    {
        var ok = QueryModelBinder.TryBind<SampleQuery>(Request("?since=2026-09-25T14:30:00Z"), out var model, out _);

        Assert.True(ok);
        Assert.Equal(20, model.Top);
        Assert.Equal(DateTimeKind.Utc, model.Since!.Value.Kind);
        Assert.Equal(new DateTime(2026, 9, 25, 14, 30, 0, DateTimeKind.Utc), model.Since);
    }
}
