﻿using StreamMasterDomain.Attributes;

using System.Xml.Serialization;

namespace StreamMasterDomain.Pagination;

[RequireAll]
public class PagedResponse<T>
{
    [XmlIgnore]
    public List<T> Data { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItemCount { get; set; }
    public int TotalPageCount { get; set; }
    public int TotalRecords { get; set; }
    public int First { get; set; }
}

public static class PagedListExtensions
{
    public static PagedResponse<T> ToPagedResponse<T>(this IPagedList<T> pagedList, int totalRecords)
    {
        int first = (pagedList.PageNumber - 1) * pagedList.PageSize;
        return new PagedResponse<T>
        {
            Data = pagedList.ToList(),
            First = first,
            PageNumber = pagedList.PageNumber,
            PageSize = pagedList.PageSize,
            TotalItemCount = pagedList.TotalItemCount,
            TotalPageCount = pagedList.PageCount,
            TotalRecords = totalRecords
        };
    }
}