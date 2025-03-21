﻿namespace DevInstance.WebServiceToolkit.Database.Queries;

/// <summary>
/// This interface represents searchable objects.
/// Most of the queries working with entitles with public id should implement this interface.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IQSearchable<T, K>
{
    /// <summary>
    /// Lookup by public id
    /// </summary>
    /// <param name="id">public id</param>
    /// <returns>returns the reference itself</returns>
    T ByPublicId(K id);
    /// <summary>
    /// Search function
    /// </summary>
    /// <param name="search">search keyword</param>
    /// <returns>returns the reference itself</returns>
    T Search(string search);
}

public interface IQSearchable<T> : IQSearchable<T, string>
{
}