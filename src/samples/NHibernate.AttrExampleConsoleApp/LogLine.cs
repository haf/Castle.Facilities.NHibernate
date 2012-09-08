using System;
using NHibernate.Mapping.Attributes;

namespace NHibernate.AttrExampleConsoleApp
{
    [Serializable]
    [Class(Table = "LOGLINES")]
    public class LogLine
    {
        /// <summary>
        /// Gets the ID of the line in the log.
        /// </summary>
        [Id(Name = "Id", Column = "ID")]
        [Generator(1, Class = "guid.comb")]
        public virtual Guid Id { get; protected set; }

        /// <summary>
        /// Gets the log-line.
        /// </summary>
        [Property(Column = "LINE", NotNull = true, TypeType = typeof(string))]
        public virtual string Line { get; protected set; }

        /// <summary> for serialization </summary>
        [Obsolete("for serialization")]
        protected LogLine()
        {
        }

        public LogLine(string line)
        {
            Line = line;
        }
    }
}