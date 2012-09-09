// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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