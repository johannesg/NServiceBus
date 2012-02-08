using System;
using NServiceBus.Saga;
using NHibernate;
using NHibernate.Criterion;

namespace NServiceBus.SagaPersisters.NHibernate
{
    /// <summary>
    /// Saga persister implementation using NHibernate.
    /// </summary>
    public class SagaPersister : ISagaPersister
    {
        /// <summary>
        /// 
        /// </summary>
        public NHibernateMessageModule MessageModule { get; set; }

        /// <summary>
        /// Saves the given saga entity using the current session of the
        /// injected session factory.
        /// </summary>
        /// <param name="saga">the saga entity that will be saved.</param>
        public void Save(ISagaEntity saga)
        {
            MessageModule.CurrentSession.Save(saga);
        }

        /// <summary>
        /// Updates the given saga entity using the current session of the
        /// injected session factory.
        /// </summary>
        /// <param name="saga">the saga entity that will be updated.</param>
        public void Update(ISagaEntity saga)
        {
            MessageModule.CurrentSession.Update(saga);
        }

        /// <summary>
        /// Gets a saga entity from the injected session factory's current session
        /// using the given saga id.
        /// </summary>
        /// <param name="sagaId">The saga id to use in the lookup.</param>
        /// <returns>The saga entity if found, otherwise null.</returns>
        public T Get<T>(Guid sagaId) where T : ISagaEntity
        {
            return MessageModule.CurrentSession.Get<T>(sagaId);
        }

        T ISagaPersister.Get<T>(string property, object value)
        {
            return MessageModule.CurrentSession.CreateCriteria(typeof(T))
                .Add(Restrictions.Eq(property, value))
                .UniqueResult<T>();
        }

        /// <summary>
        /// Deletes the given saga from the injected session factory's
        /// current session.
        /// </summary>
        /// <param name="saga">The saga entity that will be deleted.</param>
        public void Complete(ISagaEntity saga)
        {
            MessageModule.CurrentSession.Delete(saga);
        }
    }
}
