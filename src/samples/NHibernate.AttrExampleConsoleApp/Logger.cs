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