﻿namespace StreamMasterDomain.Pagination;

public class M3UFileParameters : QueryStringParameters
{
    public M3UFileParameters()
    {
        OrderBy = "name desc";

    }
}
