using System;
using System.Diagnostics;

namespace InuYasha.Service
{
    public class CustomService : ICustomService
    {
        public void Call()
        {
            Debug.WriteLine("service calling...");
        }
    }
}