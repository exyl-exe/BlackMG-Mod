namespace GDAPI.MemoryAccessing
{
    struct MemoryValue
    {
        public int[] offsets;
        public int size;

        public MemoryValue(int[] offsets, int size)
        {
            this.offsets = offsets;
            this.size = size;
        }
    }
}
