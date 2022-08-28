namespace Steelax.PgSpell.Helpers
{
    public static class TaskExtension
    {
        public static async Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> tasks)
            => await Task.WhenAll(tasks);
    }
}
