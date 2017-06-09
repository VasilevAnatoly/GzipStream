namespace GzipStream
{
    internal class ByteBlock
    {
        private int _id;
        private byte[] _buffer;
        private byte[] _compressedBuffer;

        public int ID { get { return _id; } }
        public byte[] Buffer { get { return _buffer; } }
        public byte[] CompressedBuffer { get { return _compressedBuffer; } }


        public ByteBlock(int id, byte[] buffer) : this(id, buffer, new byte[0])
        {

        }

        public ByteBlock(int id, byte[] buffer, byte[] compressedBuffer)
        {
            _id = id;
            _buffer = buffer;
            _compressedBuffer = compressedBuffer;
        }
    }
}
