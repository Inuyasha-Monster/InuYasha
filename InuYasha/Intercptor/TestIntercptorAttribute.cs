using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace InuYasha.Intercptor
{
    public class TestIntercptorAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            try
            {
                Debug.WriteLine("Before service call");
                await next(context);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Service threw an exception! {exception}");
                throw;
            }
            finally
            {
                Debug.WriteLine("After service call");
            }
        }
    }
}