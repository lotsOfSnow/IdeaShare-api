using System;
using System.Data.Common;

namespace IdeaShare.Extensions.System
{
    public static class ExceptionExtensions
    {
        // The SqlException // SqliteException might be wrapped if we are using ORM, so we go through each nested inner exception
        // recursively.
        public static T GetSqlException<T>(this Exception exception) where T : DbException
        {
            if (exception is T sqlException) return sqlException;
            return exception.InnerException == null ? null : GetSqlException<T>(exception.InnerException);
        }
    }
}
