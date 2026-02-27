using NHibernate;
using NHibernate.Cfg;

namespace DBEngine
{
    public class NHelper
    {
        private static ISessionFactory _sessionFactory;

        private static ISessionFactory SessionFactory
        {
            get
            {
                    if (_sessionFactory != null) return _sessionFactory;
                    var configuration = new Configuration();
                    configuration.Configure();
                    configuration.AddAssembly(typeof(string).Assembly);
                    _sessionFactory = configuration.BuildSessionFactory();
                    return _sessionFactory;
            }
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
