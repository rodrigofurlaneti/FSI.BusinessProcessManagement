using System.Reflection;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Tests
{
    public static class TestHelpers
    {
        public static void SetId<T>(T entity, long id)
        {
            var prop = typeof(T).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            prop!.SetValue(entity, id);
        }
    }
}
