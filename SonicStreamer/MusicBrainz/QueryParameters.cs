using SonicStreamer.Common.System;
using SonicStreamer.MusicBrainz.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SonicStreamer.MusicBrainz
{
    /// <summary>
    /// Helper for building MusicBrainz query strings.
    /// </summary>
    /// <typeparam name="T">The entity type to search for.</typeparam>
    /// <remarks>
    /// See https://musicbrainz.org/doc/Development/XML_Web_Service/Version_2/Search
    /// </remarks>
    public class QueryParameters<T> where T : Entity
    {
        List<QueryNode> _values;

        public QueryParameters()
        {
            _values = new List<QueryNode>();
        }

        /// <summary>
        /// Add a field to the query paramaters.
        /// </summary>
        /// <param name="key">The field key.</param>
        /// <param name="value">The field value.</param>
        /// <param name="negate">Negate the field (will result in 'AND NOT key:value')</param>
        public void Add(string key, string value, bool negate = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(string.Format(Constants.MusicBrainzMissingParameter, "key"));
            }

            if (!Validate(key))
            {
                throw new Exception(string.Format(Constants.MusicBrainzInvalidQueryParameter, key));
            }

            _values.Add(new QueryNode(key, value, negate));
        }

        public override string ToString()
        {
            return BuildQueryString();
        }

        private string BuildQueryString()
        {
            var sb = new StringBuilder();

            string value;

            foreach (var item in _values)
            {
                // Append operator.
                if (sb.Length > 0)
                {
                    sb.Append(" AND ");
                }

                // Negate operator.
                if (item.Negate)
                {
                    sb.Append("NOT ");
                }

                // Append key.
                sb.Append(item.Key);
                sb.Append(':');

                // Append value.
                value = item.Value;

                if (value.Contains("AND") || value.Contains("OR"))
                {
                    if (!value.StartsWith("("))
                    {
                        // The search value appears to be an expression, so enclose it in brackets.
                        sb.Append("(" + value + ")");
                    }
                    else
                    {
                        sb.Append(value);
                    }
                }
                else if (value.Contains(" ") && !value.StartsWith("\""))
                {
                    // The search value contains whitespace but isn't quoted.
                    sb.Append("\"" + value + "\"");
                }
                else
                {
                    // The search value is already quoted or doesn't need quoting, so just append it.
                    sb.AppendFormat(value);
                }
            }

            return sb.ToString();
        }

        private bool Validate(string key)
        {
            key = "-" + key + "-";

            if (typeof(T) == typeof(Artist))
            {
                return Constants.MusicBrainzArtistQueryParams.IndexOf(key) >= 0;
            }

            if (typeof(T) == typeof(Recording))
            {
                return Constants.MusicBrainzRecordingQueryParams.IndexOf(key) >= 0;
            }

            if (typeof(T) == typeof(Release))
            {
                return Constants.MusicBrainzReleaseQueryParams.IndexOf(key) >= 0;
            }

            if (typeof(T) == typeof(ReleaseGroup))
            {
                return Constants.MusicBrainzReleaseGroupQueryParams.IndexOf(key) >= 0;
            }

            return false;
        }

        class QueryNode
        {
            public string Key { get; }
            public string Value { get; }
            public bool Negate { get; }

            public QueryNode(string key, string value, bool negate)
            {
                Key = key;
                Value = value;
                Negate = negate;
            }
        }
    }
}