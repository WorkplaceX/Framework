namespace UnitTest
{
    using Framework;
    using System;
    using System.Reflection;

    /// <summary>
    /// Base class for tests.
    /// </summary>
    public abstract class UnitTestBase
    {
        /// <summary>
        /// Run invokes all parameterless methods.
        /// </summary>
        public void Run()
        {
            Type type = GetType();
            foreach (var method in type.GetTypeInfo().GetMethods())
            {
                if (method.GetParameters().Length == 0)
                {
                    if (method.DeclaringType == type) // Filter out for example method ToString();
                    {
                        method.Invoke(this, new object[] { }); // Invoke method on static class.
                        UtilFramework.Log($"Method {type.Namespace.Replace("UnitTest.", "")}.{method.Name}(); successful!");
                    }
                }
            }
        }
    }
}