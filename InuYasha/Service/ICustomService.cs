using InuYasha.Intercptor;

namespace InuYasha.Service
{
    public interface ICustomService
    {
        [TestIntercptor]
        void Call();
    }
}