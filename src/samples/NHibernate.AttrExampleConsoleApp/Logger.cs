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
using Castle.Transactions;

namespace NHibernate.AttrExampleConsoleApp
{
    public class Logger
    {
        private readonly Func<ISession> getSession;

        public Logger(Func<ISession> getSession)
        {
            this.getSession = getSession;
        }

        [Transaction]
        public virtual void WriteToLog(string text)
        {
            using (var s = getSession())
            {
                s.Save(new LogLine(text));
            }
        }

        [Transaction]
        public virtual void ReadLog(Action<string> reader)
        {
            using (var s = getSession())
            {
                foreach (var line in s.CreateCriteria<LogLine>().List<LogLine>())
                {
                    reader(line.Line);
                }
            }
        }
    }
}