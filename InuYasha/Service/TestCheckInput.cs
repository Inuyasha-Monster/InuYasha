using System;
using System.Diagnostics;
using AspectCore.Extensions.DataValidation;

namespace InuYasha.Service
{
    public class TestCheckInput : ITestCheckInput
    {
        public IDataState DataState { get; set; }

        public void Register(RegisterInput input)
        {
            if (DataState.IsValid)
            {
                //验证通过
                Debug.WriteLine("register.. name:{0},email:{1}", input.Name, input.Email);
                return;
            }

            if (!DataState.IsValid)
            {
                //验证失败
                foreach (var error in DataState.Errors)
                {
                    Debug.WriteLine("error.. key:{0},message:{1}", error.Key, error.ErrorMessage);
                }
            }
        }
    }
}