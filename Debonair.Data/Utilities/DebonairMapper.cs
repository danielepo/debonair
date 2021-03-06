﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Debonair.FluentApi;

namespace Debonair.Utilities
{
    public static class DebonairMapper
    {

        /// <typeparam name="TEntity">Objet you want to map TO</typeparam>
        /// <param name="source">Objet you want to map FROM</param>
        /// <param name="strict">Ensure property types match as well as names</param>
        /// <returns></returns>
        public static TEntity MapTo<TEntity>(this object source, bool strict = true) where TEntity : class, new()
        {
            var destination = new TEntity();
            var sourceProperties = source.GetType().GetProperties();
            var destionationProperties = destination.GetType().GetProperties();

            var commonproperties = strict ? (from x in sourceProperties
                                             join y in destionationProperties on new { x.Name, x.PropertyType } equals
                                                 new { y.Name, y.PropertyType }
                                             select new { x, y }) : (from x in sourceProperties
                                                                     join y in destionationProperties on new { Name = x.Name.ToLower() } equals
                                                                         new { Name = y.Name.ToLower() }
                                                                     select new { x, y });

            foreach (var match in commonproperties)
            {
                try
                {
                    match.y.SetValue(destination, match.x.GetValue(source, null), null);
                }
                catch
                {
                    //match didn't work, we'll just ignore them for now
                    //TODO: this needs fixing
                }
            }

            return destination;
        }

        public static List<SqlParameter> ToSqlParameters<TEntity>(this object source) where TEntity : class
        {
            var list = new List<SqlParameter>();

            var mapping = EntityMappingEngine.GetMappingForEntity<TEntity>();

            foreach (var prop in source.GetType().GetProperties())
            {
                var propMapping = mapping.GetMappingForType(prop);
                  
                if (propMapping.IsIgnored) continue;

                list.Add(new SqlParameter(prop.Name, prop.GetValue(source, null)));
            }

            return list;
        }
        public static List<SqlParameter> ToSqlParameters(this Dictionary<string,object> source)
        {
            return source?.Select(item => new SqlParameter(item.Key, item.Value)).ToList() ?? new List<SqlParameter>();
        }
    }
}
