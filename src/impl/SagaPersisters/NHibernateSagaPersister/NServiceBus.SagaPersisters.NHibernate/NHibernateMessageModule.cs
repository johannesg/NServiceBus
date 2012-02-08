using System;
using System.Transactions;
using NHibernate;

namespace NServiceBus.SagaPersisters.NHibernate
{
    /// <summary>
    /// Message module that manages NHibernate sessions.
    /// At the beginning of message handling, a session is opened,
    /// as the end, it is closed.
    /// </summary>
    public class NHibernateMessageModule : IMessageModule
    {

        [ThreadStatic]
        private static ISession _currentSession;

        /// <summary>
        /// 
        /// </summary>
        public ISession CurrentSession
        {
            get { return _currentSession ?? (_currentSession = OpenSession()); }
        }

        private ISession OpenSession()
        {
            if (SessionFactory == null)
                return null;

            var session = SessionFactory.OpenSession();
            session.BeginTransaction(GetIsolationLevel());
            return session;
        }

        void IMessageModule.HandleBeginMessage()
        {
        }

        void IMessageModule.HandleEndMessage()
        {
            if (_currentSession == null)
                return;
            
            var session = _currentSession;
            _currentSession = null;

            using (session)
            using (session.Transaction)
            {
                if (!session.Transaction.IsActive)
                    return;

                session.Transaction.Commit();
            }
        }

        void IMessageModule.HandleError()
        {
            if (_currentSession == null)
                return;

            var session = _currentSession;
            _currentSession = null;

            using (session)
            using (session.Transaction)
            {
                // HandleError is run outside of the DTC transaction so calling rollback here is pointless.

                //if (!session.Transaction.IsActive)
                //    return;

                //session.Transaction.Rollback();
            }
        }

        /// <summary>
        /// Injected NHibernate session factory.
        /// </summary>
        public ISessionFactory SessionFactory { get; set; }

        private System.Data.IsolationLevel GetIsolationLevel()
        {
            if (Transaction.Current == null)
                return System.Data.IsolationLevel.Unspecified;

            switch (Transaction.Current.IsolationLevel)
            {
                case IsolationLevel.Chaos:
                    return System.Data.IsolationLevel.Chaos;
                case IsolationLevel.ReadCommitted:
                    return System.Data.IsolationLevel.ReadCommitted;
                case IsolationLevel.ReadUncommitted:
                    return System.Data.IsolationLevel.ReadUncommitted;
                case IsolationLevel.RepeatableRead:
                    return System.Data.IsolationLevel.RepeatableRead;
                case IsolationLevel.Serializable:
                    return System.Data.IsolationLevel.Serializable;
                case IsolationLevel.Snapshot:
                    return System.Data.IsolationLevel.Snapshot;
                case IsolationLevel.Unspecified:
                    return System.Data.IsolationLevel.Unspecified;
                default:
                    return System.Data.IsolationLevel.Unspecified;
            }
        }
    }
}
