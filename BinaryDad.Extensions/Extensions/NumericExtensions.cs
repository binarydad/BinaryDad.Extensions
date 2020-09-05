namespace BinaryDad.Extensions
{
    public static class NumericExtensions
    {
        public static string ToCurrency(this decimal currency, bool alwaysShowCents = false)
        {
            // only hide the cents if value is an even amount (i.e., has no cents LOL)
            if (!alwaysShowCents && (int)currency == currency)
            {
                return ((int)currency).ToString("C0");
            }

            return currency.ToString("C");
        }

        public static string WithOrdinal(this int value)
        {
            if (value <= 0)
            {
                return value.ToString();
            }

            switch (value % 100)
            {
                case 11:
                case 12:
                case 13:
                    return $"{value}th";
            }

            switch (value % 10)
            {
                case 1:
                    return $"{value}st";
                case 2:
                    return $"{value}nd";
                case 3:
                    return $"{value}rd";
                default:
                    return $"{value}th";
            }
        }
    }
}
