﻿using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CouchDB.Driver.Extensions
{
    public static class QueryableExtensions
    {
        #region Helper methods to obtain MethodInfo in a safe way

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters")]
        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
        {
            return f.Method;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters")]
        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.Method;
        }

        #endregion

        /// <summary>
        /// Creates a <see cref="List{TSource}"/> from a sequence by enumerating it asynchronously.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}"/> that contains elements from the sequence.</retuns>
        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task<List<TSource>>.Factory.StartNew(source.ToList);
        }

        /// <summary>
        /// Creates a CouchList from the sequence.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <returns>A CouchList that contains elements from the sequence.</returns>
        public static CouchList<TSource> ToCouchList<TSource>(this IQueryable<TSource> source)
        {
            if (source is CouchQuery<TSource> couchQuery)
            {
                return couchQuery.ToCouchList();
            }
            throw new NotSupportedException($"The method CompleteResult is not supported on this type of IQueryable");
        }

        /// <summary>
        /// Creates a CouchList from a sequence by enumerating it asynchronously.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <retuns>A task that represents the asynchronous operation. The task result contains a CouchList that contains elements from the sequence.</retuns>
        public static Task<CouchList<TSource>> ToCouchListAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task<CouchList<TSource>>.Factory.StartNew(source.ToCouchList);
        }

        /// <summary>
        /// Paginates elements in the sequence using a bookmark.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <param name="bookmark">A string that enables you to specify which page of results you require.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the paginated of elements of the sequence.</return>
        public static IQueryable<TSource> UseBookmark<TSource>(this IQueryable<TSource> source, string bookmark)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (string.IsNullOrEmpty(bookmark))
            {
                throw new ArgumentNullException(nameof(bookmark));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(UseBookmark, source, bookmark),
                    new[] { source.Expression, Expression.Constant(bookmark) }));
        }

        /// <summary>
        /// Ensures that elements from the sequence will be read from at least that many replicas.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <param name="quorum">Read quorum needed for the result.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the elements of the sequence after had been read from at least that many replicas.</return>
        public static IQueryable<TSource> WithReadQuorum<TSource>(this IQueryable<TSource> source, int quorum)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (quorum < 1)
            {
                throw new ArgumentException("Read quorum cannot be less than 1.", nameof(quorum));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(WithReadQuorum, source, quorum),
                    new[] { source.Expression, Expression.Constant(quorum) }));
        }

        /// <summary>
        /// Disables the index update in the sequence.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the instruction to disable index updates in the sequence.</return>
        public static IQueryable<TSource> WithoutIndexUpdate<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(WithoutIndexUpdate, source),
                    new[] { source.Expression }));
        }

        /// <summary>
        /// Ensures that elements returned is from a "stable" set of shards in the sequence.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the instruction to request elements from a "stable" set of shards in the sequence.</return>
        public static IQueryable<TSource> FromStable<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(FromStable, source),
                    new[] { source.Expression }));
        }

        /// <summary>
        /// Applies an index when requesting elements from the sequence.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <param name="indexes">Array representing the design document and, optionally, the index name.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the index to use when requesting elements from the sequence.</return>
        public static IQueryable<TSource> UseIndex<TSource>(this IQueryable<TSource> source, params string[] indexes)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (indexes == null)
            {
                throw new ArgumentNullException(nameof(indexes));
            }
            if (indexes.Length != 1 && indexes.Length != 2)
            {
                throw new ArgumentException("Only 1 or 2 parameters are allowed. \"<design_document>\" or [\"<design_document>\",\"<index_name>\"]", nameof(indexes));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(UseIndex, source, indexes),
                    new[] { source.Expression, Expression.Constant(indexes) }));
        }

        /// <summary>
        /// Asks for execution stats when requesting elements from the sequence.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the request to ask for execution stats when requesting elements from the sequence.</return>
        public static IQueryable<TSource> IncludeExecutionStats<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(IncludeExecutionStats, source),
                    new[] { source.Expression }));
        }

        /// <summary>
        /// Asks for conflicts when requesting elements from the sequence.
        /// </summary>
        /// <param name="source">The source of items.</param>
        /// <return>An <see cref="IQueryable{TSource}"/> that contains the request to ask for conflicts when requesting elements from the sequence.</return>
        public static IQueryable<TSource> IncludeConflicts<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(IncludeConflicts, source),
                    new[] { source.Expression }));
        }
    }
}
