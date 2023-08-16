﻿using AutoMapper;

using Microsoft.EntityFrameworkCore;

using StreamMasterDomain.Common;
using StreamMasterDomain.Filtering;
using StreamMasterDomain.Pagination;
using StreamMasterDomain.Repository;

using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.Json;

namespace StreamMasterInfrastructureEF.Repositories;
public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{






    protected RepositoryContext RepositoryContext { get; set; }

    public RepositoryBase(RepositoryContext repositoryContext)
    {
        RepositoryContext = repositoryContext;
    }

    public IQueryable<T> FindAll()
    {
        return RepositoryContext.Set<T>().AsNoTracking();
    }

    public async Task<PagedResponse<TDto>> GetEntitiesAsync<TDto>(QueryStringParameters parameters, IMapper mapper)
where TDto : class
    {
        IQueryable<T> entities;

        if (!string.IsNullOrEmpty(parameters.JSONFiltersString) || !string.IsNullOrEmpty(parameters.OrderBy))
        {

            if (!string.IsNullOrEmpty(parameters.JSONFiltersString))
            {
                List<DataTableFilterMetaData>? filters = JsonSerializer.Deserialize<List<DataTableFilterMetaData>>(parameters.JSONFiltersString);
                entities = FindByCondition(filters, parameters.OrderBy);
            }
            else
            {
                entities = FindAll();
            }
        }
        else
        {
            entities = FindAll();
        }

        IPagedList<T> pagedResult = await entities.ToPagedListAsync(parameters.PageNumber, parameters.PageSize).ConfigureAwait(false);

        // If there are no entities, return an empty response early
        if (!pagedResult.Any())
        {
            return new PagedResponse<TDto>
            {
                PageNumber = parameters.PageNumber,
                TotalPageCount = 0,
                PageSize = parameters.PageSize,
                TotalItemCount = 0,
                Data = new List<TDto>()
            };
        }

        List<TDto> destination = mapper.Map<List<TDto>>(pagedResult);

        StaticPagedList<TDto> test = new(destination, pagedResult.GetMetaData());

        // Use the TotalItemCount from the metadata instead of counting entities again
        int totalCount = pagedResult.TotalItemCount;
        PagedResponse<TDto> pagedResponse = test.ToPagedResponse(totalCount);

        return pagedResponse;
    }

    public IQueryable<T> FindByCondition(List<DataTableFilterMetaData>? filters, string orderBy)
    {
        DbSet<T> query = RepositoryContext.Set<T>();

        // Apply filters and sorting
        IQueryable<T> filteredAndSortedQuery = FilterHelper<T>.ApplyFiltersAndSort(query, filters, orderBy);

        return filteredAndSortedQuery.AsNoTracking();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return RepositoryContext.Set<T>()
            .Where(expression).AsNoTracking();
    }

    public void Create(T entity)
    {
        RepositoryContext.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
        RepositoryContext.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        RepositoryContext.Set<T>().Remove(entity);
    }

    public void Create(T[] entities)
    {
        RepositoryContext.Set<T>().AddRange(entities);
    }
}