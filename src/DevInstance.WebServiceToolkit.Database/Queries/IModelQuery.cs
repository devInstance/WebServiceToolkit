﻿using System.Threading.Tasks;

namespace DevInstance.WebServiceToolkit.Database.Queries;

public interface IModelQuery<T, D>
{
    /// <summary>
    /// Creates a new instance of the model type T.
    /// </summary>
    /// <returns>A new instance of the model type T.</returns>
    T CreateNew();

    /// <summary>
    /// Adds a new record to the data store.
    /// </summary>
    /// <param name="record">The record of type T to add.</param>
    Task AddAsync(T record);

    /// <summary>
    /// Updates an existing record in the data store.
    /// </summary>
    /// <param name="record">The record of type T to update.</param>
    Task UpdateAsync(T record);

    /// <summary>
    /// Removes a record from the data store.
    /// </summary>
    /// <param name="record">The record of type T to remove.</param>
    Task RemoveAsync(T record);

    /// <summary>
    /// Creates a deep copy of the implementing query with its current state.
    /// </summary>
    /// <returns>A deep copy of the implementing instance of type D.</returns>
    D Clone();
}
