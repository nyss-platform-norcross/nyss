﻿using System;
using System.Linq;
using System.Linq.Expressions;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Queries
{
    public static class UserNationalSocietyQueries
    {
        public static readonly Expression<Func<UserNationalSociety, bool>> IsNotDeletedUser =
            uns => !uns.User.DeletedAt.HasValue;

        public static IQueryable<UserNationalSociety> FilterAvailableUsers(this IQueryable<UserNationalSociety> userNationalSocieties) =>
            userNationalSocieties.Where(uns => !uns.User.DeletedAt.HasValue);
    }
}
