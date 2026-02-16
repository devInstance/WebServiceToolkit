using DevInstance.WebServiceToolkit.Common.Model;
using DevInstance.WebServiceToolkit.Common.Tools;

namespace DevInstance.WebServiceToolkit.Common.Tests;

public class ModelListResultTests
{
    #region Test helpers

    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private class ReadOnlyPropItem
    {
        public string Name { get; set; } = string.Empty;
        public string ReadOnly => "constant";
    }

    private class IntPropItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    #endregion

    #region SingleItemList

    [Fact]
    public void SingleItemList_ReturnsListWithOneItem()
    {
        var item = new TestItem { Name = "Test" };

        var result = ModelListResult.SingleItemList(item);

        Assert.Single(result.Items);
        Assert.Same(item, result.Items[0]);
    }

    [Fact]
    public void SingleItemList_SetsPaginationMetadataToOne()
    {
        var item = new TestItem { Name = "Test" };

        var result = ModelListResult.SingleItemList(item);

        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.PagesCount);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public void SingleItemList_WorksWithValueType()
    {
        var result = ModelListResult.SingleItemList(42);

        Assert.Single(result.Items);
        Assert.Equal(42, result.Items[0]);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public void SingleItemList_WorksWithString()
    {
        var result = ModelListResult.SingleItemList("hello");

        Assert.Single(result.Items);
        Assert.Equal("hello", result.Items[0]);
    }

    #endregion

    #region CreateList - basic

    [Fact]
    public void CreateList_WithItemsOnly_ReturnsAllItems()
    {
        var items = new[] { new TestItem { Name = "A" }, new TestItem { Name = "B" } };

        var result = ModelListResult.CreateList(items);

        Assert.Equal(2, result.Items.Length);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public void CreateList_WithItemsOnly_DefaultsSinglePage()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items);

        Assert.Equal(0, result.Page);
        Assert.Equal(1, result.PagesCount);
    }

    [Fact]
    public void CreateList_EmptyArray_ReturnsEmptyList()
    {
        var items = Array.Empty<TestItem>();

        var result = ModelListResult.CreateList(items);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.PagesCount);
    }

    [Fact]
    public void CreateList_WithTotalCount_UsesProvidedValue()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, totalCount: 50);

        Assert.Equal(50, result.TotalCount);
        Assert.Equal(1, result.Count);
    }

    #endregion

    #region CreateList - pagination

    [Fact]
    public void CreateList_WithTopAndTotalCount_CalculatesPagesCount()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, totalCount: 25, top: 10);

        Assert.Equal(3, result.PagesCount);
    }

    [Fact]
    public void CreateList_WithTopEvenlyDivisible_CalculatesPagesCountExactly()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, totalCount: 20, top: 10);

        Assert.Equal(2, result.PagesCount);
    }

    [Fact]
    public void CreateList_WithTopZero_ReturnsSinglePage()
    {
        var items = new[] { new TestItem { Name = "A" }, new TestItem { Name = "B" } };

        var result = ModelListResult.CreateList(items, top: 0);

        Assert.Equal(1, result.PagesCount);
    }

    [Fact]
    public void CreateList_WithTopNegative_ReturnsSinglePage()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, top: -5);

        Assert.Equal(1, result.PagesCount);
    }

    [Fact]
    public void CreateList_WithPage_SetsPageIndex()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, totalCount: 30, top: 10, page: 1);

        Assert.Equal(1, result.Page);
    }

    [Fact]
    public void CreateList_WithPageExceedingTotal_ClampsToLastPage()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, totalCount: 30, top: 10, page: 100);

        Assert.Equal(2, result.Page); // last page index (0-based) for 3 pages
    }

    [Fact]
    public void CreateList_WithPageZero_SetsPageToZero()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, totalCount: 30, top: 10, page: 0);

        Assert.Equal(0, result.Page);
    }

    [Fact]
    public void CreateList_WithoutPage_DefaultsToPageZero()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, totalCount: 30, top: 10);

        Assert.Equal(0, result.Page);
    }

    #endregion

    #region CreateList - sort order and search

    [Fact]
    public void CreateList_WithSortOrder_SetsSortOrder()
    {
        var items = new[] { new TestItem { Name = "A" } };
        var sortOrder = new[] { "+Name", "-Description" };

        var result = ModelListResult.CreateList(items, sortOrder: sortOrder);

        Assert.Equal(sortOrder, result.SortOrder);
    }

    [Fact]
    public void CreateList_WithoutSortOrder_SortOrderIsNull()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items);

        Assert.Null(result.SortOrder);
    }

    [Fact]
    public void CreateList_WithSearch_SetsSearch()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items, search: "test");

        Assert.Equal("test", result.Search);
    }

    [Fact]
    public void CreateList_WithoutSearch_SearchIsNull()
    {
        var items = new[] { new TestItem { Name = "A" } };

        var result = ModelListResult.CreateList(items);

        Assert.Null(result.Search);
    }

    #endregion

    #region CreateList - search markup

    [Fact]
    public void CreateList_WithSearchMarkup_HighlightsMatchingStringProperties()
    {
        var items = new[]
        {
            new TestItem { Name = "Hello World", Description = "A greeting" }
        };

        var result = ModelListResult.CreateList(items, search: "World", useSearchMarkup: true);

        Assert.Equal("Hello <mark>World</mark>", result.Items[0].Name);
    }

    [Fact]
    public void CreateList_WithSearchMarkup_IsCaseInsensitive()
    {
        var items = new[]
        {
            new TestItem { Name = "Hello World", Description = "No match" }
        };

        var result = ModelListResult.CreateList(items, search: "world", useSearchMarkup: true);

        Assert.Equal("Hello <mark>World</mark>", result.Items[0].Name);
    }

    [Fact]
    public void CreateList_WithSearchMarkup_HighlightsMultipleOccurrences()
    {
        var items = new[]
        {
            new TestItem { Name = "test and test", Description = "another test" }
        };

        var result = ModelListResult.CreateList(items, search: "test", useSearchMarkup: true);

        Assert.Equal("<mark>test</mark> and <mark>test</mark>", result.Items[0].Name);
        Assert.Equal("another <mark>test</mark>", result.Items[0].Description);
    }

    [Fact]
    public void CreateList_WithSearchMarkup_DoesNotModifyNonMatchingProperties()
    {
        var items = new[]
        {
            new TestItem { Name = "Hello", Description = "No match here" }
        };

        var result = ModelListResult.CreateList(items, search: "xyz", useSearchMarkup: true);

        Assert.Equal("Hello", result.Items[0].Name);
        Assert.Equal("No match here", result.Items[0].Description);
    }

    [Fact]
    public void CreateList_WithSearchMarkupFalse_DoesNotHighlight()
    {
        var items = new[]
        {
            new TestItem { Name = "Hello World", Description = "A greeting" }
        };

        var result = ModelListResult.CreateList(items, search: "World", useSearchMarkup: false);

        Assert.Equal("Hello World", result.Items[0].Name);
    }

    [Fact]
    public void CreateList_WithSearchMarkupButNullSearch_DoesNotHighlight()
    {
        var items = new[]
        {
            new TestItem { Name = "Hello World", Description = "A greeting" }
        };

        var result = ModelListResult.CreateList(items, search: null, useSearchMarkup: true);

        Assert.Equal("Hello World", result.Items[0].Name);
    }

    [Fact]
    public void CreateList_WithSearchMarkupButEmptySearch_DoesNotHighlight()
    {
        var items = new[]
        {
            new TestItem { Name = "Hello World", Description = "A greeting" }
        };

        var result = ModelListResult.CreateList(items, search: "", useSearchMarkup: true);

        Assert.Equal("Hello World", result.Items[0].Name);
    }

    [Fact]
    public void CreateList_WithSearchMarkupAndWhitespaceSearch_DoesNotHighlight()
    {
        var items = new[]
        {
            new TestItem { Name = "Hello World", Description = "A greeting" }
        };

        var result = ModelListResult.CreateList(items, search: "   ", useSearchMarkup: true);

        Assert.Equal("Hello World", result.Items[0].Name);
    }

    [Fact]
    public void CreateList_WithSearchMarkup_SkipsNullItems()
    {
        var items = new TestItem?[] { null, new TestItem { Name = "Hello World" } };

        var result = ModelListResult.CreateList(items, search: "World", useSearchMarkup: true);

        Assert.Null(result.Items[0]);
        Assert.Equal("Hello <mark>World</mark>", result.Items[1]!.Name);
    }

    [Fact]
    public void CreateList_WithSearchMarkup_SkipsReadOnlyProperties()
    {
        var items = new[] { new ReadOnlyPropItem { Name = "test value" } };

        var result = ModelListResult.CreateList(items, search: "constant", useSearchMarkup: true);

        Assert.Equal("constant", result.Items[0].ReadOnly);
    }

    [Fact]
    public void CreateList_WithSearchMarkup_SkipsNonStringPropertyGracefully()
    {
        var items = new[] { new IntPropItem { Name = "item", Value = 123 } };

        // "123" matches the int's ToString(), but conversion of "<mark>123</mark>" to int fails
        // The code should silently keep the original value
        var result = ModelListResult.CreateList(items, search: "123", useSearchMarkup: true);

        Assert.Equal(123, result.Items[0].Value);
        Assert.Equal("item", result.Items[0].Name);
    }

    [Fact]
    public void CreateList_WithSearchMarkup_StillSetsSearchMetadata()
    {
        var items = new[] { new TestItem { Name = "Hello" } };

        var result = ModelListResult.CreateList(items, search: "Hello", useSearchMarkup: true);

        Assert.Equal("Hello", result.Search);
    }

    #endregion

    #region CreateList - combined parameters

    [Fact]
    public void CreateList_WithAllParameters_SetsAllFieldsCorrectly()
    {
        var items = new[]
        {
            new TestItem { Name = "Item 1" },
            new TestItem { Name = "Item 2" }
        };
        var sortOrder = new[] { "+Name" };

        var result = ModelListResult.CreateList(
            items,
            totalCount: 100,
            top: 10,
            page: 2,
            sortOrder: sortOrder,
            search: "Item"
        );

        Assert.Equal(100, result.TotalCount);
        Assert.Equal(2, result.Count);
        Assert.Equal(10, result.PagesCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(sortOrder, result.SortOrder);
        Assert.Equal("Item", result.Search);
        Assert.Same(items, result.Items);
    }

    #endregion
}
