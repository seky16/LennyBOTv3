namespace LennyBOTv3
{
    public static class Extensions
    {
        public static bool ImplementsInterface(this Type type, Type @interface)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (@interface == null)
                throw new ArgumentNullException(nameof(@interface));

            var interfaces = type.GetInterfaces();
            foreach (var item in interfaces)
            {
                if (@interface.IsGenericTypeDefinition)
                {
                    if (item.IsConstructedGenericType && item.GetGenericTypeDefinition() == @interface)
                    {
                        return true;
                    }
                }
                else
                {
                    if (item == @interface)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
