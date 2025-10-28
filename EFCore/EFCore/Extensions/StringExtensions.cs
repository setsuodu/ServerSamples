namespace EFCore.Extensions
{
    // 扩展方法：PascalCase → snake_case
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()))
                         .ToLower();
        }
    }
}