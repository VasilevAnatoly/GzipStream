using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace GzipStream
{

    class Decompressor : Gzip
    {
        private int counter;
        public Decompressor(string input, string output) : base(input, output)
        {
        }

        public override void Launch()
        {
            Console.WriteLine("Decompressing...\n");

            Thread reader = new Thread(Read);
            reader.Start();

            Thread[] decompressors = new Thread[_threads];
            for (int i = 0; i < _threads; i++)
            {
                decompressors[i] = new Thread(Decompress);
                doneEvents[i] = new ManualResetEvent(false);
                decompressors[i].Start(i);
            }

            Thread writer = new Thread(Write);
            writer.Start();

            WaitHandle.WaitAll(doneEvents);

            if (!_cancelled)
            {
                Console.WriteLine("\nDecompressing has been succesfully finished");
                _success = true;
            }
        }

        private void Read()
        {
            try
            {
                using (FileStream compressedFile = new FileStream(sourceFile, FileMode.Open))
                {
                    while (compressedFile.Position < compressedFile.Length)
                    {
                        byte[] lengthBuffer = new byte[8];
                        compressedFile.Read(lengthBuffer, 0, lengthBuffer.Length);
                        int blockLength = BitConverter.ToInt32(lengthBuffer, 4);
                        byte[] compressedData = new byte[blockLength];
                        lengthBuffer.CopyTo(compressedData, 0);

                        compressedFile.Read(compressedData, 8, blockLength - 8);
                        int _dataSize = BitConverter.ToInt32(compressedData, blockLength - 4);
                        byte[] lastBuffer = new byte[_dataSize];

                        ByteBlock _block = new ByteBlock(counter, lastBuffer, compressedData);
                        _queueReader.EnqueueForWriting(_block);
                        counter++;
                        ConsoleProgress.ProgressBar(compressedFile.Position, compressedFile.Length);

                    }
                    _queueReader.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _cancelled = true;
            }
        }

        private void Decompress(object i)
        {
            try
            {
                ManualResetEvent doneEvent = doneEvents[(int)i];
                while (!_cancelled)
                {
                    ByteBlock _block = _queueReader.Dequeue();
                    if (_block == null)
                    {
                        doneEvent.Set();
                        return;
                    }

                    using (MemoryStream ms = new MemoryStream(_block.CompressedBuffer))
                    {
                        using (GZipStream _gz = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            _gz.Read(_block.Buffer, 0, _block.Buffer.Length);
                            byte[] decompressedData = _block.Buffer.ToArray();
                            ByteBlock block = new ByteBlock(_block.ID, decompressedData);
                            _queueWriter.EnqueueForWriting(block);
                        }
                    }
                }
                doneEvent.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread number {0}. \n Error description: {1}", i, ex.Message);
                _cancelled = true;
            }
        }

        private void Write()
        {
            try
            {
                using (FileStream output = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
                {
                    while (!_cancelled)
                    {
                        ByteBlock _block = _queueWriter.Dequeue();
                        if (_block == null)
                            return;

                        output.Write(_block.Buffer, 0, _block.Buffer.Length);
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _cancelled = true;
            }
        }
    }
}
