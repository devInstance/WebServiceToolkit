using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.WebServiceToolkit.Common.Tests;

public class ModelListTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
    }

    #region Empty

    [Fact]
    public void Empty_ReturnsEmptyItemsArray()
    {
        var result = ModelList<TestItem>.Empty();

        Assert.Empty(result.Items);
    }

    [Fact]
    public void Empty_SetsAllCountsToZero()
    {
        var result = ModelList<TestItem>.Empty();

        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.PagesCount);
        Assert.Equal(0, result.Page);
    }

    [Fact]
    public void Empty_WorksWithValueType()
    {
        var result = ModelList<int>.Empty();

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    #endregion

    #region Single

    [Fact]
    public void Single_ReturnsListWithOneItem()
    {
        var item = new TestItem { Name = "Test" };

        var result = ModelList<TestItem>.Single(item);

        Assert.Single(result.Items);
        Assert.Same(item, result.Items[0]);
    }

    [Fact]
    public void Single_SetsAllCountsToOne()
    {
        var item = new TestItem { Name = "Test" };

        var result = ModelList<TestItem>.Single(item);

        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.PagesCount);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public void Single_WorksWithValueType()
    {
        var result = ModelList<int>.Single(42);

        Assert.Single(result.Items);
        Assert.Equal(42, result.Items[0]);
        Assert.Equal(1, result.TotalCount);
    }

    #endregion

    #region IsEmpty

    [Fact]
    public void IsEmpty_WithNull_ReturnsTrue()
    {
        Assert.True(ModelList<TestItem>.IsEmpty(null));
    }

    [Fact]
    public void IsEmpty_WithNullItems_ReturnsTrue()
    {
        var list = new ModelList<TestItem> { Items = null! };

        Assert.True(ModelList<TestItem>.IsEmpty(list));
    }

    [Fact]
    public void IsEmpty_WithEmptyItems_ReturnsTrue()
    {
        var list = ModelList<TestItem>.Empty();

        Assert.True(ModelList<TestItem>.IsEmpty(list));
    }

    [Fact]
    public void IsEmpty_WithItems_ReturnsFalse()
    {
        var list = ModelList<TestItem>.Single(new TestItem { Name = "Test" });

        Assert.False(ModelList<TestItem>.IsEmpty(list));
    }

    #endregion

    #region FromArray

    [Fact]
    public void FromArray_WithItems_SetsItemsAndCounts()
    {
        var items = new[] { new TestItem { Name = "A" }, new TestItem { Name = "B" }, new TestItem { Name = "C" } };

        var result = ModelList<TestItem>.FromArray(items);

        Assert.Equal(3, result.Items.Length);
        Assert.Same(items[0], result.Items[0]);
        Assert.Same(items[1], result.Items[1]);
        Assert.Same(items[2], result.Items[2]);
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.PagesCount);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public void FromArray_WithEmptyArray_ReturnsEmptyList()
    {
        var result = ModelList<TestItem>.FromArray(Array.Empty<TestItem>());

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.PagesCount);
        Assert.Equal(0, result.Page);
    }

    [Fact]
    public void FromArray_WithNull_ReturnsEmptyList()
    {
        var result = ModelList<TestItem>.FromArray(null!);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.PagesCount);
        Assert.Equal(0, result.Page);
    }

    [Fact]
    public void FromArray_WithSingleItem_SetsSinglePage()
    {
        var items = new[] { new TestItem { Name = "Only" } };

        var result = ModelList<TestItem>.FromArray(items);

        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.PagesCount);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public void FromArray_WorksWithValueType()
    {
        var items = new[] { 1, 2, 3 };

        var result = ModelList<int>.FromArray(items);

        Assert.Equal(3, result.Items.Length);
        Assert.Equal(3, result.TotalCount);
    }

    #endregion
}
