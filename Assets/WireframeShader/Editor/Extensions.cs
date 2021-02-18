namespace WFShader {

    public static class Extensions{

        public static bool GetBit(this int b, int pos) {
            return (b & (1 << pos)) != 0;
        }

        public static int SetBit(this int b, int pos, bool val) {
            return val ? b | (1 << pos) : b & ~(1 << pos);
        }

    }

}