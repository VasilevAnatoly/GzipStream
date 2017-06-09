using System;
using System.Threading;

namespace GzipStream
{
    internal abstract class Gzip
    {
        protected bool _cancelled;
        protected bool _success;
        protected readonly string sourceFile, destinationFile;
        protected static readonly int _threads = (Environment.ProcessorCount - 2) > 0 ? Environment.ProcessorCount - 2 : 1;

        protected const int blockSize = 1048576;
        protected ByteBlockQueue _queueReader;
        protected ByteBlockQueue _queueWriter;
        protected readonly ManualResetEvent[] doneEvents;


        public Gzip(string input, string output)
        {
            _cancelled = false;
            _success = false;
            sourceFile = input;
            destinationFile = output;
            _queueReader = new ByteBlockQueue();
            _queueWriter = new ByteBlockQueue();
            doneEvents = new ManualResetEvent[_threads];
        }

        public int CallBackResult()
        {
            if (!_cancelled && _success)
                return 0;
            return 1;
        }

        public void Cancel()
        {
            _cancelled = true;
        }

        public abstract void Launch();
    }
}
