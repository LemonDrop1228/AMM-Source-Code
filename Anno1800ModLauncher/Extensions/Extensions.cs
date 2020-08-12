using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Anno1800ModLauncher.Extensions
{
    public static class HelperExtensions
    {

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var cur in enumerable)
            {
                action(cur);
            }
        }

        public static string TrimDash(this string s)
        {
            if (s.StartsWith("-"))
                return s.TrimStart('-');
            else
                return s;
        }

        public static bool IsActive(this string s)
        {
            return !s.StartsWith("-");
        }

        public static T DeserializeXMLFileToObject<T>(string xmlStringData)
        {
            T returnObject = default(T);
            if (string.IsNullOrEmpty(xmlStringData)) return default(T);

            try
            {
                TextReader xmlStream = new StringReader(xmlStringData);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                returnObject = (T)serializer.Deserialize(xmlStream);
            }
            catch (Exception ex)
            {
                throw;
            }
            return returnObject;
        }
        public static Stream ToStream(this string s)
        {
            return s.ToStream(Encoding.UTF8);
        }

        public static Stream ToStream(this string s, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(s ?? ""));
        }

        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            return items.Contains(item);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> original)
        {
            return new ObservableCollection<T>(original);
        }
    }
}
