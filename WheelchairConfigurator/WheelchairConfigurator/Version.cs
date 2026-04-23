namespace WheelchairConfigurator
{
    public static class Version
    {
        public static int Major => 0;
        public static int Minor => 9;
        public static int Patch => 1;
        public static ulong GetVersionNumber64bit() => (((ulong)Major & 0xffffUL) << 48) | (((ulong)Minor & 0xffffUL) << 32) | ((ulong)Patch & 0xffffffffUL);
    }
}
