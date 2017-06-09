using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GzipStream
{
    class ByteBlockQueue
    {
        private object locker = new object();
        Queue<ByteBlock> queue = new Queue<ByteBlock>();
        bool isDead = false;
        private int blockId = 0;

        public void EnqueueForWriting(ByteBlock _block)
        {
            int id = _block.ID;
            lock (locker)
            {
                if (isDead)
                    throw new InvalidOperationException("Queue already stopped");

                while (id != blockId)
                {
                    Monitor.Wait(locker);
                }


                queue.Enqueue(_block);
                blockId++;
                Monitor.PulseAll(locker);
            }
        }

        public void EnqueueForCompressing(byte[] buffer)
        {
            lock (locker)
            {
                if (isDead)
                    throw new InvalidOperationException("Queue already stopped");

                ByteBlock _block = new ByteBlock(blockId, buffer);
                queue.Enqueue(_block);
                blockId++;
                Monitor.PulseAll(locker);
            }
        }


        public ByteBlock Dequeue()
        {
            lock (locker)
            {
                while (queue.Count == 0 && !isDead)
                    Monitor.Wait(locker);

                if (queue.Count == 0)
                    return null;

                return queue.Dequeue();

            }
        }

        public void Stop()
        {
            lock (locker)
            {
                isDead = true;
                Monitor.PulseAll(locker);
            }
        }

    }
}
